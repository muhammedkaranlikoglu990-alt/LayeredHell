"""Procedural synthesis for all Foley, Footsteps, Pickups & Props SFX in 45: Layered Hell.

Generates 114 distinct 44.1 kHz 16-bit mono WAV buffers in category 'Foley':
- 6 surfaces (concrete, parquet, tile, water, glass, metalstair) x 3 gaits (walk, run, crouch)
  x 3 variants (_01.._03) = 54 files
- 20 item pickups (_01) = 20 files
- 28 interaction/environment/player foley bases (12 with _01.._02, 16 with _01) = 40 files
  (including seamless loop `generator_loop_01`)
"""

from __future__ import annotations

import array
import math
import random
from typing import Callable

from synth_core import (
    SAMPLE_RATE,
    TWO_PI,
    add_buf,
    apply_adsr,
    apply_am,
    apply_exp_decay,
    bitcrush,
    biquad_bp,
    biquad_filter,
    finalize_clip,
    formant_filter,
    mix_into,
    new_buf,
    noise_brown,
    noise_pink,
    noise_velvet,
    noise_white,
    one_pole_hp,
    one_pole_lp,
    osc_fm,
    osc_pulse,
    osc_saw,
    osc_sine,
    osc_sweep,
    osc_triangle,
    osc_vocal_cord,
    room_echo,
    soft_clip,
    swept_bp,
)


