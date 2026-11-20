"""Pure-Python procedural animation pose generator for 45: Layered Hell.

Used by `bl_anim.py` to populate Blender Action keyframes, and tested standalone by
`Tools/Verify/test_anim_math.py` without requiring Blender (`bpy`).

Coordinate conventions (matching `rig_spec.py`):
- Rest pose faces -Y, Z is up, character's LEFT is +X, RIGHT is -X.
- Local bone Euler rotations `(rx, ry, rz)` in radians (`rotation_mode = 'XYZ'`):
  * For vertical trunk bones (Hips/Spine/Neck/Head):
    +rx pitches backward, -rx pitches forward (bowing/hunching),
    +ry rolls sideways, +rz twists yaw.
  * For downward-hanging legs (UpperLeg/LowerLeg):
    +rx swings leg forward (-Y), -rx swings leg backward (+Y).
  * For A-pose arms (UpperArm/LowerArm/Hand):
    -rx raises arm forward (-Y), +rx swings arm back (+Y),
    +ry / -ry abducts/adducts arm, +rz twists forearm.
- `pose(clip, style, bone, t, scale=1.0)` returns `((rx, ry, rz), (lx, ly, lz))`
  where `t` in `[0.0, 1.0]` is normalized clip time.
  For looping clips (`Idle, Walk, Run, FPS_Idle, FPS_Walk, FPS_Run`), `t=0.0` and `t=1.0`
  produce identical values (`sin(2*pi*t)`, `cos(2*pi*t)` harmonics).
"""
import math
from . import rig_spec as R

TAU = 2.0 * math.pi


def _clamp(x, a, b):
    return a if x < a else (b if x > b else x)


def _smoothstep(a, b, x):
    if x <= a:
        return 0.0
    if x >= b:
        return 1.0
    u = (x - a) / (b - a)
    return u * u * (3.0 - 2.0 * u)


def _pulse(t, t0, t1, t2):
    """Smooth 0 -> 1 -> 0 bell between t0..t1..t2."""
    if t <= t0 or t >= t2:
        return 0.0
    if t < t1:
        return _smoothstep(t0, t1, t)
    return 1.0 - _smoothstep(t1, t2, t)


def _style_base(style):
    """Returns baseline posture parameters per zombie/survivor style."""
    if style == "shamble":
        return dict(hunch=0.22, tilt=0.10, arm_fwd=0.75,asym=0.30, stride=0.55, bounce=0.035, knees=0.18)
    if style == "sprint":
        return dict(hunch=0.32, tilt=0.03, arm_fwd=0.45, asym=0.05, stride=0.95, bounce=0.065, knees=0.22)
    if style == "hunch":
        return dict(hunch=0.46, tilt=0.08, arm_fwd=0.55, asym=0.14, stride=0.50, bounce=0.030, knees=0.28)
    if style == "heavy":
        return dict(hunch=0.16, tilt=0.06, arm_fwd=0.40, asym=0.08, stride=0.48, bounce=0.025, knees=0.14)
    if style == "boss":
        return dict(hunch=0.26, tilt=0.09, arm_fwd=0.52, asym=0.22, stride=0.62, bounce=0.055, knees=0.20)
    # 'survivor' or default
    return dict(hunch=0.04, tilt=0.0, arm_fwd=0.12, asym=0.0, stride=0.65, bounce=0.030, knees=0.08)


