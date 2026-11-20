"""Procedural Architecture 3D builders (category 'Architecture', atlas ENV_STRUCT_2K)."""
import math
from .builders_common import (
    register, mk, Tags, geo_for, bx, obox, prism, tube, pick
)
from . import palettes as P

BUILDERS = {}
ATLAS = "ENV_STRUCT_2K"


@register(BUILDERS)
def b_arch_wall(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 4.0))
    h = float(params.get("h", 3.0))
    th = float(params.get("th", 0.24))
    style = int(params.get("style", 0))
    wall_t = T("wall", "plaster_white")
    trim_t = T("trim", "concrete_dark")
    wain_t = T("wainscot", "parquet_oak")

    hw = w / 2.0
    hth = th / 2.0
    # Main wall body
    bx(g, -hw, -hth, 0.0, hw, hth, h, wall_t)
    # Baseboard front & back
    bx(g, -hw, -hth - 0.02, 0.0, hw, hth + 0.02, 0.14, trim_t)
    # Crown molding
    bx(g, -hw, -hth - 0.025, h - 0.12, hw, hth + 0.025, h, trim_t)

    if style in (1, 3, 5):
        # Wainscoting lower panel + chair rail
        bx(g, -hw, -hth - 0.012, 0.14, hw, hth + 0.012, 1.05, wain_t)
        bx(g, -hw, -hth - 0.022, 1.05, hw, hth + 0.022, 1.12, trim_t)
    if style in (2, 3, 6):
        # Vertical pilasters / studs
        n_pil = max(2, int(round(w / 1.3)) + 1)
        for i in range(n_pil):
            px = -hw + 0.12 + i * (w - 0.24) / max(1, n_pil - 1)
            bx(g, px - 0.08, -hth - 0.03, 0.0, px + 0.08, hth + 0.03, h, trim_t)
    if style in (4, 5, 7):
        # Decorative recessed/raised wall panels
        cols = max(2, int(w))
        seg_w = (w - 0.4) / cols
        z_lo = 1.25 if style == 5 else 0.35
        for c in range(cols):
            x0 = -hw + 0.2 + c * seg_w + 0.08
            x1 = x0 + seg_w - 0.16
            bx(g, x0, -hth - 0.015, z_lo, x1, hth + 0.015, h - 0.3, wain_t)
    return g


@register(BUILDERS)
def b_arch_wallbroken(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 4.0))
    h = float(params.get("h", 3.0))
    th = float(params.get("th", 0.24))
    style = int(params.get("style", 0))
    wall_t = T("wall", "wall_damaged")
    core_t = T("core", "brick_red")
    rebar_t = T("rebar", "metal_plate")

    hw = w / 2.0
    hth = th / 2.0
    # Lower intact wall apron
    low_h = 0.45 + (style % 3) * 0.2
    bx(g, -hw, -hth, 0.0, hw, hth, low_h, wall_t)
    # Left & right jagged pillars
    lw = 0.7 + (style % 2) * 0.25
    rw = 0.65 + ((style + 1) % 3) * 0.2
    bx(g, -hw, -hth, low_h, -hw + lw, hth, h, wall_t)
    bx(g, hw - rw, -hth, low_h, hw, hth, h, wall_t)
    # Top lintel remnant
    if style % 2 == 0:
        bx(g, -hw + lw, -hth, h - 0.55, hw - rw, hth, h, wall_t)

    # Jagged stepped brick teeth along the breach edges
    steps = 4
    sh = (h - 0.55 - low_h) / steps
    for s in range(steps):
        z0 = low_h + s * sh
        z1 = z0 + sh * 0.85
        j_l = 0.18 + ((s + style) % 3) * 0.1
        j_r = 0.16 + ((s * 2 + style) % 3) * 0.1
        bx(g, -hw + lw, -hth * 0.8, z0, -hw + lw + j_l, hth * 0.8, z1, core_t)
        bx(g, hw - rw - j_r, -hth * 0.8, z0, hw - rw, hth * 0.8, z1, core_t)

    # Exposed rebar rods across gap
    for r in range(3):
        rz = low_h + 0.35 + r * 0.55
        tube(g, (-hw + lw - 0.05, 0.0, rz), (-hw + lw + 0.45, 0.02 * (r - 1), rz + 0.08), 0.016, 0.014, rebar_t, seg=5)
        tube(g, (hw - rw + 0.05, 0.0, rz + 0.1), (hw - rw - 0.4, -0.02 * r, rz - 0.05), 0.016, 0.014, rebar_t, seg=5)

    # Rubble chunks on floor at z >= 0.0
    for k in range(5):
        rx = -hw * 0.5 + k * (w * 0.22)
        ry = -0.25 if k % 2 == 0 else 0.22
        obox(g, (rx, ry, 0.09), (0.28, 0.22, 0.18), core_t, rot=(0.0, 0.0, 0.35 * k))
    return g


