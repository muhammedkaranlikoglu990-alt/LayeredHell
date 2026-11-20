"""Procedural 3D builders for Lab (30 entries) and Executive penthouse/rooftop (30 entries)."""
import math

from . import palettes as P
from .builders_common import (
    Tags, ball, bx, front_label, geo_for, mk, obox, pick, prism, register, tube,
)

BUILDERS = {}


# ============================================================================== LAB (30)
@register(BUILDERS)
def lab_pod(params, pal, atlas):
    """Cryo / bio-specimen stasis pod (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_steel = T("steel", "lab_steel")
    t_glass = T("glass", "glass_vial")
    t_fluid = T("fluid", "chem_yellow" if v % 2 == 1 else "battery_green")
    t_screen = T("screen", "screen_green")
    t_accent = T("accent", "tool_orange")

    r = 0.48 + (v % 2) * 0.04
    h_base = 0.34
    h_top = 2.05

    # Octagonal/cylindrical base pedestal & top dome cap
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, h_base), r * 1.12, r * 1.04, t_steel, seg=10)
    tube(g, (0.0, 0.0, h_top), (0.0, 0.0, h_top + 0.28), r * 1.04, r * 0.72, t_steel, seg=10)

    # Outer glass cylinder + inner glowing bio-fluid core column
    tube(g, (0.0, 0.0, h_base), (0.0, 0.0, h_top), r, r, t_glass, seg=10)
    tube(g, (0.0, 0.0, h_base + 0.08), (0.0, 0.0, h_top - 0.12), r * 0.56, r * 0.52, t_fluid, seg=8)

    # Vertical structural ribs on sides & back
    for ang_deg in (55, 180, 305):
        a = math.radians(ang_deg)
        rx, ry = math.sin(a) * (r + 0.02), -math.cos(a) * (r + 0.02)
        tube(g, (rx, ry, h_base), (rx, ry, h_top), 0.032, 0.032, t_steel, seg=6)

    # Front monitoring console attached to base
    bx(g, -0.26, -r - 0.24, 0.0, 0.26, -r + 0.04, 0.88, t_steel)
    front_label(g, (0.0, -r - 0.10, 0.62), (0.40, 0.28, 0.28), t_screen, face="front")
    if v >= 4:
        # Feeder hoses from top cap to rear floor manifold
        tube(g, (0.0, r * 0.6, h_top + 0.22), (0.0, r + 0.25, 1.20), 0.04, 0.04, t_accent, seg=6)
        tube(g, (0.0, r + 0.25, 1.20), (0.0, r + 0.25, 0.0), 0.045, 0.045, t_steel, seg=6)
    return g


@register(BUILDERS)
def lab_tank(params, pal, atlas):
    """Pressurized chemical/mutagen storage tank (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "chem_yellow")
    t_steel = T("steel", "lab_steel")
    t_red = T("red", "canister_red")
    t_glass = T("glass", "glass_vial")

    if v in (1, 3, 5):
        # Single large vertical pressure tank on 4 legs
        r = 0.52
        for sx in (-1, 1):
            for sy in (-1, 1):
                bx(g, sx * 0.36 - 0.04, sy * 0.36 - 0.04, 0.0, sx * 0.36 + 0.04, sy * 0.36 + 0.04, 0.35, t_steel)
        tube(g, (0.0, 0.0, 0.30), (0.0, 0.0, 1.85), r, r, t_body, seg=12)
        ball(g, (0.0, 0.0, 1.85), r, t_steel, seg=10, rings=4, scale=(1, 1, 0.45))
        # Front sight-glass gauge strip + valve wheel
        bx(g, -0.05, -r - 0.03, 0.55, 0.05, -r + 0.02, 1.65, t_glass)
        tube(g, (0.0, -r - 0.14, 1.10), (0.0, -r, 1.10), 0.025, 0.025, t_steel, seg=6)
        tube(g, (0.0, -r - 0.16, 1.10), (0.0, -r - 0.13, 1.10), 0.09, 0.09, t_red, seg=8)
    else:
        # Twin coupled cylinder tanks on heavy skid frame
        bx(g, -0.75, -0.45, 0.0, 0.75, 0.45, 0.14, t_steel)
        for sx, tag in ((-0.36, t_body), (0.36, t_red if v == 4 else t_body)):
            tube(g, (sx, 0.0, 0.14), (sx, 0.0, 1.72), 0.30, 0.30, tag, seg=10)
            ball(g, (sx, 0.0, 1.72), 0.30, t_steel, seg=8, rings=4, scale=(1, 1, 0.45))
        # Cross manifold pipe bridging top domes
        tube(g, (-0.36, 0.0, 1.92), (0.36, 0.0, 1.92), 0.04, 0.04, t_steel, seg=6)
    return g


