"""Procedural synthesis for all Weapons & Combat SFX in 45: Layered Hell.

Generates 97 distinct 44.1 kHz 16-bit mono WAV buffers in category 'Weapons':
- 6 firearms (pistol, revolver, shotgun, smg, rifle, sniper) x 6 actions
  (draw, fire, echo, reload_out, reload_in, empty) x 2 variants (_01, _02) = 72 files
- shell_{concrete,metal,wood}_{01,02} = 6 files
- melee_hit_{flesh,bone,metal}_{01,02} = 6 files
- melee_swing_{pipe,bat,axe,machete}_01 = 4 files
- explosion_{01,02}, molotov_{01,02}, impact_{concrete,metal,glass}_01, bullet_whiz_{01,02} = 9 files
"""

from __future__ import annotations

import array
import random
from typing import Callable

from synth_core import (
    SAMPLE_RATE,
    add_buf,
    apply_adsr,
    apply_am,
    apply_exp_decay,
    biquad_bp,
    biquad_filter,
    finalize_clip,
    mix_into,
    new_buf,
    noise_brown,
    noise_pink,
    noise_velvet,
    noise_white,
    one_pole_hp,
    one_pole_lp,
    osc_fm,
    osc_sine,
    osc_sweep,
    room_echo,
    soft_clip,
    swept_bp,
)

WEAPON_PROFILES: dict[str, dict[str, float]] = {
    "pistol": {
        "dur": 0.42,
        "punch_f0": 195.0,
        "punch_f1": 52.0,
        "punch_tau": 0.032,
        "crack_f": 2650.0,
        "body_lp": 3600.0,
        "tail_tau": 0.095,
        "drive": 2.1,
        "mech_f": 2100.0,
        "echo_dur": 1.15,
        "echo_lp": 1650.0,
    },
    "revolver": {
        "dur": 0.62,
        "punch_f0": 150.0,
        "punch_f1": 36.0,
        "punch_tau": 0.055,
        "crack_f": 1950.0,
        "body_lp": 2900.0,
        "tail_tau": 0.16,
        "drive": 2.9,
        "mech_f": 1750.0,
        "echo_dur": 1.45,
        "echo_lp": 1400.0,
    },
    "shotgun": {
        "dur": 0.78,
        "punch_f0": 115.0,
        "punch_f1": 26.0,
        "punch_tau": 0.085,
        "crack_f": 1350.0,
        "body_lp": 2400.0,
        "tail_tau": 0.22,
        "drive": 3.4,
        "mech_f": 1350.0,
        "echo_dur": 1.65,
        "echo_lp": 1150.0,
    },
    "smg": {
        "dur": 0.32,
        "punch_f0": 230.0,
        "punch_f1": 68.0,
        "punch_tau": 0.024,
        "crack_f": 3100.0,
        "body_lp": 4200.0,
        "tail_tau": 0.068,
        "drive": 1.9,
        "mech_f": 2550.0,
        "echo_dur": 0.95,
        "echo_lp": 1850.0,
    },
    "rifle": {
        "dur": 0.58,
        "punch_f0": 165.0,
        "punch_f1": 40.0,
        "punch_tau": 0.048,
        "crack_f": 2450.0,
        "body_lp": 3500.0,
        "tail_tau": 0.145,
        "drive": 2.7,
        "mech_f": 1950.0,
        "echo_dur": 1.40,
        "echo_lp": 1500.0,
    },
    "sniper": {
        "dur": 0.92,
        "punch_f0": 125.0,
        "punch_f1": 24.0,
        "punch_tau": 0.095,
        "crack_f": 2850.0,
        "body_lp": 3800.0,
        "tail_tau": 0.26,
        "drive": 3.6,
        "mech_f": 1650.0,
        "echo_dur": 1.85,
        "echo_lp": 1300.0,
    },
}


def _metal_click(
    rng: random.Random,
    freq: float,
    dur: float = 0.045,
    tau: float = 0.011,
) -> array.array:
    fm = osc_fm(dur, freq, freq * 1.414, 3.8, amp=0.85, mod_decay=35.0, carrier_end_freq=freq * 0.65)
    burst = biquad_bp(noise_white(dur, 0.9, rng=rng), freq * 1.25, q=3.2)
    raw = add_buf(fm, burst, 0.65)
    return apply_exp_decay(raw, tau=tau, attack=0.0008)


