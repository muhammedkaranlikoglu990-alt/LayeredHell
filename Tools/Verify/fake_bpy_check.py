#!/usr/bin/env python3
"""Static syntax and Blender 4.2+ API compatibility check for all `bl_*.py` and `run_blender.py` files."""
import ast
import os
import py_compile
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
BLENDER_DIR = os.path.join(HERE, "..", "Blender")

FORBIDDEN_APIS = (
    "use_auto_smooth",
    "calc_normals",
    "calc_normals_split",
    "scene.objects.link",
    "context.scene.objects.active",
)

FILES = [
    os.path.join(BLENDER_DIR, "run_blender.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "atlas.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "geo.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "rig_spec.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "bl_anim_math.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "bl_scene.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "bl_uv.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "bl_rig.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "bl_anim.py"),
    os.path.join(BLENDER_DIR, "ll_blender", "bl_export.py"),
]


def main():
    errors = []
    for path in FILES:
        if not os.path.isfile(path):
            errors.append(f"Missing file: {path}")
            continue
        try:
            py_compile.compile(path, doraise=True)
            with open(path, "r", encoding="utf-8") as f:
                src = f.read()
            ast.parse(src, filename=path)
            for bad in FORBIDDEN_APIS:
                if bad in src:
                    errors.append(f"{os.path.basename(path)} contains removed Blender 4.x API '{bad}'")
        except Exception as ex:
            errors.append(f"{os.path.basename(path)}: {ex}")

    if errors:
        for e in errors:
            print("ERROR:", e)
        return 1
    print(f"fake_bpy_check: PASS ({len(FILES)} Blender pipeline files verified for syntax & Blender 4.2+ API)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
