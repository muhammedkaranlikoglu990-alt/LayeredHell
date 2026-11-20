"""Procedural synthesis for all World Ambience Loops, Music Loops, Weather & UI SFX in 45: Layered Hell.

Generates 78 distinct 44.1 kHz 16-bit mono WAV buffers in category 'World':
- 12 seamless ambience zone loops (6.0 s active loop duration after raised-cosine crossfade):
  amb_{basement,lab,parking,lobby,mall,office,residential,medical,gym,executive,roof,base} (24 files)
- 7 seamless music stem loops (8.0 s active loop duration after raised-cosine crossfade):
  mus_{explore,combat,boss,safehouse,ending_helicopter,ending_escape,tension} (12 files)
- Stingers, vitals & weather: mus_stinger_01..03, heartbeat_01..02, tinnitus_01,
  wind_01..03, rain_01..03, thunder_01..03, alarm_01..03 (18 files)
- 15 UI bases: ui_{click,back,open,close,error,confirm,craft,quest,journal,toast,upgrade,save,equip,denied,tab}
  (24 files)
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
    osc_pulse,
    osc_saw,
    osc_sine,
    osc_sweep,
    osc_triangle,
    room_echo,
    soft_clip,
    swept_bp,
)

AMB_PROFILES: dict[str, dict[str, float]] = {
    "basement": {"drone_f": 44.0, "mod_f": 66.0, "noise_lp": 320.0, "am_hz": 0.45, "accent_f": 980.0},
    "lab": {"drone_f": 60.0, "mod_f": 180.0, "noise_lp": 720.0, "am_hz": 1.20, "accent_f": 2400.0},
    "parking": {"drone_f": 49.0, "mod_f": 73.5, "noise_lp": 380.0, "am_hz": 0.35, "accent_f": 620.0},
    "lobby": {"drone_f": 55.0, "mod_f": 82.5, "noise_lp": 440.0, "am_hz": 0.40, "accent_f": 840.0},
    "mall": {"drone_f": 65.4, "mod_f": 98.1, "noise_lp": 650.0, "am_hz": 0.55, "accent_f": 1450.0},
    "office": {"drone_f": 60.0, "mod_f": 120.0, "noise_lp": 580.0, "am_hz": 0.75, "accent_f": 1850.0},
    "residential": {"drone_f": 51.9, "mod_f": 77.8, "noise_lp": 460.0, "am_hz": 0.38, "accent_f": 760.0},
    "medical": {"drone_f": 69.3, "mod_f": 207.9, "noise_lp": 690.0, "am_hz": 0.95, "accent_f": 2150.0},
    "gym": {"drone_f": 58.3, "mod_f": 87.4, "noise_lp": 520.0, "am_hz": 0.50, "accent_f": 1120.0},
    "executive": {"drone_f": 46.2, "mod_f": 92.5, "noise_lp": 490.0, "am_hz": 0.32, "accent_f": 1320.0},
    "roof": {"drone_f": 73.4, "mod_f": 110.0, "noise_lp": 1250.0, "am_hz": 0.65, "accent_f": 950.0},
    "base": {"drone_f": 65.4, "mod_f": 130.8, "noise_lp": 520.0, "am_hz": 0.28, "accent_f": 880.0},
}


def synth_ambience_loop(zone: str, variant: int = 1) -> array.array:
    rng = random.Random(hash(("amb", zone, variant)) & 0xFFFFFFFF)
    cfg = AMB_PROFILES[zone]
    # Generate 6.5 s so after 0.5 s tail-into-head crossfade the seamless loop is 6.0 s
    raw_dur = 6.5
    f0 = cfg["drone_f"] * (1.0 if variant == 1 else 1.05946)

    # 1. Sub-harmonic atmospheric drone pad
    d1 = osc_fm(raw_dur, f0, cfg["mod_f"], 1.35, amp=0.55)
    d2 = osc_triangle(raw_dur, f0 * 1.5, amp=0.35)
    drone = one_pole_lp(add_buf(d1, d2, 0.7), cfg["noise_lp"] * 1.3)
    drone = apply_am(drone, cfg["am_hz"], depth=0.38)

    # 2. Filtered room air / ventilation / wind texture
    air_src = add_buf(
        noise_brown(raw_dur, 0.65, rng=rng),
        noise_pink(raw_dur, 0.45, rng=rng),
        0.65,
    )
    air = one_pole_lp(air_src, cfg["noise_lp"])
    air = one_pole_hp(air, 35.0)
    air = apply_am(air, cfg["am_hz"] * 1.4, depth=0.32)

    out = add_buf(drone, air, 0.85)

    # 3. Zone-specific intermittent micro-textures placed safely away from loop seam
    for idx, t_pos in enumerate((1.15, 2.85, 4.45)):
        t_jit = t_pos + rng.uniform(-0.18, 0.18)
        off = int(t_jit * SAMPLE_RATE)
        f_acc = cfg["accent_f"] * (1.0 + 0.09 * idx)
        if zone in ("basement", "parking", "residential"):
            # Distant pipe creak or water drip
            drop = osc_sweep(0.11, f_acc * 0.8, f_acc * 1.65, kind="sine", amp=0.25)
            drop = apply_exp_decay(drop, tau=0.032, attack=0.002)
            mix_into(out, drop, off, 0.28)
        elif zone in ("lab", "medical", "office"):
            # Subtle fluorescent ballast hum / telemetry pulse
            pulse = osc_sine(0.24, f_acc, amp=0.18)
            pulse = apply_adsr(pulse, 0.02, 0.06, 0.5, 0.08)
            mix_into(out, pulse, off, 0.22)
        elif zone == "roof":
            # High altitude wind howl swell
            gust = swept_bp(noise_pink(0.95, 0.6, rng=rng), 420.0, 980.0, q=3.2)
            gust = apply_adsr(gust, 0.2, 0.25, 0.6, 0.3)
            mix_into(out, gust, off, 0.45)
        else:
            # Structural metallic resonance sigh
            sigh = osc_fm(0.55, f_acc * 0.5, f_acc * 0.75, 1.8, amp=0.22, mod_decay=6.0)
            sigh = apply_adsr(sigh, 0.08, 0.15, 0.5, 0.2)
            mix_into(out, sigh, off, 0.24)

    out = room_echo(out, room_size=0.95, decay=0.45, wet=0.32)
    return finalize_clip(out, is_loop=True, crossfade_sec=0.50)


def synth_music_loop(theme: str, variant: int = 1) -> array.array:
    rng = random.Random(hash(("mus", theme, variant)) & 0xFFFFFFFF)
    # 8.5 s raw buffer -> 8.0 s seamless loop after 0.5 s crossfade (4 bars at 120 BPM)
    raw_dur = 8.5
    out = new_buf(raw_dur)

    if theme == "explore":
        # Dark minor ambient pulse (D minor: D2, A2, F2, E2)
        notes = (73.42, 110.0, 87.31, 82.41) if variant == 1 else (73.42, 98.0, 116.54, 82.41)
        for idx, f_n in enumerate(notes):
            seg_dur = 2.25
            pad = add_buf(
                osc_saw(seg_dur, f_n, amp=0.45),
                osc_triangle(seg_dur, f_n * 2.0, amp=0.35),
                0.75,
            )
            pad = one_pole_lp(pad, 620.0 + idx * 90.0)
            pad = apply_adsr(pad, 0.35, 0.45, 0.65, 0.55)
            mix_into(out, pad, int(idx * 2.0 * SAMPLE_RATE), 0.85)
        # Subtle high glass bell arpeggio
        arp_freqs = (587.33, 698.46, 880.0, 659.25, 587.33, 523.25, 440.0, 523.25)
        for i, af in enumerate(arp_freqs):
            bell = osc_fm(0.65, af, af * 2.0, 1.2, amp=0.22, mod_decay=5.0)
            bell = apply_exp_decay(bell, tau=0.22, attack=0.01)
            mix_into(out, bell, int(i * 1.0 * SAMPLE_RATE), 0.40)

    elif theme in ("combat", "boss"):
        is_boss = theme == "boss"
        step_sec = 0.25  # 16th/8th industrial pulse at 120 BPM
        n_steps = int(raw_dur / step_sec)
        bass_pattern = (55.0, 55.0, 65.41, 55.0, 73.42, 55.0, 61.74, 58.27)
        for s in range(n_steps):
            t_off = int(s * step_sec * SAMPLE_RATE)
            f_b = bass_pattern[(s + variant) % len(bass_pattern)] * (0.75 if is_boss else 1.0)
            # Aggressive synth bass pulse
            b_note = osc_saw(step_sec * 0.9, f_b, amp=0.75)
            b_note = biquad_filter(b_note, "lp", 780.0 if not is_boss else 950.0, q=2.4)
            b_note = apply_exp_decay(b_note, tau=0.11, attack=0.004)
            mix_into(out, b_note, t_off, 0.85)

            # Industrial kick on even beats
            if (s % 2) == 0:
                kick = osc_sweep(0.18, 135.0 if not is_boss else 155.0, 32.0, kind="sine", amp=0.95)
                kick = apply_exp_decay(kick, tau=0.065, attack=0.001)
                mix_into(out, kick, t_off, 0.95)
            # Metallic snare anvil on backbeats
            if (s % 4) == 2:
                snare = add_buf(
                    biquad_bp(noise_white(0.16, 0.85, rng=rng), 1850.0, q=1.5),
                    osc_fm(0.14, 240.0, 385.0, 3.5, amp=0.7, mod_decay=18.0),
                    0.75,
                )
                snare = apply_exp_decay(snare, tau=0.055, attack=0.001)
                mix_into(out, snare, t_off, 0.85)
            # Boss brazen brass stabs every 4 steps
            if is_boss and (s % 4) == 0:
                braam = add_buf(
                    osc_saw(0.45, f_b * 2.0, amp=0.65),
                    osc_pulse(0.45, f_b * 3.01, duty=0.4, amp=0.5),
                    0.8,
                )
                braam = soft_clip(one_pole_lp(braam, 1450.0), drive=2.4, mix=0.85)
                braam = apply_adsr(braam, 0.02, 0.12, 0.65, 0.15)
                mix_into(out, braam, t_off, 0.70)
        out = soft_clip(out, drive=1.8, mix=0.8)

    elif theme == "safehouse":
        # Warm, melancholic safe-haven chords (A minor 9 -> F major 7)
        chords = (
            (110.0, 164.81, 196.0, 246.94, 329.63),
            (87.31, 130.81, 174.61, 220.0, 261.63),
        )
        for c_idx, chord in enumerate(chords):
            c_dur = 4.35
            c_buf = new_buf(c_dur)
            for f_c in chord:
                voice = add_buf(
                    osc_triangle(c_dur, f_c, amp=0.35),
                    osc_sine(c_dur, f_c * 2.0, amp=0.15),
                    0.6,
                )
                mix_into(c_buf, voice, 0, 0.5)
            c_buf = one_pole_lp(c_buf, 1100.0)
            c_buf = apply_adsr(c_buf, 0.45, 0.6, 0.75, 0.65)
            mix_into(out, c_buf, int(c_idx * 4.0 * SAMPLE_RATE), 0.9)

    elif theme in ("ending_helicopter", "ending_escape"):
        # Heroic urgent minor-to-major progression with driving 8th-note pulse
        chords = (
            (110.0, 164.81, 220.0, 261.63),
            (87.31, 130.81, 174.61, 261.63),
            (98.0, 146.83, 196.0, 293.66),
            (110.0, 164.81, 220.0, 329.63),
        )
        for idx, ch in enumerate(chords):
            seg = new_buf(2.2)
            for f_c in ch:
                v = add_buf(osc_saw(2.2, f_c, amp=0.35), osc_triangle(2.2, f_c * 0.5, amp=0.35), 0.8)
                mix_into(seg, v, 0, 0.45)
            seg = one_pole_lp(seg, 1500.0 if theme == "ending_helicopter" else 1850.0)
            seg = apply_adsr(seg, 0.15, 0.3, 0.78, 0.35)
            mix_into(out, seg, int(idx * 2.0 * SAMPLE_RATE), 0.85)
        # Driving pulse heartbeat underneath
        for beat in range(16):
            thump = osc_sweep(0.22, 125.0, 38.0, kind="sine", amp=0.75)
            thump = apply_exp_decay(thump, tau=0.075, attack=0.002)
            mix_into(out, thump, int(beat * 0.5 * SAMPLE_RATE), 0.65)

    else:  # tension
        # Dissonant minor-second cluster drone + sub-bass heartbeat pulse
        f_base = 55.0 * (1.0 if variant == 1 else 1.08)
        t1 = osc_fm(raw_dur, f_base, f_base * 1.414, 1.8, amp=0.55)
        t2 = osc_saw(raw_dur, f_base * 1.05946, amp=0.40)
        cluster = one_pole_lp(add_buf(t1, t2, 0.75), 680.0)
        cluster = apply_am(cluster, 0.75, depth=0.45)
        mix_into(out, cluster, 0, 0.85)
        for beat in range(8):
            hb = osc_sweep(0.28, 92.0, 28.0, kind="sine", amp=0.8)
            hb = apply_exp_decay(hb, tau=0.08, attack=0.003)
            mix_into(out, hb, int(beat * 1.0 * SAMPLE_RATE), 0.75)

    out = room_echo(out, room_size=0.85, decay=0.40, wet=0.28)
    return finalize_clip(out, is_loop=True, crossfade_sec=0.50)


def synth_music_stinger(variant: int) -> array.array:
    rng = random.Random(hash(("mus_stinger", variant)) & 0xFFFFFFFF)
    dur = 1.85
    f0 = (110.0, 92.5, 123.47)[variant - 1]

    # Sharp horror FM screech + deep sub boom
    screech = osc_fm(dur, f0 * 4.0, f0 * 5.65, 5.2, amp=0.75, mod_decay=3.5, carrier_end_freq=f0 * 2.8)
    screech = apply_adsr(screech, 0.01, 0.25, 0.55, 0.85)

    boom = osc_sweep(dur * 0.75, 130.0, 26.0, kind="sine", amp=1.0)
    boom = apply_exp_decay(boom, tau=0.32, attack=0.002)

    metal = biquad_bp(noise_white(0.35, 0.9, rng=rng), 2400.0 + variant * 300.0, q=2.5)
    metal = apply_exp_decay(metal, tau=0.08, attack=0.001)

    out = add_buf(boom, screech, 0.75)
    mix_into(out, metal, 0, 0.65)
    out = room_echo(soft_clip(out, drive=2.2, mix=0.85), room_size=1.15, decay=0.55, wet=0.42)
    return finalize_clip(out)


def synth_heartbeat(variant: int) -> array.array:
    dur = 0.82 if variant == 1 else 0.66
    out = new_buf(dur)
    # Lub-dub dual low-frequency cardiac thump
    lub = osc_sweep(0.18, 88.0, 32.0, kind="sine", amp=1.0)
    lub = apply_exp_decay(lub, tau=0.048, attack=0.004)
    dub = osc_sweep(0.16, 102.0, 38.0, kind="sine", amp=0.85)
    dub = apply_exp_decay(dub, tau=0.042, attack=0.004)
    mix_into(out, lub, int(0.03 * SAMPLE_RATE), 1.0)
    mix_into(out, dub, int((0.24 if variant == 1 else 0.19) * SAMPLE_RATE), 0.85)
    out = one_pole_lp(out, 180.0)
    return finalize_clip(out)


def synth_tinnitus() -> array.array:
    dur = 2.4
    # Piercing high-frequency ear-ringing sine + subtle beating sideband
    s1 = osc_sine(dur, 3820.0, amp=0.85)
    s2 = osc_sine(dur, 3824.5, amp=0.35)
    air = biquad_bp(noise_white(dur, 0.2), 3820.0, q=8.0)
    raw = add_buf(add_buf(s1, s2, 0.4), air, 0.25)
    raw = apply_adsr(raw, 0.08, 0.35, 0.75, 0.65)
    return finalize_clip(raw)


def synth_wind(variant: int) -> array.array:
    rng = random.Random(hash(("wind", variant)) & 0xFFFFFFFF)
    dur = 2.8
    src = noise_pink(dur, 1.0, rng=rng)
    f_low = 260.0 + variant * 65.0
    f_high = 880.0 + variant * 140.0
    howl1 = swept_bp(src, f_low, f_high, q=3.2)
    howl2 = swept_bp(src, f_high * 0.9, f_low * 1.15, q=2.4)
    raw = add_buf(howl1, howl2, 0.75)
    raw = apply_adsr(raw, 0.35, 0.45, 0.78, 0.55)
    return finalize_clip(raw)


def synth_rain(variant: int) -> array.array:
    rng = random.Random(hash(("rain", variant)) & 0xFFFFFFFF)
    dur = 2.6
    wash = biquad_bp(noise_pink(dur, 0.75, rng=rng), 1850.0 + variant * 200.0, q=0.9)
    drops = biquad_bp(
        noise_velvet(dur, density=2400.0 + variant * 400.0, amp=0.9, rng=rng),
        3400.0,
        q=1.8,
    )
    raw = add_buf(wash, drops, 0.85)
    raw = apply_adsr(raw, 0.2, 0.3, 0.85, 0.3)
    return finalize_clip(raw)


def synth_thunder(variant: int) -> array.array:
    rng = random.Random(hash(("thunder", variant)) & 0xFFFFFFFF)
    dur = 2.5 + variant * 0.2
    out = new_buf(dur)

    # Lightning crack transient
    crack = biquad_bp(noise_white(0.14, 1.0, rng=rng), 1650.0 + variant * 250.0, q=1.4)
    crack = apply_exp_decay(crack, tau=0.038, attack=0.001)
    mix_into(out, crack, int(0.02 * SAMPLE_RATE), 0.9)

    # Deep rolling atmospheric rumble swells
    rumble = one_pole_lp(
        add_buf(noise_brown(dur, 1.0, rng=rng), noise_pink(dur, 0.7, rng=rng), 0.6),
        320.0 + variant * 45.0,
    )
    rumble = apply_am(rumble, 2.8 + variant * 0.5, depth=0.55)
    rumble = apply_adsr(rumble, 0.04, 0.45, 0.65, 0.95)
    mix_into(out, rumble, 0, 1.0)

    out = soft_clip(out, drive=2.4, mix=0.85)
    out = room_echo(out, room_size=1.4, decay=0.6, wet=0.45)
    return finalize_clip(out)


def synth_alarm(variant: int) -> array.array:
    dur = 1.6
    out = new_buf(dur)
    # Emergency klaxon siren pulses
    pulses = 2 if variant < 3 else 3
    p_dur = dur / pulses
    for p in range(pulses):
        f0 = 680.0 + variant * 60.0
        f1 = 980.0 + variant * 80.0
        siren = osc_sweep(p_dur * 0.82, f0, f1, kind="saw", amp=0.85)
        siren = one_pole_lp(siren, 2400.0)
        siren = apply_adsr(siren, 0.02, 0.08, 0.85, 0.08)
        mix_into(out, siren, int(p * p_dur * SAMPLE_RATE), 0.95)
    out = room_echo(out, room_size=0.8, decay=0.38, wet=0.28)
    return finalize_clip(out)


def synth_ui_sound(kind: str, variant: int = 1) -> array.array:
    rng = random.Random(hash(("ui", kind, variant)) & 0xFFFFFFFF)
    shift = 1.0 if variant == 1 else 1.06

    if kind == "click":
        dur = 0.065
        t = osc_fm(dur, 1850.0 * shift, 2750.0 * shift, 2.2, amp=0.9, mod_decay=45.0)
        return finalize_clip(apply_exp_decay(t, tau=0.011, attack=0.0005))

    if kind == "back":
        dur = 0.11
        t = osc_sweep(dur, 920.0 * shift, 540.0 * shift, kind="triangle", amp=0.85)
        return finalize_clip(apply_exp_decay(t, tau=0.026, attack=0.001))

    if kind == "open":
        dur = 0.18
        t1 = osc_sweep(dur, 440.0 * shift, 880.0 * shift, kind="sine", amp=0.8)
        t2 = biquad_bp(noise_pink(dur, 0.45, rng=rng), 1400.0, q=2.5)
        return finalize_clip(apply_adsr(add_buf(t1, t2, 0.4), 0.01, 0.04, 0.6, 0.05))

    if kind == "close":
        dur = 0.16
        t1 = osc_sweep(dur, 780.0 * shift, 360.0 * shift, kind="sine", amp=0.8)
        return finalize_clip(apply_exp_decay(t1, tau=0.038, attack=0.002))

    if kind in ("error", "denied"):
        dur = 0.26
        out = new_buf(dur)
        f_b = 185.0 * shift if kind == "error" else 155.0 * shift
        for idx, off_s in enumerate((0.0, 0.12)):
            buzz = osc_saw(0.095, f_b * (1.0 - 0.08 * idx), amp=0.85)
            buzz = one_pole_lp(buzz, 950.0)
            buzz = apply_adsr(buzz, 0.004, 0.02, 0.8, 0.02)
            mix_into(out, buzz, int(off_s * SAMPLE_RATE), 0.95)
        return finalize_clip(out)

    if kind == "confirm":
        dur = 0.24
        out = new_buf(dur)
        n1 = apply_exp_decay(osc_sine(0.11, 587.33 * shift, amp=0.85), tau=0.035, attack=0.002)
        n2 = apply_exp_decay(osc_sine(0.15, 880.0 * shift, amp=0.9), tau=0.048, attack=0.002)
        mix_into(out, n1, 0, 0.9)
        mix_into(out, n2, int(0.075 * SAMPLE_RATE), 1.0)
        return finalize_clip(out)

    if kind == "craft":
        dur = 0.34
        out = new_buf(dur)
        clink = osc_fm(0.15, 1320.0 * shift, 1980.0, 3.0, amp=0.85, mod_decay=22.0)
        clink = apply_exp_decay(clink, tau=0.032, attack=0.0008)
        chime = osc_sine(0.22, 1046.5 * shift, amp=0.75)
        chime = apply_exp_decay(chime, tau=0.065, attack=0.002)
        mix_into(out, clink, 0, 0.9)
        mix_into(out, chime, int(0.08 * SAMPLE_RATE), 0.85)
        return finalize_clip(out)

    if kind == "quest":
        dur = 0.62
        out = new_buf(dur)
        for i, f_q in enumerate((440.0, 554.37, 659.25)):
            note = osc_fm(0.35, f_q * shift, f_q * 2.0, 1.4, amp=0.75, mod_decay=8.0)
            note = apply_exp_decay(note, tau=0.11, attack=0.003)
            mix_into(out, note, int(i * 0.11 * SAMPLE_RATE), 0.85)
        return finalize_clip(room_echo(out, room_size=0.5, decay=0.28, wet=0.22))

    if kind == "journal":
        dur = 0.32
        page = swept_bp(noise_pink(dur, 0.9, rng=rng), 1200.0, 2850.0, q=2.2)
        page = apply_adsr(page, 0.02, 0.08, 0.65, 0.1)
        tone = apply_exp_decay(osc_sine(0.18, 784.0 * shift, amp=0.45), tau=0.05, attack=0.003)
        return finalize_clip(add_buf(page, tone, 0.55))

    if kind == "toast":
        dur = 0.22
        ping = osc_fm(dur, 1174.66 * shift, 2349.32, 1.1, amp=0.85, mod_decay=16.0)
        return finalize_clip(apply_exp_decay(ping, tau=0.055, attack=0.002))

    if kind == "upgrade":
        dur = 0.58
        out = new_buf(dur)
        for i, f_u in enumerate((523.25, 659.25, 783.99, 1046.5)):
            n = add_buf(osc_triangle(0.24, f_u * shift, amp=0.75), osc_sine(0.24, f_u * 2.0, amp=0.35), 0.5)
            n = apply_exp_decay(n, tau=0.075, attack=0.002)
            mix_into(out, n, int(i * 0.085 * SAMPLE_RATE), 0.85)
        return finalize_clip(out)

    if kind == "save":
        dur = 0.42
        out = new_buf(dur)
        for i, f_s in enumerate((659.25, 987.77)):
            n = osc_sine(0.24, f_s * shift, amp=0.8)
            n = apply_exp_decay(n, tau=0.07, attack=0.003)
            mix_into(out, n, int(i * 0.11 * SAMPLE_RATE), 0.9)
        return finalize_clip(out)

    if kind == "equip":
        dur = 0.22
        clack = osc_fm(0.09, 1480.0 * shift, 2120.0, 3.4, amp=0.85, mod_decay=28.0)
        clack = apply_exp_decay(clack, tau=0.018, attack=0.0006)
        thump = apply_exp_decay(osc_sweep(0.11, 220.0, 75.0, kind="sine", amp=0.75), tau=0.025, attack=0.001)
        out = new_buf(dur)
        mix_into(out, thump, 0, 0.85)
        mix_into(out, clack, int(0.045 * SAMPLE_RATE), 0.95)
        return finalize_clip(out)

    # tab
    dur = 0.085
    sw = osc_sweep(dur, 1150.0 * shift, 1580.0 * shift, kind="sine", amp=0.8)
    return finalize_clip(apply_exp_decay(sw, tau=0.018, attack=0.0008))


def get_world_sound_specs() -> list[tuple[str, str, Callable[[], array.array]]]:
    """Return list of (category, clip_stem, generator_fn) for all 78 world audio files."""
    specs: list[tuple[str, str, Callable[[], array.array]]] = []

    # 1. 12 seamless ambience loops x 2 variants (_01, _02) = 24 files
    amb_zones = (
        "basement",
        "lab",
        "parking",
        "lobby",
        "mall",
        "office",
        "residential",
        "medical",
        "gym",
        "executive",
        "roof",
        "base",
    )
    for zone in amb_zones:
        for v in (1, 2):
            stem = f"amb_{zone}_{v:02d}"
            specs.append(("World", stem, lambda z=zone, v_i=v: synth_ambience_loop(z, v_i)))

    # 2. 7 seamless music loops (5 with _01.._02, 2 endings with _01) = 12 files
    mus_two = ("explore", "combat", "boss", "safehouse", "tension")
    for m_theme in mus_two:
        for v in (1, 2):
            stem = f"mus_{m_theme}_{v:02d}"
            specs.append(("World", stem, lambda t=m_theme, v_i=v: synth_music_loop(t, v_i)))

    for m_end in ("ending_helicopter", "ending_escape"):
        stem = f"mus_{m_end}_01"
        specs.append(("World", stem, lambda t=m_end: synth_music_loop(t, 1)))

    # 3. Stingers, vitals & weather = 18 files
    for v in (1, 2, 3):
        specs.append(("World", f"mus_stinger_{v:02d}", lambda v_i=v: synth_music_stinger(v_i)))
        specs.append(("World", f"wind_{v:02d}", lambda v_i=v: synth_wind(v_i)))
        specs.append(("World", f"rain_{v:02d}", lambda v_i=v: synth_rain(v_i)))
        specs.append(("World", f"thunder_{v:02d}", lambda v_i=v: synth_thunder(v_i)))
        specs.append(("World", f"alarm_{v:02d}", lambda v_i=v: synth_alarm(v_i)))

    for v in (1, 2):
        specs.append(("World", f"heartbeat_{v:02d}", lambda v_i=v: synth_heartbeat(v_i)))

    specs.append(("World", "tinnitus_01", synth_tinnitus))

    # 4. 15 UI bases (9 with _01.._02, 6 with _01) = 24 files -> 24 + 12 + 18 + 24 = 78 files
    ui_two = ("click", "back", "open", "close", "error", "confirm", "equip", "denied", "tab")
    ui_one = ("craft", "quest", "journal", "toast", "upgrade", "save")

    for u_kind in ui_two:
        for v in (1, 2):
            stem = f"ui_{u_kind}_{v:02d}"
            specs.append(("World", stem, lambda k=u_kind, v_i=v: synth_ui_sound(k, v_i)))

    for u_kind in ui_one:
        stem = f"ui_{u_kind}_01"
        specs.append(("World", stem, lambda k=u_kind: synth_ui_sound(k, 1)))

    return specs