def synth_weapon_fire(wpn: str, variant: int) -> array.array:
    rng = random.Random(hash(("wpn_fire", wpn, variant)) & 0xFFFFFFFF)
    p = WEAPON_PROFILES[wpn]
    dur = p["dur"] + rng.uniform(-0.02, 0.03)
    out = new_buf(dur)

    # 1. Supersonic muzzle shockwave transient (0-8 ms)
    crack_dur = 0.022
    crack_src = noise_white(crack_dur, 1.0, rng=rng)
    crack = biquad_bp(crack_src, p["crack_f"] * rng.uniform(0.94, 1.07), q=1.8)
    crack = apply_exp_decay(crack, tau=0.0055, attack=0.0004)
    mix_into(out, crack, 0, 1.0)

    # 2. Propellant gas chest thump
    punch = osc_sweep(
        dur * 0.45,
        p["punch_f0"] * rng.uniform(0.95, 1.06),
        p["punch_f1"],
        kind="sine",
        amp=1.0,
    )
    punch = apply_exp_decay(punch, tau=p["punch_tau"], attack=0.0008)
    mix_into(out, punch, 0, 0.95)

    # 3. Blast body (broadband turbulent blast)
    blast_noise = add_buf(
        noise_pink(dur, 0.9, rng=rng),
        noise_brown(dur, 0.65, rng=rng),
        0.75,
    )
    blast = one_pole_lp(blast_noise, p["body_lp"])
    blast = one_pole_hp(blast, 55.0 if wpn not in ("shotgun", "sniper") else 32.0)
    blast = apply_exp_decay(blast, tau=p["tail_tau"], attack=0.0012)
    mix_into(out, blast, 0, 0.90)

    # 4. Bolt/slide metallic clack at 25-45 ms
    clack = _metal_click(rng, p["mech_f"] * rng.uniform(0.95, 1.06), dur=0.055, tau=0.014)
    mix_into(out, clack, int(0.028 * SAMPLE_RATE), 0.42)

    # Shotgun extra pellet dispersion crackle + pump/breech resonance
    if wpn == "shotgun":
        pellets = biquad_bp(noise_velvet(0.14, density=3600.0, amp=0.9, rng=rng), 3200.0, q=1.4)
        pellets = apply_exp_decay(pellets, tau=0.045, attack=0.002)
        mix_into(out, pellets, int(0.008 * SAMPLE_RATE), 0.65)
    elif wpn == "sniper":
        # High-velocity barrel ring harmonic
        ring = osc_fm(0.28, 1480.0, 2210.0, 2.1, amp=0.35, mod_decay=12.0)
        ring = apply_exp_decay(ring, tau=0.075, attack=0.002)
        mix_into(out, ring, int(0.012 * SAMPLE_RATE), 0.35)

    out = soft_clip(out, drive=p["drive"], mix=0.88)
    out = room_echo(out, room_size=0.65, decay=0.36, wet=0.25)
    return finalize_clip(out, fade_in=0.0005, fade_out=0.02)


def synth_weapon_echo(wpn: str, variant: int) -> array.array:
    rng = random.Random(hash(("wpn_echo", wpn, variant)) & 0xFFFFFFFF)
    p = WEAPON_PROFILES[wpn]
    dur = p["echo_dur"] + rng.uniform(-0.06, 0.08)

    # Distant corridor reflection tail: low-passed rumble + multi-wall slapback
    noise = add_buf(
        noise_pink(dur, 0.85, rng=rng),
        noise_brown(dur, 0.75, rng=rng),
        0.8,
    )
    body = one_pole_lp(noise, p["echo_lp"] * rng.uniform(0.92, 1.08))
    body = one_pole_hp(body, 70.0)
    body = apply_adsr(body, 0.035, 0.22, 0.48, dur * 0.55)

    thump = osc_sweep(0.35, p["punch_f0"] * 0.65, 32.0, kind="sine", amp=0.6)
    thump = apply_exp_decay(thump, tau=0.11, attack=0.018)
    mix_into(body, thump, int(0.02 * SAMPLE_RATE), 0.65)

    body = room_echo(body, room_size=1.25, decay=0.65, wet=0.62)
    return finalize_clip(body, fade_in=0.012, fade_out=0.06)


