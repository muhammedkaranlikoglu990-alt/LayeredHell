"""Blender scene cleanup, Geo -> bpy.types.Mesh conversion, vertex groups, and material setup.

Compatible with Blender 4.0 / 4.2 LTS / 4.4 / 5.0+.
"""
import os
import bpy  # type: ignore

from . import atlas as A


def clear_scene():
    """Purge all objects, meshes, armatures, actions, materials, and images so memory stays flat across 840+ models."""
    try:
        if bpy.context.active_object and bpy.context.active_object.mode != "OBJECT":
            bpy.ops.object.mode_set(mode="OBJECT")
    except Exception:
        pass

    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for block_coll in (bpy.data.meshes, bpy.data.armatures, bpy.data.actions,
                       bpy.data.materials, bpy.data.images, bpy.data.lights, bpy.data.cameras):
        for block in list(block_coll):
            try:
                block_coll.remove(block)
            except Exception:
                pass


def build_material(atlas_name, textures_dir=None):
    """Create material `M_<ATLAS>` with Principled BSDF connected to `<ATLAS>.png` if present."""
    mat_name = A.material_name(atlas_name)
    mat = bpy.data.materials.get(mat_name)
    if mat is not None:
        return mat

    mat = bpy.data.materials.new(name=mat_name)
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = None
    for n in nt.nodes:
        if n.type == "BSDF_PRINCIPLED":
            bsdf = n
            break
    if bsdf is None:
        bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")

    if "Roughness" in bsdf.inputs:
        bsdf.inputs["Roughness"].default_value = 0.72

    tex_node = nt.nodes.new("ShaderNodeTexImage")
    tex_node.interpolation = "Linear"
    if textures_dir:
        png_path = os.path.abspath(os.path.join(textures_dir, A.texture_filename(atlas_name)))
        if os.path.isfile(png_path):
            try:
                tex_node.image = bpy.data.images.load(png_path, check_existing=True)
            except Exception:
                pass

    if "Base Color" in bsdf.inputs and "Color" in tex_node.outputs:
        nt.links.new(tex_node.outputs["Color"], bsdf.inputs["Base Color"])
    return mat


def geo_to_mesh_object(entry, geo, textures_dir=None):
    """Convert a `Geo` container into a Blender Mesh Object with UV0, optional UV1 (`UVMap_Cell`),
    rigid bone vertex groups (weight 1.0), and material `M_<ATLAS>`."""
    mesh_name = "Mesh_" + entry.id
    mesh = bpy.data.meshes.new(mesh_name)
    mesh.from_pydata(geo.verts, [], geo.faces)
    mesh.update()

    # UV0 (UVMap)
    uv0_layer = mesh.uv_layers.new(name="UVMap")
    mesh.uv_layers.active = uv0_layer
    uv0_data = uv0_layer.data

    # UV1 (UVMap_Cell) - holds cell origin for 2K tiling atlases
    is_tile = (A.ATLASES[geo.atlas]["mode"] == "tile")
    uv1_data = None
    if is_tile:
        uv1_layer = mesh.uv_layers.new(name="UVMap_Cell")
        uv1_data = uv1_layer.data

    for poly in mesh.polygons:
        f_idx = poly.index
        u0_list = geo.fuv0[f_idx]
        u1_list = geo.fuv1[f_idx]
        for k, loop_idx in enumerate(poly.loop_indices):
            uv0_data[loop_idx].uv = u0_list[k]
            if uv1_data is not None:
                uv1_data[loop_idx].uv = u1_list[k]

    mesh.validate(clean_customdata=False)
    mesh.update()

    obj = bpy.data.objects.new(mesh_name, mesh)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)

    # Material assignment
    mat = build_material(geo.atlas, textures_dir=textures_dir)
    if len(mesh.materials) == 0:
        mesh.materials.append(mat)
    else:
        mesh.materials[0] = mat

    # Rigid vertex groups for rigged entries
    if entry.rig != "none":
        bone_to_verts = {}
        for v_idx, bname in enumerate(geo.vbone):
            if bname:
                bone_to_verts.setdefault(bname, []).append(v_idx)
        for bname, v_indices in bone_to_verts.items():
            vg = obj.vertex_groups.get(bname) or obj.vertex_groups.new(name=bname)
            vg.add(v_indices, 1.0, "REPLACE")

    return obj
