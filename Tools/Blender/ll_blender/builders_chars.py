"""Procedural 3D builders for Characters (27 entries) and Zombies (30 entries)."""
import math

from . import palettes as P
from . import rig_spec as R
from .builders_common import (
    Tags, ball, bx, front_label, geo_for, mk, obox, pick, prism, register, tube,
)

BUILDERS = {}


def _mid(a, b, t=0.5):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t)


def _humanoid_base(g, bones, s, tags, bulk=1.0, hunch=0.0):
    """Build articulated low-poly humanoid body bound to all 20 HUMANOID_BONES."""
    t_skin = tags["skin"]
    t_face = tags["face"]
    t_hair = tags["hair"]
    t_top = tags["top"]
    t_Bot = tags["bottom"]
    t_hand = tags["hand"]
    t_boot = tags["boot"]
    t_acc = tags["accent"]

    # Hips / Pelvis
    h0, h1 = bones["Hips"]["head"], bones["Hips"]["tail"]
    bx(g, -0.145 * s * bulk, -0.10 * s * bulk, h0[2] - 0.04 * s,
       0.145 * s * bulk, 0.10 * s * bulk, h1[2] + 0.02 * s, t_Bot, bone="Hips")

    # Spine segments
    sp0, sp1 = bones["Spine"]["head"], bones["Spine"]["tail"]
    bx(g, -0.14 * s * bulk, -0.095 * s * bulk - hunch * 0.02, sp0[2],
       0.14 * s * bulk, 0.095 * s * bulk, sp1[2] + 0.01 * s, t_top, bone="Spine")

    s10, s11 = bones["Spine1"]["head"], bones["Spine1"]["tail"]
    bx(g, -0.155 * s * bulk, -0.105 * s * bulk - hunch * 0.04, s10[2],
       0.155 * s * bulk, 0.10 * s * bulk - hunch * 0.02, s11[2] + 0.01 * s, t_top, bone="Spine1")

    s20, s21 = bones["Spine2"]["head"], bones["Spine2"]["tail"]
    bx(g, -0.17 * s * bulk, -0.11 * s * bulk - hunch * 0.06, s20[2],
       0.17 * s * bulk, 0.105 * s * bulk - hunch * 0.03, s21[2] + 0.01 * s, t_top, bone="Spine2")

    # Neck
    n0, n1 = bones["Neck"]["head"], bones["Neck"]["tail"]
    tube(g, (n0[0], n0[1] - hunch * 0.06, n0[2]),
         (n1[0], n1[1] - hunch * 0.08, n1[2]),
         0.055 * s * bulk, 0.050 * s * bulk, t_skin, seg=6, bone="Neck")

    # Head + painted front face portrait
    hd0, hd1 = bones["Head"]["head"], bones["Head"]["tail"]
    hcx = hd0[0]
    hcy = hd0[1] - hunch * 0.09
    hcz = (hd0[2] + hd1[2]) * 0.5
    hw, hd, hh = 0.18 * s, 0.19 * s, 0.21 * s
    obox(g, (hcx, hcy, hcz), (hw, hd, hh), t_skin, bone="Head")
    front_label(g, (hcx, hcy, hcz), (hw * 0.94, hd, hh * 0.94), t_face, face="front", bone="Head")
    if t_hair:
        # Hair cap on top & back of head
        bx(g, hcx - hw * 0.52, hcy - hd * 0.48, hcz + hh * 0.36,
           hcx + hw * 0.52, hcy + hd * 0.54, hcz + hh * 0.56, t_hair, bone="Head")
        bx(g, hcx - hw * 0.52, hcy + hd * 0.20, hcz - hh * 0.18,
           hcx + hw * 0.52, hcy + hd * 0.55, hcz + hh * 0.40, t_hair, bone="Head")

    # Arms (Shoulder, UpperArm, LowerArm, Hand)
    for side in ("L", "R"):
        b_sh = "Shoulder_" + side
        b_ua = "UpperArm_" + side
        b_la = "LowerArm_" + side
        b_hn = "Hand_" + side
        sh0, sh1 = bones[b_sh]["head"], bones[b_sh]["tail"]
        ua0, ua1 = bones[b_ua]["head"], bones[b_ua]["tail"]
        la0, la1 = bones[b_la]["head"], bones[b_la]["tail"]
        hn0, hn1 = bones[b_hn]["head"], bones[b_hn]["tail"]

        tube(g, sh0, sh1, 0.065 * s * bulk, 0.060 * s * bulk, t_top, seg=6, bone=b_sh)
        tube(g, ua0, ua1, 0.058 * s * bulk, 0.048 * s * bulk, t_top, seg=6, bone=b_ua)
        tube(g, la0, la1, 0.046 * s * bulk, 0.036 * s * bulk, t_acc, seg=6, bone=b_la)
        tube(g, hn0, hn1, 0.036 * s * bulk, 0.028 * s * bulk, t_hand, seg=6, bone=b_hn)

    # Legs (UpperLeg, LowerLeg, Foot)
    for side, sx in (("L", 1.0), ("R", -1.0)):
        b_ul = "UpperLeg_" + side
        b_ll = "LowerLeg_" + side
        b_ft = "Foot_" + side
        ul0, ul1 = bones[b_ul]["head"], bones[b_ul]["tail"]
        ll0, ll1 = bones[b_ll]["head"], bones[b_ll]["tail"]
        ft0, ft1 = bones[b_ft]["head"], bones[b_ft]["tail"]

        tube(g, ul0, ul1, 0.078 * s * bulk, 0.058 * s * bulk, t_Bot, seg=6, bone=b_ul)
        tube(g, ll0, ll1, 0.056 * s * bulk, 0.044 * s * bulk, t_Bot, seg=6, bone=b_ll)
        # Boot box sitting flat on floor (z >= 0.0)
        bx(g, ft0[0] - 0.052 * s * bulk, ft1[1] - 0.03 * s, 0.0,
           ft0[0] + 0.052 * s * bulk, ft0[1] + 0.05 * s, ft0[2] + 0.02 * s, t_boot, bone=b_ft)