def synth_weapon_draw(wpn: str, variant: int) -> array.array:
    rng = random.Random(hash(("wpn_draw", wpn, variant)) & 0xFFFFFFFF)
    p = WEAPON_PROFILES[wpn]
    dur = 0.42 + rng.uniform(-0.03, 0.04)
    out = new_buf(dur)

    # Holster cloth/leather friction swoosh
    cloth = swept_bp(noise_pink(dur * 0.75, 0.85, rng=rng), 420.0, 1480.0, q=2.4)
    cloth = apply_adsr(cloth, 0.025, 0.10, 0.55, 0.12)
    mix_into(out, cloth, 0, 0.85)

    # Metallic safety / receiver rattle clicks
    c1 = _metal_click(rng, p["mech_f"] * 0.82, dur=0.04, tau=0.009)
    c2 = _metal_click(rng, p["mech_f"] * 1.12, dur=0.045, tau=0.011)
    mix_into(out, c1, int(0.11 * SAMPLE_RATE), 0.65)
    mix_into(out, c2, int(0.24 * SAMPLE_RATE), 0.80)
    return finalize_clip(out)


def synth_weapon_reload_out(wpn: str, variant: int) -> array.array:
    rng = random.Random(hash(("wpn_reload_out", wpn, variant)) & 0xFFFFFFFF)
    p = WEAPON_PROFILES[wpn]
    dur = 0.48 + rng.uniform(-0.03, 0.04)
    out = new_buf(dur)

    # Mag release catch button click
    btn = _metal_click(rng, p["mech_f"] * 1.05, dur=0.04, tau=0.009)
    mix_into(out, btn, int(0.02 * SAMPLE_RATE), 0.9)

    # Sliding friction out of magwell / cylinder swing out
    slide_src = noise_white(0.22, 0.8, rng=rng)
    slide = swept_bp(slide_src, p["mech_f"] * 0.95, p["mech_f"] * 0.48, q=4.2)
    slide = apply_adsr(slide, 0.01, 0.06, 0.6, 0.08)
    mix_into(out, slide, int(0.07 * SAMPLE_RATE), 0.85)

    # Final lip separation clink
    lip = _metal_click(rng, p["mech_f"] * 0.75, dur=0.05, tau=0.012)
    mix_into(out, lip, int(0.27 * SAMPLE_RATE), 0.65)
    return finalize_clip(out)


def synth_weapon_reload_in(wpn: str, variant: int) -> array.array:
    rng = random.Random(hash(("wpn_reload_in", wpn, variant)) & 0xFFFFFFFF)
    p = WEAPON_PROFILES[wpn]
    dur = 0.56 + rng.uniform(-0.03, 0.04)
    out = new_buf(dur)

    # Guide slide upwards
    guide = swept_bp(noise_white(0.14, 0.75, rng=rng), p["mech_f"] * 0.5, p["mech_f"] * 0.9, q=3.8)
    guide = apply_adsr(guide, 0.01, 0.04, 0.6, 0.04)
    mix_into(out, guide, int(0.03 * SAMPLE_RATE), 0.65)

    # Solid magazine seating thud + latch lock
    thud = osc_sweep(0.08, 160.0, 58.0, kind="sine", amp=0.85)
    thud = apply_exp_decay(thud, tau=0.022, attack=0.001)
    lock = _metal_click(rng, p["mech_f"] * 0.92, dur=0.055, tau=0.013)
    mix_into(out, thud, int(0.15 * SAMPLE_RATE), 0.85)
    mix_into(out, lock, int(0.155 * SAMPLE_RATE), 0.90)

    # Charging handle / slide rack snap at 0.34 s
    rack_pull = _metal_click(rng, p["mech_f"] * 1.15, dur=0.05, tau=0.011)
    rack_home = _metal_click(rng, p["mech_f"] * 1.32, dur=0.06, tau=0.014)
    mix_into(out, rack_pull, int(0.32 * SAMPLE_RATE), 0.75)
    mix_into(out, rack_home, int(0.41 * SAMPLE_RATE), 0.95)
    return finalize_clip(out)


def synth_weapon_empty(wpn: str, variant: int) -> array.array:
    rng = random.Random(hash(("wpn_empty", wpn, variant)) & 0xFFFFFFFF)
    p = WEAPON_PROFILES[wpn]
    dur = 0.16 + rng.uniform(-0.01, 0.015)
    out = new_buf(dur)

    # Crisp dry firing-pin click + tiny spring ring
    pin = _metal_click(rng, p["mech_f"] * 1.35 * rng.uniform(0.95, 1.05), dur=0.038, tau=0.007)
    spring = osc_sine(0.08, p["mech_f"] * 1.75, amp=0.25)
    spring = apply_exp_decay(spring, tau=0.018, attack=0.001)
    mix_into(out, pin, int(0.005 * SAMPLE_RATE), 1.0)
    mix_into(out, spring, int(0.012 * SAMPLE_RATE), 0.45)
    return finalize_clip(out)


