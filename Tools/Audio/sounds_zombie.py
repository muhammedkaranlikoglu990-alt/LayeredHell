"""Procedural zombie audio synthesis for 45: Layered Hell (90 files in Assets/Audio/Zombies/)."""

from __future__ import annotations

import array
import math
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
    env_adsr,
    env_exp_decay,
    finalize_clip,
    formant_filter,
    mix_into,
    mul_buf,
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
    osc_vocal_cord,
    room_echo,
    soft_clip,
    swept_bp,
)

ARCHETYPES: dict[str, dict] = {
    "walker": {
        "f0": 92.0,
        "formants": [320.0, 860.0, 1920.0],
        "qs": [4.2, 5.5, 7.0],
        "roughness": 0.38,
        "sub": 0.40,
        "drive": 1.8,
        "lp": 3400.0,
    },
    "runner": {
        "f0": 225.0,
        "formants": [640.0, 1720.0, 3050.0],
        "qs": [4.8, 6.2, 8.0],
        "roughness": 0.46,
        "sub": 0.22,
        "drive": 2.4,
        "lp": 5200.0,
    },
    "spitter": {
        "f0": 142.0,
        "formants": [430.0, 1240.0, 2420.0],
        "qs": [6.5, 8.5, 10.0],
        "roughness": 0.34,
        "sub": 0.30,
        "drive": 1.7,
        "lp": 4400.0,
    },
    "armored": {
        "f0": 104.0,
        "formants": [265.0, 690.0, 1440.0],
        "qs": [5.5, 7.5, 9.0],
        "roughness": 0.42,
        "sub": 0.48,
        "drive": 2.1,
        "lp": 2200.0,
    },
    "goliath": {
        "f0": 54.0,
        "formants": [175.0, 480.0, 1140.0],
        "qs": [4.0, 5.5, 7.5],
        "roughness": 0.50,
        "sub": 0.75,
        "drive": 2.6,
        "lp": 2900.0,
    },
}


def _apply_archetype_color(
    buf: array.array,
    arch: str,
    rng: random.Random,
) -> array.array:
    cfg = ARCHETYPES[arch]
    if arch == "spitter":
        # Wet bubbling modulation + liquid resonant droplets
        bubble_rate = 14.0 + rng.uniform(-2.5, 3.5)
        buf = apply_am(buf, bubble_rate, depth=0.68, phase=rng.random() * 6.28)
        dur = len(buf) / SAMPLE_RATE
        wet = swept_bp(
            noise_pink(dur, 0.4, rng=rng),
            550.0 + rng.uniform(-80, 120),
            1650.0 + rng.uniform(-200, 250),
            q=7.5,
        )
        buf = add_buf(buf, wet, 0.55)
    elif arch == "armored":
        # Muffled helmet resonance + metallic visor clank
        r1 = biquad_bp(buf, 520.0 + rng.uniform(-30, 30), q=9.0)
        r2 = biquad_bp(buf, 1080.0 + rng.uniform(-50, 50), q=11.0)
        buf = add_buf(buf, r1, 0.55)
        buf = add_buf(buf, r2, 0.35)
        buf = one_pole_lp(buf, cfg["lp"])
    elif arch == "goliath":
        # Sub-bass chest rumble
        dur = len(buf) / SAMPLE_RATE
        sub = osc_sweep(dur, cfg["f0"] * 0.75, cfg["f0"] * 0.55, kind="sine", amp=0.55)
        sub = apply_adsr(sub, 0.04, 0.15, 0.65, 0.25)
        buf = add_buf(buf, sub, 0.65)
    buf = soft_clip(buf, drive=cfg["drive"], mix=0.85)
    buf = one_pole_lp(buf, cfg["lp"])
    return buf


