#!/usr/bin/env python3
"""Headless Blender 4.2+ automation entrypoint for 45: Layered Hell.

Generates 840+ modular 3D models, UV maps, skeletons (Humanoid & FPS Arms), and procedural
animations, exporting Unity-ready FBX/GLB files into `Assets/Art/Models/<Category>/<id>.fbx`.

Usage:
    blender -b -P Tools/Blender/run_blender.py -- --out Assets/Art/Models [--format fbx|glb|both]
            [--only <prefix_or_category>] [--limit N] [--shard i/n] [--skip-existing] [--selftest]
"""
import json
import os
import sys
import tempfile
import time
import traceback

HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)


def _parse_args():
    argv = sys.argv
    if "--" in argv:
        argv = argv[argv.index("--") + 1:]
    else:
        argv = []

    cfg = dict(
        out=os.path.abspath(os.path.join(HERE, "..", "..", "Assets", "Art", "Models")),
        textures=os.path.abspath(os.path.join(HERE, "..", "..", "Assets", "Art", "Textures")),
        fmt="fbx",
        only=None,
        limit=0,
        shard_idx=0,
        shard_total=1,
        skip_existing=False,
        selftest=False,
    )
    i = 0
    while i < len(argv):
        a = argv[i]
        if a == "--out" and i + 1 < len(argv):
            cfg["out"] = os.path.abspath(argv[i + 1]); i += 2
        elif a == "--textures" and i + 1 < len(argv):
            cfg["textures"] = os.path.abspath(argv[i + 1]); i += 2
        elif a == "--format" and i + 1 < len(argv):
            cfg["fmt"] = argv[i + 1].lower(); i += 2
        elif a == "--only" and i + 1 < len(argv):
            cfg["only"] = argv[i + 1]; i += 2
        elif a == "--limit" and i + 1 < len(argv):
            cfg["limit"] = int(argv[i + 1]); i += 2
        elif a == "--shard" and i + 1 < len(argv):
            s_i, s_n = argv[i + 1].split("/")
            cfg["shard_idx"] = int(s_i)
            cfg["shard_total"] = max(1, int(s_n))
            i += 2
        elif a == "--skip-existing":
            cfg["skip_existing"] = True; i += 1
        elif a == "--selftest":
            cfg["selftest"] = True; i += 1
        else:
            i += 1
    return cfg


def main():
    cfg = _parse_args()
    from ll_blender import catalog, builders
    from ll_blender import bl_scene, bl_uv, bl_rig, bl_anim, bl_export

    entries = list(catalog.CATALOG)
    if cfg["selftest"]:
        cfg["out"] = tempfile.mkdtemp(prefix="ll45_selftest_")
        static_e = next((e for e in entries if e.rig == "none"), None)
        hum_e = next((e for e in entries if e.rig == "humanoid"), None)
        fps_e = next((e for e in entries if e.rig == "fps_arms"), None)
        entries = [e for e in (static_e, hum_e, fps_e) if e is not None]
    else:
        if cfg["only"]:
            flt = cfg["only"]
            entries = [e for e in entries if e.category.lower() == flt.lower() or e.id.startswith(flt.lower())]
        if cfg["shard_total"] > 1:
            entries = [e for idx, e in enumerate(entries) if (idx % cfg["shard_total"]) == cfg["shard_idx"]]
        if cfg["limit"] > 0:
            entries = entries[:cfg["limit"]]

    os.makedirs(cfg["out"], exist_ok=True)
    report = []
    failures = 0
    t_start = time.time()

    print(f"[45:Layered Hell] Starting Blender generation of {len(entries)} model(s) -> {cfg['out']}")
    for idx, entry in enumerate(entries, 1):
        t0 = time.time()
        ext = ".glb" if cfg["fmt"] == "glb" else ".fbx"
        target_path = os.path.join(cfg["out"], entry.category, entry.id + ext)
        if cfg["skip_existing"] and os.path.isfile(target_path):
            continue
        try:
            bl_scene.clear_scene()
            geo = builders.build_geo(entry)
            st = geo.validate()
            mesh_obj = bl_scene.geo_to_mesh_object(entry, geo, textures_dir=cfg["textures"])
            bl_uv.verify_and_fix_uvs(entry, geo, mesh_obj)
            arm_obj = bl_rig.build_armature(entry, mesh_obj)
            clips = bl_anim.build_animations(entry, arm_obj)
            paths = bl_export.export_model(entry, cfg["out"], fmt=cfg["fmt"])
            dt = round(time.time() - t0, 3)
            report.append(dict(
                id=entry.id, category=entry.category, rig=entry.rig,
                verts=st["verts"], tris=st["tris"], clips=clips,
                files=paths, time_s=dt, status="ok"
            ))
            if idx % 25 == 0 or idx == len(entries):
                print(f"  [{idx}/{len(entries)}] {entry.id} ({st['tris']} tris, rig={entry.rig}) in {dt}s")
        except Exception as ex:
            failures += 1
            tb = traceback.format_exc()
            print(f"  [ERROR] {entry.id}: {ex}\n{tb}")
            report.append(dict(id=entry.id, category=entry.category, status="error", error=str(ex)))

    total_s = round(time.time() - t_start, 2)
    rep_path = os.path.join(cfg["out"], "build_report.json")
    with open(rep_path, "w") as f:
        json.dump(dict(total=len(entries), ok=len(entries) - failures, failures=failures,
                       elapsed_s=total_s, models=report), f, indent=1)
    print(f"[45:Layered Hell] Done in {total_s}s. OK={len(entries) - failures}, FAIL={failures}. Report: {rep_path}")
    if failures > 0:
        sys.exit(1)


if __name__ == "__main__":
    main()
