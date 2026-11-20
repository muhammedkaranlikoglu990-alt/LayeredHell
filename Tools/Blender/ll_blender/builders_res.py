"""Procedural Residential furniture & appliance builders (category 'Residential', atlas ENV_MAT_2K)."""
import math
from .builders_common import (
    register, mk, Tags, geo_for, bx, obox, tube, ball, pick
)
from . import palettes as P

BUILDERS = {}
ATLAS = "ENV_MAT_2K"


def _add_legs(g, x0, y0, x1, y1, h, r, tag):
    for lx in (x0 + r, x1 - r):
        for ly in (y0 + r, y1 - r):
            bx(g, lx - r, ly - r, 0.0, lx + r, ly + r, h, tag)


@register(BUILDERS)
def b_res_bed(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.5))
    d = float(params.get("d", 2.05))
    style = int(params.get("style", 0))
    wood_t = T("frame", "wood_dark")
    matt_t = T("mattress", "plastic_white")
    blanket_t = T("blanket", "fabric_blue")
    metal_t = T("metal", "steel_brushed")

    hw, hd = w / 2.0, d / 2.0
    _add_legs(g, -hw, -hd, hw, hd, 0.22, 0.045, metal_t if style == 2 else wood_t)
    # Bed frame & headboard
    bx(g, -hw, -hd, 0.18, hw, hd, 0.30, wood_t)
    bx(g, -hw, hd - 0.08, 0.18, hw, hd, 1.05, wood_t)
    if style in (1, 3, 5):
        # Footboard
        bx(g, -hw, -hd, 0.18, hw, -hd + 0.07, 0.62, wood_t)
    # Mattress & blanket
    bx(g, -hw + 0.04, -hd + 0.06, 0.30, hw - 0.04, hd - 0.08, 0.50, matt_t)
    bx(g, -hw + 0.03, -hd + 0.05, 0.32, hw - 0.03, hd * 0.38, 0.52, blanket_t)
    # Pillows
    n_pil = 1 if w < 1.2 else 2
    pw = (w - 0.24) / n_pil
    for i in range(n_pil):
        px = -hw + 0.12 + (i + 0.5) * pw
        obox(g, (px, hd - 0.28, 0.55), (pw * 0.85, 0.28, 0.11), matt_t, rot=(0.18, 0.0, 0.0))

    if style == 2:
        # Bunk bed upper tier
        zh = 1.05
        for lx in (-hw + 0.04, hw - 0.04):
            for ly in (-hd + 0.04, hd - 0.04):
                bx(g, lx - 0.04, ly - 0.04, 0.22, lx + 0.04, ly + 0.04, 1.75, metal_t)
        bx(g, -hw, -hd, zh + 0.18, hw, hd, zh + 0.28, wood_t)
        bx(g, -hw + 0.04, -hd + 0.06, zh + 0.28, hw - 0.04, hd - 0.06, zh + 0.44, blanket_t)
        # Guardrail & ladder
        bx(g, -hw, -hd * 0.6, zh + 0.44, -hw + 0.04, hd * 0.6, zh + 0.64, metal_t)
        for s in range(4):
            sz = 0.35 + s * 0.3
            bx(g, -hw - 0.05, -hd + 0.15, sz, -hw, -hd + 0.55, sz + 0.035, metal_t)
    elif style == 3:
        # Four-poster canopy posts
        for lx in (-hw + 0.05, hw - 0.05):
            for ly in (-hd + 0.05, hd - 0.05):
                bx(g, lx - 0.04, ly - 0.04, 0.22, lx + 0.04, ly + 0.04, 1.85, wood_t)
        bx(g, -hw, -hd, 1.80, hw, hd, 1.86, wood_t)
    return g


