"""Procedural Medical & Gym equipment builders (categories 'Medical' & 'Gym', atlas ENV_MAT_2K)."""
import math
from .builders_common import (
    register, mk, Tags, geo_for, bx, obox, tube, ball, pick
)
from . import palettes as P

BUILDERS = {}
ATLAS = "ENV_MAT_2K"


@register(BUILDERS)
def b_med_bed(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.96))
    d = float(params.get("d", 2.08))
    style = int(params.get("style", 0))
    frame_t = T("frame", "steel_brushed")
    matt_t = T("mattress", "painted_metal_blue")
    sheet_t = T("sheet", "plastic_white")
    wheel_t = T("wheel", "rubber")

    hw, hd = w / 2.0, d / 2.0
    if style == 4:
        # Surgical operating table on central hydraulic column
        bx(g, -0.28, -0.45, 0.0, 0.28, 0.45, 0.06, frame_t)
        tube(g, (0, 0, 0.06), (0, 0, 0.68), 0.14, 0.12, frame_t, seg=10)
    else:
        # 4 caster wheels & hospital gurney undercarriage
        for sx in (-hw + 0.06, hw - 0.06):
            for sy in (-hd + 0.14, hd - 0.14):
                tube(g, (sx - 0.018, sy, 0.055), (sx + 0.018, sy, 0.055), 0.055, 0.055, wheel_t, seg=8)
                tube(g, (sx, sy, 0.055), (sx, sy, 0.65), 0.024, 0.024, frame_t, seg=6)
        bx(g, -hw + 0.05, -hd + 0.12, 0.22, hw - 0.05, hd - 0.12, 0.26, frame_t)

    # Bed deck frame
    bx(g, -hw, -hd, 0.65, hw, hd, 0.70, frame_t)
    # Headboard & footboard panels
    if style != 4:
        bx(g, -hw, hd - 0.05, 0.70, hw, hd, 1.12, sheet_t)
        bx(g, -hw, -hd, 0.70, hw, -hd + 0.05, 0.98, sheet_t)
        # Side safety guardrails
        for sx in (-hw - 0.02, hw):
            bx(g, sx, -hd * 0.55, 0.88, sx + 0.02, hd * 0.55, 0.92, frame_t)
            bx(g, sx, -hd * 0.55, 0.70, sx + 0.02, -hd * 0.50, 0.90, frame_t)
            bx(g, sx, hd * 0.50, 0.70, sx + 0.02, hd * 0.55, 0.90, frame_t)

    # Mattress (flat or inclined Fowler's position)
    if style in (1, 3):
        bx(g, -hw + 0.03, -hd + 0.06, 0.70, hw - 0.03, 0.15, 0.81, matt_t)
        obox(g, (0, hd * 0.52, 0.92), (w - 0.06, d * 0.42, 0.11), matt_t, rot=(0.45, 0, 0))
    else:
        bx(g, -hw + 0.03, -hd + 0.06, 0.70, hw - 0.03, hd - 0.06, 0.81, matt_t)
        bx(g, -hw * 0.6, hd - 0.36, 0.81, hw * 0.6, hd - 0.10, 0.87, sheet_t)
    return g