@register(BUILDERS)
def b_arch_floor(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 4.0))
    d = float(params.get("d", 4.0))
    th = float(params.get("th", 0.16))
    style = int(params.get("style", 0))
    main_t = T("floor", "parquet_oak")
    border_t = T("border", "marble_white")
    sub_t = T("sub", "concrete_dark")

    hw, hd = w / 2.0, d / 2.0
    # Subfloor slab
    bx(g, -hw, -hd, 0.0, hw, hd, th * 0.75, sub_t)

    if style == 0:
        bx(g, -hw, -hd, th * 0.75, hw, hd, th, main_t)
    elif style in (1, 2):
        # Bordered floor tile with center inlay
        bw = 0.25
        bx(g, -hw, -hd, th * 0.75, hw, -hd + bw, th, border_t)
        bx(g, -hw, hd - bw, th * 0.75, hw, hd, th, border_t)
        bx(g, -hw, -hd + bw, th * 0.75, -hw + bw, hd - bw, th, border_t)
        bx(g, hw - bw, -hd + bw, th * 0.75, hw, hd - bw, th, border_t)
        bx(g, -hw + bw, -hd + bw, th * 0.75, hw - bw, hd - bw, th, main_t)
    elif style in (3, 4):
        # Grid of modular floor panels with metal strips
        grid = 2 if style == 3 else 3
        pw = w / grid
        pd = d / grid
        for ix in range(grid):
            for iy in range(grid):
                x0 = -hw + ix * pw
                y0 = -hd + iy * pd
                tag = main_t if (ix + iy) % 2 == 0 else border_t
                bx(g, x0 + 0.02, y0 + 0.02, th * 0.75, x0 + pw - 0.02, y0 + pd - 0.02, th, tag)
    else:
        # Industrial / hatch / drain floor
        bx(g, -hw, -hd, th * 0.75, hw, hd, th, main_t)
        bx(g, -0.45, -0.45, th, 0.45, 0.45, th + 0.02, border_t)
        for i in range(-2, 3):
            bx(g, -0.38, i * 0.14 - 0.02, th + 0.02, 0.38, i * 0.14 + 0.02, th + 0.035, sub_t)
    return g


@register(BUILDERS)
def b_arch_ceiling(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 4.0))
    d = float(params.get("d", 4.0))
    style = int(params.get("style", 0))
    pan_t = T("ceiling", "ceiling_panel")
    frame_t = T("frame", "metal_plate")
    beam_t = T("beam", "concrete_raw")

    hw, hd = w / 2.0, d / 2.0
    # Build sitting on z=0.0 so lo[2] >= 0.0 contract holds cleanly
    bx(g, -hw, -hd, 0.08, hw, hd, 0.22, pan_t)
    # T-bar grid ribs on bottom side (z = 0.0 .. 0.08)
    grid = 2 if style % 2 == 0 else 3
    for i in range(grid + 1):
        x = -hw + i * (w / grid)
        y = -hd + i * (d / grid)
        bx(g, max(-hw, x - 0.03), -hd, 0.0, min(hw, x + 0.03), hd, 0.08, frame_t)
        bx(g, -hw, max(-hd, y - 0.03), 0.0, hw, min(hd, y + 0.03), 0.08, frame_t)

    if style in (1, 3, 5):
        # Recessed light troffer / beam ribs on top
        bx(g, -0.6, -0.3, 0.02, 0.6, 0.3, 0.26, T("light", "tile_white"))
    if style in (2, 4, 5):
        # Structural concrete cross-beams
        bx(g, -hw, -0.18, 0.0, hw, 0.18, 0.32, beam_t)
    return g