@register(BUILDERS)
def b_res_sofa(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.0))
    d = float(params.get("d", 0.9))
    style = int(params.get("style", 0))
    up_t = T("upholstery", "fabric_blue")
    trim_t = T("trim", "wood_dark")

    hw, hd = w / 2.0, d / 2.0
    _add_legs(g, -hw + 0.04, -hd + 0.04, hw - 0.04, hd - 0.04, 0.10, 0.04, trim_t)
    # Base plinth
    bx(g, -hw, -hd, 0.10, hw, hd, 0.28, up_t)
    # Backrest & armrests
    aw = 0.18
    bx(g, -hw, hd - 0.20, 0.28, hw, hd, 0.86, up_t)
    bx(g, -hw, -hd, 0.28, -hw + aw, hd, 0.64, up_t)
    bx(g, hw - aw, -hd, 0.28, hw, hd, 0.64, up_t)

    # Seat & back cushions
    n_cush = max(2, int(round(w / 0.65)))
    cw = (w - 2 * aw) / n_cush
    for i in range(n_cush):
        x0 = -hw + aw + i * cw + 0.015
        x1 = x0 + cw - 0.03
        bx(g, x0, -hd + 0.02, 0.28, x1, hd - 0.18, 0.44, up_t)
        obox(g, ((x0 + x1) / 2, hd - 0.24, 0.64), (cw - 0.04, 0.15, 0.36), up_t, rot=(-0.12, 0.0, 0.0))

    if style in (2, 4):
        # L-sectional chaise extension on right side
        cw_ext = min(0.75, w * 0.4)
        bx(g, hw - aw - cw_ext, -hd - 0.65, 0.10, hw, -hd, 0.44, up_t)
        _add_legs(g, hw - aw - cw_ext + 0.04, -hd - 0.62, hw - 0.04, -hd - 0.05, 0.10, 0.04, trim_t)
    return g


@register(BUILDERS)
def b_res_table(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.4))
    d = float(params.get("d", 0.85))
    h = float(params.get("h", 0.76))
    style = int(params.get("style", 0))
    top_t = T("top", "wood_light")
    leg_t = T("leg", "wood_dark")

    hw, hd = w / 2.0, d / 2.0
    if style == 2:
        # Round pedestal table
        r = min(hw, hd)
        tube(g, (0, 0, h - 0.05), (0, 0, h), r, r, top_t, seg=12)
        tube(g, (0, 0, 0.06), (0, 0, h - 0.05), 0.08, 0.07, leg_t, seg=8)
        tube(g, (0, 0, 0.0), (0, 0, 0.06), r * 0.55, r * 0.45, leg_t, seg=10)
    else:
        bx(g, -hw, -hd, h - 0.05, hw, hd, h, top_t)
        bx(g, -hw + 0.06, -hd + 0.06, h - 0.12, hw - 0.06, hd - 0.06, h - 0.05, leg_t)
        _add_legs(g, -hw + 0.04, -hd + 0.04, hw - 0.04, hd - 0.04, h - 0.05, 0.04, leg_t)
        if style in (1, 3, 5):
            # Lower shelf / drawer
            bx(g, -hw + 0.08, -hd + 0.08, 0.18, hw - 0.08, hd - 0.08, 0.22, top_t)
    return g


