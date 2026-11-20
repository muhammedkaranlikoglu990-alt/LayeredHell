"""Master procedural audio generator and contract verifier for 45: Layered Hell.

Collects all sound specifications across Zombies (90), Weapons (97), Foley (114), and World (78)
= 379 total 44100 Hz 16-bit mono WAV files, renders them in parallel using multiprocessing,
verifies every required base from Docs/CONTRACT.md §2.2, validates WAV headers, peak levels,
and seamless loop boundaries, and writes Assets/Audio/audio_manifest.json.
"""

from __future__ import annotations

import array
import json
import math
import multiprocessing
import os
import re
import sys
import time
import wave
from typing import Any, Callable

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))
AUDIO_ROOT = os.path.join(PROJECT_ROOT, "Assets", "Audio")
MANIFEST_PATH = os.path.join(AUDIO_ROOT, "audio_manifest.json")

if SCRIPT_DIR not in sys.path:
    sys.path.insert(0, SCRIPT_DIR)

from synth_core import SAMPLE_RATE, write_wav
from sounds_foley import get_foley_sound_specs
from sounds_weapons import get_weapons_sound_specs
from sounds_world import get_world_sound_specs
from sounds_zombie import get_zombie_sound_specs

_VARIANT_SUFFIX_RE = re.compile(r"^(.*)_\d{2}$")


def strip_variant_suffix(stem: str) -> str:
    m = _VARIANT_SUFFIX_RE.match(stem)
    return m.group(1) if m else stem


def is_loop_stem(stem: str) -> bool:
    return stem.startswith("amb_") or (
        stem.startswith("mus_") and not stem.startswith("mus_stinger")
    ) or stem.startswith("generator_loop")


def collect_all_specs() -> list[tuple[str, str, Callable[[], array.array]]]:
    specs: list[tuple[str, str, Callable[[], array.array]]] = []
    specs.extend(get_zombie_sound_specs())
    specs.extend(get_weapons_sound_specs())
    specs.extend(get_foley_sound_specs())
    specs.extend(get_world_sound_specs())
    return specs


_ALL_SPECS: list[tuple[str, str, Callable[[], array.array]]] = collect_all_specs()
_SPEC_LOOKUP: dict[tuple[str, str], Callable[[], array.array]] = {
    (cat, stem): fn for cat, stem, fn in _ALL_SPECS
}


def _render_worker(task: tuple[str, str]) -> dict[str, Any]:
    category, stem = task
    fn = _SPEC_LOOKUP[(category, stem)]
    samples = fn()
    n = len(samples)
    if n == 0:
        raise RuntimeError(f"Empty sample buffer generated for {category}/{stem}")

    out_path = os.path.join(AUDIO_ROOT, category, f"{stem}.wav")
    write_wav(out_path, samples, rate=SAMPLE_RATE)

    peak = 0.0
    sum_sq = 0.0
    for v in samples:
        av = abs(v)
        if av > peak:
            peak = av
        sum_sq += v * v
    rms = math.sqrt(sum_sq / n)
    peak_db = 20.0 * math.log10(max(1e-9, peak))
    rms_db = 20.0 * math.log10(max(1e-9, rms))
    loop_flag = is_loop_stem(stem)
    seam_delta = abs(samples[0] - samples[-1]) if n > 1 else 0.0

    return {
        "category": category,
        "stem": stem,
        "base": strip_variant_suffix(stem),
        "rel_path": f"Assets/Audio/{category}/{stem}.wav",
        "abs_path": out_path,
        "samples": n,
        "duration_sec": round(n / SAMPLE_RATE, 4),
        "peak_amp": round(peak, 5),
        "peak_db": round(peak_db, 3),
        "rms_db": round(rms_db, 3),
        "is_loop": loop_flag,
        "seam_delta": round(seam_delta, 6),
    }


def get_contract_required_bases() -> dict[str, list[str]]:
    zombies: list[str] = []
    for arch in ("walker", "runner", "spitter", "armored"):
        for act in ("breath", "scream_far", "scratch", "attack", "bone", "death"):
            zombies.append(f"zmb_{arch}_{act}")
    for act in ("breath", "roar_far", "scratch", "attack", "bone", "death", "step", "enrage"):
        zombies.append(f"zmb_goliath_{act}")
    zombies.extend(["zmb_spit", "zmb_spit_hit"])

    weapons: list[str] = []
    for wpn in ("pistol", "revolver", "shotgun", "smg", "rifle", "sniper"):
        for act in ("draw", "fire", "echo", "reload_out", "reload_in", "empty"):
            weapons.append(f"wpn_{wpn}_{act}")
    for surf in ("concrete", "metal", "wood"):
        weapons.append(f"shell_{surf}")
    for target in ("flesh", "bone", "metal"):
        weapons.append(f"melee_hit_{target}")
    for mw in ("pipe", "bat", "axe", "machete"):
        weapons.append(f"melee_swing_{mw}")
    weapons.extend(
        [
            "explosion",
            "molotov",
            "impact_concrete",
            "impact_metal",
            "impact_glass",
            "bullet_whiz",
        ]
    )

    foley: list[str] = []
    for surf in ("concrete", "parquet", "tile", "water", "glass", "metalstair"):
        for gait in ("walk", "run", "crouch"):
            foley.append(f"step_{surf}_{gait}")
    for p in (
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
    ):
        foley.append(f"pickup_{p}")
    foley.extend(
        [
            "door_creak",
            "door_open",
            "door_close",
            "door_locked",
            "lockpick",
            "lock_break",
            "generator_start",
            "generator_loop",
            "generator_stop",
            "radio_static",
            "radio_tune",
            "radio_squelch",
            "radio_beep",
            "radio_voice",
            "hammer",
            "craft",
            "drip",
            "elevator_move",
            "elevator_ding",
            "elevator_door",
            "jump_land",
            "cough",
            "eat",
            "drink",
            "bandage",
            "inject",
            "flashlight_click",
            "barricade_build",
        ]
    )

    world: list[str] = []
    for z in (
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
    ):
        world.append(f"amb_{z}")
    for m in (
        "explore",
        "combat",
        "boss",
        "safehouse",
        "ending_helicopter",
        "ending_escape",
        "tension",
    ):
        world.append(f"mus_{m}")
    world.extend(
        [
            "mus_stinger",
            "heartbeat",
            "tinnitus",
            "wind",
            "rain",
            "thunder",
            "alarm",
            "ui_click",
            "ui_back",
            "ui_open",
            "ui_close",
            "ui_error",
            "ui_confirm",
            "ui_craft",
            "ui_quest",
            "ui_journal",
            "ui_toast",
            "ui_upgrade",
            "ui_save",
            "ui_equip",
            "ui_denied",
            "ui_tab",
        ]
    )

    return {
        "Zombies": zombies,
        "Weapons": weapons,
        "Foley": foley,
        "World": world,
    }