@register(BUILDERS)
def b_arch_door(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.2))
    h = float(params.get("h", 2.3))
    style = int(params.get("style", 0))
    frame_t = T("frame", "concrete_dark")
    leaf_t = T("door", "parquet_oak")
    metal_t = T("metal", "metal_plate")
    glass_t = T("glass", "tile_white")

    hw = w / 2.0
    fw = 0.09
    fd = 0.18
    # Door frame jambs + header + threshold
    bx(g, -hw, -fd / 2, 0.0, -hw + fw, fd / 2, h, frame_t)
    bx(g, hw - fw, -fd / 2, 0.0, hw, fd / 2, h, frame_t)
    bx(g, -hw, -fd / 2, h - fw, hw, fd / 2, h, frame_t)
    bx(g, -hw, -fd / 2, 0.0, hw, fd / 2, 0.025, metal_t)

    iw = w - 2 * fw
    ih = h - fw - 0.025
    if style in (2, 5):
        # Double door leaves
        lw = iw / 2.0 - 0.008
        for side in (-1, 1):
            cx = side * (lw / 2.0 + 0.004)
            bx(g, cx - lw / 2, -0.025, 0.025, cx + lw / 2, 0.025, 0.025 + ih, leaf_t)
            # Vision panel
            bx(g, cx - lw * 0.25, -0.032, 1.35, cx + lw * 0.25, 0.032, 1.85, glass_t)
            # Handle
            hx = cx - side * (lw * 0.35)
            bx(g, hx - 0.015, -0.06, 0.95, hx + 0.015, 0.06, 1.12, metal_t)
    elif style == 3:
        # Ajar door swung open 35 deg
        pivot_x = -hw + fw + 0.02
        ang = 0.55
        cx = pivot_x + (iw / 2.0) * math.cos(ang)
        cy = -(iw / 2.0) * math.sin(ang)
        obox(g, (cx, cy, 0.025 + ih / 2.0), (iw, 0.05, ih), leaf_t, rot=(0.0, 0.0, -ang))
    else:
        # Single door leaf with decorative recessed/raised panels
        bx(g, -iw / 2, -0.025, 0.025, iw / 2, 0.025, 0.025 + ih, leaf_t)
        if style in (0, 1, 4):
            bx(g, -iw * 0.34, -0.033, 0.22, iw * 0.34, 0.033, 0.95, frame_t if style == 4 else leaf_t)
            bx(g, -iw * 0.34, -0.033, 1.15, iw * 0.34, 0.033, ih - 0.15, glass_t if style == 1 else leaf_t)
        elif style == 6:
            # Heavy security door with horizontal reinforcement bars
            for z in (0.4, 1.0, 1.6, 2.0):
                bx(g, -iw * 0.45, -0.04, z, iw * 0.45, 0.04, z + 0.08, metal_t)
        # Door handle lever
        bx(g, iw * 0.35, -0.065, 1.0, iw * 0.35 + 0.04, 0.065, 1.04, metal_t)
        bx(g, iw * 0.26, -0.065, 1.0, iw * 0.38, -0.045, 1.03, metal_t)
    return g