def synth_footstep(surface: str, gait: str, variant: int) -> array.array:
    rng = random.Random(hash(("step", surface, gait, variant)) & 0xFFFFFFFF)
    gait_cfg = {
        "walk": (0.28, 1.0, 0.048, 0.055),
        "run": (0.22, 1.25, 0.032, 0.038),
        "crouch": (0.34, 0.65, 0.068, 0.075),
    }[gait]
    dur, energy, toe_delay, tau_scale = gait_cfg
    out = new_buf(dur)

    # Heel + toe strike timings
    t_heel = 0
    t_toe = int((toe_delay + rng.uniform(-0.005, 0.007)) * SAMPLE_RATE)

    if surface == "concrete":
        heel = osc_sweep(0.09, 135.0 * energy, 42.0, kind="sine", amp=0.85)
        heel = apply_exp_decay(heel, tau=tau_scale * 0.55, attack=0.001)
        grit = biquad_bp(noise_pink(0.11, 0.85, rng=rng), 1250.0 + variant * 120.0, q=1.8)
        grit = apply_exp_decay(grit, tau=tau_scale * 0.65, attack=0.002)
        mix_into(out, heel, t_heel, 0.95)
        mix_into(out, grit, t_toe, 0.75)

    elif surface == "parquet":
        # Hollow wooden board thump + slight floorboard creak
        thump = osc_fm(0.12, 195.0, 290.0, 2.2, amp=0.9, mod_decay=22.0, carrier_end_freq=85.0)
        thump = apply_exp_decay(thump, tau=tau_scale * 0.75, attack=0.001)
        wood_bp = biquad_bp(noise_pink(0.10, 0.75, rng=rng), 680.0 + variant * 70.0, q=3.2)
        wood_bp = apply_exp_decay(wood_bp, tau=tau_scale * 0.6, attack=0.002)
        mix_into(out, thump, t_heel, 0.95)
        mix_into(out, wood_bp, t_toe, 0.80)

    elif surface == "tile":
        # Sharp ceramic heel click + high reflection
        click = osc_fm(0.055, 2200.0 + variant * 150.0, 3150.0, 3.0, amp=0.85, mod_decay=35.0)
        click = apply_exp_decay(click, tau=0.014, attack=0.0005)
        sole = biquad_bp(noise_white(0.075, 0.8, rng=rng), 2850.0, q=2.5)
        sole = apply_exp_decay(sole, tau=0.018, attack=0.001)
        sub = osc_sweep(0.06, 160.0, 60.0, kind="sine", amp=0.6)
        sub = apply_exp_decay(sub, tau=0.022, attack=0.001)
        mix_into(out, sub, t_heel, 0.7)
        mix_into(out, click, t_heel, 0.95)
        mix_into(out, sole, t_toe, 0.80)

    elif surface == "water":
        # Splash slosh + bubble modulation
        slosh = swept_bp(noise_pink(dur * 0.85, 0.95, rng=rng), 380.0, 2200.0 * energy, q=2.4)
        slosh = apply_adsr(slosh, 0.012, 0.06, 0.55, 0.09)
        mix_into(out, slosh, t_heel, 0.95)
        for b in range(4):
            f_b = 550.0 + rng.uniform(0.0, 950.0)
            bub = osc_sweep(0.04, f_b * 0.75, f_b * 1.35, kind="sine", amp=0.5)
            bub = apply_adsr(bub, 0.003, 0.015, 0.4, 0.015)
            mix_into(out, bub, int((0.02 + b * 0.045) * SAMPLE_RATE), 0.55)

    elif surface == "glass":
        # Crunchy glass shards underfoot
        crunch_src = add_buf(
            noise_white(dur * 0.8, 0.75, rng=rng),
            noise_velvet(dur * 0.8, density=2800.0, amp=1.0, rng=rng),
            0.9,
        )
        crunch = biquad_bp(crunch_src, 3600.0 + variant * 250.0, q=2.2)
        crunch = apply_adsr(crunch, 0.006, 0.05, 0.5, 0.08)
        mix_into(out, crunch, t_heel, 0.95)
        for s in range(3):
            f_s = 3900.0 + rng.uniform(0.0, 2600.0)
            tink = osc_sine(0.05, f_s, amp=0.45)
            tink = apply_exp_decay(tink, tau=0.014, attack=0.0005)
            mix_into(out, tink, int((0.015 + s * 0.048) * SAMPLE_RATE), 0.65)

    else:  # metalstair
        # Resonant industrial grate clank
        clank = osc_fm(dur * 0.9, 540.0 + variant * 35.0, 825.0, 4.5, amp=0.85, mod_decay=12.0)
        clank2 = osc_fm(dur * 0.9, 1180.0, 1710.0, 3.2, amp=0.55, mod_decay=16.0)
        metal = apply_exp_decay(add_buf(clank, clank2, 0.65), tau=tau_scale * 1.15, attack=0.0008)
        thud = osc_sweep(0.08, 150.0, 52.0, kind="sine", amp=0.75)
        thud = apply_exp_decay(thud, tau=0.025, attack=0.001)
        mix_into(out, thud, t_heel, 0.8)
        mix_into(out, metal, t_heel, 0.95)

    return finalize_clip(out)


