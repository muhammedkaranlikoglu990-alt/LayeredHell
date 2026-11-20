"""Procedural Office furniture & equipment builders (category 'Office', atlas ENV_MAT_2K)."""
import math
from .builders_common import (
    register, mk, Tags, geo_for, bx, obox, tube, pick
)
from . import palettes as P

BUILDERS = {}
ATLAS = "ENV_MAT_2K"


@register(BUILDERS)
def b_off_desk(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.5))
    d = float(params.get("d", 0.78))
    h = float(params.get("h", 0.75))
    style = int(params.get("style", 0))
    top_t = T("top", "wood_light")
    frame_t = T("frame", "steel_brushed")
    mod_t = T("panel", "wood_dark")

    hw, hd = w / 2.0, d / 2.0
    # Desktop slab
    bx(g, -hw, -hd, h - 0.04, hw, hd, h, top_t)

    if style == 1:
        # L-shaped corner return extension on left
        rw, rd = 0.60, 0.85
        bx(g, -hw, -hd - rd, h - 0.04, -hw + rw, -hd, h, top_t)
        bx(g, -hw + 0.04, -hd - rd + 0.04, 0.0, -hw + rw - 0.04, -hd - rd + 0.08, h - 0.04, frame_t)

    if style == 3:
        # Reception counter desk with raised transaction shelf
        bx(g, -hw, hd - 0.06, 0.0, hw, hd, h + 0.35, mod_t)
        bx(g, -hw, -hd, 0.0, -hw + 0.06, hd, h - 0.04, mod_t)
        bx(g, hw - 0.06, -hd, 0.0, hw, hd, h - 0.04, mod_t)
        bx(g, -hw - 0.02, hd - 0.28, h + 0.35, hw + 0.02, hd + 0.04, h + 0.39, top_t)
    else:
        # Metal legs or full side panels
        for sx in (-1, 1):
            lx = sx * (hw - 0.05)
            bx(g, lx - 0.025, -hd + 0.05, 0.0, lx + 0.025, -hd + 0.10, h - 0.04, frame_t)
            bx(g, lx - 0.025, hd - 0.10, 0.0, lx + 0.025, hd - 0.05, h - 0.04, frame_t)
            bx(g, lx - 0.025, -hd + 0.05, 0.02, lx + 0.025, hd - 0.05, 0.06, frame_t)
        # Modesty panel
        bx(g, -hw + 0.08, hd - 0.10, 0.25, hw - 0.08, hd - 0.06, h - 0.04, mod_t)

    # Drawer pedestal(s)
    sides = (-1, 1) if style in (2, 4) else (1,)
    for s in sides:
        px0 = (hw - 0.44) if s == 1 else (-hw + 0.06)
        px1 = px0 + 0.38
        bx(g, px0, -hd + 0.06, 0.06, px1, hd - 0.08, h - 0.04, mod_t)
        for dr in range(3):
            z0 = 0.10 + dr * 0.20
            bx(g, px0 + 0.02, -hd + 0.045, z0, px1 - 0.02, -hd + 0.06, z0 + 0.17, top_t)
            bx(g, (px0 + px1) / 2 - 0.05, -hd + 0.025, z0 + 0.08, (px0 + px1) / 2 + 0.05, -hd + 0.045, z0 + 0.10, frame_t)
    return g