@register(BUILDERS)
def chr_fps_arms(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    bones = R.fps_bones()

    t_sleeve = T("sleeve", "cloth_jacket_orange" if v == 1 else "cloth_overalls")
    t_skin = T("skin", "skin_tan" if v == 1 else "skin_light")
    t_glove = T("glove", "glove_leather")
    t_watch = T("watch", "boot_black")

    # Root collar anchor geometry (assigned to Arms_Root)
    r0, r1 = bones["Arms_Root"]["head"], bones["Arms_Root"]["tail"]
    tube(g, r0, r1, 0.035, 0.030, t_sleeve, seg=6, bone="Arms_Root")

    for side in ("L", "R"):
        b_ua = "UpperArm_" + side
        b_la = "LowerArm_" + side
        b_hn = "Hand_" + side
        ua0, ua1 = bones[b_ua]["head"], bones[b_ua]["tail"]
        la0, la1 = bones[b_la]["head"], bones[b_la]["tail"]
        hn0, hn1 = bones[b_hn]["head"], bones[b_hn]["tail"]

        # Upper arm sleeve
        tube(g, ua0, ua1, 0.055, 0.048, t_sleeve, seg=8, bone=b_ua)
        # Forearm (jacket sleeve + cuff or rolled sleeve + bare skin)
        la_mid = _mid(la0, la1, 0.65 if v == 1 else 0.35)
        tube(g, la0, la_mid, 0.048, 0.042, t_sleeve, seg=8, bone=b_la)
        tube(g, la_mid, la1, 0.040, 0.033, t_skin, seg=8, bone=b_la)
        if side == "L":
            # Survivor wristwatch on left wrist
            w0 = _mid(la0, la1, 0.82)
            w1 = _mid(la0, la1, 0.92)
            tube(g, w0, w1, 0.038, 0.038, t_watch, seg=8, bone=b_la)

        # Hand palm + thumb + fingers
        h_mid = _mid(hn0, hn1, 0.55)
        tube(g, hn0, h_mid, 0.035, 0.032, t_glove, seg=6, squash=(1.2, 0.7), bone=b_hn)
        tube(g, h_mid, hn1, 0.030, 0.022, t_skin if v == 2 else t_glove, seg=6, squash=(1.1, 0.6), bone=b_hn)

    # Weapon bone grip mount (small receiver anchor bound to Weapon bone)
    w0, w1 = bones["Weapon"]["head"], bones["Weapon"]["tail"]
    tube(g, w0, _mid(w0, w1, 0.35), 0.014, 0.012, t_watch, seg=6, bone="Weapon")
    return g


@register(BUILDERS)
def chr_survivor(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    s = float(params.get("_scale", 1.0))
    v = int(params.get("variant", 1))
    role = params.get("role", "npc")
    bones = R.humanoid_bones(s)

    tags = dict(
        skin=T("skin", "skin_tan"),
        face=T("face", "face_aras"),
        hair=T("hair", "hair_black"),
        top=T("top", "cloth_jacket_orange"),
        bottom=T("bottom", "cloth_jeans"),
        hand=T("hand", "glove_leather"),
        boot=T("boot", "boot_black"),
        accent=T("accent", "cloth_jacket_orange"),
    )
    bulk = 1.06 if role in ("kemal", "tarik") or (v % 5 == 0) else (0.94 if role in ("selin", "elif", "nehir") else 1.0)
    _humanoid_base(g, bones, s, tags, bulk=bulk, hunch=0.0)

    # Character-specific visual silhouette props (jacket collar, backpack, labcoat skirt, toolbelt, vest)
    s20, s21 = bones["Spine2"]["head"], bones["Spine2"]["tail"]
    sp0, _ = bones["Spine"]["head"], bones["Spine"]["tail"]
    hd0, hd1 = bones["Head"]["head"], bones["Head"]["tail"]

    if role == "aras" or (v % 4 == 1):
        # High jacket collar + survivor backpack on Spine2
        bx(g, -0.13 * s, 0.08 * s, s20[2] - 0.10 * s, 0.13 * s, 0.22 * s, s21[2] + 0.02 * s,
           T("pack", "glove_leather"), bone="Spine2")
        bx(g, -0.10 * s, -0.06 * s, s21[2] - 0.02 * s, 0.10 * s, 0.09 * s, s21[2] + 0.05 * s,
           tags["top"], bone="Neck")
    if role in ("selin", "elif") or (v % 3 == 2):
        # Labcoat lower skirt panels attached to Hips + pocket badge on Spine2
        h0 = bones["Hips"]["head"]
        bx(g, -0.155 * s, -0.11 * s, h0[2] - 0.16 * s, 0.155 * s, 0.11 * s, h0[2] + 0.04 * s,
           tags["top"], bone="Hips")
    if role in ("kemal", "murat", "tarik") or (v % 4 == 3):
        # Utility belt pouches on Hips + shoulder pads
        h0 = bones["Hips"]["head"]
        for sx in (-1, 1):
            bx(g, sx * 0.15 * s - 0.03 * s, -0.06 * s, h0[2] - 0.02 * s,
               sx * 0.15 * s + 0.03 * s, 0.06 * s, h0[2] + 0.07 * s, T("belt", "glove_leather"), bone="Hips")
    if role in ("elif", "nehir") or (v % 5 == 2):
        # Ponytail / bun on back of Head
        ball(g, (0.0, 0.12 * s, (hd0[2] + hd1[2]) * 0.5), 0.055 * s, tags["hair"], seg=6, rings=4, bone="Head")
    return g


@register(BUILDERS)
def zmb_humanoid(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    s = float(params.get("_scale", 1.0))
    v = int(params.get("variant", 1))
    ztype = params.get("ztype", "walker")
    bones = R.humanoid_bones(s)

    t_skin = T("skin", "skin_pale")
    t_cloth = T("cloth", "cloth_casual")
    t_blood = T("blood", "blood_fresh")
    t_dried = T("dried", "blood_dried")
    t_boil = T("boil", "boil_flesh")
    t_armor = T("armor", "armor_plate")

    face_default = {
        "walker": "face_walker",
        "runner": "face_runner",
        "spitter": "face_spitter",
        "armored": "face_walker",
        "goliath": "face_goliath",
    }.get(ztype, "face_walker")

    bulk = {
        "walker": 0.96,
        "runner": 0.88,
        "spitter": 1.02,
        "armored": 1.15,
        "goliath": 1.28,
    }.get(ztype, 1.0)
    hunch = 0.45 if ztype == "spitter" else (0.20 if ztype in ("walker", "goliath") else 0.10)

    tags = dict(
        skin=t_skin,
        face=T("face", face_default),
        hair=t_dried if (v % 2 == 0) else None,
        top=t_armor if ztype == "armored" else t_cloth,
        bottom=t_cloth,
        hand=t_skin,
        boot=t_dried,
        accent=t_skin if ztype in ("runner", "goliath") else t_blood,
    )
    _humanoid_base(g, bones, s, tags, bulk=bulk, hunch=hunch)

    s10, s11 = bones["Spine1"]["head"], bones["Spine1"]["tail"]
    s20, s21 = bones["Spine2"]["head"], bones["Spine2"]["tail"]
    hd0, hd1 = bones["Head"]["head"], bones["Head"]["tail"]

    if ztype == "walker":
        # Torn chest wound / blood stain patch + exposed rib asymmetry
        bx(g, -0.09 * s, -0.115 * s, s10[2] + 0.02 * s, 0.07 * s, -0.08 * s, s21[2] - 0.03 * s,
           t_blood, bone="Spine2")
        if v % 2 == 1:
            ball(g, (0.11 * s, 0.04 * s, s20[2] + 0.06 * s), 0.05 * s, t_dried, seg=6, rings=4, bone="Spine2")

    elif ztype == "runner":
        # Lean ragged torso tears & blood-soaked hands/jaw
        bx(g, -0.08 * s, -0.11 * s, s20[2], 0.08 * s, -0.07 * s, s21[2], t_blood, bone="Spine2")
        for side in ("L", "R"):
            hn1 = bones["Hand_" + side]["tail"]
            ball(g, hn1, 0.032 * s, t_blood, seg=5, rings=3, bone="Hand_" + side)

    elif ztype == "spitter":
        # Grotesque swollen toxic boil sacs on hunchback, neck & belly
        ball(g, (0.0, 0.10 * s, s20[2] + 0.06 * s), 0.14 * s, t_boil, seg=7, rings=4, bone="Spine2")
        ball(g, (0.10 * s, -0.06 * s, s10[2] + 0.05 * s), 0.09 * s, t_boil, seg=6, rings=4, bone="Spine1")
        ball(g, (-0.08 * s, -0.08 * s, hd0[2] - 0.02 * s), 0.065 * s, t_boil, seg=6, rings=4, bone="Neck")
        if v >= 3:
            ball(g, (-0.11 * s, 0.08 * s, s21[2] + 0.02 * s), 0.085 * s, t_boil, seg=6, rings=4, bone="Spine2")

    elif ztype == "armored":
        # Heavy riot/security chest plate, shoulder pauldrons, helmet visor & shin guards
        bx(g, -0.19 * s, -0.14 * s, s10[2] - 0.02 * s, 0.19 * s, 0.14 * s, s21[2] + 0.03 * s,
           t_armor, bone="Spine2")
        # Helmet shell over top/sides of head (leaving front face visible)
        hcz = (hd0[2] + hd1[2]) * 0.5
        bx(g, -0.11 * s, -0.11 * s, hcz + 0.04 * s, 0.11 * s, 0.11 * s, hcz + 0.13 * s, t_armor, bone="Head")
        bx(g, -0.115 * s, -0.02 * s, hcz - 0.08 * s, 0.115 * s, 0.115 * s, hcz + 0.06 * s, t_armor, bone="Head")
        for side, sx in (("L", 1.0), ("R", -1.0)):
            sh1 = bones["Shoulder_" + side]["tail"]
            bx(g, sh1[0] - 0.07 * s, sh1[1] - 0.08 * s, sh1[2] - 0.05 * s,
               sh1[0] + 0.07 * s, sh1[1] + 0.08 * s, sh1[2] + 0.07 * s, t_armor, bone="Shoulder_" + side)
            ll0, ll1 = bones["LowerLeg_" + side]["head"], bones["LowerLeg_" + side]["tail"]
            tube(g, ll0, ll1, 0.070 * s, 0.058 * s, t_armor, seg=6, bone="LowerLeg_" + side)

    elif ztype == "goliath":
        # Massive mutated shoulder humps, fused armor scrap plates, boil growths & oversized forearms
        ball(g, (0.0, 0.08 * s, s21[2] + 0.02 * s), 0.17 * s, t_boil, seg=7, rings=4, bone="Spine2")
        for side, sx in (("L", 1.0), ("R", -1.0)):
            sh1 = bones["Shoulder_" + side]["tail"]
            ball(g, (sh1[0] + sx * 0.04 * s, sh1[1], sh1[2] + 0.03 * s), 0.11 * s, t_boil,
                 seg=6, rings=4, bone="Shoulder_" + side)
            la0, la1 = bones["LowerArm_" + side]["head"], bones["LowerArm_" + side]["tail"]
            tube(g, la0, la1, 0.075 * s, 0.068 * s, t_blood, seg=6, bone="LowerArm_" + side)
        if v >= 2:
            bx(g, -0.20 * s, -0.15 * s, s10[2], 0.08 * s, 0.12 * s, s21[2], t_armor, bone="Spine2")
    return g


def entries():
    out = []
    # 1. FPS Arms (2 entries)
    out.append(mk("chr_fps_arms_01", "Characters", "chr_fps_arms",
                  params=dict(variant=1), atlas="HERO_SURVIVOR_1K",
                  palette=dict(sleeve="cloth_jacket_orange", skin="skin_tan", glove="glove_leather"),
                  rig="fps_arms", anim="fps", budget=3000))
    out.append(mk("chr_fps_arms_02", "Characters", "chr_fps_arms",
                  params=dict(variant=2), atlas="HERO_SURVIVOR_1K",
                  palette=dict(sleeve="cloth_overalls", skin="skin_light", glove="glove_leather"),
                  rig="fps_arms", anim="fps", budget=3000))

    # 2. Named Hero Survivors (7 entries)
    heroes = [
        ("chr_aras_01", "aras", dict(skin="skin_tan", face="face_aras", hair="hair_black",
                                     top="cloth_jacket_orange", bottom="cloth_jeans",
                                     hand="glove_leather", boot="boot_black", accent="cloth_jacket_orange")),
        ("chr_selin_01", "selin", dict(skin="skin_light", face="face_selin", hair="hair_black",
                                       top="cloth_labcoat_clean", bottom="cloth_jeans",
                                       hand="skin_light", boot="boot_black", accent="cloth_labcoat_clean")),
        ("chr_kemal_01", "kemal", dict(skin="skin_dark", face="face_kemal", hair="hair_grey",
                                       top="cloth_overalls", bottom="cloth_overalls",
                                       hand="glove_leather", boot="boot_black", accent="skin_dark")),
        ("chr_elif_01", "elif", dict(skin="skin_light", face="face_elif", hair="hair_brown",
                                     top="cloth_jeans", bottom="cloth_overalls",
                                     hand="skin_light", boot="boot_black", accent="cloth_jacket_orange")),
        ("chr_murat_01", "murat", dict(skin="skin_tan", face="face_kemal", hair="hair_black",
                                       top="cloth_overalls", bottom="cloth_jeans",
                                       hand="glove_leather", boot="boot_black", accent="cloth_overalls")),
        ("chr_nehir_01", "nehir", dict(skin="skin_light", face="face_selin", hair="hair_brown",
                                       top="cloth_jacket_orange", bottom="cloth_jeans",
                                       hand="skin_light", boot="boot_black", accent="cloth_labcoat_clean")),
        ("chr_tarik_01", "tarik", dict(skin="skin_dark", face="face_aras", hair="hair_grey",
                                       top="cloth_labcoat_clean", bottom="cloth_overalls",
                                       hand="glove_leather", boot="boot_black", accent="cloth_overalls")),
    ]
    for idx, (eid, role, pal) in enumerate(heroes, 1):
        out.append(mk(eid, "Characters", "chr_survivor",
                      params=dict(variant=idx, role=role, seed=idx),
                      atlas="HERO_SURVIVOR_1K", palette=pal,
                      rig="humanoid", scale=1.0, anim="survivor"))

    # 3. Generic Survivor NPCs (chr_npc_01..18)
    faces = ["face_aras", "face_selin", "face_kemal", "face_elif"]
    tops = ["cloth_jacket_orange", "cloth_jeans", "cloth_labcoat_clean", "cloth_overalls"]
    bottoms = ["cloth_jeans", "cloth_overalls"]
    hands = ["glove_leather", "skin_tan", "skin_light", "skin_dark"]
    for i in range(1, 19):
        skin = pick(P.SURV_SKIN, i - 1)
        pal = dict(
            skin=skin,
            face=pick(faces, i - 1),
            hair=pick(P.SURV_HAIR, i - 1),
            top=pick(tops, i - 1),
            bottom=pick(bottoms, i - 1),
            hand=skin if (i % 2 == 0) else "glove_leather",
            boot="boot_black",
            accent=pick(tops, i),
        )
        out.append(mk("chr_npc_%02d" % i, "Characters", "chr_survivor",
                      params=dict(variant=i, role="npc", seed=100 + i),
                      atlas="HERO_SURVIVOR_1K", palette=pal,
                      rig="humanoid", scale=1.0, anim="survivor"))

    # 4. Zombies (30 entries)
    zspecs = [
        ("walker", 8, "shamble", 1.0, "face_walker"),
        ("runner", 7, "sprint", 1.0, "face_runner"),
        ("spitter", 6, "hunch", 1.0, "face_spitter"),
        ("armored", 6, "heavy", 1.0, "face_walker"),
        ("goliath", 3, "boss", 1.35, "face_goliath"),
    ]
    for ztype, count, anim, sc, ftag in zspecs:
        for i in range(1, count + 1):
            pal = dict(
                skin=pick(P.ZOMBIE_SKIN, i - 1 + (2 if ztype == "spitter" else 0)),
                cloth="cloth_security" if ztype == "armored" else pick(P.ZOMBIE_CLOTH, i - 1),
                face=ftag,
                blood="blood_fresh" if (i % 2 == 1) else "blood_dried",
                dried="blood_dried",
                boil="boil_flesh",
                armor="armor_plate",
            )
            out.append(mk("zmb_%s_%02d" % (ztype, i), "Zombies", "zmb_humanoid",
                          params=dict(variant=i, ztype=ztype, seed=200 + i),
                          atlas="HERO_ZOMBIE_1K", palette=pal,
                          rig="humanoid", scale=sc, anim=anim))
    return out
