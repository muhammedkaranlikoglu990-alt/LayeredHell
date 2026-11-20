#!/usr/bin/env python3
"""Standalone pure-Python Binary glTF 2.0 (.glb) exporter for all 840+ models in CATALOG.

Allows generating all 840+ 3D mesh assets directly into `Assets/Art/Models/<Category>/<id>.glb`
on machines without Blender (`bpy`) installed, while `run_blender.py` provides full Blender 4.2+
FBX/GLB export with Armature + NLA Action baking when Blender is available.
"""
import json
import math
import os
import struct
import sys
import time
from multiprocessing import Pool, cpu_count

HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)

from ll_blender import atlas as A          # noqa: E402
from ll_blender import builders            # noqa: E402
from ll_blender import catalog             # noqa: E402


def _face_normal(p0, p1, p2):
    ux, uy, uz = p1[0] - p0[0], p1[1] - p0[1], p1[2] - p0[2]
    vx, vy, vz = p2[0] - p0[0], p2[1] - p0[1], p2[2] - p0[2]
    nx = uy * vz - uz * vy
    ny = uz * vx - ux * vz
    nz = ux * vy - uy * vx
    length = math.sqrt(nx * nx + ny * ny + nz * nz) or 1.0
    return (nx / length, ny / length, nz / length)


