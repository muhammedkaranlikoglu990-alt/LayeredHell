#!/usr/bin/env python3
"""Unit tests for `ll_blender.bl_anim_math` (runs without Blender)."""
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "Blender"))

from ll_blender import rig_spec as R         # noqa: E402
from ll_blender import bl_anim_math as AM    # noqa: E402


def main():
    styles = ("shamble", "sprint", "hunch", "heavy", "boss", "survivor")
    samples = 17
    checked = 0

    for clip, (frames, loop) in R.HUMANOID_CLIPS.items():
        assert frames >= 10, clip
        for style in styles:
            for bone in R.HUMANOID_BONES:
                p0 = AM.pose(clip, style, bone, 0.0, scale=1.0)
                p1 = AM.pose(clip, style, bone, 1.0, scale=1.0)
                for k in range(samples):
                    t = k / (samples - 1)
                    rot, loc = AM.pose(clip, style, bone, t, scale=1.0)
                    assert len(rot) == 3 and len(loc) == 3
                    for v in rot:
                        assert math.isfinite(v) and abs(v) <= math.pi * 1.25, (clip, style, bone, v)
                    for v in loc:
                        assert math.isfinite(v) and abs(v) <= 2.5, (clip, style, bone, v)
                    checked += 1
                if loop:
                    for a, b in zip(p0[0] + p0[1], p1[0] + p1[1]):
                        assert abs(a - b) < 1e-6, f"Loop discontinuity in {clip}/{style}/{bone}: {a} vs {b}"
            # Death clip must drop Hips Z lower at end than start
            if clip == "Death":
                z0 = AM.pose(clip, style, "Hips", 0.0)[1][2]
                z1 = AM.pose(clip, style, "Hips", 1.0)[1][2]
                assert z1 < z0 - 0.5, f"Death clip Hips Z did not drop enough: {z0} -> {z1}"

    for clip, (frames, loop) in R.FPS_CLIPS.items():
        assert frames >= 8, clip
        for bone in R.FPS_BONES:
            p0 = AM.pose(clip, "fps", bone, 0.0)
            p1 = AM.pose(clip, "fps", bone, 1.0)
            for k in range(samples):
                t = k / (samples - 1)
                rot, loc = AM.pose(clip, "fps", bone, t)
                for v in rot + loc:
                    assert math.isfinite(v)
                checked += 1
            if loop:
                for a, b in zip(p0[0] + p0[1], p1[0] + p1[1]):
                    assert abs(a - b) < 1e-6, f"FPS loop discontinuity in {clip}/{bone}: {a} vs {b}"

    print(f"test_anim_math: PASS ({checked} pose samples verified across all humanoid & FPS clips)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