def _humanoid_pose(clip, style, bone, t, scale=1.0):
    t = _clamp(float(t), 0.0, 1.0)
    st = _style_base(style)
    rx = ry = rz = 0.0
    lx = ly = lz = 0.0
    ph = TAU * t
    s1, c1 = math.sin(ph), math.cos(ph)
    s2, c2 = math.sin(2.0 * ph), math.cos(2.0 * ph)

    hunch = st["hunch"]
    tilt = st["tilt"]
    arm_fwd = st["arm_fwd"]
    asym = st["asym"]

    # ------------------------------------------------------------------ IDLE (loop)
    if clip == "Idle":
        if bone == "Hips":
            lz = -0.015 * scale * (1.0 - c1)
            ry = tilt * 0.4 + 0.025 * s1
        elif bone in ("Spine", "Spine1", "Spine2"):
            rx = -hunch * 0.35 - 0.03 * s1
            ry = tilt * 0.25 + 0.02 * c1
            rz = 0.02 * s1
        elif bone == "Neck":
            rx = -hunch * 0.15 + 0.04 * c1
            rz = 0.05 * s1
        elif bone == "Head":
            rx = 0.06 * s1 - 0.05
            ry = tilt * 0.3 + 0.04 * c1
            rz = 0.07 * s1
        elif bone in ("UpperArm_L", "UpperArm_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = -arm_fwd * (1.0 + 0.15 * side * asym) + 0.04 * s1 * side
            ry = 0.06 * side
        elif bone in ("LowerArm_L", "LowerArm_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = -0.35 - 0.15 * arm_fwd + 0.05 * c1 * side
        elif bone in ("UpperLeg_L", "UpperLeg_R"):
            rx = st["knees"] * 0.6
        elif bone in ("LowerLeg_L", "LowerLeg_R"):
            rx = -st["knees"] * 1.1

    # ------------------------------------------------------------------ WALK / RUN (loop)
    elif clip in ("Walk", "Run"):
        is_run = (clip == "Run")
        stride = st["stride"] * (1.35 if is_run else 0.85)
        bounce = st["bounce"] * (1.45 if is_run else 0.9) * scale
        lean = hunch * (1.25 if is_run else 1.0)

        if bone == "Hips":
            lz = bounce * s2 - 0.02 * scale
            lx = 0.025 * scale * s1
            rx = -0.06 * (1.0 if is_run else 0.5)
            ry = 0.05 * s1 + tilt * 0.35
            rz = 0.07 * s1
        elif bone in ("Spine", "Spine1", "Spine2"):
            rx = -lean * 0.35 + 0.04 * c2
            ry = tilt * 0.3 - 0.04 * s1
            rz = -0.06 * s1
        elif bone in ("Neck", "Head"):
            rx = 0.08 * lean + 0.05 * s2
            ry = tilt * 0.35 + 0.04 * c1
            rz = -0.04 * s1
        elif bone == "UpperLeg_L":
            rx = stride * s1 + st["knees"] * 0.4
        elif bone == "UpperLeg_R":
            drag = (1.0 - 0.35 * asym)
            rx = -stride * drag * s1 + st["knees"] * 0.4
        elif bone == "LowerLeg_L":
            bend = max(0.0, -c1) * stride * (1.25 if is_run else 0.95) + st["knees"] * 0.5
            rx = -bend
        elif bone == "LowerLeg_R":
            bend = max(0.0, c1) * stride * (1.25 if is_run else 0.95) * (1.0 - 0.3 * asym) + st["knees"] * 0.5
            rx = -bend
        elif bone == "Foot_L":
            rx = 0.20 * c1
        elif bone == "Foot_R":
            rx = -0.20 * c1
        elif bone == "UpperArm_L":
            if style in ("shamble", "hunch"):
                rx = -arm_fwd - 0.18 * s1
            else:
                rx = -arm_fwd * 0.5 - 0.45 * stride * s1
            ry = 0.08
        elif bone == "UpperArm_R":
            if style in ("shamble", "hunch"):
                rx = -arm_fwd * (1.0 - 0.25 * asym) + 0.18 * s1
            else:
                rx = -arm_fwd * 0.5 + 0.45 * stride * s1
            ry = -0.08
        elif bone in ("LowerArm_L", "LowerArm_R"):
            rx = -0.45 - (0.35 if is_run else 0.15)

    # ------------------------------------------------------------------ ATTACK / ATTACK2 (one-shot)
    elif clip in ("Attack", "Attack2"):
        wind = _pulse(t, 0.0, 0.28, 0.52)
        strike = _pulse(t, 0.25, 0.52, 0.88)
        heavy_mul = 1.35 if (style == "boss" or clip == "Attack2") else 1.0
        if bone == "Hips":
            ly = -0.14 * scale * strike + 0.06 * scale * wind
            lz = -0.05 * scale * strike
            rx = -0.12 * strike + 0.08 * wind
        elif bone in ("Spine", "Spine1", "Spine2"):
            rx = (0.16 * wind - 0.34 * strike) * heavy_mul - hunch * 0.25
            rz = (0.18 * wind - 0.26 * strike) * (1.0 if clip == "Attack" else 0.0)
        elif bone in ("Neck", "Head"):
            rx = -0.18 * strike + 0.12 * wind
        elif bone == "UpperArm_R":
            rx = -1.55 * wind - 1.05 * strike
            ry = -0.25 * wind + 0.15 * strike
            rz = -0.30 * strike
        elif bone == "LowerArm_R":
            rx = -0.85 * wind - 0.35 * strike
        elif bone == "UpperArm_L":
            if clip == "Attack2" or style in ("boss", "shamble"):
                rx = -1.45 * wind - 1.00 * strike
                ry = 0.25 * wind - 0.15 * strike
            else:
                rx = -0.55 * wind - 0.40 * strike
        elif bone == "LowerArm_L":
            rx = -0.70 * wind - 0.30 * strike
        elif bone in ("UpperLeg_L", "UpperLeg_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = 0.25 * strike * side
        elif bone in ("LowerLeg_L", "LowerLeg_R"):
            rx = -0.25 * strike

    # ------------------------------------------------------------------ HIT (stagger back, one-shot)
    elif clip == "Hit":
        k = _pulse(t, 0.0, 0.28, 1.0)
        if bone == "Hips":
            ly = 0.10 * scale * k
            lz = -0.04 * scale * k
            rx = 0.18 * k
        elif bone in ("Spine", "Spine1", "Spine2"):
            rx = 0.26 * k - hunch * 0.2
            ry = 0.14 * k
            rz = -0.18 * k
        elif bone in ("Neck", "Head"):
            rx = 0.35 * k
            rz = 0.22 * k
        elif bone in ("UpperArm_L", "UpperArm_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = -0.50 * k
            ry = 0.35 * side * k
        elif bone in ("LowerArm_L", "LowerArm_R"):
            rx = -0.45 * k

    # ------------------------------------------------------------------ DEATH (collapse & hold, one-shot)
    elif clip == "Death":
        kneel = _smoothstep(0.0, 0.48, t)
        fall = _smoothstep(0.32, 0.88, t)
        if bone == "Hips":
            lz = (-0.38 * kneel - 0.47 * fall) * scale
            ly = -0.35 * fall * scale
            rx = -0.35 * kneel - 1.12 * fall
            ry = 0.12 * fall
            rz = 0.15 * kneel
        elif bone in ("Spine", "Spine1", "Spine2"):
            rx = -0.20 * kneel + 0.10 * fall
            ry = 0.08 * fall
        elif bone in ("Neck", "Head"):
            rx = 0.25 * fall - 0.20 * kneel
            rz = 0.30 * fall
        elif bone in ("UpperLeg_L", "UpperLeg_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = 0.85 * kneel * (1.0 - 0.7 * fall) + 0.12 * side * fall
        elif bone in ("LowerLeg_L", "LowerLeg_R"):
            rx = -1.25 * kneel * (1.0 - 0.65 * fall)
        elif bone in ("UpperArm_L", "UpperArm_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = -0.75 * kneel - 0.40 * fall
            ry = 0.45 * side * fall
        elif bone in ("LowerArm_L", "LowerArm_R"):
            rx = -0.55 * kneel * (1.0 - 0.5 * fall)

    # ------------------------------------------------------------------ SCREAM (roar, one-shot)
    elif clip == "Scream":
        roar = _pulse(t, 0.0, 0.35, 1.0)
        vib = math.sin(t * TAU * 4.0) * 0.04 * roar
        if bone == "Hips":
            lz = -0.05 * scale * roar
        elif bone in ("Spine", "Spine1", "Spine2"):
            rx = 0.22 * roar + vib
        elif bone in ("Neck", "Head"):
            rx = 0.38 * roar + vib * 1.5
            rz = math.sin(t * TAU * 2.0) * 0.12 * roar
        elif bone in ("UpperArm_L", "UpperArm_R"):
            side = 1.0 if bone.endswith("_L") else -1.0
            rx = -0.85 * roar
            ry = 0.55 * side * roar
        elif bone in ("LowerArm_L", "LowerArm_R"):
            rx = -0.65 * roar

    # ------------------------------------------------------------------ DOOR OPEN (reach & pull)
    elif clip == "DoorOpen":
        reach = _pulse(t, 0.0, 0.38, 0.75)
        pull = _pulse(t, 0.35, 0.68, 1.0)
        if bone in ("Spine", "Spine1", "Spine2"):
            rx = -0.10 * reach + 0.06 * pull
            rz = -0.12 * reach + 0.14 * pull
        elif bone == "UpperArm_R":
            rx = -1.15 * reach - 0.45 * pull
            ry = -0.15 * reach
        elif bone == "LowerArm_R":
            rx = -0.35 * reach - 0.85 * pull
        elif bone == "Hand_R":
            rz = -0.45 * pull

    return ((rx, ry, rz), (lx, ly, lz))


def _fps_pose(clip, bone, t):
    """First-person arms + weapon bone procedural animation."""
    t = _clamp(float(t), 0.0, 1.0)
    rx = ry = rz = 0.0
    lx = ly = lz = 0.0
    ph = TAU * t
    s1, c1 = math.sin(ph), math.cos(ph)
    s2, c2 = math.sin(2.0 * ph), math.cos(2.0 * ph)

    if clip == "FPS_Idle":
        if bone == "Arms_Root":
            lz = 0.006 * s1
            lx = 0.003 * c1
            rx = 0.015 * s1
        elif bone in ("Hand_R", "Weapon"):
            rx = 0.01 * c1

    elif clip in ("FPS_Walk", "FPS_Run"):
        amp = 1.85 if clip == "FPS_Run" else 1.0
        if bone == "Arms_Root":
            lx = 0.018 * amp * s1
            lz = -0.014 * amp * abs(c1) + 0.007 * amp * c2
            rx = -0.05 * (amp - 1.0) + 0.03 * amp * c2
            rz = 0.035 * amp * s1
        elif bone == "UpperArm_L":
            rx = 0.05 * amp * s1
        elif bone == "UpperArm_R":
            rx = -0.05 * amp * s1

    elif clip == "FPS_Fire":
        kick = _pulse(t, 0.0, 0.18, 1.0)
        if bone == "Arms_Root":
            ly = 0.045 * kick
            lz = 0.015 * kick
            rx = 0.18 * kick
        elif bone in ("Hand_R", "Weapon"):
            rx = 0.22 * kick
            ly = 0.02 * kick

    elif clip == "FPS_Reload":
        tilt_up = _pulse(t, 0.0, 0.20, 0.90)
        mag_out = _pulse(t, 0.12, 0.35, 0.55)
        mag_in = _pulse(t, 0.48, 0.68, 0.85)
        rack = _pulse(t, 0.72, 0.86, 0.98)
        if bone == "Arms_Root":
            rx = 0.22 * tilt_up - 0.12 * rack
            rz = -0.25 * tilt_up
            lz = -0.03 * mag_out + 0.02 * mag_in
        elif bone == "UpperArm_L":
            rx = 0.55 * mag_out - 0.25 * mag_in - 0.35 * rack
            ry = 0.25 * mag_out
        elif bone == "LowerArm_L":
            rx = 0.65 * mag_out - 0.45 * mag_in - 0.50 * rack
            lz = -0.10 * mag_out
        elif bone == "Hand_L":
            ly = 0.06 * rack

    elif clip == "FPS_Draw":
        raise_k = 1.0 - _smoothstep(0.0, 0.85, t)
        overshoot = _pulse(t, 0.65, 0.85, 1.0)
        if bone == "Arms_Root":
            lz = -0.24 * raise_k + 0.015 * overshoot
            rx = -0.65 * raise_k + 0.08 * overshoot

    elif clip == "FPS_DoorOpen":
        reach = _pulse(t, 0.0, 0.40, 0.80)
        pull = _pulse(t, 0.35, 0.70, 1.0)
        if bone == "UpperArm_L":
            rx = -0.65 * reach - 0.25 * pull
        elif bone == "LowerArm_L":
            rx = -0.30 * reach - 0.55 * pull
        elif bone == "Hand_L":
            rz = -0.40 * pull

    elif clip == "FPS_Melee":
        wind = _pulse(t, 0.0, 0.28, 0.50)
        slash = _pulse(t, 0.26, 0.52, 0.92)
        if bone == "Arms_Root":
            rx = 0.35 * wind - 0.75 * slash
            rz = -0.45 * wind + 0.65 * slash
            ly = -0.08 * slash + 0.04 * wind
        elif bone in ("UpperArm_R", "LowerArm_R"):
            rx = 0.25 * wind - 0.55 * slash

    return ((rx, ry, rz), (lx, ly, lz))


def pose(clip, style, bone, t, scale=1.0):
    """Unified pose entry point for both humanoid and FPS arms rigs."""
    if clip in R.FPS_CLIPS or style == "fps" or bone in ("Arms_Root", "Weapon"):
        return _fps_pose(clip, bone, t)
    return _humanoid_pose(clip, style, bone, t, scale=scale)