@register(BUILDERS)
def b_off_chair(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    seat_t = T("seat", "fabric_blue")
    frame_t = T("frame", "plastic_black")
    metal_t = T("metal", "steel_brushed")

    sh = 0.62 if style == 5 else 0.46
    if style in (3, 4):
        # 4-leg / sled conference guest chair
        for sx in (-0.23, 0.23):
            for sy in (-0.22, 0.22):
                tube(g, (sx, sy, 0.0), (sx, sy, sh), 0.018, 0.018, metal_t, seg=6)
            if style == 4:
                bx(g, sx - 0.018, -0.22, 0.0, sx + 0.018, 0.22, 0.025, metal_t)
    else:
        # 5-star swivel caster base + pneumatic cylinder
        for k in range(5):
            ang = 2 * math.pi * k / 5.0
            rx, ry = 0.28 * math.cos(ang), 0.28 * math.sin(ang)
            tube(g, (0, 0, 0.07), (rx, ry, 0.04), 0.022, 0.018, frame_t, seg=6)
            bx(g, rx - 0.02, ry - 0.02, 0.0, rx + 0.02, ry + 0.02, 0.04, frame_t)
        tube(g, (0, 0, 0.05), (0, 0, sh), 0.028, 0.025, metal_t, seg=8)
        if style == 5:
            # Drafting stool footring
            tube(g, (0, 0, 0.24), (0, 0, 0.26), 0.20, 0.20, metal_t, seg=10)

    # Seat cushion & spine bar
    bx(g, -0.24, -0.24, sh, 0.24, 0.22, sh + 0.06, seat_t)
    bh = 0.56 if style == 1 else 0.42
    bx(g, -0.04, 0.18, sh, 0.04, 0.25, sh + 0.22, frame_t)
    obox(g, (0, 0.22, sh + 0.10 + bh / 2), (0.44, 0.05, bh), seat_t, rot=(-0.10, 0.0, 0.0))

    if style != 6:
        # Armrests
        for sx in (-1, 1):
            ax = sx * 0.25
            bx(g, ax - 0.02, -0.04, sh + 0.03, ax + 0.02, 0.02, sh + 0.22, frame_t)
            bx(g, ax - 0.03, -0.14, sh + 0.22, ax + 0.03, 0.14, sh + 0.25, frame_t)
    return g


@register(BUILDERS)
def b_off_partition(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.6))
    h = float(params.get("h", 1.45))
    style = int(params.get("style", 0))
    fab_t = T("panel", "fabric_blue")
    frame_t = T("frame", "steel_brushed")
    glass_t = T("glass", "glass_dirty")

    hw = w / 2.0
    th = 0.06
    # Stabilizer feet on floor (z = 0.0 .. 0.03)
    for sx in (-hw + 0.12, hw - 0.12):
        bx(g, sx - 0.04, -0.22, 0.0, sx + 0.04, 0.22, 0.03, frame_t)
    # Outer aluminum frame
    bx(g, -hw, -th / 2, 0.03, -hw + 0.045, th / 2, h, frame_t)
    bx(g, hw - 0.045, -th / 2, 0.03, hw, th / 2, h, frame_t)
    bx(g, -hw, -th / 2, h - 0.045, hw, th / 2, h, frame_t)
    bx(g, -hw, -th / 2, 0.03, hw, th / 2, 0.09, frame_t)

    if style in (1, 4):
        # Half-fabric lower, half-glass upper window
        mid_z = h * 0.62
        bx(g, -hw + 0.045, -th * 0.4, 0.09, hw - 0.045, th * 0.4, mid_z, fab_t)
        bx(g, -hw + 0.045, -th / 2, mid_z, hw - 0.045, th / 2, mid_z + 0.04, frame_t)
        bx(g, -hw + 0.045, -0.01, mid_z + 0.04, hw - 0.045, 0.01, h - 0.045, glass_t)
    else:
        bx(g, -hw + 0.045, -th * 0.4, 0.09, hw - 0.045, th * 0.4, h - 0.045, fab_t)

    if style == 2:
        # L-corner return wing
        rw = 0.85
        bx(g, hw - 0.045, 0.0, 0.03, hw, rw, h, frame_t)
        bx(g, hw - 0.035, 0.045, 0.09, hw - 0.01, rw - 0.045, h - 0.045, fab_t)
    return g