@register(BUILDERS)
def b_arch_window(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.6))
    h = float(params.get("h", 1.5))
    style = int(params.get("style", 0))
    frame_t = T("frame", "concrete_raw")
    sill_t = T("sill", "marble_white")
    pane_t = T("pane", "tile_white")
    bar_t = T("bar", "metal_plate")

    hw = w / 2.0
    fd = 0.16
    fw = 0.08
    # Window sill at z=0.0
    bx(g, -hw - 0.06, -fd / 2 - 0.04, 0.0, hw + 0.06, fd / 2 + 0.04, 0.07, sill_t)
    # Outer frame
    bx(g, -hw, -fd / 2, 0.07, -hw + fw, fd / 2, h, frame_t)
    bx(g, hw - fw, -fd / 2, 0.07, hw, fd / 2, h, frame_t)
    bx(g, -hw, -fd / 2, h - fw, hw, fd / 2, h, frame_t)
    # Glass pane
    bx(g, -hw + fw, -0.015, 0.07, hw - fw, 0.015, h - fw, pane_t)

    # Mullions / transoms
    cols = 2 if style in (0, 2, 4) else 3
    for c in range(1, cols):
        mx = -hw + fw + c * (w - 2 * fw) / cols
        bx(g, mx - 0.025, -0.035, 0.07, mx + 0.025, 0.035, h - fw, frame_t)
    if style in (1, 2, 3, 6):
        hz = 0.07 + (h - 0.07 - fw) * (0.65 if style == 3 else 0.5)
        bx(g, -hw + fw, -0.035, hz - 0.025, hw - fw, 0.035, hz + 0.025, frame_t)
    if style in (4, 5):
        # Security bars
        for b in range(4):
            bx_pos = -hw + 0.25 + b * (w - 0.5) / 3.0
            tube(g, (bx_pos, -0.055, 0.07), (bx_pos, -0.055, h - fw), 0.015, 0.015, bar_t, seg=6)
    return g


@register(BUILDERS)
def b_arch_pillar(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.55))
    h = float(params.get("h", 3.0))
    style = int(params.get("style", 0))
    main_t = T("pillar", "concrete_raw")
    trim_t = T("trim", "concrete_dark")
    metal_t = T("metal", "metal_plate")

    hw = w / 2.0
    # Base plinth & top capital
    bx(g, -hw * 1.18, -hw * 1.18, 0.0, hw * 1.18, hw * 1.18, 0.22, trim_t)
    bx(g, -hw * 1.18, -hw * 1.18, h - 0.22, hw * 1.18, hw * 1.18, h, trim_t)

    if style == 0:
        bx(g, -hw, -hw, 0.22, hw, hw, h - 0.22, main_t)
    elif style == 1:
        # Chamfered / octagonal column
        c = hw * 0.42
        pts = [(-hw + c, -hw), (hw - c, -hw), (hw, -hw + c), (hw, hw - c),
               (hw - c, hw), (-hw + c, hw), (-hw, hw - c), (-hw, -hw + c)]
        prism(g, pts, 0.22, h - 0.22, main_t)
    elif style == 2:
        # Round column
        tube(g, (0, 0, 0.22), (0, 0, h - 0.22), hw * 0.95, hw * 0.88, main_t, seg=12)
    elif style == 3:
        # Industrial steel I-beam pillar
        fl = hw * 0.9
        th = 0.06
        bx(g, -fl, -fl, 0.22, fl, -fl + th, h - 0.22, metal_t)
        bx(g, -fl, fl - th, 0.22, fl, fl, h - 0.22, metal_t)
        bx(g, -th / 2, -fl + th, 0.22, th / 2, fl - th, h - 0.22, metal_t)
    elif style == 4:
        # Fluted / banded square column
        bx(g, -hw, -hw, 0.22, hw, hw, h - 0.22, main_t)
        for z in (0.9, 1.5, 2.1):
            bx(g, -hw * 1.08, -hw * 1.08, z - 0.05, hw * 1.08, hw * 1.08, z + 0.05, metal_t)
    else:
        # Damaged pillar with exposed core and rebar
        bx(g, -hw, -hw, 0.22, hw, hw, 1.25, main_t)
        bx(g, -hw * 0.72, -hw * 0.72, 1.25, hw * 0.72, hw * 0.72, 1.95, T("core", "brick_red"))
        bx(g, -hw, -hw, 1.95, hw, hw, h - 0.22, main_t)
        for sx in (-1, 1):
            for sy in (-1, 1):
                tube(g, (sx * hw * 0.8, sy * hw * 0.8, 1.2), (sx * hw * 0.8, sy * hw * 0.8, 2.0), 0.02, 0.02, metal_t, seg=5)
    return g