def synth_shell_drop(surface: str, variant: int) -> array.array:
    rng = random.Random(hash(("shell", surface, variant)) & 0xFFFFFFFF)
    dur = 0.48
    out = new_buf(dur)

    if surface == "concrete":
        freqs = (4450.0, 6280.0, 8120.0)
        q_val = 28.0
        tau = 0.022
    elif surface == "metal":
        freqs = (3850.0, 5620.0, 7950.0)
        q_val = 40.0
        tau = 0.036
    else:  # wood
        freqs = (3400.0, 4850.0, 6100.0)
        q_val = 18.0
        tau = 0.016

    # 3 bouncing brass casing impacts with decreasing interval and amplitude
    bounces = (0.01, 0.14 + rng.uniform(-0.015, 0.02), 0.24 + rng.uniform(-0.01, 0.02), 0.31)
    for idx, b_t in enumerate(bounces):
        b_dur = 0.09
        gain = 0.95 / (1.0 + 0.65 * idx)
        ping = new_buf(b_dur)
        for f_base in freqs:
            f = f_base * rng.uniform(0.97, 1.03)
            partial = osc_sine(b_dur, f, amp=0.45)
            mix_into(ping, partial, 0, 1.0)
        if surface == "wood":
            thud = biquad_bp(noise_pink(b_dur, 0.6, rng=rng), 420.0, q=3.0)
            mix_into(ping, thud, 0, 0.5)
        ping = apply_exp_decay(ping, tau=tau / (1.0 + 0.2 * idx), attack=0.0005)
        ping = biquad_filter(ping, "hp", 1200.0, q=min(2.0, q_val * 0.05))
        mix_into(out, ping, int(b_t * SAMPLE_RATE), gain)

    return finalize_clip(out)


def synth_melee_hit(target: str, variant: int) -> array.array:
    rng = random.Random(hash(("melee_hit", target, variant)) & 0xFFFFFFFF)
    dur = 0.44
    out = new_buf(dur)

    if target == "flesh":
        thud = osc_sweep(0.18, 145.0, 42.0, kind="sine", amp=0.95)
        thud = apply_exp_decay(thud, tau=0.045, attack=0.001)
        splat = swept_bp(noise_pink(0.24, 1.0, rng=rng), 1650.0, 480.0, q=2.4)
        splat = apply_exp_decay(splat, tau=0.06, attack=0.002)
        mix_into(out, thud, 0, 1.0)
        mix_into(out, splat, int(0.006 * SAMPLE_RATE), 0.85)
    elif target == "bone":
        thud = osc_sweep(0.16, 175.0, 48.0, kind="sine", amp=0.9)
        thud = apply_exp_decay(thud, tau=0.038, attack=0.001)
        crack = osc_fm(0.12, 410.0, 710.0, 5.2, amp=0.9, mod_decay=28.0, carrier_end_freq=140.0)
        crack = apply_exp_decay(crack, tau=0.025, attack=0.0008)
        snap = biquad_bp(noise_white(0.11, 1.0, rng=rng), 2350.0 + variant * 180.0, q=2.6)
        snap = apply_exp_decay(snap, tau=0.022, attack=0.0008)
        mix_into(out, thud, 0, 0.85)
        mix_into(out, crack, int(0.005 * SAMPLE_RATE), 0.95)
        mix_into(out, snap, int(0.012 * SAMPLE_RATE), 0.90)
    else:  # metal
        clang1 = osc_fm(dur, 880.0 + variant * 45.0, 1340.0, 4.2, amp=0.75, mod_decay=10.0)
        clang2 = osc_fm(dur, 1720.0, 2490.0, 3.1, amp=0.55, mod_decay=14.0)
        ring = apply_exp_decay(add_buf(clang1, clang2, 0.7), tau=0.11, attack=0.0006)
        spark = biquad_bp(noise_white(0.06, 0.9, rng=rng), 4200.0, q=2.5)
        spark = apply_exp_decay(spark, tau=0.015, attack=0.0005)
        mix_into(out, ring, 0, 0.95)
        mix_into(out, spark, 0, 0.80)

    out = soft_clip(out, drive=2.1, mix=0.85)
    return finalize_clip(out)


