"""UV verification, clamping, and optional Blender smart UV fallback."""
import bpy  # type: ignore

from . import atlas as A


def verify_and_fix_uvs(entry, geo, mesh_obj):
    """Validates UV coordinates on `mesh_obj` and clamps cell-atlas UVs inside their designated atlas cell.
    If `entry.params.get('smart_uv')` is True, runs `bpy.ops.uv.smart_project` first and maps into the cell."""
    mesh = mesh_obj.data
    if not mesh.uv_layers:
        return

    if entry.params.get("smart_uv", False):
        try:
            bpy.context.view_layer.objects.active = mesh_obj
            mesh_obj.select_set(True)
            bpy.ops.object.mode_set(mode="EDIT")
            bpy.ops.mesh.select_all(action="SELECT")
            bpy.ops.uv.smart_project(angle_limit=1.15192, island_margin=0.02)
            bpy.ops.object.mode_set(mode="OBJECT")
        except Exception:
            try:
                bpy.ops.object.mode_set(mode="OBJECT")
            except Exception:
                pass

    atlas_info = A.ATLASES[geo.atlas]
    if atlas_info["mode"] != "cell":
        return

    uv_layer = mesh.uv_layers.active
    if uv_layer is None:
        return
    uv_data = uv_layer.data

    for poly in mesh.polygons:
        f_idx = poly.index
        tag = geo.ftag[f_idx] if f_idx < len(geo.ftag) else atlas_info["tiles"][0]
        cell_idx = A.tile_index(geo.atlas, tag)
        cu, cv, du, dv = A.cell_rect(geo.atlas, cell_idx)
        u_min, u_max = cu + du * 0.02, cu + du * 0.98
        v_min, v_max = cv + dv * 0.02, cv + dv * 0.98
        for loop_idx in poly.loop_indices:
            u, v = uv_data[loop_idx].uv
            if u < u_min or u > u_max or v < v_min or v > v_max:
                u = max(u_min, min(u_max, u))
                v = max(v_min, min(v_max, v))
                uv_data[loop_idx].uv = (u, v)
