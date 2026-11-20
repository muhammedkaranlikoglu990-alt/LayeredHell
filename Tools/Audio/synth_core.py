"""Pure Python 3.14 procedural audio synthesis core for 45: Layered Hell.

Uses only the Python standard library (math, random, array, struct, wave).
All output WAV files are 44100 Hz, 16-bit mono PCM, DC-centered, and peak-normalized <= -0.8 dBFS.
"""

from __future__ import annotations

import array
import math
import os
import random
import struct
import wave
from typing import Iterable, Sequence

SAMPLE_RATE: int = 44100
TWO_PI: float = 2.0 * math.pi
DEFAULT_PEAK_DB: float = -1.0  # <= -0.8 dBFS contract requirement


def db_to_amp(db: float) -> float:
    return 10.0 ** (db / 20.0)


def new_buf(duration: float, rate: int = SAMPLE_RATE) -> array.array:
    n = max(1, int(round(duration * rate)))
    return array.array("f", [0.0]) * n


def clone_buf(buf: array.array) -> array.array:
    return array.array("f", buf)


def mix_into(
    dst: array.array,
    src: array.array,
    offset: int = 0,
    gain: float = 1.0,
) -> array.array:
    n_dst = len(dst)
    n_src = len(src)
    if offset < 0:
        src_start = -offset
        dst_start = 0
    else:
        src_start = 0
        dst_start = offset
    count = min(n_src - src_start, n_dst - dst_start)
    if count <= 0:
        return dst
    if gain == 1.0:
        for i in range(count):
            dst[dst_start + i] += src[src_start + i]
    else:
        for i in range(count):
            dst[dst_start + i] += src[src_start + i] * gain
    return dst


def add_buf(a: array.array, b: array.array, gain_b: float = 1.0) -> array.array:
    n = max(len(a), len(b))
    out = array.array("f", [0.0]) * n
    for i in range(len(a)):
        out[i] = a[i]
    mix_into(out, b, 0, gain_b)
    return out


def mul_buf(buf: array.array, env: array.array | float) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    if isinstance(env, (int, float)):
        g = float(env)
        for i in range(n):
            out[i] = buf[i] * g
    else:
        m = min(n, len(env))
        for i in range(m):
            out[i] = buf[i] * env[i]
    return out


def concat_bufs(*bufs: array.array) -> array.array:
    out = array.array("f")
    for b in bufs:
        out.extend(b)
    return out


def upsample_2x(buf: array.array) -> array.array:
    n = len(buf)
    if n == 0:
        return array.array("f")
    out = array.array("f", [0.0]) * (n * 2)
    for i in range(n - 1):
        v0 = buf[i]
        v1 = buf[i + 1]
        j = i * 2
        out[j] = v0
        out[j + 1] = 0.5 * (v0 + v1)
    out[2 * n - 2] = buf[n - 1]
    out[2 * n - 1] = buf[n - 1]
    return out


# ---------------------------------------------------------------------------
# Oscillators
# ---------------------------------------------------------------------------