@register(BUILDERS)
def b_res_chair(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    frame_t = T("frame", "wood_dark")
    seat_t = T("seat", "fabric_red")

    sh = 0.65 if style == 2 else 0.45
    hw = 0.24
    _add_legs(g, -hw, -hw, hw, hw, sh, 0.025, frame_t)
    bx(g, -hw, -hw, sh, hw, hw, sh + 0.05, seat_t)

    if style == 2:
        # Barstool footrest ring
        bx(g, -hw, -hw, 0.22, hw, hw, 0.25, frame_t)
    else:
        # Backrest posts & slats
        bh = sh + 0.46
        bx(g, -hw, hw - 0.05, sh + 0.05, -hw + 0.045, hw, bh, frame_t)
        bx(g, hw - 0.045, hw - 0.05, sh + 0.05, hw, hw, bh, frame_t)
        bx(g, -hw, hw - 0.045, bh - 0.16, hw, hw, bh, seat_t if style in (1, 5) else frame_t)
        bx(g, -hw, hw - 0.045, sh + 0.14, hw, hw, sh + 0.24, frame_t)
        if style in (1, 3, 5):
            # Armrests
            for s in (-1, 1):
                ax = s * (hw - 0.02)
                bx(g, ax - 0.025, -hw + 0.05, sh + 0.05, ax + 0.025, -hw + 0.10, sh + 0.22, frame_t)
                bx(g, ax - 0.03, -hw + 0.03, sh + 0.22, ax + 0.03, hw, sh + 0.26, frame_t)
    return g


@register(BUILDERS)
def b_res_wardrobe(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.2))
    d = float(params.get("d", 0.58))
    h = float(params.get("h", 2.0))
    style = int(params.get("style", 0))
    wood_t = T("body", "wood_dark")
    trim_t = T("trim", "wood_light")
    metal_t = T("handle", "steel_brushed")

    hw, hd = w / 2.0, d / 2.0
    # Plinth & main carcass
    bx(g, -hw, -hd, 0.0, hw, hd, 0.08, trim_t)
    bx(g, -hw, -hd, 0.08, hw, hd, h - 0.06, wood_t)
    bx(g, -hw - 0.02, -hd - 0.02, h - 0.06, hw + 0.02, hd + 0.02, h, trim_t)

    if style == 3:
        # Chest of drawers (dresser)
        n_dr = 4
        dh = (h - 0.20) / n_dr
        for i in range(n_dr):
            z0 = 0.10 + i * dh
            bx(g, -hw + 0.04, -hd - 0.02, z0 + 0.015, hw - 0.04, -hd, z0 + dh - 0.015, trim_t)
            for s in (-1, 1):
                bx(g, s * hw * 0.45 - 0.05, -hd - 0.045, z0 + dh * 0.45, s * hw * 0.45 + 0.05, -hd - 0.02, z0 + dh * 0.58, metal_t)
    else:
        # Doors & lower drawer
        n_doors = 3 if style == 1 else 2
        dw = (w - 0.08) / n_doors
        for i in range(n_doors):
            x0 = -hw + 0.04 + i * dw + 0.008
            x1 = x0 + dw - 0.016
            tag = T("mirror", "glass_dirty") if (style == 1 and i == 1) else trim_t
            bx(g, x0, -hd - 0.02, 0.40, x1, -hd, h - 0.10, tag)
            hx = x1 - 0.06 if i == 0 else x0 + 0.06
            bx(g, hx - 0.012, -hd - 0.045, 0.95, hx + 0.012, -hd - 0.02, 1.15, metal_t)
        # Bottom drawer
        bx(g, -hw + 0.04, -hd - 0.02, 0.10, hw - 0.04, -hd, 0.37, trim_t)
    return g


@register(BUILDERS)
def b_res_shelf(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.1))
    d = float(params.get("d", 0.36))
    h = float(params.get("h", 1.85))
    tiers = int(params.get("tiers", 5))
    style = int(params.get("style", 0))
    wood_t = T("shelf", "wood_light")
    back_t = T("back", "wood_dark")
    book_t = T("accent", "fabric_red")

    hw, hd = w / 2.0, d / 2.0
    # Side panels
    bx(g, -hw, -hd, 0.0, -hw + 0.04, hd, h, wood_t)
    bx(g, hw - 0.04, -hd, 0.0, hw, hd, h, wood_t)
    if style % 2 == 0:
        bx(g, -hw + 0.04, hd - 0.02, 0.0, hw - 0.04, hd, h, back_t)

    for i in range(tiers):
        z = 0.04 + i * (h - 0.08) / max(1, tiers - 1)
        bx(g, -hw + 0.04, -hd, z, hw - 0.04, hd, z + 0.035, wood_t)
        if i < tiers - 1 and (i + style) % 2 == 0:
            # Book cluster on shelf
            bx(g, -hw + 0.08, -hd + 0.05, z + 0.035, -hw + 0.36, hd - 0.04, z + 0.24, book_t)
    if style in (3, 5):
        # Center vertical divider
        bx(g, -0.02, -hd + 0.02, 0.04, 0.02, hd - 0.02, h - 0.04, wood_t)
    return g