def synth_melee_swing(weapon: str) -> array.array:
    rng = random.Random(hash(("melee_swing", weapon)) & 0xFFFFFFFF)
    cfg = {
        "pipe": (0.34, 240.0, 720.0, 2.2),
        "bat": (0.38, 180.0, 560.0, 1.9),
        "axe": (0.42, 150.0, 890.0, 2.8),
        "machete": (0.28, 420.0, 1850.0, 3.6),
    }[weapon]
    dur, f0, f1, q_val = cfg
    src = noise_pink(dur, 1.0, rng=rng)
    whoosh = swept_bp(src, f0, f1, q=q_val)
    whoosh = apply_adsr(whoosh, dur * 0.25, dur * 0.25, 0.7, dur * 0.35)
    if weapon in ("axe", "machete"):
        blade = swept_bp(noise_white(dur, 0.6, rng=rng), 1600.0, 3600.0, q=5.0)
        blade = apply_adsr(blade, dur * 0.3, dur * 0.2, 0.5, dur * 0.3)
        whoosh = add_buf(whoosh, blade, 0.45)
    return finalize_clip(whoosh)


def synth_explosion(variant: int) -> array.array:
    rng = random.Random(hash(("explosion", variant)) & 0xFFFFFFFF)
    dur = 1.95 + rng.uniform(-0.08, 0.10)
    out = new_buf(dur)

    # Detonation shockwave crack
    det = biquad_bp(noise_white(0.06, 1.0, rng=rng), 1850.0, q=1.2)
    det = apply_exp_decay(det, tau=0.016, attack=0.0005)
    mix_into(out, det, 0, 1.0)

    # Massive sub-bass boom
    sub = osc_sweep(0.85, 130.0 + variant * 12.0, 22.0, kind="sine", amp=1.0)
    sub = apply_exp_decay(sub, tau=0.26, attack=0.002)
    mix_into(out, sub, 0, 1.0)

    # Roaring fireball rumble
    roar_src = add_buf(noise_brown(dur, 0.95, rng=rng), noise_pink(dur, 0.75, rng=rng), 0.7)
    roar = one_pole_lp(roar_src, 950.0)
    roar = apply_exp_decay(roar, tau=0.48, attack=0.005)
    mix_into(out, roar, 0, 0.95)

    # Falling debris & shrapnel crackles
    debris = biquad_bp(noise_velvet(dur * 0.85, density=1600.0, amp=0.85, rng=rng), 2400.0, q=1.8)
    debris = apply_adsr(debris, 0.08, 0.35, 0.45, 0.65)
    mix_into(out, debris, int(0.12 * SAMPLE_RATE), 0.60)

    out = soft_clip(out, drive=3.1, mix=0.9)
    out = room_echo(out, room_size=1.35, decay=0.58, wet=0.42)
    return finalize_clip(out)


def synth_molotov(variant: int) -> array.array:
    rng = random.Random(hash(("molotov", variant)) & 0xFFFFFFFF)
    dur = 1.65
    out = new_buf(dur)

    # 1. Glass bottle shatter at impact (0.0 - 0.35 s)
    for s_idx in range(9):
        off = int((s_idx * 0.022 + rng.uniform(0.0, 0.015)) * SAMPLE_RATE)
        f_sh = 2800.0 + rng.uniform(0.0, 4200.0)
        shard = osc_fm(0.09, f_sh, f_sh * 1.37, 3.2, amp=0.6, mod_decay=25.0)
        shard = apply_exp_decay(shard, tau=0.022, attack=0.0005)
        mix_into(out, shard, off, 0.75 / (1.0 + 0.15 * s_idx))

    # 2. Ignition fireball whoosh (0.04 - 0.85 s)
    whoosh_src = add_buf(noise_pink(1.2, 0.95, rng=rng), noise_brown(1.2, 0.8, rng=rng), 0.8)
    whoosh = swept_bp(whoosh_src, 220.0, 1150.0, q=1.6)
    whoosh = apply_adsr(whoosh, 0.04, 0.25, 0.6, 0.55)
    mix_into(out, whoosh, int(0.03 * SAMPLE_RATE), 1.0)

    # 3. Burning gasoline crackle tail
    crackle = biquad_bp(noise_velvet(dur * 0.9, density=2600.0, amp=0.9, rng=rng), 3400.0, q=2.0)
    crackle = apply_adsr(crackle, 0.08, 0.3, 0.65, 0.5)
    mix_into(out, crackle, int(0.08 * SAMPLE_RATE), 0.75)

    out = soft_clip(out, drive=2.0, mix=0.8)
    return finalize_clip(out)