@register(BUILDERS)
def b_off_monitor(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.56))
    style = int(params.get("style", 0))
    bez_t = T("bezel", "plastic_black")
    scr_t = T("screen", "glass_dirty")
    st_t = T("stand", "steel_brushed")

    if style == 1:
        # Dual-monitor desk arm setup
        bx(g, -0.12, 0.04, 0.0, 0.12, 0.18, 0.02, st_t)
        tube(g, (0, 0.12, 0.02), (0, 0.12, 0.34), 0.022, 0.022, st_t, seg=6)
        for sx in (-1, 1):
            cx = sx * 0.28
            obox(g, (cx, 0.05, 0.28), (0.50, 0.03, 0.30), bez_t, rot=(0, 0, -sx * 0.16))
            obox(g, (cx, 0.032, 0.28), (0.46, 0.008, 0.26), scr_t, rot=(0, 0, -sx * 0.16))
    else:
        # Single monitor + stand + keyboard & mouse
        bx(g, -0.14, -0.02, 0.0, 0.14, 0.14, 0.018, st_t)
        bx(g, -0.03, 0.06, 0.018, 0.03, 0.10, 0.16, st_t)
        mh = 0.34 if style != 4 else 0.48
        mw = w if style != 4 else 0.32
        bx(g, -mw / 2, 0.03, 0.12, mw / 2, 0.065, 0.12 + mh, bez_t)
        bx(g, -mw / 2 + 0.02, 0.025, 0.14, mw / 2 - 0.02, 0.032, 0.10 + mh, scr_t)
        if style in (2, 5):
            # PC desktop tower beside monitor
            bx(g, mw / 2 + 0.06, -0.12, 0.0, mw / 2 + 0.25, 0.26, 0.42, bez_t)
    # Keyboard & mouse on desk plane (z >= 0.0)
    bx(g, -0.20, -0.22, 0.0, 0.14, -0.08, 0.02, bez_t)
    bx(g, 0.19, -0.18, 0.0, 0.25, -0.09, 0.025, bez_t)
    return g


@register(BUILDERS)
def b_off_cabinet(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.52))
    d = float(params.get("d", 0.62))
    h = float(params.get("h", 1.35))
    drawers = int(params.get("drawers", 4))
    style = int(params.get("style", 0))
    body_t = T("body", "steel_brushed")
    trim_t = T("trim", "plastic_black")
    lbl_t = T("label", "plastic_white")

    hw, hd = w / 2.0, d / 2.0
    bx(g, -hw, -hd, 0.0, hw, hd, 0.05, trim_t)
    bx(g, -hw, -hd, 0.05, hw, hd, h, body_t)

    if style in (2, 5):
        # Double-door office supply cabinet
        for s in (-1, 1):
            x0 = (-hw + 0.02) if s == -1 else 0.008
            x1 = -0.008 if s == -1 else (hw - 0.02)
            tag = T("glass", "glass_dirty") if style == 5 else body_t
            bx(g, x0, -hd - 0.02, 0.07, x1, -hd, h - 0.03, tag)
            hx = s * 0.04
            bx(g, hx - 0.01, -hd - 0.045, h * 0.48, hx + 0.01, -hd - 0.02, h * 0.60, trim_t)
    else:
        # Vertical or lateral filing cabinet drawers
        dh = (h - 0.09) / drawers
        for i in range(drawers):
            z0 = 0.07 + i * dh
            bx(g, -hw + 0.025, -hd - 0.02, z0 + 0.01, hw - 0.025, -hd, z0 + dh - 0.01, body_t)
            # Handle & label card holder
            bx(g, -0.08, -hd - 0.04, z0 + dh * 0.42, 0.08, -hd - 0.02, z0 + dh * 0.54, trim_t)
            bx(g, -0.05, -hd - 0.026, z0 + dh * 0.68, 0.05, -hd - 0.018, z0 + dh * 0.82, lbl_t)
    return g