def synth_pickup(kind: str) -> array.array:
    rng = random.Random(hash(("pickup", kind)) & 0xFFFFFFFF)
    dur = 0.36
    out = new_buf(dur)

    if kind in ("can", "metal", "tool"):
        f0 = {"can": 1450.0, "metal": 980.0, "tool": 1220.0}[kind]
        c1 = osc_fm(0.18, f0, f0 * 1.47, 3.6, amp=0.8, mod_decay=18.0)
        c1 = apply_exp_decay(c1, tau=0.038, attack=0.0008)
        c2 = osc_fm(0.16, f0 * 1.24, f0 * 1.81, 3.0, amp=0.65, mod_decay=22.0)
        c2 = apply_exp_decay(c2, tau=0.032, attack=0.0008)
        mix_into(out, c1, int(0.01 * SAMPLE_RATE), 0.9)
        mix_into(out, c2, int(0.09 * SAMPLE_RATE), 0.75)

    elif kind in ("bottle", "glass", "chem"):
        f0 = {"bottle": 2150.0, "glass": 2950.0, "chem": 1850.0}[kind]
        ping = osc_fm(0.22, f0, f0 * 1.52, 2.2, amp=0.85, mod_decay=20.0)
        ping = apply_exp_decay(ping, tau=0.045, attack=0.0006)
        mix_into(out, ping, int(0.01 * SAMPLE_RATE), 0.9)
        if kind == "chem":
            slosh = swept_bp(noise_pink(0.22, 0.8, rng=rng), 480.0, 1350.0, q=3.5)
            slosh = apply_adsr(slosh, 0.02, 0.06, 0.5, 0.08)
            mix_into(out, slosh, int(0.05 * SAMPLE_RATE), 0.75)

    elif kind in ("paper", "cloth", "book", "med", "food"):
        f_bp = {"paper": 2400.0, "cloth": 1100.0, "book": 850.0, "med": 1650.0, "food": 1950.0}[kind]
        rustle = swept_bp(noise_pink(dur * 0.85, 0.95, rng=rng), f_bp * 0.7, f_bp * 1.35, q=2.0)
        rustle = apply_adsr(rustle, 0.018, 0.07, 0.65, 0.09)
        mix_into(out, rustle, 0, 0.95)
        if kind == "book":
            thump = osc_sweep(0.09, 150.0, 55.0, kind="sine", amp=0.8)
            thump = apply_exp_decay(thump, tau=0.025, attack=0.001)
            mix_into(out, thump, int(0.08 * SAMPLE_RATE), 0.85)

    elif kind in ("ammo", "weapon", "key", "coin"):
        f0 = {"ammo": 2650.0, "weapon": 1350.0, "key": 3850.0, "coin": 4650.0}[kind]
        count = 4 if kind in ("key", "coin") else 2
        for i in range(count):
            f = f0 * (1.0 + 0.12 * i + rng.uniform(-0.03, 0.03))
            clink = osc_fm(0.14, f, f * 1.38, 2.8, amp=0.75, mod_decay=24.0)
            clink = apply_exp_decay(clink, tau=0.028, attack=0.0005)
            mix_into(out, clink, int((0.01 + i * 0.058) * SAMPLE_RATE), 0.85 / (1.0 + 0.18 * i))

    else:  # battery, wood, plastic, electronics, card
        f0 = {
            "battery": 1550.0,
            "wood": 520.0,
            "plastic": 1750.0,
            "electronics": 2350.0,
            "card": 3100.0,
        }[kind]
        pop = osc_fm(0.12, f0, f0 * 1.33, 2.5, amp=0.85, mod_decay=28.0, carrier_end_freq=f0 * 0.7)
        pop = apply_exp_decay(pop, tau=0.022, attack=0.0008)
        scrape = biquad_bp(noise_white(0.14, 0.75, rng=rng), f0 * 1.2, q=2.6)
        scrape = apply_adsr(scrape, 0.01, 0.04, 0.5, 0.05)
        mix_into(out, pop, int(0.015 * SAMPLE_RATE), 0.9)
        mix_into(out, scrape, int(0.04 * SAMPLE_RATE), 0.65)
        if kind == "electronics":
            chirp = osc_sweep(0.06, 1800.0, 3200.0, kind="sine", amp=0.4)
            chirp = apply_exp_decay(chirp, tau=0.02, attack=0.001)
            mix_into(out, chirp, int(0.14 * SAMPLE_RATE), 0.55)

    return finalize_clip(out)


