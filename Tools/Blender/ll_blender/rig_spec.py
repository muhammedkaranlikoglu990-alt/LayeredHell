"""Skeleton + animation clip specification (pure python, shared by geometry builders and bpy rigging).

Humanoid rest pose = A-pose, model faces -Y, character's LEFT side is +X, height 1.8 m * scale.
Zombie/NPC builders attach every body segment to a bone with `bone=` so the bpy layer can skin rigidly
(vertex group per bone, weight 1.0) - robust in background mode and perfect for low-poly mobile characters.
"""
import math
from collections import OrderedDict

FPS = 30

HUMANOID_BONES = ["Hips", "Spine", "Spine1", "Spine2", "Neck", "Head",
                  "Shoulder_L", "UpperArm_L", "LowerArm_L", "Hand_L",
                  "Shoulder_R", "UpperArm_R", "LowerArm_R", "Hand_R",
                  "UpperLeg_L", "LowerLeg_L", "Foot_L",
                  "UpperLeg_R", "LowerLeg_R", "Foot_R"]

FPS_BONES = ["Arms_Root", "UpperArm_L", "LowerArm_L", "Hand_L",
             "UpperArm_R", "LowerArm_R", "Hand_R", "Weapon"]

# name -> (frame_count, loop)
HUMANOID_CLIPS = OrderedDict([
    ("Idle", (60, True)), ("Walk", (32, True)), ("Run", (20, True)),
    ("Attack", (24, False)), ("Attack2", (28, False)), ("Hit", (14, False)),
    ("Death", (48, False)), ("Scream", (45, False)), ("DoorOpen", (36, False)),
])
FPS_CLIPS = OrderedDict([
    ("FPS_Idle", (90, True)), ("FPS_Walk", (36, True)), ("FPS_Run", (24, True)),
    ("FPS_Fire", (10, False)), ("FPS_Reload", (75, False)), ("FPS_Draw", (24, False)),
    ("FPS_DoorOpen", (36, False)), ("FPS_Melee", (22, False)),
])


def _v(a, b): return (a[0] + b[0], a[1] + b[1], a[2] + b[2])
def _s(a, k): return (a[0] * k, a[1] * k, a[2] * k)


def humanoid_bones(scale=1.0):
    """OrderedDict name -> dict(head, tail, parent). Parents always precede children."""
    b = OrderedDict()

    def add(name, head, tail, parent):
        b[name] = dict(head=_s(head, scale), tail=_s(tail, scale), parent=parent)

    add("Hips", (0, 0, .95), (0, 0, 1.05), None)
    add("Spine", (0, 0, 1.05), (0, 0, 1.20), "Hips")
    add("Spine1", (0, 0, 1.20), (0, 0, 1.35), "Spine")
    add("Spine2", (0, 0, 1.35), (0, 0, 1.50), "Spine1")
    add("Neck", (0, 0, 1.50), (0, 0, 1.60), "Spine2")
    add("Head", (0, 0, 1.60), (0, 0, 1.80), "Neck")
    for side, sx in (("L", 1.0), ("R", -1.0)):
        sh_h = (0.04 * sx, 0, 1.48)
        sh_t = (0.19 * sx, 0, 1.48)
        add("Shoulder_" + side, sh_h, sh_t, "Spine2")
        d_up = (math.sin(math.radians(22)) * sx, -0.02, -math.cos(math.radians(22)))
        e = _v(sh_t, _s(d_up, 0.30))
        add("UpperArm_" + side, sh_t, e, "Shoulder_" + side)
        d_lo = (math.sin(math.radians(14)) * sx, -0.10, -math.cos(math.radians(14)))
        w = _v(e, _s(d_lo, 0.27))
        add("LowerArm_" + side, e, w, "UpperArm_" + side)
        hnd = _v(w, _s(d_lo, 0.10))
        add("Hand_" + side, w, hnd, "LowerArm_" + side)
    for side, sx in (("L", 1.0), ("R", -1.0)):
        add("UpperLeg_" + side, (0.09 * sx, 0, .95), (0.09 * sx, 0, .52), "Hips")
        add("LowerLeg_" + side, (0.09 * sx, 0, .52), (0.09 * sx, 0, .10), "UpperLeg_" + side)
        add("Foot_" + side, (0.09 * sx, 0, .10), (0.09 * sx, -0.18, .03), "LowerLeg_" + side)
    # re-order so L/R arms come in declared HUMANOID_BONES order is not required, parents precede children.
    return b


def fps_bones():
    """First-person arms rig. Origin == camera position, arms extend toward -Y (forward)."""
    b = OrderedDict()

    def add(name, head, tail, parent):
        b[name] = dict(head=head, tail=tail, parent=parent)

    add("Arms_Root", (0, 0, -0.35), (0, -0.08, -0.35), None)
    add("UpperArm_R", (-0.22, 0.05, -0.20), (-0.18, -0.20, -0.38), "Arms_Root")
    add("LowerArm_R", (-0.18, -0.20, -0.38), (-0.10, -0.45, -0.32), "UpperArm_R")
    add("Hand_R", (-0.10, -0.45, -0.32), (-0.06, -0.58, -0.28), "LowerArm_R")
    add("Weapon", (-0.06, -0.55, -0.28), (-0.06, -0.75, -0.28), "Hand_R")
    add("UpperArm_L", (0.22, 0.05, -0.20), (0.18, -0.20, -0.40), "Arms_Root")
    add("LowerArm_L", (0.18, -0.20, -0.40), (0.09, -0.50, -0.31), "UpperArm_L")
    add("Hand_L", (0.09, -0.50, -0.31), (0.05, -0.64, -0.27), "LowerArm_L")
    return b


def joint(name, scale=1.0, end="head"):
    """Convenience: position of a humanoid joint (bone head/tail) for builders."""
    return humanoid_bones(scale)[name][end]