@register(BUILDERS)
def b_arch_stair(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.6))
    h = float(params.get("h", 2.4))
    d = float(params.get("d", 3.2))
    steps = int(params.get("steps", 10))
    style = int(params.get("style", 0))
    step_t = T("step", "concrete_raw")
    rail_t = T("rail", "metal_plate")
    str_t = T("stringer", "concrete_dark")

    hw = w / 2.0
    sh = h / steps
    sd = d / steps
    for i in range(steps):
        y0 = -d / 2.0 + i * sd
        y1 = y0 + sd
        z1 = (i + 1) * sh
        z0 = 0.0 if style in (0, 2) else max(0.0, i * sh - sh * 0.35)
        bx(g, -hw, y0, z0, hw, y1, z1, step_t)
        # Nosing strip
        bx(g, -hw, y0 - 0.02, z1 - 0.03, hw, y0, z1, rail_t)

    # Side stringers / handrails
    for side in (-1, 1):
        if style == 4 and side == 1:
            continue
        rx = side * (hw - 0.05)
        # Bottom, mid, top balusters
        for idx in (0, steps // 2, steps - 1):
            py = -d / 2.0 + (idx + 0.5) * sd
            pz = (idx + 1) * sh
            bx(g, rx - 0.025, py - 0.025, pz, rx + 0.025, py + 0.025, pz + 0.9, rail_t)
        # Slanted handrail tube
        tube(g, (rx, -d / 2.0 + 0.5 * sd, sh + 0.9), (rx, d / 2.0 - 0.5 * sd, h + 0.9), 0.028, 0.028, rail_t, seg=6)
        if style in (1, 3, 5):
            tube(g, (rx, -d / 2.0, 0.08), (rx, d / 2.0, h - 0.08), 0.045, 0.045, str_t, seg=6)
    return g


@register(BUILDERS)
def b_arch_elevator(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.0))
    h = float(params.get("h", 2.5))
    style = int(params.get("style", 0))
    frame_t = T("frame", "marble_white")
    door_t = T("door", "metal_plate")
    cab_t = T("cab", "concrete_dark")
    floor_t = T("floor", "tile_checker")

    hw = w / 2.0
    # Outer portal frame
    bx(g, -hw, -0.15, 0.0, -hw + 0.22, 0.15, h, frame_t)
    bx(g, hw - 0.22, -0.15, 0.0, hw, 0.15, h, frame_t)
    bx(g, -hw, -0.15, h - 0.28, hw, 0.15, h, frame_t)
    # Threshold sill with grooves
    bx(g, -hw, -0.15, 0.0, hw, 0.15, 0.03, door_t)
    # Floor indicator above header & call button box on right jamb
    bx(g, -0.25, -0.17, h - 0.2, 0.25, -0.14, h - 0.08, cab_t)
    bx(g, hw - 0.16, -0.17, 1.0, hw - 0.06, -0.14, 1.24, door_t)

    iw = w - 0.44
    ih = h - 0.31
    gap = 0.01 if style in (0, 2, 4) else (0.28 if style in (1, 5) else 0.55)
    dw = (iw / 2.0) - gap / 2.0
    if dw > 0.08:
        bx(g, -iw / 2.0, -0.04, 0.03, -gap / 2.0, 0.04, 0.03 + ih, door_t)
        bx(g, gap / 2.0, -0.04, 0.03, iw / 2.0, 0.04, 0.03 + ih, door_t)

    if style in (1, 3, 4, 5):
        # Elevator cabin box behind doors
        cd = 1.6
        bx(g, -hw, 0.15, 0.0, hw, 0.15 + cd, 0.06, floor_t)
        bx(g, -hw, 0.15, h - 0.08, hw, 0.15 + cd, h, T("ceil", "ceiling_panel"))
        bx(g, -hw, 0.15, 0.06, -hw + 0.08, 0.15 + cd, h - 0.08, cab_t)
        bx(g, hw - 0.08, 0.15, 0.06, hw, 0.15 + cd, h - 0.08, cab_t)
        bx(g, -hw + 0.08, 0.15 + cd - 0.08, 0.06, hw - 0.08, 0.15 + cd, h - 0.08, cab_t)
        # Interior handrail
        bx(g, -hw + 0.12, 0.15 + cd - 0.14, 0.9, hw - 0.12, 0.15 + cd - 0.1, 0.95, door_t)
    return g


