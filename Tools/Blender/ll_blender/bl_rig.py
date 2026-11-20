"""Armature builder for humanoid (zombies/survivors) and FPS arms rigs in Blender 4.2+."""
import bpy  # type: ignore

from . import rig_spec as R


def build_armature(entry, mesh_obj):
    """Creates an `Armature` object from `rig_spec` and attaches `mesh_obj` via an Armature modifier."""
    if entry.rig == "none":
        return None

    spec = R.humanoid_bones(entry.scale) if entry.rig == "humanoid" else R.fps_bones()
    arm_data = bpy.data.armatures.new("Armature")
    arm_obj = bpy.data.objects.new("Armature", arm_data)
    bpy.context.collection.objects.link(arm_obj)

    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")

    ebones = arm_data.edit_bones
    created = {}
    for bname, binfo in spec.items():
        eb = ebones.new(bname)
        eb.head = binfo["head"]
        eb.tail = binfo["tail"]
        eb.roll = 0.0
        eb.use_connect = False
        eb.use_deform = True
        parent_name = binfo["parent"]
        if parent_name and parent_name in created:
            eb.parent = created[parent_name]
        created[bname] = eb

    bpy.ops.object.mode_set(mode="OBJECT")

    # Configure pose bones to Euler XYZ
    for pb in arm_obj.pose.bones:
        pb.rotation_mode = "XYZ"

    # Parent mesh to armature and attach Armature modifier
    mesh_obj.parent = arm_obj
    mod = mesh_obj.modifiers.new(name="Armature", type="ARMATURE")
    mod.object = arm_obj
    mod.use_vertex_groups = True

    return arm_obj