@register(BUILDERS)
def b_res_tv(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.3))
    style = int(params.get("style", 0))
    cab_t = T("console", "wood_dark")
    tv_t = T("bezel", "plastic_black")
    scr_t = T("screen", "glass_dirty")
    metal_t = T("stand", "steel_brushed")

    hw = w / 2.0
    # Media console table beneath TV
    ch = 0.48
    cd = 0.42
    _add_legs(g, -hw + 0.05, -cd / 2 + 0.04, hw - 0.05, cd / 2 - 0.04, 0.10, 0.03, metal_t)
    bx(g, -hw, -cd / 2, 0.10, hw, cd / 2, ch, cab_t)
    bx(g, -hw * 0.35, -cd / 2 - 0.01, 0.14, hw * 0.35, cd / 2, ch - 0.04, tv_t)

    if style == 1:
        # Retro CRT TV box
        bx(g, -0.36, -0.24, ch, 0.36, 0.24, ch + 0.56, tv_t)
        bx(g, -0.30, -0.25, ch + 0.07, 0.22, -0.23, ch + 0.49, scr_t)
    else:
        # Flatscreen TV + stand neck
        tw = w * 0.85
        th = tw * 0.56
        bx(g, -0.22, -0.12, ch, 0.22, 0.12, ch + 0.025, metal_t)
        bx(g, -0.04, -0.02, ch + 0.025, 0.04, 0.04, ch + 0.14, metal_t)
        bx(g, -tw / 2, -0.03, ch + 0.12, tw / 2, 0.03, ch + 0.12 + th, tv_t)
        bx(g, -tw / 2 + 0.03, -0.036, ch + 0.15, tw / 2 - 0.03, -0.028, ch + 0.09 + th, scr_t)
        if style in (2, 5):
            # Side speakers / soundbar
            bx(g, -tw * 0.35, -0.16, ch, tw * 0.35, -0.10, ch + 0.06, tv_t)
    return g


@register(BUILDERS)
def b_res_fridge(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.82))
    d = float(params.get("d", 0.76))
    h = float(params.get("h", 1.85))
    style = int(params.get("style", 0))
    body_t = T("body", "steel_brushed")
    trim_t = T("trim", "plastic_black")
    in_t = T("inside", "plastic_white")

    hw, hd = w / 2.0, d / 2.0
    # Feet & main insulated box
    _add_legs(g, -hw + 0.04, -hd + 0.04, hw - 0.04, hd - 0.04, 0.04, 0.035, trim_t)
    bx(g, -hw, -hd, 0.04, hw, hd, h, body_t)

    if style == 1:
        # French door double refrigerator
        for s in (-1, 1):
            x0 = -hw + 0.01 if s == -1 else 0.008
            x1 = -0.008 if s == -1 else hw - 0.01
            bx(g, x0, -hd - 0.04, 0.65, x1, -hd, h - 0.02, body_t)
            hx = s * 0.045
            bx(g, hx - 0.012, -hd - 0.075, 0.90, hx + 0.012, -hd - 0.04, 1.50, trim_t)
        # Bottom freezer drawer
        bx(g, -hw + 0.01, -hd - 0.04, 0.06, hw - 0.01, -hd, 0.63, body_t)
        bx(g, -hw * 0.6, -hd - 0.075, 0.52, hw * 0.6, -hd - 0.04, 0.56, trim_t)
    else:
        # Top freezer & lower main door
        split_z = h * 0.68
        bx(g, -hw + 0.01, -hd - 0.04, 0.06, hw - 0.01, -hd, split_z - 0.015, body_t)
        bx(g, -hw + 0.01, -hd - 0.04, split_z + 0.015, hw - 0.01, -hd, h - 0.02, body_t)
        # Handles
        bx(g, hw - 0.08, -hd - 0.075, split_z - 0.45, hw - 0.05, -hd - 0.04, split_z - 0.08, trim_t)
        bx(g, hw - 0.08, -hd - 0.075, split_z + 0.06, hw - 0.05, -hd - 0.04, split_z + 0.32, trim_t)
        if style == 5:
            # Water/ice dispenser recess
            bx(g, -hw * 0.55, -hd - 0.045, 0.95, -0.05, -hd - 0.02, 1.25, trim_t)
    return g