@register(BUILDERS)
def b_off_printer(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.68))
    d = float(params.get("d", 0.62))
    h = float(params.get("h", 1.05))
    style = int(params.get("style", 0))
    body_t = T("body", "plastic_white")
    dark_t = T("dark", "plastic_black")
    glass_t = T("glass", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    if style == 2:
        # Wide-format blueprint plotter on stand legs
        for sx in (-hw + 0.08, hw - 0.08):
            bx(g, sx - 0.03, -hd + 0.08, 0.0, sx + 0.03, hd - 0.08, 0.05, dark_t)
            bx(g, sx - 0.03, -0.04, 0.05, sx + 0.03, 0.04, 0.72, dark_t)
        bx(g, -hw, -hd * 0.65, 0.72, hw, hd * 0.65, 0.98, body_t)
        bx(g, -hw * 0.85, -hd * 0.7, 0.86, hw * 0.85, hd * 0.5, 1.04, dark_t)
    else:
        # Floor copier / multifunction printer with paper drawer trays
        bx(g, -hw, -hd, 0.0, hw, hd, h * 0.68, body_t)
        for tr in range(3):
            z0 = 0.06 + tr * 0.18
            bx(g, -hw * 0.75, -hd - 0.015, z0, hw * 0.75, -hd, z0 + 0.15, body_t)
            bx(g, -0.08, -hd - 0.03, z0 + 0.06, 0.08, -hd - 0.015, z0 + 0.10, dark_t)
        # Recessed paper exit cavity + top scanner bed + ADF feeder
        bx(g, -hw * 0.65, -hd, h * 0.68, hw, hd, h * 0.82, dark_t)
        bx(g, -hw, -hd, h * 0.82, hw, hd, h * 0.92, body_t)
        bx(g, -hw * 0.78, -hd * 0.75, h * 0.92, hw * 0.78, hd * 0.75, h * 0.94, glass_t)
        if style == 6:
            # Open scanner lid
            obox(g, (0, hd * 0.45, h + 0.16), (w * 0.92, d * 0.85, 0.06), body_t, rot=(-0.65, 0, 0))
        else:
            bx(g, -hw * 0.94, -hd * 0.88, h * 0.94, hw * 0.94, hd * 0.88, h, body_t)
            bx(g, -hw * 0.55, -hd * 0.5, h, hw * 0.35, hd * 0.5, h + 0.08, dark_t)
        # Side paper bypass tray
        bx(g, hw, -0.18, h * 0.55, hw + 0.16, 0.18, h * 0.60, dark_t)
    return g


@register(BUILDERS)
def b_off_whiteboard(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.6))
    h = float(params.get("h", 1.85))
    style = int(params.get("style", 0))
    board_t = T("board", "plastic_white")
    frame_t = T("frame", "steel_brushed")
    dark_t = T("dark", "plastic_black")

    hw = w / 2.0
    if style == 4:
        # Tripod flipchart easel stand
        for sx in (-0.34, 0.34):
            tube(g, (sx, -0.25, 0.0), (sx * 0.5, -0.04, h), 0.018, 0.016, frame_t, seg=6)
        tube(g, (0, 0.42, 0.0), (0, 0.0, h * 0.85), 0.018, 0.016, frame_t, seg=6)
        obox(g, (0, -0.08, h * 0.66), (0.76, 0.03, 0.96), board_t, rot=(-0.14, 0, 0))
    else:
        # Mobile rolling whiteboard on H-frame with casters
        for sx in (-hw, hw):
            bx(g, sx - 0.03, -0.28, 0.0, sx + 0.03, 0.28, 0.05, dark_t)
            bx(g, sx - 0.025, -0.025, 0.05, sx + 0.025, 0.025, h * 0.75, frame_t)
        bx(g, -hw, -0.02, 0.22, hw, 0.02, 0.26, frame_t)
        # Board panel + aluminum frame + marker tray
        z0 = 0.72
        bx(g, -hw + 0.03, -0.03, z0, hw - 0.03, 0.03, h, frame_t)
        bx(g, -hw + 0.07, -0.036, z0 + 0.04, hw - 0.07, 0.036, h - 0.04, board_t)
        bx(g, -hw * 0.65, -0.09, z0 - 0.02, hw * 0.65, -0.03, z0 + 0.02, frame_t)
        if style == 2:
            # Half corkboard (wood_light) / half whiteboard
            bx(g, -hw + 0.07, -0.04, z0 + 0.04, 0.0, 0.04, h - 0.04, T("cork", "wood_light"))
    return g