def synth_foley_clip(base: str, variant: int = 1) -> array.array:
    rng = random.Random(hash(("foley", base, variant)) & 0xFFFFFFFF)

    if base == "door_creak":
        dur = 0.95 + variant * 0.12
        # Stick-slip hinge friction via slow FM + resonant wood formants
        f0 = 260.0 + variant * 35.0
        stick = osc_sweep(dur, f0, f0 * 1.45, kind="saw", amp=0.85, vibrato_hz=11.0, vibrato_depth=0.08)
        stick = apply_am(stick, 18.0 + variant * 4.0, depth=0.65)
        wood = formant_filter(stick, [480.0, 1120.0, 2450.0], [5.5, 7.0, 9.0])
        wood = apply_adsr(wood, 0.06, 0.2, 0.75, 0.25)
        return finalize_clip(wood)

    if base == "door_open":
        dur = 0.65
        out = new_buf(dur)
        latch = osc_fm(0.06, 1450.0, 2100.0, 3.5, amp=0.85, mod_decay=28.0)
        latch = apply_exp_decay(latch, tau=0.014, attack=0.001)
        swing = swept_bp(noise_pink(0.48, 0.75, rng=rng), 280.0, 720.0, q=2.5)
        swing = apply_adsr(swing, 0.04, 0.12, 0.6, 0.18)
        mix_into(out, latch, int(0.01 * SAMPLE_RATE), 0.9)
        mix_into(out, swing, int(0.08 * SAMPLE_RATE), 0.8)
        return finalize_clip(out)

    if base == "door_close":
        dur = 0.58
        out = new_buf(dur)
        thud = osc_sweep(0.22, 130.0, 36.0, kind="sine", amp=1.0)
        thud = apply_exp_decay(thud, tau=0.055, attack=0.001)
        click = osc_fm(0.05, 1620.0, 2380.0, 3.8, amp=0.75, mod_decay=30.0)
        click = apply_exp_decay(click, tau=0.012, attack=0.0008)
        mix_into(out, thud, int(0.05 * SAMPLE_RATE), 1.0)
        mix_into(out, click, int(0.065 * SAMPLE_RATE), 0.75)
        out = room_echo(out, room_size=0.6, decay=0.32, wet=0.25)
        return finalize_clip(out)

    if base == "door_locked":
        dur = 0.42
        out = new_buf(dur)
        for i, off_s in enumerate((0.02, 0.17)):
            rattle = osc_fm(0.09, 980.0 + i * 160.0, 1420.0, 4.2, amp=0.85, mod_decay=24.0)
            rattle = apply_exp_decay(rattle, tau=0.022, attack=0.0008)
            thump = osc_sweep(0.07, 150.0, 58.0, kind="sine", amp=0.65)
            thump = apply_exp_decay(thump, tau=0.02, attack=0.001)
            mix_into(out, rattle, int(off_s * SAMPLE_RATE), 0.9)
            mix_into(out, thump, int(off_s * SAMPLE_RATE), 0.7)
        return finalize_clip(out)

    if base == "lockpick":
        dur = 0.55
        out = new_buf(dur)
        for i, t_s in enumerate((0.03, 0.16, 0.31, 0.42)):
            f = 2400.0 + i * 320.0 + variant * 140.0
            pin = osc_fm(0.06, f, f * 1.43, 3.2, amp=0.8, mod_decay=32.0)
            pin = apply_exp_decay(pin, tau=0.012, attack=0.0006)
            mix_into(out, pin, int(t_s * SAMPLE_RATE), 0.85)
        return finalize_clip(out)

    if base == "lock_break":
        dur = 0.38
        snap = osc_fm(0.18, 2850.0, 4120.0, 5.5, amp=0.95, mod_decay=18.0, carrier_end_freq=920.0)
        snap = apply_exp_decay(snap, tau=0.035, attack=0.0005)
        burst = biquad_bp(noise_white(0.12, 0.9, rng=rng), 3600.0, q=2.2)
        burst = apply_exp_decay(burst, tau=0.022, attack=0.0005)
        return finalize_clip(add_buf(snap, burst, 0.7))

    if base == "generator_start":
        dur = 1.85
        # Starter crank + diesel ignition spin-up
        motor = osc_sweep(dur, 24.0, 82.0, kind="saw", exp_curve=False, amp=0.75)
        motor = one_pole_lp(motor, 480.0)
        chug = apply_am(motor, 11.0, depth=0.75)
        noise = one_pole_lp(noise_brown(dur, 0.7, rng=rng), 620.0)
        raw = apply_adsr(add_buf(chug, noise, 0.65), 0.08, 0.25, 0.85, 0.15)
        return finalize_clip(soft_clip(raw, drive=2.2, mix=0.85))

    if base == "generator_loop":
        # Seamless diesel generator loop (~3.5 s active after crossfade)
        raw_dur = 4.0
        f0 = 55.0
        drone = add_buf(
            osc_saw(raw_dur, f0, amp=0.65),
            osc_pulse(raw_dur, f0 * 0.5, duty=0.35, amp=0.55),
            0.8,
        )
        drone = one_pole_lp(drone, 380.0)
        drone = apply_am(drone, 13.75, depth=0.48)
        rumble = one_pole_lp(noise_brown(raw_dur, 0.65, rng=rng), 420.0)
        valves = biquad_bp(noise_pink(raw_dur, 0.45, rng=rng), 1250.0, q=2.8)
        valves = apply_am(valves, 27.5, depth=0.65)
        loop_buf = add_buf(add_buf(drone, rumble, 0.7), valves, 0.4)
        loop_buf = soft_clip(loop_buf, drive=1.9, mix=0.8)
        return finalize_clip(loop_buf, is_loop=True, crossfade_sec=0.45)

    if base == "generator_stop":
        dur = 1.45
        motor = osc_sweep(dur, 78.0, 14.0, kind="saw", exp_curve=True, amp=0.8)
        motor = one_pole_lp(motor, 380.0)
        motor = apply_am(motor, 7.5, depth=0.65)
        raw = apply_exp_decay(motor, tau=0.45, attack=0.01)
        return finalize_clip(raw)

    if base == "radio_static":
        dur = 0.85 + variant * 0.15
        src = add_buf(
            noise_white(dur, 0.75, rng=rng),
            noise_velvet(dur, density=2400.0, amp=0.95, rng=rng),
            0.85,
        )
        band = biquad_bp(src, 1650.0 + variant * 180.0, q=1.2)
        band = apply_am(band, 16.0 + variant * 5.0, depth=0.45)
        band = apply_adsr(band, 0.02, 0.1, 0.8, 0.08)
        return finalize_clip(band)

    if base == "radio_tune":
        dur = 0.75
        whistle = osc_sweep(dur, 620.0, 2850.0, kind="sine", amp=0.55, vibrato_hz=9.0, vibrato_depth=0.06)
        hiss = biquad_bp(noise_white(dur, 0.7, rng=rng), 1850.0, q=1.5)
        raw = apply_adsr(add_buf(whistle, hiss, 0.8), 0.02, 0.12, 0.75, 0.08)
        return finalize_clip(raw)

    if base == "radio_squelch":
        dur = 0.18
        burst = biquad_bp(noise_white(dur, 1.0, rng=rng), 2100.0, q=1.6)
        click = osc_sine(0.03, 1350.0, amp=0.8)
        raw = add_buf(apply_exp_decay(burst, tau=0.04, attack=0.001), click, 0.7)
        return finalize_clip(raw)

    if base == "radio_beep":
        dur = 0.22
        tone = osc_sine(0.14, 1046.5, amp=0.85)
        tone = apply_adsr(tone, 0.004, 0.02, 0.85, 0.02)
        out = new_buf(dur)
        mix_into(out, tone, int(0.02 * SAMPLE_RATE), 1.0)
        return finalize_clip(out)

    if base == "radio_voice":
        dur = 1.15 + variant * 0.12
        # Band-limited speech cadence bursts through walkie-talkie filter
        vox = osc_vocal_cord(
            dur,
            135.0 + variant * 18.0,
            118.0 + variant * 12.0,
            roughness=0.15,
            subharmonic=0.1,
            vibrato_hz=6.2,
            vibrato_depth=0.04,
            rng=rng,
        )
        speech = formant_filter(vox, [650.0, 1450.0, 2450.0], [5.0, 7.0, 9.0])
        speech = apply_am(speech, 5.5 + variant * 0.8, depth=0.82)
        speech = bitcrush(speech, steps=20, hold=2)
        hiss = noise_white(dur, 0.22, rng=rng)
        radio = biquad_bp(add_buf(speech, hiss, 0.35), 1450.0, q=1.1)
        radio = soft_clip(radio, drive=2.2, mix=0.85)
        radio = apply_adsr(radio, 0.015, 0.1, 0.85, 0.06)
        return finalize_clip(radio)

    if base == "hammer":
        dur = 0.42
        clank = osc_fm(0.18, 680.0 + variant * 65.0, 1050.0, 4.2, amp=0.9, mod_decay=20.0)
        clank = apply_exp_decay(clank, tau=0.038, attack=0.0006)
        wood = osc_sweep(0.14, 190.0, 62.0, kind="sine", amp=0.85)
        wood = apply_exp_decay(wood, tau=0.03, attack=0.001)
        return finalize_clip(room_echo(add_buf(clank, wood, 0.85), room_size=0.45, decay=0.25, wet=0.2))

    if base == "craft":
        dur = 0.78
        out = new_buf(dur)
        for i, off_s in enumerate((0.02, 0.22, 0.44)):
            f = 1150.0 + i * 280.0 + variant * 90.0
            tap = osc_fm(0.14, f, f * 1.42, 3.4, amp=0.8, mod_decay=24.0)
            tap = apply_exp_decay(tap, tau=0.028, attack=0.0008)
            mix_into(out, tap, int(off_s * SAMPLE_RATE), 0.85)
        ratchet = swept_bp(noise_white(0.24, 0.7, rng=rng), 1200.0, 2600.0, q=3.0)
        ratchet = apply_adsr(ratchet, 0.01, 0.06, 0.6, 0.06)
        mix_into(out, ratchet, int(0.48 * SAMPLE_RATE), 0.7)
        return finalize_clip(out)

    if base == "drip":
        dur = 0.28
        f0 = 1150.0 + variant * 180.0
        drop = osc_sweep(0.075, f0, f0 * 2.15, kind="sine", amp=0.9)
        drop = apply_adsr(drop, 0.003, 0.025, 0.35, 0.035)
        out = new_buf(dur)
        mix_into(out, drop, int(0.015 * SAMPLE_RATE), 1.0)
        out = room_echo(out, room_size=0.85, decay=0.48, wet=0.38)
        return finalize_clip(out)

    if base == "elevator_move":
        dur = 1.65
        hum = add_buf(osc_sine(dur, 110.0, amp=0.65), osc_triangle(dur, 220.0, amp=0.4), 0.7)
        cable = one_pole_lp(noise_brown(dur, 0.75, rng=rng), 480.0)
        cable = apply_am(cable, 6.5, depth=0.45)
        raw = apply_adsr(add_buf(hum, cable, 0.85), 0.12, 0.2, 0.85, 0.18)
        return finalize_clip(raw)

    if base == "elevator_ding":
        dur = 0.95
        bell = add_buf(
            osc_sine(dur, 1046.5, amp=0.85),
            osc_sine(dur, 2093.0, amp=0.35),
            0.5,
        )
        bell = apply_exp_decay(bell, tau=0.28, attack=0.002)
        return finalize_clip(room_echo(bell, room_size=0.65, decay=0.35, wet=0.25))

    if base == "elevator_door":
        dur = 1.15
        out = new_buf(dur)
        roll = swept_bp(noise_pink(0.85, 0.85, rng=rng), 320.0, 680.0, q=2.2)
        roll = apply_adsr(roll, 0.06, 0.18, 0.7, 0.2)
        clunk = osc_fm(0.16, 220.0, 340.0, 3.5, amp=0.9, mod_decay=20.0, carrier_end_freq=75.0)
        clunk = apply_exp_decay(clunk, tau=0.04, attack=0.001)
        mix_into(out, roll, int(0.04 * SAMPLE_RATE), 0.85)
        mix_into(out, clunk, int(0.86 * SAMPLE_RATE), 0.95)
        return finalize_clip(out)

    if base == "jump_land":
        dur = 0.45
        thud = osc_sweep(0.22, 135.0, 34.0, kind="sine", amp=0.95)
        thud = apply_exp_decay(thud, tau=0.055, attack=0.001)
        scuff = biquad_bp(noise_pink(0.18, 0.85, rng=rng), 950.0 + variant * 120.0, q=1.8)
        scuff = apply_exp_decay(scuff, tau=0.045, attack=0.003)
        out = new_buf(dur)
        mix_into(out, thud, 0, 1.0)
        mix_into(out, scuff, int(0.03 * SAMPLE_RATE), 0.75)
        return finalize_clip(out)

    if base == "cough":
        dur = 0.68
        out = new_buf(dur)
        for i, off_s in enumerate((0.02, 0.28)):
            c_dur = 0.22
            vox = osc_vocal_cord(c_dur, 155.0 - i * 18.0, 105.0, roughness=0.5, subharmonic=0.35, rng=rng)
            burst = noise_pink(c_dur, 0.85, rng=rng)
            bark = formant_filter(add_buf(vox, burst, 0.7), [550.0, 1450.0, 2600.0], [4.5, 6.0, 8.0])
            bark = apply_exp_decay(bark, tau=0.065, attack=0.005)
            mix_into(out, bark, int(off_s * SAMPLE_RATE), 0.95 / (1.0 + 0.25 * i))
        return finalize_clip(out)

    if base == "eat":
        dur = 0.65
        out = new_buf(dur)
        for i, off_s in enumerate((0.03, 0.22, 0.42)):
            crunch = biquad_bp(
                add_buf(noise_pink(0.14, 0.8, rng=rng), noise_velvet(0.14, 2200.0, 0.9, rng=rng), 0.8),
                1350.0 + i * 220.0,
                q=2.2,
            )
            crunch = apply_exp_decay(crunch, tau=0.035, attack=0.002)
            mix_into(out, crunch, int(off_s * SAMPLE_RATE), 0.9)
        return finalize_clip(out)

    if base == "drink":
        dur = 0.72
        out = new_buf(dur)
        for i, off_s in enumerate((0.04, 0.34)):
            gulp = osc_sweep(0.18, 220.0, 520.0, kind="sine", amp=0.75)
            gulp = apply_adsr(gulp, 0.02, 0.05, 0.5, 0.06)
            slosh = swept_bp(noise_pink(0.18, 0.7, rng=rng), 450.0, 1250.0, q=3.0)
            slosh = apply_adsr(slosh, 0.02, 0.05, 0.5, 0.06)
            mix_into(out, add_buf(gulp, slosh, 0.7), int(off_s * SAMPLE_RATE), 0.9)
        return finalize_clip(out)

    if base == "bandage":
        dur = 0.78
        tear = swept_bp(noise_white(dur, 0.85, rng=rng), 950.0, 2650.0, q=2.4)
        tear = apply_am(tear, 4.5, depth=0.55)
        tear = apply_adsr(tear, 0.04, 0.15, 0.75, 0.18)
        return finalize_clip(tear)

    if base == "inject":
        dur = 0.52
        out = new_buf(dur)
        click = osc_fm(0.04, 2600.0, 3800.0, 3.0, amp=0.85, mod_decay=35.0)
        click = apply_exp_decay(click, tau=0.009, attack=0.0005)
        hiss = swept_bp(noise_white(0.32, 0.85, rng=rng), 3800.0, 1650.0, q=3.2)
        hiss = apply_adsr(hiss, 0.015, 0.08, 0.6, 0.12)
        mix_into(out, click, int(0.02 * SAMPLE_RATE), 0.95)
        mix_into(out, hiss, int(0.06 * SAMPLE_RATE), 0.85)
        return finalize_clip(out)

    if base == "flashlight_click":
        dur = 0.14
        out = new_buf(dur)
        c1 = osc_fm(0.032, 2950.0, 4200.0, 3.6, amp=0.95, mod_decay=40.0)
        c1 = apply_exp_decay(c1, tau=0.006, attack=0.0004)
        c2 = osc_fm(0.028, 3450.0, 4900.0, 3.2, amp=0.75, mod_decay=45.0)
        c2 = apply_exp_decay(c2, tau=0.005, attack=0.0004)
        mix_into(out, c1, int(0.008 * SAMPLE_RATE), 1.0)
        mix_into(out, c2, int(0.042 * SAMPLE_RATE), 0.8)
        return finalize_clip(out)

    # barricade_build
    dur = 1.05
    out = new_buf(dur)
    plank = osc_sweep(0.18, 210.0, 68.0, kind="sine", amp=0.85)
    plank = apply_exp_decay(plank, tau=0.045, attack=0.001)
    mix_into(out, plank, int(0.03 * SAMPLE_RATE), 0.9)
    for i, off_s in enumerate((0.24, 0.52, 0.78)):
        hit = osc_fm(0.14, 720.0 + i * 85.0, 1120.0, 4.0, amp=0.9, mod_decay=22.0)
        hit = apply_exp_decay(hit, tau=0.032, attack=0.0006)
        mix_into(out, hit, int(off_s * SAMPLE_RATE), 0.95)
    return finalize_clip(room_echo(out, room_size=0.5, decay=0.28, wet=0.2))