@register(BUILDERS)
def b_res_stove(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.78))
    d = float(params.get("d", 0.68))
    h = float(params.get("h", 0.90))
    style = int(params.get("style", 0))
    body_t = T("body", "steel_brushed")
    dark_t = T("dark", "plastic_black")
    glass_t = T("glass", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    bx(g, -hw, -hd, 0.0, hw, hd, h, body_t)
    # Backsplash control console
    bx(g, -hw, hd - 0.10, h, hw, hd, h + 0.18, body_t)
    # Oven door + window + handle
    bx(g, -hw + 0.04, -hd - 0.03, 0.14, hw - 0.04, -hd, h - 0.14, body_t)
    bx(g, -hw + 0.12, -hd - 0.035, 0.24, hw - 0.12, -hd - 0.025, h - 0.26, glass_t)
    bx(g, -hw + 0.08, -hd - 0.07, h - 0.20, hw - 0.08, -hd - 0.03, h - 0.17, dark_t)

    # 4 burner rings on top surface
    for bx_pos in (-hw * 0.48, hw * 0.48):
        for by_pos in (-hd * 0.42, hd * 0.28):
            tube(g, (bx_pos, by_pos, h), (bx_pos, by_pos, h + 0.025), 0.11, 0.10, dark_t, seg=8)
    # Control knobs along front top edge
    for k in range(5):
        kx = -hw * 0.7 + k * (w * 0.35)
        tube(g, (kx, -hd, h - 0.06), (kx, -hd - 0.03, h - 0.06), 0.022, 0.020, dark_t, seg=6)
    return g


@register(BUILDERS)
def b_res_sink(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.95))
    d = float(params.get("d", 0.58))
    h = float(params.get("h", 0.88))
    style = int(params.get("style", 0))
    cab_t = T("cabinet", "wood_dark")
    top_t = T("top", "plastic_white")
    metal_t = T("faucet", "steel_brushed")

    hw, hd = w / 2.0, d / 2.0
    if style == 2:
        # Pedestal bathroom sink
        tube(g, (0, 0, 0.0), (0, 0, h - 0.18), 0.16, 0.12, top_t, seg=10)
    else:
        # Vanity cabinet with doors
        bx(g, -hw, -hd, 0.0, hw, hd, h - 0.16, cab_t)
        for s in (-1, 1):
            bx(g, (-hw + 0.03) if s == -1 else 0.01, -hd - 0.02, 0.08,
               -0.01 if s == -1 else (hw - 0.03), -hd, h - 0.18, cab_t)

    # Basin rim walls (hollow sink bowl)
    rw = 0.07
    bx(g, -hw, -hd, h - 0.16, hw, hd, h - 0.12, top_t)
    bx(g, -hw, -hd, h - 0.12, hw, -hd + rw, h, top_t)
    bx(g, -hw, hd - rw * 1.4, h - 0.12, hw, hd, h, top_t)
    bx(g, -hw, -hd + rw, h - 0.12, -hw + rw, hd - rw * 1.4, h, top_t)
    bx(g, hw - rw, -hd + rw, h - 0.12, hw, hd - rw * 1.4, h, top_t)
    # Gooseneck faucet & handles
    fy = hd - rw * 0.7
    tube(g, (0, fy, h), (0, fy, h + 0.18), 0.02, 0.018, metal_t, seg=6)
    tube(g, (0, fy, h + 0.18), (0, fy - 0.14, h + 0.15), 0.018, 0.016, metal_t, seg=6)
    for s in (-1, 1):
        tube(g, (s * 0.10, fy, h), (s * 0.10, fy, h + 0.05), 0.022, 0.022, metal_t, seg=6)
    if style in (1, 5):
        # Mirror panel above vanity
        bx(g, -hw * 0.85, hd - 0.04, h + 0.20, hw * 0.85, hd, h + 0.95, T("mirror", "glass_dirty"))
    return g