@register(BUILDERS)
def b_off_cooler(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    h = float(params.get("h", 1.02))
    style = int(params.get("style", 0))
    body_t = T("body", "plastic_white")
    dark_t = T("dark", "plastic_black")
    jug_t = T("jug", "glass_dirty")
    tap_t = T("tap", "painted_metal_blue")

    hw = 0.18
    # Main cooler tower column
    bx(g, -hw, -hw, 0.0, hw, hw, h, body_t)
    # Recessed drip tray alcove & hot/cold taps
    bx(g, -hw * 0.75, -hw - 0.01, h * 0.56, hw * 0.75, -hw + 0.04, h * 0.85, dark_t)
    bx(g, -hw * 0.78, -hw - 0.04, h * 0.54, hw * 0.78, -hw, h * 0.58, dark_t)
    for sx, tag in ((-0.06, "fabric_red"), (0.06, tap_t)):
        bx(g, sx - 0.018, -hw - 0.03, h * 0.74, sx + 0.018, -hw, h * 0.80, T("accent", tag))
    # Side cup dispenser tube
    tube(g, (hw + 0.04, 0.0, h * 0.55), (hw + 0.04, 0.0, h * 0.92), 0.038, 0.038, body_t, seg=8)

    if style not in (1, 2):
        # Inverted 5-gallon water bottle on top
        tube(g, (0, 0, h), (0, 0, h + 0.08), 0.06, 0.14, jug_t, seg=10)
        tube(g, (0, 0, h + 0.08), (0, 0, h + 0.36), 0.14, 0.14, jug_t, seg=10)
        tube(g, (0, 0, h + 0.36), (0, 0, h + 0.42), 0.14, 0.08, jug_t, seg=10)
    return g


def entries():
    out = []
    woods = P.MAT["wood"]
    softs = P.MAT["soft"]
    metals = P.MAT["metal"]
    plastics = P.MAT["plastic"]

    # 1. off_desk_01..07
    for i in range(7):
        out.append(mk(f"off_desk_{i + 1:02d}", "Office", "b_off_desk",
                      params={"w": 1.4 + (i % 3) * 0.25, "d": 0.78, "h": 0.75, "style": i},
                      atlas=ATLAS,
                      palette={"top": pick(woods + plastics, i), "frame": pick(metals, i),
                               "panel": pick(woods + ["plastic_black"], i + 1)}))

    # 2. off_chair_01..07
    for i in range(7):
        out.append(mk(f"off_chair_{i + 1:02d}", "Office", "b_off_chair",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"seat": pick(softs, i), "frame": "plastic_black", "metal": "steel_brushed"}))

    # 3. off_partition_01..07
    for i in range(7):
        out.append(mk(f"off_partition_{i + 1:02d}", "Office", "b_off_partition",
                      params={"w": 1.2 + (i % 3) * 0.35, "h": 1.3 + (i % 2) * 0.35, "style": i},
                      atlas=ATLAS,
                      palette={"panel": pick(P.MAT["fabric"] + woods, i),
                               "frame": pick(["steel_brushed", "plastic_black"], i),
                               "glass": "glass_dirty"}))

    # 4. off_monitor_01..07
    for i in range(7):
        out.append(mk(f"off_monitor_{i + 1:02d}", "Office", "b_off_monitor",
                      params={"w": 0.52 + (i % 3) * 0.12, "style": i},
                      atlas=ATLAS,
                      palette={"bezel": pick(["plastic_black", "plastic_white"], i),
                               "screen": "glass_dirty", "stand": "steel_brushed"}))

    # 5. off_cabinet_01..07
    for i in range(7):
        out.append(mk(f"off_cabinet_{i + 1:02d}", "Office", "b_off_cabinet",
                      params={"w": 0.90 if i in (1, 2, 3) else 0.50,
                              "d": 0.58,
                              "h": 0.78 if i == 3 else (1.80 if i in (2, 5) else 1.35),
                              "drawers": 3 if i in (1, 3) else 4,
                              "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["steel_brushed", "painted_metal_green", "painted_metal_blue",
                                             "plastic_white", "wood_dark"], i),
                               "trim": "plastic_black", "label": "plastic_white"}))

    # 6. off_printer_01..07
    for i in range(7):
        out.append(mk(f"off_printer_{i + 1:02d}", "Office", "b_off_printer",
                      params={"w": 1.15 if i == 2 else 0.68, "d": 0.62, "h": 1.05, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["plastic_white", "plastic_black", "steel_brushed"], i),
                               "dark": "plastic_black", "glass": "glass_dirty"}))

    # 7. off_whiteboard_01..07
    for i in range(7):
        out.append(mk(f"off_whiteboard_{i + 1:02d}", "Office", "b_off_whiteboard",
                      params={"w": 1.4 + (i % 3) * 0.3, "h": 1.80, "style": i},
                      atlas=ATLAS,
                      palette={"board": "plastic_white", "frame": "steel_brushed", "dark": "plastic_black"}))

    # 8. off_cooler_01..07
    for i in range(7):
        out.append(mk(f"off_cooler_{i + 1:02d}", "Office", "b_off_cooler",
                      params={"h": 1.02, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["plastic_white", "plastic_black", "steel_brushed"], i),
                               "dark": "plastic_black", "jug": "glass_dirty", "tap": "painted_metal_blue"}))

    return out