@register(BUILDERS)
def b_med_ivstand(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    h = float(params.get("h", 1.85))
    style = int(params.get("style", 0))
    pole_t = T("pole", "steel_brushed")
    bag_t = T("bag", "glass_dirty")
    pump_t = T("pump", "painted_metal_blue")
    wheel_t = T("wheel", "plastic_black")

    # 5-leg rolling spider base at z=0.0
    for k in range(5):
        ang = 2 * math.pi * k / 5.0
        rx, ry = 0.25 * math.cos(ang), 0.25 * math.sin(ang)
        tube(g, (0, 0, 0.06), (rx, ry, 0.035), 0.016, 0.014, pole_t, seg=6)
        bx(g, rx - 0.018, ry - 0.018, 0.0, rx + 0.018, ry + 0.018, 0.035, wheel_t)

    # Telescoping vertical pole + collar
    tube(g, (0, 0, 0.05), (0, 0, h * 0.55), 0.018, 0.018, pole_t, seg=6)
    tube(g, (0, 0, h * 0.53), (0, 0, h * 0.57), 0.026, 0.026, wheel_t, seg=6)
    tube(g, (0, 0, h * 0.55), (0, 0, h), 0.013, 0.013, pole_t, seg=6)

    # Top crossbar & hanging IV bags
    tube(g, (-0.18, 0, h - 0.02), (0.18, 0, h - 0.02), 0.010, 0.010, pole_t, seg=6)
    for sx in (-0.13, 0.13):
        bx(g, sx - 0.045, -0.02, h - 0.26, sx + 0.045, 0.02, h - 0.05, bag_t)
    if style in (1, 3, 5):
        # Electronic infusion pump mounted mid-pole
        bx(g, -0.09, -0.08, h * 0.58, 0.09, 0.06, h * 0.72, pump_t)
    return g


@register(BUILDERS)
def b_med_monitor(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    body_t = T("body", "plastic_white")
    dark_t = T("dark", "plastic_black")
    scr_t = T("screen", "glass_dirty")
    pole_t = T("pole", "steel_brushed")

    # Mobile rolling cart base
    bx(g, -0.24, -0.24, 0.03, 0.24, 0.24, 0.08, dark_t)
    for sx in (-0.20, 0.20):
        for sy in (-0.20, 0.20):
            bx(g, sx - 0.02, sy - 0.02, 0.0, sx + 0.02, sy + 0.02, 0.03, dark_t)

    if style in (1, 3):
        # Full anesthesia / ultrasound workstation cart
        bx(g, -0.22, -0.20, 0.08, 0.22, 0.20, 0.88, body_t)
        bx(g, -0.25, -0.24, 0.88, 0.25, 0.22, 0.93, dark_t)
        z_mon = 0.93
    else:
        # Slim telemetry pole stand with accessory basket
        tube(g, (0, 0.05, 0.08), (0, 0.05, 1.05), 0.028, 0.025, pole_t, seg=8)
        bx(g, -0.16, -0.14, 0.62, 0.16, 0.05, 0.76, pole_t)
        z_mon = 1.02

    # Patient vitals monitor head unit + screen
    bx(g, -0.22, -0.10, z_mon, 0.22, 0.12, z_mon + 0.34, body_t)
    bx(g, -0.18, -0.11, z_mon + 0.05, 0.12, -0.095, z_mon + 0.29, scr_t)
    # Control dial / buttons strip on right bezel
    bx(g, 0.14, -0.11, z_mon + 0.06, 0.19, -0.095, z_mon + 0.26, T("accent", "painted_metal_green"))
    return g


@register(BUILDERS)
def b_med_cabinet(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.92))
    d = float(params.get("d", 0.46))
    h = float(params.get("h", 1.75))
    style = int(params.get("style", 0))
    body_t = T("body", "plastic_white")
    metal_t = T("metal", "steel_brushed")
    glass_t = T("glass", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    # 4 legs / casters
    for sx in (-hw + 0.05, hw - 0.05):
        for sy in (-hd + 0.05, hd - 0.05):
            bx(g, sx - 0.03, sy - 0.03, 0.0, sx + 0.03, sy + 0.03, 0.08, metal_t)

    if style == 1:
        # Red emergency crash cart with drawer rows
        ch = 1.02
        bx(g, -hw, -hd, 0.08, hw, hd, ch, T("crash", "fabric_red"))
        for dr in range(5):
            z0 = 0.12 + dr * 0.17
            bx(g, -hw + 0.03, -hd - 0.02, z0, hw - 0.03, -hd, z0 + 0.14, body_t)
            bx(g, -0.10, -hd - 0.04, z0 + 0.06, 0.10, -hd - 0.02, z0 + 0.09, metal_t)
        bx(g, -hw - 0.02, -hd - 0.02, ch, hw + 0.02, hd + 0.02, ch + 0.04, metal_t)
    else:
        # Lower solid cupboard & upper glass-door medicine cabinet
        mid_z = h * 0.45
        bx(g, -hw, -hd, 0.08, hw, hd, mid_z, body_t)
        bx(g, -hw, -hd + 0.02, mid_z, hw, hd, h, body_t)
        for s in (-1, 1):
            x0 = (-hw + 0.03) if s == -1 else 0.01
            x1 = -0.01 if s == -1 else (hw - 0.03)
            bx(g, x0, -hd - 0.02, 0.10, x1, -hd, mid_z - 0.02, metal_t)
            bx(g, x0, -hd, mid_z + 0.02, x1, -hd + 0.02, h - 0.02, glass_t)
    return g


@register(BUILDERS)
def b_med_wheelchair(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    frame_t = T("frame", "steel_brushed")
    seat_t = T("seat", "leather_black")
    tire_t = T("tire", "rubber")

    # Large rear wheels (r = 0.28, center z = 0.28 -> lowest z = 0.0)
    r_big = 0.28
    for sx in (-0.28, 0.28):
        tube(g, (sx - 0.02, 0.10, r_big), (sx + 0.02, 0.10, r_big), r_big, r_big, tire_t, seg=14)
        tube(g, (sx - 0.03, 0.10, r_big), (sx + 0.03, 0.10, r_big), 0.05, 0.05, frame_t, seg=8)
        for k in range(4):
            ang = math.pi * k / 4.0
            dx, dz = (r_big - 0.02) * math.cos(ang), (r_big - 0.02) * math.sin(ang)
            tube(g, (sx, 0.10 - dx, r_big - dz), (sx, 0.10 + dx, r_big + dz), 0.008, 0.008, frame_t, seg=4)

    # Small front caster wheels (r = 0.09, center z = 0.09 -> lowest z = 0.0)
    r_sm = 0.09
    for sx in (-0.23, 0.23):
        tube(g, (sx - 0.018, -0.26, r_sm), (sx + 0.018, -0.26, r_sm), r_sm, r_sm, tire_t, seg=10)
        tube(g, (sx, -0.26, r_sm), (sx, -0.22, 0.46), 0.016, 0.016, frame_t, seg=6)

    # Seat sling, backrest & push handles
    bx(g, -0.24, -0.22, 0.46, 0.24, 0.18, 0.51, seat_t)
    bx(g, -0.24, 0.14, 0.51, 0.24, 0.19, 0.92, seat_t)
    for sx in (-0.24, 0.24):
        tube(g, (sx, 0.17, 0.28), (sx, 0.17, 0.94), 0.016, 0.016, frame_t, seg=6)
        tube(g, (sx, 0.17, 0.94), (sx, 0.28, 0.94), 0.018, 0.018, tire_t, seg=6)
        bx(g, sx - 0.025, -0.18, 0.68, sx + 0.025, 0.17, 0.72, seat_t)
        tube(g, (sx, -0.16, 0.46), (sx, -0.16, 0.68), 0.015, 0.015, frame_t, seg=6)
        bx(g, sx - 0.08 * (1 if sx < 0 else -1) - 0.06, -0.40, 0.11,
           sx - 0.08 * (1 if sx < 0 else -1) + 0.06, -0.26, 0.14, tire_t)
    return g


@register(BUILDERS)
def b_med_lab(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.5))
    d = float(params.get("d", 0.75))
    h = float(params.get("h", 0.90))
    style = int(params.get("style", 0))
    cab_t = T("cabinet", "steel_brushed")
    top_t = T("top", "plastic_black")
    eq_t = T("equip", "plastic_white")
    glass_t = T("glass", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    bx(g, -hw, -hd, 0.0, hw, hd, h - 0.05, cab_t)
    bx(g, -hw - 0.02, -hd - 0.02, h - 0.05, hw + 0.02, hd + 0.02, h, top_t)

    # Microscope on left side
    mx = -hw * 0.55
    bx(g, mx - 0.10, -0.12, h, mx + 0.10, 0.12, h + 0.04, eq_t)
    tube(g, (mx, 0.08, h + 0.04), (mx, 0.04, h + 0.28), 0.028, 0.024, eq_t, seg=6)
    tube(g, (mx, 0.04, h + 0.28), (mx, -0.08, h + 0.36), 0.022, 0.018, top_t, seg=6)

    # Centrifuge / autoclave box on right side
    cx = hw * 0.45
    bx(g, cx - 0.22, -0.18, h, cx + 0.22, 0.20, h + 0.26, eq_t)
    bx(g, cx - 0.18, -0.19, h + 0.05, cx + 0.10, -0.17, h + 0.21, glass_t)

    if style in (2, 4, 5):
        for sx in (-hw + 0.08, hw - 0.08):
            bx(g, sx - 0.02, hd - 0.16, h, sx + 0.02, hd - 0.12, h + 0.48, cab_t)
        bx(g, -hw + 0.06, hd - 0.22, h + 0.45, hw - 0.06, hd, h + 0.49, top_t)
    return g


@register(BUILDERS)
def b_gym_bench(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    frame_t = T("frame", "steel_brushed")
    pad_t = T("pad", "leather_black")
    plate_t = T("plate", "rubber")

    bx(g, -0.28, -0.62, 0.0, 0.28, -0.54, 0.05, frame_t)
    bx(g, -0.34, 0.48, 0.0, 0.34, 0.56, 0.05, frame_t)
    bx(g, -0.04, -0.58, 0.05, 0.04, 0.52, 0.40, frame_t)

    if style in (1, 4):
        bx(g, -0.16, -0.60, 0.40, 0.16, -0.15, 0.47, pad_t)
        obox(g, (0, 0.22, 0.66), (0.32, 0.72, 0.07), pad_t, rot=(0.55, 0, 0))
    else:
        bx(g, -0.16, -0.60, 0.40, 0.16, 0.55, 0.47, pad_t)

    if style in (2, 5):
        for sx in (-0.42, 0.42):
            bx(g, sx - 0.03, 0.48, 0.0, sx + 0.03, 0.56, 1.05, frame_t)
        tube(g, (-0.95, 0.50, 0.96), (0.95, 0.50, 0.96), 0.018, 0.018, frame_t, seg=8)
        for sx in (-0.75, -0.68, 0.68, 0.75):
            tube(g, (sx - 0.02, 0.50, 0.96), (sx + 0.02, 0.50, 0.96), 0.20, 0.20, plate_t, seg=12)
    return g


@register(BUILDERS)
def b_gym_rack(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.35))
    d = float(params.get("d", 1.25))
    h = float(params.get("h", 2.20))
    style = int(params.get("style", 0))
    frame_t = T("frame", "plastic_black")
    bar_t = T("bar", "steel_brushed")
    weight_t = T("weight", "rubber")

    hw, hd = w / 2.0, d / 2.0
    if style == 1:
        for sx in (-hw + 0.08, hw - 0.08):
            bx(g, sx - 0.04, -0.28, 0.0, sx + 0.04, 0.28, 0.05, frame_t)
            bx(g, sx - 0.04, -0.06, 0.05, sx + 0.04, 0.06, 0.86, frame_t)
        for zt in (0.42, 0.82):
            bx(g, -hw, -0.18, zt, hw, 0.18, zt + 0.05, frame_t)
            for k in range(4):
                dx = -hw + 0.24 + k * (w - 0.48) / 3.0
                tube(g, (dx, -0.14, zt + 0.11), (dx, 0.14, zt + 0.11), 0.02, 0.02, bar_t, seg=6)
                tube(g, (dx, -0.16, zt + 0.11), (dx, -0.10, zt + 0.11), 0.075, 0.075, weight_t, seg=8)
                tube(g, (dx, 0.10, zt + 0.11), (dx, 0.16, zt + 0.11), 0.075, 0.075, weight_t, seg=8)
        return g

    for sx in (-hw, hw - 0.07):
        bx(g, sx, -hd, 0.0, sx + 0.07, hd, 0.06, frame_t)
        bx(g, sx, -hd, h - 0.07, sx + 0.07, hd, h, frame_t)
        for sy in (-hd, hd - 0.07):
            bx(g, sx, sy, 0.06, sx + 0.07, sy + 0.07, h - 0.07, frame_t)
        bx(g, sx, -hd + 0.07, 0.78, sx + 0.07, hd - 0.07, 0.83, bar_t)
    bx(g, -hw, hd - 0.07, h - 0.07, hw, hd, h, frame_t)
    tube(g, (-hw, -hd + 0.04, h - 0.06), (hw, -hd + 0.04, h - 0.06), 0.022, 0.022, bar_t, seg=8)
    tube(g, (-hw - 0.35, -hd - 0.05, 1.35), (hw + 0.35, -hd - 0.05, 1.35), 0.018, 0.018, bar_t, seg=8)
    for sx in (-hw - 0.18, hw + 0.18):
        tube(g, (sx - 0.025, -hd - 0.05, 1.35), (sx + 0.025, -hd - 0.05, 1.35), 0.21, 0.21, weight_t, seg=12)
    return g


@register(BUILDERS)
def b_gym_treadmill(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.88))
    d = float(params.get("d", 1.85))
    style = int(params.get("style", 0))
    frame_t = T("frame", "steel_brushed")
    belt_t = T("belt", "rubber")
    cons_t = T("console", "plastic_black")
    scr_t = T("screen", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    if style == 1:
        bx(g, -0.26, -0.52, 0.0, 0.26, 0.52, 0.05, frame_t)
        tube(g, (-0.03, -0.28, 0.32), (0.03, -0.28, 0.32), 0.24, 0.24, frame_t, seg=12)
        tube(g, (0, 0.18, 0.05), (0, 0.25, 0.82), 0.03, 0.025, frame_t, seg=6)
        bx(g, -0.10, 0.16, 0.82, 0.10, 0.34, 0.88, belt_t)
        tube(g, (0, -0.24, 0.32), (0, -0.32, 1.08), 0.03, 0.025, frame_t, seg=6)
        bx(g, -0.22, -0.38, 1.06, 0.22, -0.22, 1.18, cons_t)
        return g

    bx(g, -hw, -hd, 0.0, hw, hd, 0.14, frame_t)
    bx(g, -hw + 0.09, -hd + 0.06, 0.14, hw - 0.09, hd - 0.38, 0.16, belt_t)
    bx(g, -hw, hd - 0.38, 0.14, hw, hd, 0.28, cons_t)
    for sx in (-hw + 0.05, hw - 0.05):
        tube(g, (sx, hd - 0.25, 0.14), (sx, hd - 0.35, 1.25), 0.035, 0.032, frame_t, seg=6)
        tube(g, (sx, hd - 0.35, 1.08), (sx, hd - 0.72, 1.02), 0.024, 0.024, belt_t, seg=6)
    obox(g, (0, hd - 0.34, 1.32), (w + 0.04, 0.34, 0.14), cons_t, rot=(0.42, 0, 0))
    obox(g, (0, hd - 0.36, 1.35), (w * 0.65, 0.22, 0.06), scr_t, rot=(0.42, 0, 0))
    return g


@register(BUILDERS)
def b_gym_weights(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    bar_t = T("bar", "steel_brushed")
    plate_t = T("plate", "rubber")
    accent_t = T("accent", "painted_metal_blue")

    if style in (0, 5):
        r = 0.225
        tube(g, (-0.98, 0, r), (0.98, 0, r), 0.018, 0.018, bar_t, seg=8)
        for s in (-1, 1):
            for off, pr, tag in ((0.62, r, plate_t), (0.68, r, accent_t), (0.74, r * 0.82, plate_t)):
                px = s * off
                tube(g, (px - 0.022, 0, r), (px + 0.022, 0, r), pr, pr, tag, seg=14)
    elif style in (1, 3):
        r = 0.095
        for sy in (-0.16, 0.16):
            tube(g, (-0.18, sy, r), (0.18, sy, r), 0.018, 0.018, bar_t, seg=8)
            for sx in (-0.14, 0.14):
                tube(g, (sx - 0.04, sy, r), (sx + 0.04, sy, r), r, r, plate_t, seg=6)
    else:
        for kx, kr, tag in ((-0.24, 0.11, plate_t), (0.0, 0.095, accent_t), (0.22, 0.08, plate_t)):
            ball(g, (kx, 0, kr), kr, tag, seg=10, rings=6)
            for sx in (-kr * 0.55, kr * 0.55):
                tube(g, (kx + sx, 0, kr * 1.7), (kx + sx * 0.85, 0, kr * 2.55), 0.016, 0.016, bar_t, seg=6)
            tube(g, (kx - kr * 0.5, 0, kr * 2.55), (kx + kr * 0.5, 0, kr * 2.55), 0.016, 0.016, bar_t, seg=6)
    return g


def entries():
    out = []
    paints = P.MAT["paint"]
    softs = P.MAT["soft"]

    # MEDICAL (36 entries)
    for i in range(6):
        out.append(mk(f"med_bed_{i + 1:02d}", "Medical", "b_med_bed",
                      params={"w": 0.96, "d": 2.08, "style": i},
                      atlas=ATLAS,
                      palette={"frame": "steel_brushed",
                               "mattress": pick(["painted_metal_blue", "painted_metal_green", "plastic_white"], i),
                               "sheet": "plastic_white", "wheel": "rubber"}))

    for i in range(6):
        out.append(mk(f"med_ivstand_{i + 1:02d}", "Medical", "b_med_ivstand",
                      params={"h": 1.80 + (i % 2) * 0.15, "style": i},
                      atlas=ATLAS,
                      palette={"pole": "steel_brushed", "bag": "glass_dirty",
                               "pump": pick(paints, i), "wheel": "plastic_black"}))

    for i in range(6):
        out.append(mk(f"med_monitor_{i + 1:02d}", "Medical", "b_med_monitor",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["plastic_white", "steel_brushed"], i),
                               "dark": "plastic_black", "screen": "glass_dirty", "pole": "steel_brushed"}))

    for i in range(6):
        out.append(mk(f"med_cabinet_{i + 1:02d}", "Medical", "b_med_cabinet",
                      params={"w": 0.92 + (i % 2) * 0.2, "d": 0.46, "h": 1.75, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["plastic_white", "steel_brushed", "painted_metal_green"], i),
                               "metal": "steel_brushed", "glass": "glass_dirty"}))

    for i in range(6):
        out.append(mk(f"med_wheelchair_{i + 1:02d}", "Medical", "b_med_wheelchair",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["steel_brushed", "painted_metal_blue", "plastic_black"], i),
                               "seat": pick(["leather_black", "fabric_blue", "fabric_green"], i),
                               "tire": "rubber"}))

    for i in range(6):
        out.append(mk(f"med_lab_{i + 1:02d}", "Medical", "b_med_lab",
                      params={"w": 1.5 + (i % 2) * 0.3, "d": 0.75, "h": 0.90, "style": i},
                      atlas=ATLAS,
                      palette={"cabinet": pick(["steel_brushed", "plastic_white", "painted_metal_blue"], i),
                               "top": "plastic_black", "equip": "plastic_white", "glass": "glass_dirty"}))

    # GYM (24 entries)
    for i in range(6):
        out.append(mk(f"gym_bench_{i + 1:02d}", "Gym", "b_gym_bench",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["steel_brushed", "plastic_black"], i),
                               "pad": pick(softs, i), "plate": "rubber"}))

    for i in range(6):
        out.append(mk(f"gym_rack_{i + 1:02d}", "Gym", "b_gym_rack",
                      params={"w": 1.35, "d": 1.25, "h": 2.20, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["plastic_black", "painted_metal_blue", "fabric_red"], i),
                               "bar": "steel_brushed", "weight": "rubber"}))

    for i in range(6):
        out.append(mk(f"gym_treadmill_{i + 1:02d}", "Gym", "b_gym_treadmill",
                      params={"w": 0.88, "d": 1.85, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["steel_brushed", "plastic_black"], i),
                               "belt": "rubber", "console": "plastic_black", "screen": "glass_dirty"}))

    for i in range(6):
        out.append(mk(f"gym_weights_{i + 1:02d}", "Gym", "b_gym_weights",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"bar": "steel_brushed", "plate": "rubber",
                               "accent": pick(["painted_metal_blue", "fabric_red", "painted_metal_green"], i)}))

    return out