def verify_outputs(results: list[dict[str, Any]]) -> None:
    total = len(results)
    if not (350 <= total <= 400):
        raise AssertionError(f"Expected 350..400 audio files per CONTRACT §2.2, got {total}")

    generated_bases_by_cat: dict[str, set[str]] = {
        "Zombies": set(),
        "Weapons": set(),
        "Foley": set(),
        "World": set(),
    }

    for entry in results:
        cat = entry["category"]
        stem = entry["stem"]
        base = entry["base"]
        abs_path = entry["abs_path"]
        generated_bases_by_cat[cat].add(base)

        if not os.path.isfile(abs_path):
            raise AssertionError(f"Missing generated WAV file: {abs_path}")

        with wave.open(abs_path, "rb") as wf:
            if wf.getnchannels() != 1:
                raise AssertionError(f"{stem}: expected 1 channel (mono), got {wf.getnchannels()}")
            if wf.getsampwidth() != 2:
                raise AssertionError(f"{stem}: expected 16-bit PCM (sampwidth=2), got {wf.getsampwidth()}")
            if wf.getframerate() != SAMPLE_RATE:
                raise AssertionError(f"{stem}: expected {SAMPLE_RATE} Hz, got {wf.getframerate()}")
            if wf.getnframes() != entry["samples"]:
                raise AssertionError(f"{stem}: frame count mismatch ({wf.getnframes()} != {entry['samples']})")

        if entry["peak_amp"] < 0.15:
            raise AssertionError(f"{stem}: audio clip is near-silent (peak={entry['peak_amp']})")
        if entry["peak_db"] > -0.8 + 1e-3:
            raise AssertionError(f"{stem}: peak exceeds -0.8 dBFS ceiling ({entry['peak_db']} dBFS)")
        if entry["is_loop"] and entry["seam_delta"] > 0.005:
            raise AssertionError(f"{stem}: loop seam discontinuity too large ({entry['seam_delta']})")

    req_by_cat = get_contract_required_bases()
    for cat, req_bases in req_by_cat.items():
        missing = [b for b in req_bases if b not in generated_bases_by_cat[cat]]
        if missing:
            raise AssertionError(f"Category {cat} missing required CONTRACT §2.2 bases: {missing}")


def main() -> None:
    t0 = time.time()
    tasks = [(cat, stem) for cat, stem, _ in _ALL_SPECS]
    if len(tasks) != len(_SPEC_LOOKUP):
        raise RuntimeError("Duplicate (category, stem) detected in audio specs!")

    workers = max(1, min(8, (os.cpu_count() or 4)))
    print(f"[Audio] Synthesizing {len(tasks)} WAV files across {workers} workers...")

    with multiprocessing.Pool(processes=workers) as pool:
        results = pool.map(_render_worker, tasks, chunksize=8)

    results.sort(key=lambda r: (r["category"], r["stem"]))
    verify_outputs(results)

    by_category_counts: dict[str, int] = {}
    bases_index: dict[str, list[str]] = {}
    total_bytes = 0
    for r in results:
        cat = r["category"]
        by_category_counts[cat] = by_category_counts.get(cat, 0) + 1
        bases_index.setdefault(r["base"], []).append(r["rel_path"])
        total_bytes += os.path.getsize(r["abs_path"])

    manifest = {
        "project": "45: Layered Hell",
        "sample_rate": SAMPLE_RATE,
        "channels": 1,
        "sample_width_bits": 16,
        "total_files": len(results),
        "total_bases": len(bases_index),
        "total_size_mb": round(total_bytes / (1024 * 1024), 2),
        "category_counts": by_category_counts,
        "bases": bases_index,
        "clips": [
            {
                "category": r["category"],
                "stem": r["stem"],
                "base": r["base"],
                "path": r["rel_path"],
                "duration_sec": r["duration_sec"],
                "peak_db": r["peak_db"],
                "rms_db": r["rms_db"],
                "is_loop": r["is_loop"],
                "seam_delta": r["seam_delta"],
            }
            for r in results
        ],
    }

    os.makedirs(AUDIO_ROOT, exist_ok=True)
    with open(MANIFEST_PATH, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    elapsed = time.time() - t0
    print(
        f"[Audio] SUCCESS: Generated & verified {len(results)} WAV files "
        f"({len(bases_index)} required+extra bases, {manifest['total_size_mb']} MB) "
        f"in {elapsed:.2f}s -> {MANIFEST_PATH}"
    )
    for cat, count in sorted(by_category_counts.items()):
        print(f"  - {cat}: {count} files")


if __name__ == "__main__":
    main()