def build_glb_bytes(entry) -> bytes:
    """Converts `builders.build_geo(entry)` into a valid glTF 2.0 binary `.glb` buffer.
    Converts Blender coordinates (X right, -Y forward, Z up) to glTF/Unity Y-up coordinates:
        x_gltf = x_bl
        y_gltf = z_bl
        z_gltf = -y_bl
    and flips v -> 1.0 - v for glTF top-left UV convention.
    """
    g = builders.build_geo(entry)

    positions = []
    normals = []
    uv0s = []
    uv1s = []
    indices = []

    min_x = min_y = min_z = 1e9
    max_x = max_y = max_z = -1e9

    v_cursor = 0
    for f_idx, face in enumerate(g.faces):
        f_uv0 = g.fuv0[f_idx]
        f_uv1 = g.fuv1[f_idx]
        pts = []
        for vi in face:
            bx, by, bz = g.verts[vi]
            gx, gy, gz = bx, bz, -by
            pts.append((gx, gy, gz))
            if gx < min_x: min_x = gx
            if gy < min_y: min_y = gy
            if gz < min_z: min_z = gz
            if gx > max_x: max_x = gx
            if gy > max_y: max_y = gy
            if gz > max_z: max_z = gz

        nx, ny, nz = _face_normal(pts[0], pts[1], pts[2])

        for loop_k, (gx, gy, gz) in enumerate(pts):
            positions.extend((gx, gy, gz))
            normals.extend((nx, ny, nz))
            u0, v0 = f_uv0[loop_k]
            u1, v1 = f_uv1[loop_k]
            uv0s.extend((float(u0), float(1.0 - v0)))
            uv1s.extend((float(u1), float(1.0 - v1)))

        n_poly = len(pts)
        for k in range(1, n_poly - 1):
            indices.extend((v_cursor, v_cursor + k, v_cursor + k + 1))
        v_cursor += n_poly

    v_count = v_cursor
    i_count = len(indices)

    pos_bytes = struct.pack(f"<{v_count * 3}f", *positions)
    norm_bytes = struct.pack(f"<{v_count * 3}f", *normals)
    uv0_bytes = struct.pack(f"<{v_count * 2}f", *uv0s)
    uv1_bytes = struct.pack(f"<{v_count * 2}f", *uv1s)
    idx_bytes = struct.pack(f"<{i_count}H", *indices)
    if len(idx_bytes) % 4 != 0:
        idx_bytes += b"\x00" * (4 - (len(idx_bytes) % 4))

    off_pos = 0
    len_pos = len(pos_bytes)
    off_norm = off_pos + len_pos
    len_norm = len(norm_bytes)
    off_uv0 = off_norm + len_norm
    len_uv0 = len(uv0_bytes)
    off_uv1 = off_uv0 + len_uv0
    len_uv1 = len(uv1_bytes)
    off_idx = off_uv1 + len_uv1
    len_idx_raw = i_count * 2

    bin_blob = pos_bytes + norm_bytes + uv0_bytes + uv1_bytes + idx_bytes

    gltf = {
        "asset": {"version": "2.0", "generator": "LayeredHell45-ProceduralGLB"},
        "scene": 0,
        "scenes": [{"name": entry.id, "nodes": [0]}],
        "nodes": [{"name": entry.id, "mesh": 0}],
        "materials": [
            {
                "name": A.material_name(entry.atlas),
                "pbrMetallicRoughness": {
                    "baseColorFactor": [1.0, 1.0, 1.0, 1.0],
                    "metallicFactor": 0.1,
                    "roughnessFactor": 0.72,
                },
            }
        ],
        "meshes": [
            {
                "name": entry.id,
                "primitives": [
                    {
                        "attributes": {
                            "POSITION": 0,
                            "NORMAL": 1,
                            "TEXCOORD_0": 2,
                            "TEXCOORD_1": 3,
                        },
                        "indices": 4,
                        "material": 0,
                        "mode": 4,
                    }
                ],
            }
        ],
        "accessors": [
            {
                "bufferView": 0,
                "componentType": 5126,
                "count": v_count,
                "type": "VEC3",
                "min": [round(min_x, 5), round(min_y, 5), round(min_z, 5)],
                "max": [round(max_x, 5), round(max_y, 5), round(max_z, 5)],
            },
            {
                "bufferView": 1,
                "componentType": 5126,
                "count": v_count,
                "type": "VEC3",
            },
            {
                "bufferView": 2,
                "componentType": 5126,
                "count": v_count,
                "type": "VEC2",
            },
            {
                "bufferView": 3,
                "componentType": 5126,
                "count": v_count,
                "type": "VEC2",
            },
            {
                "bufferView": 4,
                "componentType": 5123,
                "count": i_count,
                "type": "SCALAR",
            },
        ],
        "bufferViews": [
            {"buffer": 0, "byteOffset": off_pos, "byteLength": len_pos, "target": 34962},
            {"buffer": 0, "byteOffset": off_norm, "byteLength": len_norm, "target": 34962},
            {"buffer": 0, "byteOffset": off_uv0, "byteLength": len_uv0, "target": 34962},
            {"buffer": 0, "byteOffset": off_uv1, "byteLength": len_uv1, "target": 34962},
            {"buffer": 0, "byteOffset": off_idx, "byteLength": len_idx_raw, "target": 34963},
        ],
        "buffers": [{"byteLength": len(bin_blob)}],
    }

    json_bytes = json.dumps(gltf, separators=(",", ":")).encode("utf-8")
    if len(json_bytes) % 4 != 0:
        json_bytes += b" " * (4 - (len(json_bytes) % 4))

    total_len = 12 + 8 + len(json_bytes) + 8 + len(bin_blob)
    header = struct.pack("<III", 0x46546C67, 2, total_len)
    json_chunk = struct.pack("<II", len(json_bytes), 0x4E4F534A) + json_bytes
    bin_chunk = struct.pack("<II", len(bin_blob), 0x004E4942) + bin_blob
    return header + json_chunk + bin_chunk


def _worker(args):
    entry, out_root = args
    cat_dir = os.path.join(out_root, entry.category)
    os.makedirs(cat_dir, exist_ok=True)
    glb_path = os.path.join(cat_dir, entry.id + ".glb")
    data = build_glb_bytes(entry)
    with open(glb_path, "wb") as f:
        f.write(data)
    return len(data)


def main():
    t0 = time.time()
    out_root = os.path.abspath(os.path.join(HERE, "..", "..", "Assets", "Art", "Models"))
    entries = list(catalog.CATALOG)
    if not entries:
        print("No entries in catalog.CATALOG yet.")
        return 1

    workers = max(1, min(cpu_count(), 6))
    tasks = [(e, out_root) for e in entries]
    with Pool(workers) as pool:
        sizes = pool.map(_worker, tasks, chunksize=24)

    dt = time.time() - t0
    total_mb = sum(sizes) / (1024.0 * 1024.0)
    print(f"[export_standalone_glb] Exported {len(entries)} 3D models (.glb) to {out_root} ({total_mb:.2f} MB in {dt:.1f}s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