def osc_sine(
    duration: float,
    freq: float,
    phase: float = 0.0,
    amp: float = 1.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    step = TWO_PI * freq / rate
    ph = phase
    sin = math.sin
    for i in range(n):
        out[i] = sin(ph) * amp
        ph += step
    return out


def osc_triangle(
    duration: float,
    freq: float,
    phase: float = 0.0,
    amp: float = 1.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    step = freq / rate
    ph = (phase / TWO_PI) % 1.0
    for i in range(n):
        out[i] = (4.0 * abs(ph - 0.5) - 1.0) * amp
        ph += step
        if ph >= 1.0:
            ph -= int(ph)
    return out


def osc_saw(
    duration: float,
    freq: float,
    phase: float = 0.0,
    amp: float = 1.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    step = freq / rate
    ph = (phase / TWO_PI) % 1.0
    for i in range(n):
        out[i] = (2.0 * ph - 1.0) * amp
        ph += step
        if ph >= 1.0:
            ph -= int(ph)
    return out


def osc_pulse(
    duration: float,
    freq: float,
    duty: float = 0.5,
    phase: float = 0.0,
    amp: float = 1.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    step = freq / rate
    ph = (phase / TWO_PI) % 1.0
    pos = amp
    neg = -amp
    for i in range(n):
        out[i] = pos if ph < duty else neg
        ph += step
        if ph >= 1.0:
            ph -= int(ph)
    return out


def osc_fm(
    duration: float,
    carrier_freq: float,
    mod_freq: float,
    mod_index: float,
    amp: float = 1.0,
    mod_decay: float = 0.0,
    carrier_end_freq: float | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    c_ph = 0.0
    m_ph = 0.0
    m_step = TWO_PI * mod_freq / rate
    sin = math.sin
    exp = math.exp
    inv_n = 1.0 / max(1, n - 1)
    f0 = carrier_freq
    f1 = carrier_freq if carrier_end_freq is None else carrier_end_freq
    decay_step = -mod_decay / rate if mod_decay > 0.0 else 0.0
    idx = mod_index
    idx_mul = exp(decay_step) if mod_decay > 0.0 else 1.0
    for i in range(n):
        t = i * inv_n
        cf = f0 + (f1 - f0) * t
        out[i] = sin(c_ph + idx * sin(m_ph)) * amp
        c_ph += TWO_PI * cf / rate
        m_ph += m_step
        idx *= idx_mul
    return out


def osc_sweep(
    duration: float,
    f_start: float,
    f_end: float,
    kind: str = "sine",
    exp_curve: bool = True,
    amp: float = 1.0,
    vibrato_hz: float = 0.0,
    vibrato_depth: float = 0.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    ph = 0.0
    v_ph = 0.0
    v_step = TWO_PI * vibrato_hz / rate
    sin = math.sin
    inv_n = 1.0 / max(1, n - 1)
    f0 = max(1.0, f_start)
    f1 = max(1.0, f_end)
    ratio = f1 / f0
    for i in range(n):
        t = i * inv_n
        if exp_curve:
            freq = f0 * (ratio ** t)
        else:
            freq = f0 + (f1 - f0) * t
        if vibrato_depth != 0.0:
            freq *= 1.0 + vibrato_depth * sin(v_ph)
            v_ph += v_step
        if kind == "sine":
            out[i] = sin(ph * TWO_PI) * amp
        elif kind == "saw":
            out[i] = (2.0 * ph - 1.0) * amp
        elif kind == "triangle":
            out[i] = (4.0 * abs(ph - 0.5) - 1.0) * amp
        elif kind == "pulse":
            out[i] = amp if ph < 0.5 else -amp
        else:
            out[i] = sin(ph * TWO_PI) * amp
        ph += freq / rate
        if ph >= 1.0:
            ph -= int(ph)
    return out


def osc_vocal_cord(
    duration: float,
    f_start: float,
    f_end: float,
    roughness: float = 0.25,
    subharmonic: float = 0.35,
    vibrato_hz: float = 5.5,
    vibrato_depth: float = 0.03,
    rng: random.Random | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Rich glottal pulse + subharmonic + FM growl generator for vocal synthesis."""
    if rng is None:
        rng = random.Random(1337)
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    ph = 0.0
    sub_ph = 0.0
    v_ph = rng.random() * TWO_PI
    v_step = TWO_PI * vibrato_hz / rate
    g_ph = rng.random() * TWO_PI
    g_step = TWO_PI * (28.0 + rng.random() * 22.0) / rate
    sin = math.sin
    inv_n = 1.0 / max(1, n - 1)
    jitter = 0.0
    rand = rng.random
    for i in range(n):
        t = i * inv_n
        # Contour with mid-phrase swell
        swell = 1.0 + 0.12 * sin(math.pi * t)
        freq = (f_start + (f_end - f_start) * t) * swell
        if (i & 31) == 0:
            jitter = (rand() * 2.0 - 1.0) * roughness * 0.12
        freq *= 1.0 + vibrato_depth * sin(v_ph) + jitter
        v_ph += v_step
        g_ph += g_step

        # Glottal wave: fundamental + 2nd + 3rd + sharp edge saw + sub-octave growl
        p2 = ph * TWO_PI
        val = (
            0.50 * sin(p2)
            + 0.30 * sin(2.0 * p2 + 0.4)
            + 0.18 * sin(3.0 * p2 + 0.8)
            + 0.12 * sin(4.0 * p2)
            + 0.28 * (2.0 * ph - 1.0)
            + subharmonic * sin(sub_ph * TWO_PI)
        )
        # Amplitude flutter from pharyngeal growl
        flutter = 1.0 - roughness * 0.45 * (0.5 + 0.5 * sin(g_ph))
        out[i] = val * flutter
        step = max(5.0, freq) / rate
        ph += step
        if ph >= 1.0:
            ph -= int(ph)
        sub_ph += 0.5 * step
        if sub_ph >= 1.0:
            sub_ph -= int(sub_ph)
    return out


# ---------------------------------------------------------------------------
# Noise Generators
# ---------------------------------------------------------------------------

def noise_white(
    duration: float,
    amp: float = 1.0,
    rng: random.Random | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    if rng is None:
        rng = random.Random()
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    rand = rng.random
    scale = 2.0 * amp
    for i in range(n):
        out[i] = (rand() - 0.5) * scale
    return out


def noise_pink(
    duration: float,
    amp: float = 1.0,
    rng: random.Random | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Fast 3-pole Paul Kellet pink noise approximation."""
    if rng is None:
        rng = random.Random()
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    rand = rng.random
    b0 = 0.0
    b1 = 0.0
    b2 = 0.0
    scale = amp * 0.32
    for i in range(n):
        w = rand() * 2.0 - 1.0
        b0 = 0.99765 * b0 + w * 0.0990460
        b1 = 0.96300 * b1 + w * 0.2965164
        b2 = 0.57000 * b2 + w * 1.0526913
        out[i] = (b0 + b1 + b2 + w * 0.1848) * scale
    return out


def noise_brown(
    duration: float,
    amp: float = 1.0,
    rng: random.Random | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Leaky integrator brown (1/f^2) noise."""
    if rng is None:
        rng = random.Random()
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    rand = rng.random
    acc = 0.0
    for i in range(n):
        w = rand() * 2.0 - 1.0
        acc = (acc + 0.04 * w) * 0.992
        out[i] = acc * amp * 1.8
    return out


def noise_velvet(
    duration: float,
    density: float = 900.0,
    amp: float = 1.0,
    rng: random.Random | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Sparse impulse noise for crackle, rain drops, grit, fire sparks."""
    if rng is None:
        rng = random.Random()
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    prob = min(0.95, max(1e-5, density / rate))
    rand = rng.random
    for i in range(n):
        if rand() < prob:
            out[i] = (rand() * 2.0 - 1.0) * amp
    return out


# ---------------------------------------------------------------------------
# Filters (1-pole LP/HP, resonant biquads, time-varying biquad, formant bank)
# ---------------------------------------------------------------------------

def one_pole_lp(
    buf: array.array,
    cutoff_hz: float,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    c = max(10.0, min(rate * 0.49, cutoff_hz))
    x = math.exp(-TWO_PI * c / rate)
    a0 = 1.0 - x
    b1 = x
    y = 0.0
    for i in range(n):
        y = a0 * buf[i] + b1 * y
        out[i] = y
    return out


def one_pole_hp(
    buf: array.array,
    cutoff_hz: float,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    c = max(5.0, min(rate * 0.49, cutoff_hz))
    rc = 1.0 / (TWO_PI * c)
    dt = 1.0 / rate
    alpha = rc / (rc + dt)
    y = 0.0
    prev_x = buf[0] if n > 0 else 0.0
    for i in range(n):
        x = buf[i]
        y = alpha * (y + x - prev_x)
        prev_x = x
        out[i] = y
    return out


def biquad_filter(
    buf: array.array,
    kind: str,
    freq_hz: float,
    q: float = 1.0,
    gain_db: float = 0.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    f = max(15.0, min(rate * 0.48, freq_hz))
    q_val = max(0.15, q)
    w0 = TWO_PI * f / rate
    cos_w0 = math.cos(w0)
    sin_w0 = math.sin(w0)
    alpha = sin_w0 / (2.0 * q_val)

    if kind == "lp":
        b0 = (1.0 - cos_w0) * 0.5
        b1 = 1.0 - cos_w0
        b2 = (1.0 - cos_w0) * 0.5
        a0 = 1.0 + alpha
        a1 = -2.0 * cos_w0
        a2 = 1.0 - alpha
    elif kind == "hp":
        b0 = (1.0 + cos_w0) * 0.5
        b1 = -(1.0 + cos_w0)
        b2 = (1.0 + cos_w0) * 0.5
        a0 = 1.0 + alpha
        a1 = -2.0 * cos_w0
        a2 = 1.0 - alpha
    elif kind == "bp":
        # Constant 0 dB peak gain bandpass
        b0 = alpha
        b1 = 0.0
        b2 = -alpha
        a0 = 1.0 + alpha
        a1 = -2.0 * cos_w0
        a2 = 1.0 - alpha
    elif kind == "notch":
        b0 = 1.0
        b1 = -2.0 * cos_w0
        b2 = 1.0
        a0 = 1.0 + alpha
        a1 = -2.0 * cos_w0
        a2 = 1.0 - alpha
    elif kind == "peak":
        amp = 10.0 ** (gain_db / 40.0)
        b0 = 1.0 + alpha * amp
        b1 = -2.0 * cos_w0
        b2 = 1.0 - alpha * amp
        a0 = 1.0 + alpha / amp
        a1 = -2.0 * cos_w0
        a2 = 1.0 - alpha / amp
    else:
        raise ValueError(f"Unknown biquad filter kind: {kind}")

    inv_a0 = 1.0 / a0
    c_b0 = b0 * inv_a0
    c_b1 = b1 * inv_a0
    c_b2 = b2 * inv_a0
    c_a1 = a1 * inv_a0
    c_a2 = a2 * inv_a0

    x1 = 0.0
    x2 = 0.0
    y1 = 0.0
    y2 = 0.0
    for i in range(n):
        x0 = buf[i]
        y0 = c_b0 * x0 + c_b1 * x1 + c_b2 * x2 - c_a1 * y1 - c_a2 * y2
        x2 = x1
        x1 = x0
        y2 = y1
        y1 = y0
        out[i] = y0
    return out


def biquad_bp(
    buf: array.array,
    freq_hz: float,
    q: float = 2.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    return biquad_filter(buf, "bp", freq_hz, q=q, rate=rate)


def swept_bp(
    buf: array.array,
    f_start: float,
    f_end: float,
    q: float = 2.5,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Time-varying resonant bandpass filter (coefficient update every 32 samples)."""
    n = len(buf)
    out = array.array("f", [0.0]) * n
    inv_n = 1.0 / max(1, n - 1)
    q_val = max(0.2, q)
    x1 = 0.0
    x2 = 0.0
    y1 = 0.0
    y2 = 0.0
    c_b0 = 0.0
    c_b2 = 0.0
    c_a1 = 0.0
    c_a2 = 0.0
    cos = math.cos
    sin = math.sin
    f0 = max(20.0, f_start)
    f1 = max(20.0, f_end)
    ratio = f1 / f0

    for i in range(n):
        if (i & 31) == 0:
            t = i * inv_n
            f = max(20.0, min(rate * 0.46, f0 * (ratio ** t)))
            w0 = TWO_PI * f / rate
            cos_w0 = cos(w0)
            alpha = sin(w0) / (2.0 * q_val)
            inv_a0 = 1.0 / (1.0 + alpha)
            c_b0 = alpha * inv_a0
            c_b2 = -c_b0
            c_a1 = (-2.0 * cos_w0) * inv_a0
            c_a2 = (1.0 - alpha) * inv_a0
        x0 = buf[i]
        y0 = c_b0 * x0 + c_b2 * x2 - c_a1 * y1 - c_a2 * y2
        x2 = x1
        x1 = x0
        y2 = y1
        y1 = y0
        out[i] = y0
    return out


def formant_filter(
    buf: array.array,
    formants: Sequence[float],
    qs: Sequence[float] | None = None,
    weights: Sequence[float] | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Parallel resonant formant filter bank for vocal tract resonances."""
    n = len(buf)
    out = array.array("f", [0.0]) * n
    k = len(formants)
    if k == 0:
        return clone_buf(buf)
    if qs is None:
        qs = [4.5 + idx * 1.5 for idx in range(k)]
    if weights is None:
        weights = [1.0 / (1.0 + 0.45 * idx) for idx in range(k)]

    for f_hz, q_val, w in zip(formants, qs, weights):
        band = biquad_bp(buf, f_hz, q=q_val, rate=rate)
        mix_into(out, band, 0, w)
    return out


# ---------------------------------------------------------------------------
# Envelopes & Modulation
# ---------------------------------------------------------------------------

def env_adsr(
    duration: float,
    attack: float,
    decay: float,
    sustain_level: float,
    release: float,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    n_att = max(1, int(round(attack * rate)))
    n_dec = max(1, int(round(decay * rate)))
    n_rel = max(1, int(round(release * rate)))
    rel_start = max(0, n - n_rel)

    for i in range(n):
        if i >= rel_start:
            # Release phase
            if rel_start < n_att:
                base = rel_start / n_att
            elif rel_start < n_att + n_dec:
                base = 1.0 - (1.0 - sustain_level) * ((rel_start - n_att) / n_dec)
            else:
                base = sustain_level
            rem = (n - 1 - i) / max(1, n - 1 - rel_start)
            out[i] = base * (rem * rem)
        elif i < n_att:
            out[i] = i / n_att
        elif i < n_att + n_dec:
            out[i] = 1.0 - (1.0 - sustain_level) * ((i - n_att) / n_dec)
        else:
            out[i] = sustain_level
    return out


def env_exp_decay(
    duration: float,
    tau: float,
    attack: float = 0.002,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = max(1, int(round(duration * rate)))
    out = array.array("f", [0.0]) * n
    n_att = max(1, int(round(attack * rate)))
    decay_mul = math.exp(-1.0 / (max(0.001, tau) * rate))
    val = 1.0
    for i in range(n):
        if i < n_att:
            out[i] = i / n_att
        else:
            out[i] = val
            val *= decay_mul
    return out


def apply_adsr(
    buf: array.array,
    attack: float,
    decay: float,
    sustain_level: float,
    release: float,
    rate: int = SAMPLE_RATE,
) -> array.array:
    env = env_adsr(len(buf) / rate, attack, decay, sustain_level, release, rate=rate)
    return mul_buf(buf, env)


def apply_exp_decay(
    buf: array.array,
    tau: float,
    attack: float = 0.002,
    rate: int = SAMPLE_RATE,
) -> array.array:
    env = env_exp_decay(len(buf) / rate, tau, attack=attack, rate=rate)
    return mul_buf(buf, env)


def apply_am(
    buf: array.array,
    mod_hz: float,
    depth: float = 0.5,
    phase: float = 0.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    ph = phase
    step = TWO_PI * mod_hz / rate
    sin = math.sin
    base = 1.0 - 0.5 * depth
    scale = 0.5 * depth
    for i in range(n):
        out[i] = buf[i] * (base + scale * sin(ph))
        ph += step
    return out


# ---------------------------------------------------------------------------
# Distortion, Saturation & Lo-Fi
# ---------------------------------------------------------------------------

def soft_clip(
    buf: array.array,
    drive: float = 1.5,
    mix: float = 1.0,
) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    tanh = math.tanh
    norm = 1.0 / max(1e-4, tanh(drive))
    dry = 1.0 - mix
    for i in range(n):
        x = buf[i]
        wet = tanh(x * drive) * norm
        out[i] = x * dry + wet * mix
    return out


def bitcrush(
    buf: array.array,
    steps: int = 16,
    hold: int = 3,
) -> array.array:
    n = len(buf)
    out = array.array("f", [0.0]) * n
    s = max(2, steps)
    inv_s = 1.0 / s
    h = max(1, hold)
    cur = 0.0
    for i in range(n):
        if (i % h) == 0:
            cur = round(buf[i] * s) * inv_s
        out[i] = cur
    return out


# ---------------------------------------------------------------------------
# Delay & Room Echo
# ---------------------------------------------------------------------------

def multitap_delay(
    buf: array.array,
    taps: Sequence[tuple[float, float]],
    feedback: float = 0.0,
    feedback_delay: float = 0.075,
    wet: float = 0.35,
    damp_hz: float = 3200.0,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = len(buf)
    out = clone_buf(buf)
    tap_samples = [(max(1, int(round(d * rate))), g) for d, g in taps]
    for delay_s, gain in tap_samples:
        if delay_s < n:
            for i in range(delay_s, n):
                out[i] += buf[i - delay_s] * (gain * wet)

    if feedback > 0.01:
        fb_s = max(1, int(round(feedback_delay * rate)))
        fb_buf = clone_buf(buf)
        if damp_hz > 0:
            fb_buf = one_pole_lp(fb_buf, damp_hz, rate=rate)
        g = feedback
        cur_delay = fb_s
        for _ in range(4):
            if cur_delay >= n or g < 0.015:
                break
            for i in range(cur_delay, n):
                out[i] += fb_buf[i - cur_delay] * (g * wet)
            cur_delay += fb_s
            g *= feedback
    return out


def room_echo(
    buf: array.array,
    room_size: float = 0.5,
    decay: float = 0.35,
    wet: float = 0.25,
    rate: int = SAMPLE_RATE,
) -> array.array:
    base_d = 0.023 + 0.055 * max(0.05, min(1.5, room_size))
    taps = [
        (base_d * 0.71, decay * 0.75),
        (base_d * 1.13, decay * 0.55),
        (base_d * 1.67, decay * 0.40),
        (base_d * 2.29, decay * 0.28),
        (base_d * 3.07, decay * 0.18),
    ]
    return multitap_delay(
        buf,
        taps=taps,
        feedback=decay * 0.45,
        feedback_delay=base_d * 1.37,
        wet=wet,
        damp_hz=2800.0,
        rate=rate,
    )


# ---------------------------------------------------------------------------
# Post-Processing, Seamless Loop Crossfade & WAV Export
# ---------------------------------------------------------------------------

def remove_dc(buf: array.array) -> array.array:
    n = len(buf)
    if n == 0:
        return buf
    mean = sum(buf) / n
    if abs(mean) < 1e-7:
        return buf
    out = array.array("f", [0.0]) * n
    for i in range(n):
        out[i] = buf[i] - mean
    return out


def fade_in_out(
    buf: array.array,
    fade_in: float = 0.004,
    fade_out: float = 0.012,
    rate: int = SAMPLE_RATE,
) -> array.array:
    n = len(buf)
    if n == 0:
        return buf
    out = clone_buf(buf)
    n_in = min(n // 2, max(1, int(round(fade_in * rate)))) if fade_in > 0 else 0
    n_out = min(n // 2, max(1, int(round(fade_out * rate)))) if fade_out > 0 else 0
    for i in range(n_in):
        out[i] *= i / n_in
    for i in range(n_out):
        idx = n - 1 - i
        out[idx] *= i / n_out
    return out


def normalize(
    buf: array.array,
    peak_db: float = DEFAULT_PEAK_DB,
) -> array.array:
    n = len(buf)
    if n == 0:
        return buf
    # Strict cap <= -0.8 dBFS
    safe_db = min(-0.85, peak_db)
    target = db_to_amp(safe_db)
    max_abs = 0.0
    for i in range(n):
        v = buf[i]
        if math.isnan(v) or math.isinf(v):
            buf[i] = 0.0
            v = 0.0
        av = abs(v)
        if av > max_abs:
            max_abs = av
    if max_abs < 1e-8:
        return buf
    gain = target / max_abs
    out = array.array("f", [0.0]) * n
    for i in range(n):
        out[i] = buf[i] * gain
    return out


def make_seamless_loop(
    buf: array.array,
    crossfade_samples: int | None = None,
    rate: int = SAMPLE_RATE,
) -> array.array:
    """Overlap-add raised-cosine tail-into-head crossfader for seamless loops.

    Takes the last `M = crossfade_samples` samples of `buf` (length `N + M`)
    and crossfades them into the first `M` samples of `buf`, returning a buffer
    of length `N` whose wrap-around `out[N - 1] -> out[0]` and internal merge
    `out[M - 1] -> out[M]` are both continuous samples of the original signal.
    """
    total = len(buf)
    if crossfade_samples is None:
        crossfade_samples = min(total // 4, int(0.45 * rate))
    m = max(64, min(total // 3, int(crossfade_samples)))
    n = total - m
    if n <= m:
        return clone_buf(buf)

    out = array.array("f", [0.0]) * n
    cos = math.cos
    pi = math.pi
    inv_m = 1.0 / max(1, m - 1)
    for i in range(m):
        # Raised cosine crossfade: at i=0, w_head=0, w_tail=1 -> out[0] = buf[n]
        # At i=m-1, w_head=1, w_tail=0 -> out[m-1] = buf[m-1]
        w_head = 0.5 * (1.0 - cos(pi * i * inv_m))
        w_tail = 1.0 - w_head
        out[i] = buf[i] * w_head + buf[n + i] * w_tail

    for i in range(m, n):
        out[i] = buf[i]

    # Ensure exact endpoint continuity over the last 32 samples so
    # abs(out[0] - out[-1]) is near 0 even after high-frequency noise layers.
    blend = min(32, n // 8)
    target0 = out[0]
    for k in range(blend):
        idx = n - blend + k
        alpha = (k + 1) / (blend + 1)
        out[idx] = out[idx] * (1.0 - alpha) + target0 * alpha
    return out


def finalize_clip(
    buf: array.array,
    peak_db: float = DEFAULT_PEAK_DB,
    fade_in: float = 0.003,
    fade_out: float = 0.015,
    is_loop: bool = False,
    crossfade_sec: float = 0.45,
    rate: int = SAMPLE_RATE,
) -> array.array:
    out = remove_dc(buf)
    if is_loop:
        out = make_seamless_loop(out, int(round(crossfade_sec * rate)), rate=rate)
        out = remove_dc(out)
        # Re-anchor endpoints after DC removal & normalize
        out = normalize(out, peak_db=peak_db)
        out[-1] = out[0]
    else:
        out = fade_in_out(out, fade_in=fade_in, fade_out=fade_out, rate=rate)
        out = normalize(out, peak_db=peak_db)
    return out


def write_wav(
    path: str,
    samples: Sequence[float] | array.array,
    rate: int = SAMPLE_RATE,
) -> None:
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    n = len(samples)
    pcm = array.array("h", [0]) * n
    for i in range(n):
        v = samples[i]
        if v > 0.999:
            v = 0.999
        elif v < -0.999:
            v = -0.999
        pcm[i] = int(round(v * 32767.0))

    with wave.open(path, "wb") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(rate)
        wf.writeframes(pcm.tobytes())
