"""FBX and GLB exporters configured for Unity 6 URP (`axis_forward='-Z'`, `axis_up='Y'`, 1 unit = 1m)."""
import os
import bpy  # type: ignore


def export_model(entry, out_dir, fmt="fbx"):
    """Exports the active scene model (`Mesh_<id>` and optional `Armature`) to `<out_dir>/<Category>/<id>.fbx`
    and/or `.glb`."""
    cat_dir = os.path.join(out_dir, entry.category)
    os.makedirs(cat_dir, exist_ok=True)
    exported_paths = []
    has_rig = (entry.rig != "none")

    if fmt in ("fbx", "both"):
        fbx_path = os.path.join(cat_dir, entry.id + ".fbx")
        obj_types = {"ARMATURE", "MESH"} if has_rig else {"MESH"}
        bpy.ops.export_scene.fbx(
            filepath=fbx_path,
            use_selection=False,
            global_scale=1.0,
            apply_unit_scale=True,
            apply_scale_options="FBX_SCALE_ALL",
            axis_forward="-Z",
            axis_up="Y",
            object_types=obj_types,
            use_mesh_modifiers=True,
            mesh_smooth_type="FACE",
            use_tspace=False,
            use_armature_deform_only=True,
            add_leaf_bones=False,
            primary_bone_axis="Y",
            secondary_bone_axis="X",
            bake_anim=has_rig,
            bake_anim_use_all_bones=True,
            bake_anim_use_nla_strips=False,
            bake_anim_use_all_actions=has_rig,
            bake_anim_force_startend_keying=True,
            bake_anim_step=1.0,
            bake_anim_simplify_factor=0.5,
            path_mode="STRIP",
        )
        exported_paths.append(fbx_path)

    if fmt in ("glb", "both"):
        glb_path = os.path.join(cat_dir, entry.id + ".glb")
        kwargs = dict(
            filepath=glb_path,
            export_format="GLB",
            export_yup=True,
            export_apply=False,
            export_animations=has_rig,
        )
        try:
            bpy.ops.export_scene.gltf(**kwargs, export_animation_mode="ACTIONS")
        except TypeError:
            bpy.ops.export_scene.gltf(**kwargs)
        exported_paths.append(glb_path)

    return exported_paths
