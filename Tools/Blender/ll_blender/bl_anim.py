"""Procedural Action keyframing & NLA strip builder for Blender 4.2 / 4.4 / 5.0+."""
import bpy  # type: ignore

from . import rig_spec as R
from . import bl_anim_math as AM


def _assign_action(arm_obj, action):
    """Assigns `action` to `arm_obj.animation_data`, supporting Blender 4.4+ slotted actions if present."""
    if arm_obj.animation_data is None:
        arm_obj.animation_data_create()
    ad = arm_obj.animation_data
    ad.action = action
    # Blender 4.4+ slotted action compatibility
    if hasattr(action, "slots") and hasattr(ad, "action_slot"):
        try:
            if len(action.slots) == 0:
                slot = action.slots.new(id_type="OBJECT", name=arm_obj.name)
            else:
                slot = action.slots[0]
            ad.action_slot = slot
        except Exception:
            pass


def build_animations(entry, arm_obj):
    """Creates all Actions for `entry` (`HUMANOID_CLIPS` or `FPS_CLIPS`), populates keyframes
    using `bl_anim_math.pose`, and pushes each Action to an NLA track so FBX/GLB export includes all clips."""
    if arm_obj is None or entry.rig == "none":
        return []

    clips = R.HUMANOID_CLIPS if entry.rig == "humanoid" else R.FPS_CLIPS
    bones = R.HUMANOID_BONES if entry.rig == "humanoid" else R.FPS_BONES
    style = entry.anim_style if entry.anim_style != "none" else ("fps" if entry.rig == "fps_arms" else "shamble")

    bpy.context.scene.render.fps = R.FPS
    created_actions = []

    if arm_obj.animation_data is None:
        arm_obj.animation_data_create()

    # Step size: sample every 2 frames plus exact start and end frame (fast & smooth)
    for clip_name, (num_frames, is_loop) in clips.items():
        action = bpy.data.actions.new(name=clip_name)
        action.use_fake_user = True
        _assign_action(arm_obj, action)

        frame_list = list(range(1, num_frames + 1, 2))
        if frame_list[-1] != num_frames:
            frame_list.append(num_frames)

        denom = float(max(1, num_frames - 1))
        pbones = arm_obj.pose.bones

        for f in frame_list:
            t = (f - 1) / denom
            if is_loop and f == num_frames:
                t = 1.0
            for bname in bones:
                pb = pbones.get(bname)
                if pb is None:
                    continue
                rot, loc = AM.pose(clip_name, style, bname, t, scale=entry.scale)
                pb.rotation_euler = rot
                pb.keyframe_insert(data_path="rotation_euler", frame=f)
                if bname in ("Hips", "Arms_Root", "Hand_R", "Hand_L", "LowerArm_L", "Weapon"):
                    pb.location = loc
                    pb.keyframe_insert(data_path="location", frame=f)

        try:
            action.frame_range = (1, num_frames)
        except Exception:
            pass

        created_actions.append((clip_name, action, num_frames))

    # Reset pose to rest and push all actions into NLA tracks for multi-clip export
    for pb in arm_obj.pose.bones:
        pb.rotation_euler = (0.0, 0.0, 0.0)
        pb.location = (0.0, 0.0, 0.0)

    ad = arm_obj.animation_data
    for clip_name, action, num_frames in created_actions:
        try:
            track = ad.nla_tracks.new()
            track.name = clip_name
            strip = track.strips.new(clip_name, 1, action)
            strip.name = clip_name
            strip.action_frame_start = 1
            strip.action_frame_end = num_frames
            track.mute = False
        except Exception:
            pass

    if created_actions:
        _assign_action(arm_obj, created_actions[0][1])

    return [a[0] for a in created_actions]