def get_foley_sound_specs() -> list[tuple[str, str, Callable[[], array.array]]]:
    """Return list of (category, clip_stem, generator_fn) for all 114 foley audio files."""
    specs: list[tuple[str, str, Callable[[], array.array]]] = []

    # 1. Footsteps: 6 surfaces x 3 gaits x 3 variants = 54 files
    surfaces = ("concrete", "parquet", "tile", "water", "glass", "metalstair")
    gaits = ("walk", "run", "crouch")
    for surf in surfaces:
        for gait in gaits:
            for v in (1, 2, 3):
                stem = f"step_{surf}_{gait}_{v:02d}"
                specs.append(("Foley", stem, lambda s=surf, g=gait, v_i=v: synth_footstep(s, g, v_i)))

    # 2. Pickups: 20 kinds x 1 variant = 20 files
    pickups = (
        "can",
        "bottle",
        "paper",
        "metal",
        "cloth",
        "med",
        "ammo",
        "weapon",
        "key",
        "battery",
        "wood",
        "plastic",
        "glass",
        "food",
        "tool",
        "chem",
        "electronics",
        "book",
        "coin",
        "card",
    )
    for p_kind in pickups:
        stem = f"pickup_{p_kind}_01"
        specs.append(("Foley", stem, lambda k=p_kind: synth_pickup(k)))

    # 3. 28 interaction/environment/player foley bases (12 with _01.._02, 16 with _01 = 40 files)
    two_variant_bases = (
        "door_creak",
        "door_open",
        "door_close",
        "door_locked",
        "lockpick",
        "radio_static",
        "radio_voice",
        "hammer",
        "craft",
        "drip",
        "jump_land",
        "cough",
    )
    one_variant_bases = (
        "lock_break",
        "generator_start",
        "generator_loop",
        "generator_stop",
        "radio_tune",
        "radio_squelch",
        "radio_beep",
        "elevator_move",
        "elevator_ding",
        "elevator_door",
        "eat",
        "drink",
        "bandage",
        "inject",
        "flashlight_click",
        "barricade_build",
    )

    for b in two_variant_bases:
        for v in (1, 2):
            stem = f"{b}_{v:02d}"
            specs.append(("Foley", stem, lambda base=b, v_i=v: synth_foley_clip(base, v_i)))

    for b in one_variant_bases:
        stem = f"{b}_01"
        specs.append(("Foley", stem, lambda base=b: synth_foley_clip(base, 1)))

    return specs