def synth_zombie_breath(arch: str, variant: int) -> array.array:
    rng = random.Random(hash(("zmb_breath", arch, variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES[arch]
    dur = (0.75 if arch == "runner" else 1.15 if arch != "goliath" else 1.45) + rng.uniform(-0.08, 0.10)
    out = new_buf(dur)

    # Phase 1: raspy inhale (first 42% of clip)
    d_in = dur * 0.42
    f0 = cfg["f0"] * rng.uniform(0.92, 1.08)
    inhale_noise = noise_pink(d_in, amp=0.65, rng=rng)
    inhale_formants = [f * 1.12 for f in cfg["formants"]]
    inhale = formant_filter(inhale_noise, inhale_formants, cfg["qs"])
    inhale_vox = osc_vocal_cord(
        d_in,
        f0 * 0.95,
        f0 * 1.12,
        roughness=cfg["roughness"],
        subharmonic=cfg["sub"] * 0.5,
        rng=rng,
    )
    inhale_vox = formant_filter(inhale_vox, inhale_formants, cfg["qs"])
    inhale = add_buf(inhale, inhale_vox, 0.45)
    inhale = apply_adsr(inhale, 0.06, 0.10, 0.75, 0.08)
    mix_into(out, inhale, 0, 0.75)

    # Phase 2: guttural exhale (last 55% of clip)
    d_ex = dur * 0.54
    ex_offset = int(dur * 0.45 * SAMPLE_RATE)
    ex_vox = osc_vocal_cord(
        d_ex,
        f0 * 1.08,
        f0 * 0.78,
        roughness=cfg["roughness"] * 1.15,
        subharmonic=cfg["sub"],
        rng=rng,
    )
    ex_noise = noise_pink(d_ex, amp=0.55, rng=rng)
    ex_src = add_buf(ex_vox, ex_noise, 0.65)
    ex_filt = formant_filter(ex_src, cfg["formants"], cfg["qs"])
    ex_filt = apply_adsr(ex_filt, 0.04, 0.14, 0.65, 0.16)
    mix_into(out, ex_filt, ex_offset, 1.0)

    out = _apply_archetype_color(out, arch, rng)
    out = room_echo(out, room_size=0.45, decay=0.28, wet=0.20)
    return finalize_clip(out)


def synth_zombie_scream_far(arch: str, variant: int) -> array.array:
    rng = random.Random(hash(("zmb_scream_far", arch, variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES[arch]
    dur = (1.65 if arch != "goliath" else 2.10) + rng.uniform(-0.10, 0.15)
    f0 = cfg["f0"] * (1.45 if arch != "goliath" else 1.25) * rng.uniform(0.92, 1.10)

    # Rising then falling agonizing shriek/roar
    vox1 = osc_vocal_cord(
        dur * 0.82,
        f0 * 0.85,
        f0 * 1.28,
        roughness=cfg["roughness"] * 1.25,
        subharmonic=cfg["sub"],
        vibrato_hz=6.5 + rng.uniform(-1.0, 1.5),
        vibrato_depth=0.055,
        rng=rng,
    )
    vox2 = osc_sweep(
        dur * 0.82,
        f0 * 1.52,
        f0 * 1.05,
        kind="saw",
        amp=0.38,
        vibrato_hz=7.2,
        vibrato_depth=0.04,
    )
    rasp = noise_pink(dur * 0.82, amp=0.55, rng=rng)
    raw = add_buf(vox1, vox2, 0.45)
    raw = add_buf(raw, rasp, 0.50)

    formants = [f * (1.18 if arch != "goliath" else 1.05) for f in cfg["formants"]]
    shaped = formant_filter(raw, formants, cfg["qs"])
    shaped = apply_adsr(shaped, 0.08, 0.25, 0.72, 0.35)
    shaped = _apply_archetype_color(shaped, arch, rng)

    # Pad buffer for distant corridor tail and filter high frequencies for distance
    out = new_buf(dur)
    mix_into(out, shaped, 0, 0.85)
    out = one_pole_hp(out, 110.0 if arch != "goliath" else 48.0)
    out = one_pole_lp(out, 2400.0 if arch != "runner" else 3100.0)
    out = room_echo(out, room_size=1.15, decay=0.62, wet=0.55)
    return finalize_clip(out)


def synth_zombie_scratch(arch: str, variant: int) -> array.array:
    rng = random.Random(hash(("zmb_scratch", arch, variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES[arch]
    dur = 0.55 + rng.uniform(-0.05, 0.08)
    out = new_buf(dur)

    num_strokes = 2 + (variant % 2)
    for s in range(num_strokes):
        st_dur = 0.13 + rng.uniform(-0.02, 0.03)
        st_off = int((0.03 + s * 0.16 + rng.uniform(-0.01, 0.015)) * SAMPLE_RATE)
        noise = noise_white(st_dur, amp=0.9, rng=rng)
        grit = noise_velvet(st_dur, density=2200.0, amp=0.8, rng=rng)
        src = add_buf(noise, grit, 0.65)
        f_hi = 1800.0 + rng.uniform(-250, 450) + (500.0 if arch == "runner" else 0.0)
        scrape = swept_bp(src, f_hi * 0.65, f_hi * 1.45, q=3.8)
        scrape = apply_adsr(scrape, 0.008, 0.04, 0.6, 0.04)
        mix_into(out, scrape, st_off, 0.9 / (1.0 + 0.15 * s))

    # Add short low throat grunt behind claw scratch
    grunt_dur = dur * 0.55
    grunt = osc_vocal_cord(
        grunt_dur,
        cfg["f0"] * 1.05,
        cfg["f0"] * 0.82,
        roughness=cfg["roughness"],
        subharmonic=cfg["sub"],
        rng=rng,
    )
    grunt = formant_filter(grunt, cfg["formants"], cfg["qs"])
    grunt = apply_exp_decay(grunt, tau=0.14, attack=0.015)
    mix_into(out, grunt, int(0.02 * SAMPLE_RATE), 0.45)

    out = _apply_archetype_color(out, arch, rng)
    return finalize_clip(out)


def synth_zombie_attack(arch: str, variant: int) -> array.array:
    rng = random.Random(hash(("zmb_attack", arch, variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES[arch]
    dur = (0.72 if arch != "goliath" else 1.05) + rng.uniform(-0.05, 0.07)
    f0 = cfg["f0"] * rng.uniform(1.10, 1.28)

    # Explosive vocal snarl
    vox = osc_vocal_cord(
        dur,
        f0 * 1.30,
        f0 * 0.72,
        roughness=cfg["roughness"] * 1.35,
        subharmonic=cfg["sub"] * 1.15,
        vibrato_hz=8.5,
        vibrato_depth=0.05,
        rng=rng,
    )
    burst = noise_pink(dur, amp=0.70, rng=rng)
    snarl = formant_filter(add_buf(vox, burst, 0.55), cfg["formants"], cfg["qs"])
    snarl = apply_adsr(snarl, 0.015, 0.16, 0.62, 0.18)

    # Bite / jaw snap impact at ~90 ms
    snap_dur = 0.12
    snap = osc_fm(snap_dur, 180.0, 95.0, 4.5, amp=0.8, mod_decay=18.0, carrier_end_freq=65.0)
    snap_crisp = biquad_bp(noise_white(snap_dur, 0.9, rng=rng), 1650.0, q=2.5)
    snap = apply_exp_decay(add_buf(snap, snap_crisp, 0.65), tau=0.035, attack=0.002)
    mix_into(snarl, snap, int(0.07 * SAMPLE_RATE), 0.85)

    snarl = _apply_archetype_color(snarl, arch, rng)
    snarl = room_echo(snarl, room_size=0.4, decay=0.25, wet=0.18)
    return finalize_clip(snarl)


def synth_zombie_bone(arch: str, variant: int) -> array.array:
    rng = random.Random(hash(("zmb_bone", arch, variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES[arch]
    dur = 0.52 + rng.uniform(-0.04, 0.05)
    out = new_buf(dur)

    # 3 staggered bone fracture snaps + wet tissue crunch
    cracks = 3
    for c in range(cracks):
        c_off = int((0.02 + c * 0.095 + rng.uniform(-0.01, 0.02)) * SAMPLE_RATE)
        c_dur = 0.11
        # Calcified FM pop
        f_base = (320.0 if arch != "goliath" else 210.0) + rng.uniform(-45, 75) + c * 60.0
        pop = osc_fm(c_dur, f_base, f_base * 1.73, 5.5, amp=0.9, mod_decay=28.0, carrier_end_freq=f_base * 0.4)
        pop = apply_exp_decay(pop, tau=0.022, attack=0.001)
        # Sharp high-mid crack transient
        crack_n = biquad_bp(noise_white(c_dur, 1.0, rng=rng), 2100.0 + c * 350.0, q=2.8)
        crack_n = apply_exp_decay(crack_n, tau=0.018, attack=0.001)
        mix_into(out, pop, c_off, 0.95 / (1.0 + 0.2 * c))
        mix_into(out, crack_n, c_off, 0.85 / (1.0 + 0.2 * c))

    # Painful short choked gasp
    gasp = osc_vocal_cord(
        dur * 0.65,
        cfg["f0"] * 1.15,
        cfg["f0"] * 0.65,
        roughness=0.5,
        subharmonic=cfg["sub"],
        rng=rng,
    )
    gasp = formant_filter(gasp, cfg["formants"], cfg["qs"])
    gasp = apply_exp_decay(gasp, tau=0.14, attack=0.01)
    mix_into(out, gasp, int(0.04 * SAMPLE_RATE), 0.45)

    out = soft_clip(out, drive=2.0, mix=0.8)
    return finalize_clip(out)


def synth_zombie_death(arch: str, variant: int) -> array.array:
    rng = random.Random(hash(("zmb_death", arch, variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES[arch]
    dur = (1.35 if arch != "goliath" else 1.95) + rng.uniform(-0.08, 0.12)
    f0 = cfg["f0"] * rng.uniform(0.95, 1.12)

    # Long descending death rattle groan
    vox = osc_vocal_cord(
        dur * 0.85,
        f0 * 1.18,
        f0 * 0.42,
        roughness=cfg["roughness"] * 1.4,
        subharmonic=cfg["sub"] * 1.25,
        vibrato_hz=4.5,
        vibrato_depth=0.06,
        rng=rng,
    )
    gurgle = noise_pink(dur * 0.85, amp=0.6, rng=rng)
    gurgle = apply_am(gurgle, 13.0 + rng.uniform(-2, 3), depth=0.75)
    rattle = formant_filter(add_buf(vox, gurgle, 0.55), cfg["formants"], cfg["qs"])
    rattle = apply_adsr(rattle, 0.03, 0.28, 0.55, 0.38)
    rattle = _apply_archetype_color(rattle, arch, rng)

    out = new_buf(dur)
    mix_into(out, rattle, 0, 0.9)

    # Heavy body collapse thud near the end
    thud_off = int(dur * 0.62 * SAMPLE_RATE)
    thud_dur = dur * 0.35
    thud_f = 95.0 if arch != "goliath" else 58.0
    thud = osc_sweep(thud_dur, thud_f, 28.0, kind="sine", amp=0.85)
    thud = add_buf(thud, one_pole_lp(noise_brown(thud_dur, 0.7, rng=rng), 280.0), 0.6)
    thud = apply_exp_decay(thud, tau=0.09 if arch != "goliath" else 0.16, attack=0.004)
    mix_into(out, thud, thud_off, 0.75)

    out = room_echo(out, room_size=0.55, decay=0.32, wet=0.22)
    return finalize_clip(out)


def synth_goliath_step(variant: int) -> array.array:
    rng = random.Random(hash(("zmb_goliath_step", variant)) & 0xFFFFFFFF)
    dur = 0.68 + rng.uniform(-0.04, 0.05)

    # Massive sub-bass impact + concrete dust rattle
    sub = osc_sweep(dur, 115.0 + rng.uniform(-10, 12), 26.0, kind="sine", amp=1.0)
    sub = apply_exp_decay(sub, tau=0.14, attack=0.003)

    punch = osc_fm(0.18, 92.0, 48.0, 3.2, amp=0.85, mod_decay=16.0, carrier_end_freq=34.0)
    punch = apply_exp_decay(punch, tau=0.05, attack=0.002)

    grit = noise_brown(dur, amp=0.75, rng=rng)
    grit = one_pole_lp(grit, 420.0)
    grit = apply_exp_decay(grit, tau=0.12, attack=0.004)

    crumble = biquad_bp(noise_velvet(dur * 0.7, density=1400.0, amp=0.6, rng=rng), 1150.0, q=2.2)
    crumble = apply_exp_decay(crumble, tau=0.11, attack=0.015)

    out = add_buf(sub, punch, 0.85)
    out = add_buf(out, grit, 0.70)
    mix_into(out, crumble, int(0.03 * SAMPLE_RATE), 0.45)
    out = soft_clip(out, drive=2.2, mix=0.85)
    out = room_echo(out, room_size=0.75, decay=0.38, wet=0.28)
    return finalize_clip(out)


def synth_goliath_enrage(variant: int) -> array.array:
    rng = random.Random(hash(("zmb_goliath_enrage", variant)) & 0xFFFFFFFF)
    cfg = ARCHETYPES["goliath"]
    dur = 2.15 + rng.uniform(-0.08, 0.12)
    f0 = cfg["f0"] * rng.uniform(0.95, 1.08)

    # Dual dissonant monster vocal cords sweeping upward then locking into massive roar
    v1 = osc_vocal_cord(
        dur,
        f0 * 0.85,
        f0 * 1.48,
        roughness=0.55,
        subharmonic=0.85,
        vibrato_hz=5.8,
        vibrato_depth=0.06,
        rng=rng,
    )
    v2 = osc_vocal_cord(
        dur,
        f0 * 1.22,
        f0 * 1.85,
        roughness=0.48,
        subharmonic=0.65,
        vibrato_hz=7.1,
        vibrato_depth=0.05,
        rng=rng,
    )
    roar_noise = noise_pink(dur, amp=0.75, rng=rng)
    raw = add_buf(v1, v2, 0.75)
    raw = add_buf(raw, roar_noise, 0.55)
    shaped = formant_filter(raw, [195.0, 540.0, 1280.0, 2350.0], [3.8, 5.0, 6.5, 8.0])
    shaped = apply_adsr(shaped, 0.08, 0.25, 0.82, 0.35)

    # Two chest-pound sub impacts during enrage
    for p_time in (0.12, 0.52):
        pound = osc_sweep(0.28, 110.0, 30.0, kind="sine", amp=0.9)
        pound = apply_exp_decay(pound, tau=0.08, attack=0.002)
        mix_into(shaped, pound, int(p_time * SAMPLE_RATE), 0.75)

    shaped = _apply_archetype_color(shaped, "goliath", rng)
    shaped = room_echo(shaped, room_size=1.05, decay=0.52, wet=0.38)
    return finalize_clip(shaped)


def synth_zombie_spit(variant: int = 1) -> array.array:
    rng = random.Random(hash(("zmb_spit", variant)) & 0xFFFFFFFF)
    dur = 0.62

    # Pressurized throat hawk + wet projectile launch sweep
    vox = osc_vocal_cord(dur * 0.55, 165.0, 240.0, roughness=0.45, subharmonic=0.3, rng=rng)
    vox = formant_filter(vox, [480.0, 1380.0, 2550.0], [6.0, 8.0, 9.5])
    vox = apply_am(vox, 19.0, depth=0.7)
    vox = apply_adsr(vox, 0.02, 0.08, 0.7, 0.08)

    whoosh = swept_bp(noise_white(dur * 0.65, 1.0, rng=rng), 520.0, 3200.0, q=4.5)
    whoosh = apply_adsr(whoosh, 0.015, 0.09, 0.5, 0.14)

    pop = osc_sweep(0.09, 680.0, 190.0, kind="sine", amp=0.85)
    pop = apply_exp_decay(pop, tau=0.025, attack=0.001)

    out = new_buf(dur)
    mix_into(out, vox, 0, 0.85)
    mix_into(out, pop, int(0.18 * SAMPLE_RATE), 0.90)
    mix_into(out, whoosh, int(0.18 * SAMPLE_RATE), 0.95)
    out = soft_clip(out, drive=1.8, mix=0.8)
    return finalize_clip(out)


def synth_zombie_spit_hit(variant: int = 1) -> array.array:
    rng = random.Random(hash(("zmb_spit_hit", variant)) & 0xFFFFFFFF)
    dur = 0.78
    out = new_buf(dur)

    # Wet splatter slap
    slap = swept_bp(noise_pink(0.16, 1.0, rng=rng), 1800.0, 380.0, q=3.0)
    slap = apply_exp_decay(slap, tau=0.045, attack=0.001)
    mix_into(out, slap, 0, 1.0)

    # Liquid bubble pops
    for b in range(7):
        b_off = int((0.03 + b * 0.075 + rng.uniform(0.0, 0.03)) * SAMPLE_RATE)
        f_b = 450.0 + rng.uniform(0.0, 950.0)
        bub = osc_sweep(0.055, f_b * 0.7, f_b * 1.45, kind="sine", amp=0.6)
        bub = apply_adsr(bub, 0.004, 0.02, 0.4, 0.02)
        mix_into(out, bub, b_off, 0.55)

    # Chemical acid sizzle hiss
    hiss_src = add_buf(
        noise_white(dur * 0.9, 0.7, rng=rng),
        noise_velvet(dur * 0.9, density=3200.0, amp=0.9, rng=rng),
        0.8,
    )
    hiss = biquad_bp(hiss_src, 4200.0, q=1.6)
    hiss = apply_exp_decay(hiss, tau=0.24, attack=0.015)
    mix_into(out, hiss, int(0.03 * SAMPLE_RATE), 0.85)
    return finalize_clip(out)


def get_zombie_sound_specs() -> list[tuple[str, str, Callable[[], array.array]]]:
    """Return list of (category, clip_stem, generator_fn) for all 90 zombie audio files."""
    specs: list[tuple[str, str, Callable[[], array.array]]] = []

    action_map = {
        "breath": synth_zombie_breath,
        "scream_far": synth_zombie_scream_far,
        "scratch": synth_zombie_scratch,
        "attack": synth_zombie_attack,
        "bone": synth_zombie_bone,
        "death": synth_zombie_death,
    }

    # 4 archetypes x 6 actions x 3 variants = 72 files
    for arch in ("walker", "runner", "spitter", "armored"):
        for act_name, fn in action_map.items():
            for v in (1, 2, 3):
                stem = f"zmb_{arch}_{act_name}_{v:02d}"
                specs.append(("Zombies", stem, lambda a=arch, v_i=v, f=fn: f(a, v_i)))

    # Goliath: 8 bases x 2 variants = 16 files
    goliath_map = {
        "breath": lambda v: synth_zombie_breath("goliath", v),
        "roar_far": lambda v: synth_zombie_scream_far("goliath", v),
        "scratch": lambda v: synth_zombie_scratch("goliath", v),
        "attack": lambda v: synth_zombie_attack("goliath", v),
        "bone": lambda v: synth_zombie_bone("goliath", v),
        "death": lambda v: synth_zombie_death("goliath", v),
        "step": synth_goliath_step,
        "enrage": synth_goliath_enrage,
    }
    for act_name, g_fn in goliath_map.items():
        for v in (1, 2):
            stem = f"zmb_goliath_{act_name}_{v:02d}"
            specs.append(("Zombies", stem, lambda v_i=v, f=g_fn: f(v_i)))

    # Acid spit projectile & impact (2 files) -> 72 + 16 + 2 = 90 files
    specs.append(("Zombies", "zmb_spit_01", lambda: synth_zombie_spit(1)))
    specs.append(("Zombies", "zmb_spit_hit_01", lambda: synth_zombie_spit_hit(1)))
    return specs