def synth_surface_impact(surface: str) -> array.array:
    rng = random.Random(hash(("impact", surface)) & 0xFFFFFFFF)
    dur = 0.38
    out = new_buf(dur)

    if surface == "concrete":
        thud = osc_sweep(0.12, 165.0, 48.0, kind="sine", amp=0.9)
        thud = apply_exp_decay(thud, tau=0.028, attack=0.0008)
        chip = biquad_bp(noise_white(0.18, 0.95, rng=rng), 1950.0, q=2.0)
        chip = apply_exp_decay(chip, tau=0.042, attack=0.001)
        mix_into(out, thud, 0, 0.95)
        mix_into(out, chip, 0, 0.90)
    elif surface == "metal":
        ping = osc_fm(dur, 1120.0, 1760.0, 4.8, amp=0.85, mod_decay=12.0, carrier_end_freq=960.0)
        ping = apply_exp_decay(ping, tau=0.095, attack=0.0005)
        mix_into(out, ping, 0, 1.0)
    else:  # glass
        burst = biquad_bp(noise_white(0.12, 1.0, rng=rng), 3800.0, q=2.2)
        burst = apply_exp_decay(burst, tau=0.03, attack=0.0005)
        mix_into(out, burst, 0, 0.9)
        for i in range(6):
            f_g = 3200.0 + i * 620.0 + rng.uniform(-150, 150)
            tinkle = osc_sine(0.11, f_g, amp=0.45)
            tinkle = apply_exp_decay(tinkle, tau=0.026, attack=0.0006)
            mix_into(out, tinkle, int((0.01 + i * 0.038) * SAMPLE_RATE), 0.7)

    return finalize_clip(out)


def synth_bullet_whiz(variant: int) -> array.array:
    rng = random.Random(hash(("bullet_whiz", variant)) & 0xFFFFFFFF)
    dur = 0.32
    f_start = 2900.0 + variant * 350.0
    f_end = 780.0 - variant * 80.0

    whine = osc_sweep(dur, f_start, f_end, kind="sine", amp=0.75)
    air = swept_bp(noise_white(dur, 0.95, rng=rng), f_start * 1.1, f_end * 0.9, q=4.5)
    raw = add_buf(whine, air, 0.85)
    raw = apply_adsr(raw, 0.04, 0.08, 0.85, 0.12)
    return finalize_clip(raw)


def get_weapons_sound_specs() -> list[tuple[str, str, Callable[[], array.array]]]:
    """Return list of (category, clip_stem, generator_fn) for all 97 weapon audio files."""
    specs: list[tuple[str, str, Callable[[], array.array]]] = []

    wpn_actions: dict[str, Callable[[str, int], array.array]] = {
        "draw": synth_weapon_draw,
        "fire": synth_weapon_fire,
        "echo": synth_weapon_echo,
        "reload_out": synth_weapon_reload_out,
        "reload_in": synth_weapon_reload_in,
        "empty": synth_weapon_empty,
    }

    for wpn in ("pistol", "revolver", "shotgun", "smg", "rifle", "sniper"):
        for act_name, fn in wpn_actions.items():
            for v in (1, 2):
                stem = f"wpn_{wpn}_{act_name}_{v:02d}"
                specs.append(("Weapons", stem, lambda w=wpn, v_i=v, f=fn: f(w, v_i)))

    for surf in ("concrete", "metal", "wood"):
        for v in (1, 2):
            stem = f"shell_{surf}_{v:02d}"
            specs.append(("Weapons", stem, lambda s=surf, v_i=v: synth_shell_drop(s, v_i)))

    for target in ("flesh", "bone", "metal"):
        for v in (1, 2):
            stem = f"melee_hit_{target}_{v:02d}"
            specs.append(("Weapons", stem, lambda t=target, v_i=v: synth_melee_hit(t, v_i)))

    for melee_wpn in ("pipe", "bat", "axe", "machete"):
        stem = f"melee_swing_{melee_wpn}_01"
        specs.append(("Weapons", stem, lambda mw=melee_wpn: synth_melee_swing(mw)))

    for v in (1, 2):
        specs.append(("Weapons", f"explosion_{v:02d}", lambda v_i=v: synth_explosion(v_i)))
        specs.append(("Weapons", f"molotov_{v:02d}", lambda v_i=v: synth_molotov(v_i)))
        specs.append(("Weapons", f"bullet_whiz_{v:02d}", lambda v_i=v: synth_bullet_whiz(v_i)))

    for surf in ("concrete", "metal", "glass"):
        specs.append(("Weapons", f"impact_{surf}_01", lambda s=surf: synth_surface_impact(s)))

    return specs