@register(BUILDERS)
def lab_server(params, pal, atlas):
    """High-density lab mainframe server cabinet (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_cabinet = T("cabinet", "vehicle_armor")
    t_steel = T("steel", "lab_steel")
    t_screen = T("screen", "screen_green")
    t_pcb = T("pcb", "circuit_green")
    t_accent = T("accent", "tool_orange")

    w = 0.72 + (v % 2) * 0.18
    d = 0.86
    h = 2.06
    hx, hy = w / 2.0, d / 2.0

    # Base plinth + main rack chassis
    bx(g, -hx, -hy, 0.0, hx, hy, 0.08, t_steel)
    bx(g, -hx + 0.02, -hy + 0.02, 0.08, hx - 0.02, hy - 0.02, h - 0.06, t_cabinet)
    bx(g, -hx, -hy, h - 0.06, hx, hy, h, t_steel)

    # Stacked server blade drawers & LED status strips on front (-Y)
    for k in range(5):
        z0 = 0.16 + k * 0.34
        bx(g, -hx + 0.06, -hy - 0.01, z0, hx - 0.06, -hy + 0.03, z0 + 0.26, t_steel)
        tag = t_screen if (k == 3 or (v % 2 == 0 and k == 2)) else t_pcb
        front_label(g, (0.0, -hy + 0.01, z0 + 0.13), (w - 0.18, 0.04, 0.18), tag, face="front")

    if v >= 3:
        # Top cooling duct / cable bridge
        bx(g, -hx + 0.10, -0.20, h, hx - 0.10, 0.20, h + 0.18, t_accent if v == 5 else t_steel)
    return g


@register(BUILDERS)
def lab_cage(params, pal, atlas):
    """Reinforced bio-research specimen containment cage (atlas ENV_MAT_2K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_steel = T("steel", "steel_brushed")
    t_rust = T("rust", "rust_heavy")
    t_frame = T("frame", "painted_metal_green")
    t_tray = T("tray", "plastic_black")

    w = 1.25 + (v % 3) * 0.20
    d = 1.10
    h = 1.65 + (v % 2) * 0.25
    hx, hy = w / 2.0, d / 2.0

    # Heavy tray floor & top roof slab
    bx(g, -hx, -hy, 0.0, hx, hy, 0.10, t_tray)
    bx(g, -hx - 0.02, -hy - 0.02, 0.08, hx + 0.02, hy + 0.02, 0.14, t_frame)
    bx(g, -hx - 0.02, -hy - 0.02, h - 0.08, hx + 0.02, hy + 0.02, h, t_frame)

    # 4 corner posts
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (hx - 0.03), sy * (hy - 0.03)
            bx(g, x - 0.03, y - 0.03, 0.14, x + 0.03, y + 0.03, h - 0.08, t_frame)

    # Vertical containment bars along front, back, left, right
    t_bar = t_rust if (v % 2 == 0) else t_steel
    for k in range(-3, 4):
        x = k * (w * 0.13)
        tube(g, (x, -hy + 0.02, 0.14), (x, -hy + 0.02, h - 0.08), 0.014, 0.014, t_bar, seg=5)
        tube(g, (x, hy - 0.02, 0.14), (x, hy - 0.02, h - 0.08), 0.014, 0.014, t_bar, seg=5)
    for k in range(-2, 3):
        y = k * (d * 0.18)
        tube(g, (-hx + 0.02, y, 0.14), (-hx + 0.02, y, h - 0.08), 0.014, 0.014, t_bar, seg=5)
        tube(g, (hx - 0.02, y, 0.14), (hx - 0.02, y, h - 0.08), 0.014, 0.014, t_bar, seg=5)

    # Mid horizontal lock rail & front door lock box
    bx(g, -hx, -hy - 0.02, h * 0.52, hx, -hy + 0.04, h * 0.56, t_frame)
    bx(g, 0.22, -hy - 0.05, h * 0.46, 0.42, -hy + 0.03, h * 0.62, t_rust)
    return g


@register(BUILDERS)
def lab_pipe(params, pal, atlas):
    """Modular laboratory pipe & valve manifold assembly (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_pipe = T("pipe", "lab_steel")
    t_chem = T("chem", "chem_yellow")
    t_red = T("red", "canister_red")
    t_orange = T("orange", "tool_orange")

    # Floor mounting stanchions (z >= 0.0)
    for sx in (-0.75, 0.75):
        bx(g, sx - 0.12, -0.16, 0.0, sx + 0.12, 0.16, 0.06, t_pipe)
        bx(g, sx - 0.04, -0.05, 0.06, sx + 0.04, 0.05, 1.35, t_pipe)

    # Dual horizontal main runs (lower & upper)
    tube(g, (-1.05, 0.0, 0.52), (1.05, 0.0, 0.52), 0.085, 0.085, t_pipe, seg=10)
    tube(g, (-1.05, 0.0, 1.22), (1.05, 0.0, 1.22), 0.075, 0.075, t_chem if v % 2 == 1 else t_orange, seg=10)

    # Flange collars along pipes
    for fx in (-0.98, -0.35, 0.35, 0.98):
        tube(g, (fx - 0.025, 0.0, 0.52), (fx + 0.025, 0.0, 0.52), 0.115, 0.115, t_pipe, seg=10)
        tube(g, (fx - 0.025, 0.0, 1.22), (fx + 0.025, 0.0, 1.22), 0.105, 0.105, t_pipe, seg=10)

    # Vertical bypass riser & red gate valve wheel
    tube(g, (0.0, 0.0, 0.52), (0.0, 0.0, 1.22), 0.065, 0.065, t_pipe, seg=8)
    tube(g, (0.0, -0.18, 0.87), (0.0, 0.0, 0.87), 0.025, 0.025, t_pipe, seg=6)
    tube(g, (0.0, -0.21, 0.87), (0.0, -0.17, 0.87), 0.11, 0.11, t_red, seg=8)
    if v >= 4:
        # Vertical riser extension up to ceiling height
        tube(g, (-0.35, 0.0, 1.22), (-0.35, 0.0, 2.20), 0.065, 0.065, t_pipe, seg=8)
    return g


# ============================================================================== EXECUTIVE (30)
@register(BUILDERS)
def exe_chandelier(params, pal, atlas):
    """Grand multi-tier crystal/brass chandelier (lo[2] >= 0.05 so floor check passes)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_metal = T("metal", "steel_brushed")
    t_glass = T("glass", "glass_dirty")
    t_glow = T("glow", "plastic_white")
    t_dark = T("dark", "wood_dark")

    # Central finial at bottom (z=0.10) up to ceiling canopy (z=1.75)
    ball(g, (0.0, 0.0, 0.14), 0.08, t_glass, seg=8, rings=4)
    tube(g, (0.0, 0.0, 0.18), (0.0, 0.0, 1.65), 0.045, 0.035, t_metal, seg=8)
    tube(g, (0.0, 0.0, 1.65), (0.0, 0.0, 1.78), 0.16, 0.24, t_dark, seg=10)

    # 3 Concentric crystal & lamp tiers
    tiers = [
        (0.36, 0.34, 6),
        (0.68, 0.56, 8),
        (1.04, 0.38 + (v % 2) * 0.12, 6),
    ]
    for z, rad, n_lamps in tiers:
        tube(g, (0.0, 0.0, z), (0.0, 0.0, z + 0.07), rad, rad * 0.96, t_metal, seg=12)
        tube(g, (0.0, 0.0, z - 0.14), (0.0, 0.0, z), rad * 0.88, rad * 0.94, t_glass, seg=12)
        for k in range(n_lamps):
            a = 2.0 * math.pi * k / n_lamps
            lx, ly = math.cos(a) * (rad + 0.08), math.sin(a) * (rad + 0.08)
            tube(g, (math.cos(a) * rad * 0.85, math.sin(a) * rad * 0.85, z + 0.02),
                 (lx, ly, z + 0.05), 0.015, 0.015, t_metal, seg=5)
            tube(g, (lx, ly, z + 0.05), (lx, ly, z + 0.17), 0.026, 0.018, t_glow, seg=6)
    return g


@register(BUILDERS)
def exe_banquet(params, pal, atlas):
    """Executive penthouse formal banquet table (atlas ENV_MAT_2K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wood = T("wood", "wood_dark")
    t_runner = T("runner", "fabric_red")
    t_metal = T("metal", "steel_brushed")
    t_plate = T("plate", "plastic_white")

    w = 2.80 + (v % 3) * 0.30
    d = 1.15
    h = 0.78
    hx, hy = w / 2.0, d / 2.0

    # Twin sculpted column pedestals + spreader beam
    for sx in (-hx * 0.58, hx * 0.58):
        bx(g, sx - 0.24, -0.36, 0.0, sx + 0.24, 0.36, 0.08, t_wood)
        tube(g, (sx, 0.0, 0.08), (sx, 0.0, h - 0.06), 0.16, 0.12, t_wood, seg=8)
    bx(g, -hx * 0.58, -0.06, 0.16, hx * 0.58, 0.06, 0.24, t_wood)

    # Thick beveled tabletop + velvet table runner
    bx(g, -hx, -hy, h - 0.06, hx, hy, h, t_wood)
    bx(g, -hx + 0.08, -0.22, h, hx - 0.08, 0.22, h + 0.012, t_runner)

    # Place settings & centerpiece candelabra
    for kx in (-0.95, -0.32, 0.32, 0.95):
        for sy in (-0.36, 0.36):
            tube(g, (kx, sy, h), (kx, sy, h + 0.02), 0.13, 0.11, t_plate, seg=8)
    tube(g, (0.0, 0.0, h + 0.012), (0.0, 0.0, h + 0.34), 0.08, 0.02, t_metal, seg=8)
    bx(g, -0.18, -0.02, h + 0.26, 0.18, 0.02, h + 0.30, t_metal)
    for cx in (-0.16, 0.0, 0.16):
        tube(g, (cx, 0.0, h + 0.30), (cx, 0.0, h + 0.44), 0.014, 0.012, t_plate, seg=6)
    return g


@register(BUILDERS)
def exe_desk(params, pal, atlas):
    """CEO executive desk with leather inlay & credenza (atlas ENV_MAT_2K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wood = T("wood", "wood_dark")
    t_inlay = T("inlay", "leather_black")
    t_metal = T("metal", "steel_brushed")
    t_screen = T("screen", "plastic_black")

    w, d, h = 2.10, 0.96, 0.78
    hx, hy = w / 2.0, d / 2.0

    # Left & right pedestal drawer towers + modesty front panel
    bx(g, -hx + 0.04, -hy + 0.06, 0.0, -hx + 0.56, hy - 0.06, h - 0.06, t_wood)
    bx(g, hx - 0.56, -hy + 0.06, 0.0, hx - 0.04, hy - 0.06, h - 0.06, t_wood)
    bx(g, -hx + 0.56, -hy + 0.08, 0.12, hx - 0.56, -hy + 0.16, h - 0.06, t_wood)
    # Brushed metal plinth strips
    bx(g, -hx + 0.03, -hy + 0.05, 0.0, -hx + 0.57, hy - 0.05, 0.05, t_metal)
    bx(g, hx - 0.57, -hy + 0.05, 0.0, hx - 0.03, hy - 0.05, 0.05, t_metal)

    # Desktop slab + recessed leather writing blotter
    bx(g, -hx, -hy, h - 0.06, hx, hy, h, t_wood)
    bx(g, -0.58, -0.32, h, 0.58, 0.32, h + 0.012, t_inlay)

    # Widescreen executive monitor + banker's lamp
    bx(g, -0.14, -0.22, h + 0.01, 0.14, -0.10, h + 0.03, t_metal)
    bx(g, -0.03, -0.18, h + 0.03, 0.03, -0.14, h + 0.16, t_metal)
    bx(g, -0.38, -0.16, h + 0.14, 0.38, -0.12, h + 0.50, t_screen)

    if v >= 2:
        # Side L-return credenza
        bx(g, hx - 0.56, hy - 0.06, 0.0, hx, hy + 0.85, h - 0.02, t_wood)
    if v >= 4:
        # Desk lamp on right corner
        tube(g, (hx - 0.25, -0.20, h), (hx - 0.25, -0.20, h + 0.36), 0.06, 0.02, t_metal, seg=6)
        bx(g, hx - 0.38, -0.28, h + 0.34, hx - 0.12, -0.12, h + 0.42, T("shade", "painted_metal_green"))
    return g


@register(BUILDERS)
def exe_safe(params, pal, atlas):
    """Heavy executive armored vault safe (atlas ENV_MAT_2K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "plastic_black")
    t_steel = T("steel", "steel_brushed")
    t_trim = T("trim", "wood_dark")

    w = 0.78 + (v % 2) * 0.14
    d = 0.72
    h = 1.05 + (v % 3) * 0.28
    hx, hy = w / 2.0, d / 2.0

    # Base plinth + main armored safe body
    bx(g, -hx, -hy, 0.0, hx, hy, 0.08, t_steel)
    bx(g, -hx + 0.02, -hy + 0.04, 0.08, hx - 0.02, hy, h, t_body)
    # Thick recessed vault door on front (-Y)
    bx(g, -hx + 0.07, -hy - 0.02, 0.14, hx - 0.07, -hy + 0.05, h - 0.06, t_body)
    bx(g, -hx + 0.09, -hy - 0.025, 0.16, hx - 0.09, -hy - 0.015, h - 0.08, t_trim)

    # Heavy external barrel hinges on right (+X)
    for hz in (0.26, h - 0.24):
        tube(g, (hx - 0.06, -hy - 0.03, hz - 0.07), (hx - 0.06, -hy - 0.03, hz + 0.07), 0.032, 0.032, t_steel, seg=8)

    # Combination dial & 3-spoke vault wheel handle
    zc = h * 0.54
    tube(g, (-0.12, -hy - 0.06, zc), (-0.12, -hy - 0.02, zc), 0.065, 0.065, t_steel, seg=10)
    tube(g, (0.10, -hy - 0.09, zc), (0.10, -hy - 0.02, zc), 0.035, 0.035, t_steel, seg=8)
    for k in range(3):
        a = k * 2.0 * math.pi / 3.0
        tube(g, (0.10, -hy - 0.08, zc),
             (0.10 + math.cos(a) * 0.14, -hy - 0.08, zc + math.sin(a) * 0.14),
             0.014, 0.012, t_steel, seg=6)
    return g


@register(BUILDERS)
def exe_helipad(params, pal, atlas):
    """Rooftop executive helipad deck platform & marker beacons (atlas ENV_STRUCT_2K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_pad = T("pad", "concrete_dark")
    t_rim = T("rim", "concrete_raw")
    t_mark = T("mark", "tile_white")
    t_red = T("red", "carpet_red")
    t_steel = T("steel", "metal_plate")

    r = 4.2 + (v % 3) * 0.4
    # Raised octadecagonal/12-sided concrete landing pad with access ramp
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.22), r + 0.35, r + 0.15, t_rim, seg=12)
    tube(g, (0.0, 0.0, 0.22), (0.0, 0.0, 0.28), r, r, t_pad, seg=12)

    # Landing 'H' marking inlaid on top surface
    z_m = 0.28
    bx(g, -1.25, -1.60, z_m, -0.75, 1.60, z_m + 0.02, t_mark)
    bx(g, 0.75, -1.60, z_m, 1.25, 1.60, z_m + 0.02, t_mark)
    bx(g, -0.75, -0.25, z_m, 0.75, 0.25, z_m + 0.02, t_mark)

    # Perimeter beacon lights around pad edge
    for k in range(8):
        a = 2.0 * math.pi * k / 8.0
        bx_pt, by_pt = math.cos(a) * (r - 0.18), math.sin(a) * (r - 0.18)
        tube(g, (bx_pt, by_pt, 0.28), (bx_pt, by_pt, 0.42), 0.10, 0.08, t_steel, seg=6)
        ball(g, (bx_pt, by_pt, 0.44), 0.07, t_red, seg=6, rings=4)

    if v >= 2:
        # Windsock mast on perimeter corner
        mx, my = r + 0.2, 0.0
        tube(g, (mx, my, 0.0), (mx, my, 2.8), 0.06, 0.04, t_steel, seg=6)
        tube(g, (mx, my, 2.65), (mx + 0.95, my, 2.50), 0.18, 0.08, t_red, seg=8)
    return g


@register(BUILDERS)
def exe_radar(params, pal, atlas):
    """Rooftop executive satellite dish / radar array (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_steel = T("steel", "lab_steel")
    t_armor = T("armor", "vehicle_armor")
    t_orange = T("accent", "tool_orange")
    t_screen = T("screen", "screen_green")

    # Heavy concrete/steel base box + equipment cabinet
    bx(g, -0.85, -0.85, 0.0, 0.85, 0.85, 0.24, t_armor)
    bx(g, -0.55, 0.25, 0.24, 0.55, 0.75, 1.15, t_steel)
    front_label(g, (0.0, 0.50, 0.75), (0.65, 0.50, 0.35), t_screen, face="back")

    # Azimuth turret column + elevation yoke
    tube(g, (0.0, -0.10, 0.24), (0.0, -0.10, 1.55), 0.26, 0.20, t_armor, seg=8)
    bx(g, -0.32, -0.32, 1.50, 0.32, 0.12, 1.85, t_steel)

    if v in (1, 3, 5):
        # Parabolic dish reflector tilted toward -Y/+Z + feedhorn tripod
        d_center = (0.0, -0.45, 2.25)
        d_norm = (0.0, -0.82, 0.57)
        d_end = (d_center[0] + d_norm[0] * 0.28, d_center[1] + d_norm[1] * 0.28, d_center[2] + d_norm[2] * 0.28)
        f_tip = (d_center[0] + d_norm[0] * 0.95, d_center[1] + d_norm[1] * 0.95, d_center[2] + d_norm[2] * 0.95)
        tube(g, d_center, d_end, 0.28, 1.15, t_steel, seg=12)
        tube(g, d_center, f_tip, 0.05, 0.08, t_orange, seg=6)
    else:
        # Rotating rectangular airfield/weather radar array bar
        bx(g, -1.45, -0.32, 1.85, 1.45, 0.12, 2.28, t_orange if v == 4 else t_steel)
        bx(g, -1.52, -0.36, 1.92, -1.42, 0.16, 2.22, t_armor)
        bx(g, 1.42, -0.36, 1.92, 1.52, 0.16, 2.22, t_armor)
    return g


def entries():
    out = []
    # Lab (5 prefixes * 6 variants = 30 entries)
    for i in range(1, 7):
        out.append(mk("lab_pod_%02d" % i, "Lab", "lab_pod",
                      params=dict(variant=i, seed=i), atlas="HERO_ITEMS_1K",
                      palette=dict(steel="lab_steel", glass="glass_vial",
                                   fluid="chem_yellow" if i % 2 == 1 else "battery_green")))
    for i in range(1, 7):
        body = ["chem_yellow", "canister_red", "lab_steel", "battery_green", "generator_orange", "radio_olive"][i - 1]
        out.append(mk("lab_tank_%02d" % i, "Lab", "lab_tank",
                      params=dict(variant=i, seed=10 + i), atlas="HERO_ITEMS_1K",
                      palette=dict(body=body, steel="lab_steel")))
    for i in range(1, 7):
        cab = "vehicle_armor" if i % 2 == 1 else "lab_steel"
        out.append(mk("lab_server_%02d" % i, "Lab", "lab_server",
                      params=dict(variant=i, seed=20 + i), atlas="HERO_ITEMS_1K",
                      palette=dict(cabinet=cab, steel="lab_steel", screen="screen_green")))
    for i in range(1, 7):
        fr = ["painted_metal_green", "rust_heavy", "steel_brushed", "painted_metal_blue",
              "plastic_black", "rust_heavy"][i - 1]
        out.append(mk("lab_cage_%02d" % i, "Lab", "lab_cage",
                      params=dict(variant=i, seed=30 + i), atlas="ENV_MAT_2K",
                      palette=dict(frame=fr, steel="steel_brushed", rust="rust_heavy")))
    for i in range(1, 7):
        pcol = ["lab_steel", "chem_yellow", "canister_red", "tool_orange", "radio_olive", "vehicle_armor"][i - 1]
        out.append(mk("lab_pipe_%02d" % i, "Lab", "lab_pipe",
                      params=dict(variant=i, seed=40 + i), atlas="HERO_ITEMS_1K",
                      palette=dict(pipe=pcol, chem="chem_yellow", red="canister_red")))

    # Executive (6 prefixes * 5 variants = 30 entries)
    for i in range(1, 6):
        out.append(mk("exe_chandelier_%02d" % i, "Executive", "exe_chandelier",
                      params=dict(variant=i, seed=50 + i), atlas="ENV_MAT_2K",
                      palette=dict(metal="steel_brushed", glass="glass_dirty", glow="plastic_white")))
    for i in range(1, 6):
        run = ["fabric_red", "fabric_blue", "fabric_green", "leather_brown", "plastic_white"][i - 1]
        out.append(mk("exe_banquet_%02d" % i, "Executive", "exe_banquet",
                      params=dict(variant=i, seed=60 + i), atlas="ENV_MAT_2K",
                      palette=dict(wood="wood_dark" if i % 2 == 1 else "wood_light", runner=run)))
    for i in range(1, 6):
        inl = "leather_black" if i % 2 == 1 else "leather_brown"
        out.append(mk("exe_desk_%02d" % i, "Executive", "exe_desk",
                      params=dict(variant=i, seed=70 + i), atlas="ENV_MAT_2K",
                      palette=dict(wood="wood_dark", inlay=inl, metal="steel_brushed")))
    for i in range(1, 6):
        bcol = ["plastic_black", "steel_brushed", "painted_metal_green", "painted_metal_blue", "wood_dark"][i - 1]
        out.append(mk("exe_safe_%02d" % i, "Executive", "exe_safe",
                      params=dict(variant=i, seed=80 + i), atlas="ENV_MAT_2K",
                      palette=dict(body=bcol, steel="steel_brushed")))
    for i in range(1, 6):
        pad = "concrete_dark" if i % 2 == 1 else "metal_plate"
        out.append(mk("exe_helipad_%02d" % i, "Executive", "exe_helipad",
                      params=dict(variant=i, seed=90 + i), atlas="ENV_STRUCT_2K",
                      palette=dict(pad=pad, rim="concrete_raw", mark="tile_white")))
    for i in range(1, 6):
        st = "lab_steel" if i % 2 == 1 else "helicopter_body"
        out.append(mk("exe_radar_%02d" % i, "Executive", "exe_radar",
                      params=dict(variant=i, seed=100 + i), atlas="HERO_ITEMS_1K",
                      palette=dict(steel=st, armor="vehicle_armor", accent="tool_orange")))
    return out