@register(BUILDERS)
def b_arch_vent(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.9))
    d = float(params.get("d", 1.4))
    h = float(params.get("h", 0.55))
    style = int(params.get("style", 0))
    duct_t = T("duct", "metal_plate")
    dark_t = T("dark", "concrete_dark")

    hw, hd = w / 2.0, d / 2.0
    # Main duct body sitting on floor (z >= 0.0)
    bx(g, -hw, -hd, 0.02, hw, hd, h, duct_t)
    # Flange ribs along length
    for fy in (-hd, 0.0, hd - 0.04):
        bx(g, -hw - 0.03, fy, 0.0, hw + 0.03, fy + 0.04, h + 0.03, duct_t)

    # Front louvered vent slats
    bx(g, -hw * 0.85, -hd - 0.015, 0.08, hw * 0.85, -hd, h - 0.06, dark_t)
    n_slats = 5
    for s in range(n_slats):
        sz = 0.1 + s * (h - 0.18) / max(1, n_slats - 1)
        obox(g, (0.0, -hd - 0.02, sz), (w * 0.82, 0.04, 0.025), duct_t, rot=(0.35, 0.0, 0.0))

    if style in (2, 4, 5):
        # Vertical exhaust stack riser on top
        bx(g, -hw * 0.6, -0.25, h, hw * 0.6, 0.35, h + 0.55, duct_t)
        bx(g, -hw * 0.66, -0.31, h + 0.52, hw * 0.66, 0.41, h + 0.58, duct_t)
    return g