@register(BUILDERS)
def b_res_toilet(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    cer_t = T("ceramic", "plastic_white")
    seat_t = T("seat", "plastic_white")
    metal_t = T("handle", "steel_brushed")

    # Pedestal foot & oval bowl
    tube(g, (0, -0.02, 0.0), (0, -0.02, 0.22), 0.15, 0.18, cer_t, seg=10, squash=(0.85, 1.15))
    tube(g, (0, -0.05, 0.22), (0, -0.05, 0.42), 0.18, 0.22, cer_t, seg=10, squash=(0.85, 1.25))
    # Rear bridge to tank
    bx(g, -0.17, 0.05, 0.0, 0.17, 0.26, 0.42, cer_t)
    # Water tank & lid
    bx(g, -0.21, 0.12, 0.42, 0.21, 0.28, 0.78, cer_t)
    bx(g, -0.22, 0.11, 0.78, 0.22, 0.29, 0.82, cer_t)
    # Flush lever
    bx(g, -0.25, 0.16, 0.72, -0.21, 0.19, 0.75, metal_t)
    # Toilet seat / lid
    if style in (1, 4):
        # Lid raised upright
        obox(g, (0, 0.09, 0.62), (0.36, 0.03, 0.40), seat_t, rot=(-0.15, 0.0, 0.0))
    else:
        tube(g, (0, -0.05, 0.42), (0, -0.05, 0.45), 0.22, 0.22, seat_t, seg=10, squash=(0.85, 1.25))
    return g


@register(BUILDERS)
def b_res_bath(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.65))
    d = float(params.get("d", 0.78))
    h = float(params.get("h", 0.58))
    style = int(params.get("style", 0))
    tub_t = T("tub", "plastic_white")
    metal_t = T("faucet", "steel_brushed")
    glass_t = T("glass", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    if style in (3, 4):
        # Walk-in shower enclosure stall
        sw, sd, sh = 0.95, 0.95, 2.05
        bx(g, -sw / 2, -sd / 2, 0.0, sw / 2, sd / 2, 0.12, tub_t)
        bx(g, -sw / 2, sd / 2 - 0.04, 0.12, sw / 2, sd / 2, sh, tub_t)
        bx(g, -sw / 2, -sd / 2, 0.12, -sw / 2 + 0.04, sd / 2, sh, glass_t)
        bx(g, -sw / 2, -sd / 2, 0.12, sw / 2, -sd / 2 + 0.04, sh, glass_t)
        tube(g, (0, sd / 2 - 0.04, 1.1), (0, sd / 2 - 0.04, 1.9), 0.018, 0.018, metal_t, seg=6)
        tube(g, (0, sd / 2 - 0.22, 1.88), (0, sd / 2 - 0.22, 1.92), 0.12, 0.10, metal_t, seg=8)
    else:
        # Bathtub with hollow basin walls
        z0 = 0.08 if style == 0 else 0.0
        if style == 0:
            _add_legs(g, -hw + 0.1, -hd + 0.08, hw - 0.1, hd - 0.08, 0.08, 0.04, metal_t)
        rw = 0.08
        bx(g, -hw, -hd, z0, hw, hd, z0 + 0.12, tub_t)
        bx(g, -hw, -hd, z0 + 0.12, hw, -hd + rw, h, tub_t)
        bx(g, -hw, hd - rw, z0 + 0.12, hw, hd, h, tub_t)
        bx(g, -hw, -hd + rw, z0 + 0.12, -hw + rw, hd - rw, h, tub_t)
        bx(g, hw - rw, -hd + rw, z0 + 0.12, hw, hd - rw, h, tub_t)
        # Faucet on right rim
        tube(g, (hw - 0.04, 0.0, h), (hw - 0.04, 0.0, h + 0.16), 0.022, 0.02, metal_t, seg=6)
        tube(g, (hw - 0.04, 0.0, h + 0.15), (hw - 0.18, 0.0, h + 0.12), 0.02, 0.018, metal_t, seg=6)
    return g


@register(BUILDERS)
def b_res_lamp(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    h = float(params.get("h", 1.60))
    style = int(params.get("style", 0))
    stem_t = T("stem", "steel_brushed")
    shade_t = T("shade", "fabric_red")

    if style == 1:
        # Tripod floor lamp
        for k in range(3):
            ang = 2 * math.pi * k / 3.0
            tube(g, (0.24 * math.cos(ang), 0.24 * math.sin(ang), 0.0), (0, 0, h - 0.30), 0.018, 0.015, stem_t, seg=6)
    else:
        tube(g, (0, 0, 0.0), (0, 0, 0.04), 0.18, 0.16, stem_t, seg=10)
        tube(g, (0, 0, 0.04), (0, 0, h - 0.28), 0.02, 0.018, stem_t, seg=6)
    # Lamp shade frustum + bulb
    tube(g, (0, 0, h - 0.32), (0, 0, h), 0.24, 0.15, shade_t, seg=10)
    ball(g, (0, 0, h - 0.22), 0.05, T("bulb", "plastic_white"), seg=6, rings=4)
    return g


@register(BUILDERS)
def b_res_rug(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.4))
    d = float(params.get("d", 1.6))
    style = int(params.get("style", 0))
    main_t = T("main", "fabric_red")
    border_t = T("border", "fabric_blue")
    fringe_t = T("fringe", "wood_light")

    hw, hd = w / 2.0, d / 2.0
    if style == 2:
        # Round layered rug
        r = min(hw, hd)
        tube(g, (0, 0, 0.0), (0, 0, 0.015), r, r, border_t, seg=14)
        tube(g, (0, 0, 0.015), (0, 0, 0.024), r * 0.78, r * 0.78, main_t, seg=14)
    else:
        bx(g, -hw, -hd, 0.0, hw, hd, 0.014, border_t)
        bx(g, -hw + 0.14, -hd + 0.14, 0.014, hw - 0.14, hd - 0.14, 0.022, main_t)
        # Decorative end fringes
        bx(g, -hw - 0.05, -hd + 0.04, 0.0, -hw, hd - 0.04, 0.008, fringe_t)
        bx(g, hw, -hd + 0.04, 0.0, hw + 0.05, hd - 0.04, 0.008, fringe_t)
    return g


def entries():
    out = []
    woods = P.MAT["wood"]
    fabrics = P.MAT["fabric"]
    softs = P.MAT["soft"]
    metals = P.MAT["metal"]
    plastics = P.MAT["plastic"]

    # 1. res_bed_01..06
    for i in range(6):
        out.append(mk(f"res_bed_{i + 1:02d}", "Residential", "b_res_bed",
                      params={"w": 1.05 if i in (0, 2) else 1.65, "d": 2.05, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(woods, i), "mattress": "plastic_white",
                               "blanket": pick(fabrics, i), "metal": pick(metals, i)}))

    # 2. res_sofa_01..06
    for i in range(6):
        out.append(mk(f"res_sofa_{i + 1:02d}", "Residential", "b_res_sofa",
                      params={"w": 1.5 + (i % 3) * 0.45, "d": 0.88, "style": i},
                      atlas=ATLAS,
                      palette={"upholstery": pick(softs, i), "trim": pick(woods, i)}))

    # 3. res_table_01..06
    for i in range(6):
        out.append(mk(f"res_table_{i + 1:02d}", "Residential", "b_res_table",
                      params={"w": 1.4 if i % 2 == 0 else 1.0, "d": 0.85 if i != 2 else 1.0,
                              "h": 0.45 if i == 1 else 0.76, "style": i},
                      atlas=ATLAS,
                      palette={"top": pick(woods, i), "leg": pick(woods + metals, i + 1)}))

    # 4. res_chair_01..06
    for i in range(6):
        out.append(mk(f"res_chair_{i + 1:02d}", "Residential", "b_res_chair",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(woods + metals, i), "seat": pick(softs, i)}))

    # 5. res_wardrobe_01..06
    for i in range(6):
        out.append(mk(f"res_wardrobe_{i + 1:02d}", "Residential", "b_res_wardrobe",
                      params={"w": 1.1 + (i % 3) * 0.25, "d": 0.58, "h": 1.1 if i == 3 else 2.0, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(woods, i), "trim": pick(woods, i + 1), "handle": "steel_brushed"}))

    # 6. res_shelf_01..06
    for i in range(6):
        out.append(mk(f"res_shelf_{i + 1:02d}", "Residential", "b_res_shelf",
                      params={"w": 0.9 + (i % 3) * 0.25, "d": 0.36, "h": 1.2 + (i % 3) * 0.35,
                              "tiers": 4 + (i % 2), "style": i},
                      atlas=ATLAS,
                      palette={"shelf": pick(woods + metals, i), "back": pick(woods, i + 1),
                               "accent": pick(fabrics, i)}))

    # 7. res_tv_01..06
    for i in range(6):
        out.append(mk(f"res_tv_{i + 1:02d}", "Residential", "b_res_tv",
                      params={"w": 1.2 + (i % 3) * 0.25, "style": i},
                      atlas=ATLAS,
                      palette={"console": pick(woods, i), "bezel": "plastic_black",
                               "screen": "glass_dirty", "stand": "steel_brushed"}))

    # 8. res_fridge_01..06
    for i in range(6):
        out.append(mk(f"res_fridge_{i + 1:02d}", "Residential", "b_res_fridge",
                      params={"w": 0.95 if i == 1 else 0.78, "d": 0.74, "h": 1.1 if i == 2 else 1.85, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["steel_brushed", "plastic_white", "plastic_black", "painted_metal_green"], i),
                               "trim": "plastic_black", "inside": "plastic_white"}))

    # 9. res_stove_01..06
    for i in range(6):
        out.append(mk(f"res_stove_{i + 1:02d}", "Residential", "b_res_stove",
                      params={"w": 0.75 + (i % 2) * 0.15, "d": 0.68, "h": 0.90, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["steel_brushed", "plastic_white", "plastic_black", "rust_heavy"], i),
                               "dark": "plastic_black", "glass": "glass_dirty"}))

    # 10. res_sink_01..06
    for i in range(6):
        out.append(mk(f"res_sink_{i + 1:02d}", "Residential", "b_res_sink",
                      params={"w": 0.65 if i == 2 else 0.95, "d": 0.56, "h": 0.88, "style": i},
                      atlas=ATLAS,
                      palette={"cabinet": pick(woods, i), "top": pick(plastics + ["steel_brushed"], i),
                               "faucet": "steel_brushed"}))

    # 11. res_toilet_01..06
    for i in range(6):
        out.append(mk(f"res_toilet_{i + 1:02d}", "Residential", "b_res_toilet",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"ceramic": pick(["plastic_white", "plastic_white", "steel_brushed"], i),
                               "seat": pick(["plastic_white", "wood_dark", "plastic_black"], i),
                               "handle": "steel_brushed"}))

    # 12. res_bath_01..06
    for i in range(6):
        out.append(mk(f"res_bath_{i + 1:02d}", "Residential", "b_res_bath",
                      params={"w": 1.65, "d": 0.78, "h": 0.58, "style": i},
                      atlas=ATLAS,
                      palette={"tub": pick(["plastic_white", "plastic_white", "steel_brushed"], i),
                               "faucet": "steel_brushed", "glass": "glass_dirty"}))

    # 13. res_lamp_01..06
    for i in range(6):
        out.append(mk(f"res_lamp_{i + 1:02d}", "Residential", "b_res_lamp",
                      params={"h": 0.65 if i == 2 else 1.60, "style": i},
                      atlas=ATLAS,
                      palette={"stem": pick(["steel_brushed", "wood_dark", "plastic_black"], i),
                               "shade": pick(fabrics + ["plastic_white"], i)}))

    # 14. res_rug_01..06
    for i in range(6):
        out.append(mk(f"res_rug_{i + 1:02d}", "Residential", "b_res_rug",
                      params={"w": 2.2 + (i % 3) * 0.4, "d": 1.4 + (i % 2) * 0.4, "style": i},
                      atlas=ATLAS,
                      palette={"main": pick(fabrics + ["leather_brown"], i),
                               "border": pick(fabrics + ["wood_dark"], i + 1),
                               "fringe": "wood_light"}))

    return out