def entries():
    out = []
    walls = P.STRUCT["wall"]
    floors = P.STRUCT["floor"]
    ceils = P.STRUCT["ceiling"]

    # 1. arch_wall_01..08 (8)
    for i in range(8):
        out.append(mk(f"arch_wall_{i + 1:02d}", "Architecture", "b_arch_wall",
                      params={"w": 4.0 if i < 6 else 2.0, "h": 3.0, "style": i},
                      atlas=ATLAS,
                      palette={"wall": pick(walls, i), "trim": pick(["concrete_dark", "marble_white", "metal_plate"], i),
                               "wainscot": pick(["parquet_oak", "tile_white", "concrete_raw"], i)}))

    # 2. arch_wallbroken_01..07 (7)
    for i in range(7):
        out.append(mk(f"arch_wallbroken_{i + 1:02d}", "Architecture", "b_arch_wallbroken",
                      params={"w": 4.0, "h": 3.0, "style": i},
                      atlas=ATLAS,
                      palette={"wall": pick(["wall_damaged", "plaster_dirty", "brick_red", "concrete_raw"], i),
                               "core": pick(["brick_red", "concrete_dark", "wall_damaged"], i),
                               "rebar": "metal_plate"}))

    # 3. arch_floor_01..08 (8)
    for i in range(8):
        out.append(mk(f"arch_floor_{i + 1:02d}", "Architecture", "b_arch_floor",
                      params={"w": 4.0, "d": 4.0, "style": i % 6},
                      atlas=ATLAS,
                      palette={"floor": pick(floors, i), "border": pick(floors, i + 3), "sub": "concrete_dark"}))

    # 4. arch_ceiling_01..06 (6)
    for i in range(6):
        out.append(mk(f"arch_ceiling_{i + 1:02d}", "Architecture", "b_arch_ceiling",
                      params={"w": 4.0, "d": 4.0, "style": i},
                      atlas=ATLAS,
                      palette={"ceiling": pick(ceils, i), "frame": "metal_plate", "beam": "concrete_raw"}))

    # 5. arch_door_01..07 (7)
    for i in range(7):
        out.append(mk(f"arch_door_{i + 1:02d}", "Architecture", "b_arch_door",
                      params={"w": 1.6 if i in (2, 5) else 1.15, "h": 2.3, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["concrete_dark", "marble_white", "metal_plate"], i),
                               "door": pick(["parquet_oak", "metal_plate", "plaster_white", "concrete_dark"], i),
                               "metal": "metal_plate", "glass": "tile_white"}))

    # 6. arch_window_01..07 (7)
    for i in range(7):
        out.append(mk(f"arch_window_{i + 1:02d}", "Architecture", "b_arch_window",
                      params={"w": 1.4 + (i % 3) * 0.4, "h": 1.4 + (i % 2) * 0.3, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["concrete_raw", "metal_plate", "parquet_oak", "plaster_white"], i),
                               "sill": pick(["marble_white", "concrete_dark"], i),
                               "pane": "tile_white", "bar": "metal_plate"}))

    # 7. arch_pillar_01..06 (6)
    for i in range(6):
        out.append(mk(f"arch_pillar_{i + 1:02d}", "Architecture", "b_arch_pillar",
                      params={"w": 0.55 + (i % 2) * 0.1, "h": 3.0, "style": i},
                      atlas=ATLAS,
                      palette={"pillar": pick(["concrete_raw", "marble_white", "brick_red", "metal_plate", "plaster_dirty"], i),
                               "trim": pick(["concrete_dark", "marble_white"], i),
                               "metal": "metal_plate"}))

    # 8. arch_stair_01..06 (6)
    for i in range(6):
        out.append(mk(f"arch_stair_{i + 1:02d}", "Architecture", "b_arch_stair",
                      params={"w": 1.5 + (i % 2) * 0.4, "h": 2.4, "d": 3.2, "steps": 9 + (i % 3) * 2, "style": i},
                      atlas=ATLAS,
                      palette={"step": pick(["concrete_raw", "parquet_oak", "marble_white", "metal_plate"], i),
                               "rail": "metal_plate", "stringer": "concrete_dark"}))

    # 9. arch_elevator_01..06 (6)
    for i in range(6):
        out.append(mk(f"arch_elevator_{i + 1:02d}", "Architecture", "b_arch_elevator",
                      params={"w": 2.0, "h": 2.5, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["marble_white", "concrete_dark", "metal_plate"], i),
                               "door": "metal_plate",
                               "cab": pick(["concrete_dark", "parquet_oak", "plaster_dirty"], i),
                               "floor": pick(["tile_checker", "carpet_red", "metal_plate"], i)}))

    # 10. arch_vent_01..06 (6)
    for i in range(6):
        out.append(mk(f"arch_vent_{i + 1:02d}", "Architecture", "b_arch_vent",
                      params={"w": 0.75 + (i % 3) * 0.2, "d": 1.2 + (i % 2) * 0.4, "h": 0.45 + (i % 2) * 0.15, "style": i},
                      atlas=ATLAS,
                      palette={"duct": "metal_plate", "dark": "concrete_dark"}))

    return out
