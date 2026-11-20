"""Procedural 3D builders for Crafting stations (category 'Crafting', 30 entries)."""
import math

from . import builders_common as C
from . import palettes as P
from .builders_common import (
    Tags, ball, bx, front_label, geo_for, mk, obox, pick, prism, qpaint, register, rnd, tube,
)

BUILDERS = {}


def _table_frame(g, w, d, h, leg, t_frame, t_top, shelf_z=0.18):
    hx, hy = w / 2.0, d / 2.0
    for sx in (-1, 1):
        for sy in (-1, 1):
            x = sx * (hx - leg * 0.7)
            y = sy * (hy - leg * 0.7)
            bx(g, x - leg / 2, y - leg / 2, 0.0, x + leg / 2, y + leg / 2, h - 0.04, t_frame)
    # aprons
    bx(g, -hx + leg, -hy + leg * 0.3, h - 0.09, hx - leg, -hy + leg * 1.1, h - 0.04, t_frame)
    bx(g, -hx + leg, hy - leg * 1.1, h - 0.09, hx - leg, hy - leg * 0.3, h - 0.04, t_frame)
    if shelf_z is not None:
        bx(g, -hx + leg * 0.5, -hy + leg * 0.5, shelf_z, hx - leg * 0.5, hy - leg * 0.5, shelf_z + 0.025, t_frame)
    bx(g, -hx, -hy, h - 0.04, hx, hy, h, t_top)


@register(BUILDERS)
def craft_workbench(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    w = 1.45 + 0.1 * (v - 1)
    d = 0.72
    h = 0.82
    t_wood = T("wood", "wood_plank_worn")
    t_dark = T("frame", "wood_dark")
    t_metal = T("metal", "steel_brushed")
    t_rust = T("rust", "rust_heavy")
    t_accent = T("accent", "painted_metal_green")

    _table_frame(g, w, d, h, 0.06, t_dark, t_wood, shelf_z=0.18)

    # Back pegboard & uprights
    hx, hy = w / 2.0, d / 2.0
    bx(g, -hx + 0.02, hy - 0.05, h, -hx + 0.07, hy, h + 0.62, t_dark)
    bx(g, hx - 0.07, hy - 0.05, h, hx - 0.02, hy, h + 0.62, t_dark)
    bx(g, -hx + 0.04, hy - 0.03, h + 0.06, hx - 0.04, hy - 0.005, h + 0.60, t_wood)
    # Top shelf on pegboard
    bx(g, -hx + 0.04, hy - 0.16, h + 0.56, hx - 0.04, hy, h + 0.59, t_dark)

    # Bench vise on front-left
    vx, vy = -hx + 0.22, -hy + 0.08
    bx(g, vx - 0.07, vy - 0.06, h, vx + 0.07, vy + 0.08, h + 0.09, t_accent)
    bx(g, vx - 0.08, vy - 0.11, h + 0.05, vx + 0.08, vy - 0.06, h + 0.12, t_metal)
    tube(g, (vx, vy - 0.16, h + 0.07), (vx, vy - 0.05, h + 0.07), 0.014, 0.014, t_metal, seg=6)
    tube(g, (vx - 0.07, vy - 0.16, h + 0.04), (vx + 0.07, vy - 0.16, h + 0.10), 0.007, 0.007, t_metal, seg=5)

    # Toolbox on tabletop or lower shelf
    tb_x = 0.28 if v != 2 else -0.20
    bx(g, tb_x - 0.18, -0.05, h, tb_x + 0.18, 0.16, h + 0.15, t_accent)
    bx(g, tb_x - 0.19, -0.06, h + 0.14, tb_x + 0.19, 0.17, h + 0.18, t_metal)
    tube(g, (tb_x - 0.08, 0.055, h + 0.20), (tb_x + 0.08, 0.055, h + 0.20), 0.01, 0.01, t_metal, seg=5)

    # Hanging tools on pegboard (hammer, wrench, saw silhouette)
    bx(g, -0.35, hy - 0.04, h + 0.20, -0.32, hy - 0.025, h + 0.44, t_wood)
    bx(g, -0.40, hy - 0.045, h + 0.42, -0.27, hy - 0.02, h + 0.47, t_metal)
    bx(g, -0.10, hy - 0.04, h + 0.22, -0.06, hy - 0.025, h + 0.46, t_metal)
    bx(g, 0.12, hy - 0.04, h + 0.25, 0.34, hy - 0.025, h + 0.36, t_metal)
    bx(g, 0.33, hy - 0.045, h + 0.24, 0.40, hy - 0.02, h + 0.35, t_dark)

    # Lower shelf crates
    bx(g, -hx + 0.12, -0.22, 0.205, -0.06, 0.22, 0.46, t_wood)
    if v >= 2:
        bx(g, 0.06, -0.20, 0.205, hx - 0.14, 0.18, 0.42, t_rust)
    if v == 3:
        # Anvil block on right
        bx(g, hx - 0.34, -0.15, h, hx - 0.12, 0.05, h + 0.10, t_rust)
        prism(g, [(hx - 0.38, -0.09), (hx - 0.10, -0.09), (hx - 0.10, -0.01), (hx - 0.38, -0.03)],
              h + 0.10, h + 0.16, t_metal)
    return g


@register(BUILDERS)
def craft_weaponbench(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    w, d, h = 1.55, 0.75, 0.84
    t_steel = T("steel", "steel_brushed")
    t_frame = T("frame", "painted_metal_green")
    t_top = T("top", "wood_dark")
    t_black = T("black", "plastic_black")
    t_rust = T("rust", "rust_heavy")

    _table_frame(g, w, d, h, 0.055, t_frame, t_top, shelf_z=0.14)
    hx, hy = w / 2.0, d / 2.0

    # Under-desk drawer cabinet on right
    bx(g, 0.15, -hy + 0.06, 0.17, hx - 0.07, hy - 0.06, h - 0.04, t_frame)
    for k in range(3):
        z0 = 0.21 + k * 0.19
        bx(g, 0.18, -hy + 0.045, z0, hx - 0.10, -hy + 0.065, z0 + 0.15, t_steel)
        bx(g, 0.36, -hy + 0.03, z0 + 0.06, 0.52, -hy + 0.05, z0 + 0.09, t_black)

    # Reinforced metal back wall & weapon mount brackets
    bx(g, -hx + 0.03, hy - 0.05, h, hx - 0.03, hy, h + 0.72, t_frame)
    bx(g, -hx + 0.07, hy - 0.055, h + 0.06, hx - 0.07, hy - 0.045, h + 0.66, t_steel)
    for rx in (-0.42, -0.12, 0.22, 0.50):
        bx(g, rx - 0.015, hy - 0.13, h + 0.28, rx + 0.015, hy - 0.05, h + 0.32, t_black)
        bx(g, rx - 0.015, hy - 0.13, h + 0.46, rx + 0.015, hy - 0.05, h + 0.50, t_black)

    # Bench grinder on left
    gx, gy = -hx + 0.26, 0.02
    bx(g, gx - 0.09, gy - 0.08, h, gx + 0.09, gy + 0.08, h + 0.05, t_black)
    tube(g, (gx - 0.11, gy, h + 0.11), (gx + 0.11, gy, h + 0.11), 0.065, 0.065, t_frame, seg=8)
    tube(g, (gx - 0.14, gy, h + 0.11), (gx - 0.10, gy, h + 0.11), 0.085, 0.085, t_rust, seg=8)
    tube(g, (gx + 0.10, gy, h + 0.11), (gx + 0.14, gy, h + 0.11), 0.085, 0.085, t_steel, seg=8)

    # Gun cradle / cleaning mat in center
    bx(g, -0.24, -0.22, h, 0.26, 0.14, h + 0.012, t_black)
    bx(g, -0.18, -0.06, h + 0.012, -0.13, 0.04, h + 0.14, t_top)
    bx(g, 0.15, -0.06, h + 0.012, 0.20, 0.04, h + 0.14, t_top)
    if v >= 2:
        # Rested barrel assembly on cradle
        tube(g, (-0.25, -0.01, h + 0.14), (0.30, -0.01, h + 0.14), 0.02, 0.016, t_black, seg=6)
    if v == 3:
        # Gooseneck work lamp
        tube(g, (hx - 0.15, hy - 0.08, h), (hx - 0.15, hy - 0.14, h + 0.38), 0.014, 0.012, t_steel, seg=6)
        tube(g, (hx - 0.15, hy - 0.14, h + 0.38), (hx - 0.25, -0.02, h + 0.45), 0.012, 0.012, t_steel, seg=6)
        tube(g, (hx - 0.25, -0.02, h + 0.45), (hx - 0.28, -0.06, h + 0.39), 0.02, 0.055, t_frame, seg=8)
    return g


@register(BUILDERS)
def craft_chem(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    w, d, h = 1.35, 0.70, 0.84
    t_steel = T("steel", "lab_steel")
    t_chem = T("chem", "chem_yellow")
    t_glass = T("glass", "glass_vial")
    t_red = T("canister", "canister_red")
    t_screen = T("screen", "screen_green")
    t_orange = T("accent", "tool_orange")

    _table_frame(g, w, d, h, 0.05, t_steel, t_steel, shelf_z=0.16)
    hx, hy = w / 2.0, d / 2.0

    # Retort stands & glassware apparatus
    for sx, cx in ((-1, -0.32), (1, 0.04)):
        bx(g, cx - 0.09, -0.10, h, cx + 0.09, 0.12, h + 0.02, t_steel)
        tube(g, (cx, 0.08, h + 0.02), (cx, 0.08, h + 0.56), 0.01, 0.01, t_steel, seg=6)
        # Flask
        tube(g, (cx, -0.01, h + 0.08), (cx, -0.01, h + 0.22), 0.065, 0.022, t_glass, seg=8)
        tube(g, (cx, -0.01, h + 0.22), (cx, -0.01, h + 0.30), 0.022, 0.024, t_chem, seg=8)
    # Condenser bridge tube between stands
    tube(g, (-0.32, -0.01, h + 0.29), (0.04, -0.01, h + 0.23), 0.018, 0.018, t_glass, seg=6)

    # Centrifuge / digital analyzer on right
    bx(g, 0.24, -0.18, h, 0.56, 0.16, h + 0.22, t_steel)
    front_label(g, (0.40, 0.0, h + 0.11), (0.24, 0.36, 0.12), t_screen, face="front")

    # Chemical drum + hazard canister underneath
    tube(g, (-0.34, 0.0, 0.185), (-0.34, 0.0, 0.58), 0.14, 0.14, t_chem, seg=10)
    tube(g, (0.28, 0.0, 0.185), (0.28, 0.0, 0.52), 0.11, 0.11, t_red, seg=8)

    if v >= 2:
        # Vial rack front left
        bx(g, -0.58, -0.24, h, -0.38, -0.12, h + 0.07, t_orange)
        for k in range(4):
            vx = -0.55 + k * 0.045
            tube(g, (vx, -0.18, h + 0.03), (vx, -0.18, h + 0.14), 0.012, 0.012, t_glass, seg=6)
    if v == 3:
        # Tall distillation column at back
        tube(g, (0.46, 0.18, h), (0.46, 0.18, h + 0.62), 0.055, 0.045, t_steel, seg=8)
    return g


@register(BUILDERS)
def craft_medstation(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    w, d, h = 1.30, 0.66, 0.85
    t_steel = T("steel", "lab_steel")
    t_med = T("med", "medkit_white")
    t_band = T("bandage", "bandage")
    t_vial = T("vial", "glass_vial")
    t_screen = T("screen", "screen_green")
    t_red = T("red", "canister_red")

    # Cabinet base with clean drawers and table surface
    bx(g, -w / 2, -d / 2, 0.0, w / 2, d / 2, 0.06, t_steel)
    bx(g, -w / 2 + 0.03, -d / 2 + 0.03, 0.06, w / 2 - 0.03, d / 2 - 0.03, h - 0.04, t_band)
    bx(g, -w / 2, -d / 2, h - 0.04, w / 2, d / 2, h, t_steel)
    front_label(g, (-0.30, 0.0, 0.44), (0.34, d - 0.06, 0.34), t_med, face="front")
    front_label(g, (0.30, 0.0, 0.44), (0.34, d - 0.06, 0.34), t_med, face="front")

    # Back hutch with cross cabinet & shelf
    hx, hy = w / 2.0, d / 2.0
    bx(g, -hx + 0.04, hy - 0.22, h, hx - 0.04, hy, h + 0.64, t_band)
    front_label(g, (0.0, hy - 0.11, h + 0.34), (0.42, 0.22, 0.42), t_med, face="front")

    # Medkit box on left
    bx(g, -0.50, -0.18, h, -0.18, 0.06, h + 0.14, t_med)
    front_label(g, (-0.34, -0.06, h + 0.14), (0.26, 0.20, 0.002), t_med, face="top")

    # Sterilizer / diagnostic tray in center-right
    bx(g, 0.08, -0.20, h, 0.48, 0.06, h + 0.16, t_steel)
    front_label(g, (0.28, -0.07, h + 0.08), (0.28, 0.26, 0.10), t_screen, face="front")

    # IV / dispenser pole attached to right corner
    if v >= 2:
        tube(g, (hx - 0.06, -hy + 0.08, h), (hx - 0.06, -hy + 0.08, h + 0.85), 0.015, 0.012, t_steel, seg=6)
        tube(g, (hx - 0.20, -hy + 0.08, h + 0.82), (hx + 0.06, -hy + 0.08, h + 0.82), 0.008, 0.008, t_steel, seg=5)
        bx(g, hx - 0.22, -hy + 0.05, h + 0.64, hx - 0.14, -hy + 0.11, h + 0.79, t_vial)
    if v == 3:
        tube(g, (-hx - 0.09, 0.0, 0.0), (-hx - 0.09, 0.0, 0.72), 0.08, 0.08, t_red, seg=8)
        ball(g, (-hx - 0.09, 0.0, 0.72), 0.08, t_red, seg=8, rings=4)
    return g


@register(BUILDERS)
def craft_purifier(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wood = T("wood", "wood_plank_worn")
    t_steel = T("steel", "steel_brushed")
    t_blue = T("tank", "painted_metal_blue")
    t_rust = T("rust", "rust_heavy")
    t_glass = T("glass", "glass_dirty")
    t_black = T("hose", "rubber")

    # Wooden frame rack holding upper & lower barrels
    w, d, h = 1.10, 0.68, 1.45
    hx, hy = w / 2.0, d / 2.0
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (hx - 0.04), sy * (hy - 0.04)
            bx(g, x - 0.035, y - 0.035, 0.0, x + 0.035, y + 0.035, h, t_wood)
    for z in (0.18, 0.78, 1.32):
        bx(g, -hx, -hy, z, hx, hy, z + 0.045, t_wood)

    # Top raw-water barrel
    tube(g, (-0.24, 0.0, 0.83), (-0.24, 0.0, 1.38), 0.20, 0.20, t_blue, seg=10)
    # Charcoal/sand filter column
    tube(g, (0.22, 0.0, 0.83), (0.22, 0.0, 1.35), 0.14, 0.14, t_steel, seg=10)
    # Connecting pipe top
    tube(g, (-0.04, 0.0, 1.15), (0.08, 0.0, 1.15), 0.025, 0.025, t_black, seg=6)
    # Drain pipe down to lower clean tank
    tube(g, (0.22, -0.14, 0.84), (0.22, -0.14, 0.56), 0.022, 0.022, t_steel, seg=6)

    # Lower purified water glass carboy + storage tank
    tube(g, (0.22, 0.0, 0.225), (0.22, 0.0, 0.66), 0.17, 0.15, t_glass, seg=10)
    tube(g, (-0.24, 0.0, 0.225), (-0.24, 0.0, 0.68), 0.18, 0.18, t_steel, seg=10)

    # Spigot valve at front
    tube(g, (-0.24, -0.18, 0.34), (-0.24, -0.28, 0.34), 0.02, 0.018, t_rust, seg=6)
    if v >= 2:
        # Secondary filter canisters mounted on side
        tube(g, (hx + 0.06, -0.12, 0.55), (hx + 0.06, -0.12, 0.95), 0.06, 0.06, t_blue, seg=8)
        tube(g, (hx + 0.06, 0.12, 0.55), (hx + 0.06, 0.12, 0.95), 0.06, 0.06, t_steel, seg=8)
    if v == 3:
        # Catch bucket on floor in front
        tube(g, (-0.24, -0.44, 0.0), (-0.24, -0.44, 0.24), 0.11, 0.14, t_rust, seg=8)
    return g


@register(BUILDERS)
def craft_stove(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_rust = T("rust", "rust_heavy")
    t_steel = T("steel", "steel_brushed")
    t_black = T("black", "plastic_black")
    t_wood = T("wood", "wood_dark")
    t_red = T("tank", "fabric_red")

    # Heavy cast-iron / steel drum stove on 4 legs
    for sx in (-1, 1):
        for sy in (-1, 1):
            bx(g, sx * 0.36 - 0.035, sy * 0.24 - 0.035, 0.0, sx * 0.36 + 0.035, sy * 0.24 + 0.035, 0.26, t_black)

    # Firebox body
    bx(g, -0.42, -0.30, 0.24, 0.42, 0.30, 0.78, t_rust)
    bx(g, -0.45, -0.33, 0.78, 0.45, 0.33, 0.83, t_black)
    # Firebox door & draft grate
    bx(g, -0.26, -0.32, 0.34, 0.12, -0.295, 0.66, t_black)
    bx(g, 0.14, -0.34, 0.46, 0.18, -0.295, 0.54, t_steel)

    # Burners & cooking pot
    tube(g, (-0.18, -0.02, 0.83), (-0.18, -0.02, 0.86), 0.14, 0.14, t_steel, seg=8)
    tube(g, (0.20, -0.02, 0.83), (0.20, -0.02, 0.86), 0.12, 0.12, t_steel, seg=8)
    tube(g, (-0.18, -0.02, 0.86), (-0.18, -0.02, 1.04), 0.13, 0.14, t_steel, seg=10)

    # Chimney stovepipe at back
    tube(g, (0.24, 0.20, 0.83), (0.24, 0.20, 1.65), 0.075, 0.075, t_black, seg=8)
    tube(g, (0.24, 0.20, 1.65), (0.24, 0.20, 1.72), 0.12, 0.04, t_rust, seg=8)

    # Firewood stack underneath
    for k in range(3):
        tube(g, (-0.25, -0.10 + k * 0.10, 0.055), (0.25, -0.10 + k * 0.10, 0.055), 0.045, 0.045, t_wood, seg=6)
    if v >= 2:
        # Side prep shelf
        bx(g, 0.45, -0.26, 0.76, 0.74, 0.26, 0.81, t_wood)
    if v == 3:
        # Auxiliary gas bottle beside stove
        tube(g, (-0.60, 0.0, 0.0), (-0.60, 0.0, 0.56), 0.13, 0.13, t_red, seg=10)
    return g


@register(BUILDERS)
def craft_generator(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "generator_orange")
    t_steel = T("steel", "lab_steel")
    t_tank = T("tank", "canister_red")
    t_screen = T("screen", "screen_green")
    t_batt = T("battery", "battery_green")

    w, d, h = 0.92 + 0.06 * (v - 1), 0.58, 0.68
    hx, hy = w / 2.0, d / 2.0
    r = 0.022

    # Roll-cage base skids + vertical posts + top bars
    for sy in (-1, 1):
        y = sy * (hy - 0.03)
        tube(g, (-hx, y, r), (hx, y, r), r, r, t_steel, seg=6)
        tube(g, (-hx, y, h), (hx, y, h), r, r, t_steel, seg=6)
        for sx in (-1, 1):
            x = sx * (hx - 0.03)
            tube(g, (x, y, r), (x, y, h), r, r, t_steel, seg=6)
    for sx in (-1, 1):
        x = sx * (hx - 0.03)
        tube(g, (x, -hy + 0.03, h), (x, hy - 0.03, h), r, r, t_steel, seg=6)

    # Engine + alternator housing inside cage
    bx(g, -hx + 0.10, -hy + 0.08, 0.07, 0.05, hy - 0.08, 0.44, t_steel)
    tube(g, (0.04, 0.0, 0.25), (hx - 0.10, 0.0, 0.25), 0.17, 0.17, t_body, seg=10)

    # Top fuel tank
    bx(g, -hx + 0.08, -hy + 0.07, 0.47, hx - 0.08, hy - 0.07, 0.63, t_tank if v == 2 else t_body)
    tube(g, (-0.15, 0.0, 0.63), (-0.15, 0.0, 0.67), 0.04, 0.04, t_steel, seg=8)

    # Control panel with screen/meters
    bx(g, 0.04, -hy + 0.05, 0.26, hx - 0.10, -hy + 0.12, 0.45, t_steel)
    front_label(g, (0.20, -hy + 0.085, 0.355), (0.24, 0.07, 0.13), t_screen, face="front")

    # Exhaust muffler cylinder
    tube(g, (-hx + 0.16, hy - 0.06, 0.36), (-hx + 0.02, hy + 0.04, 0.44), 0.055, 0.055, t_steel, seg=8)

    if v >= 2:
        # Starter battery mounted on skid
        bx(g, -hx - 0.02, -0.14, 0.05, -hx + 0.12, 0.14, 0.22, t_batt)
    if v == 3:
        # Heavy wheels & folding handle
        for sy in (-1, 1):
            tube(g, (hx - 0.10, sy * (hy + 0.02), 0.11), (hx - 0.10, sy * (hy + 0.07), 0.11), 0.11, 0.11, t_steel, seg=10)
    return g


@register(BUILDERS)
def craft_radio(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_radio = T("radio", "radio_olive")
    t_screen = T("screen", "screen_green")
    t_steel = T("steel", "lab_steel")
    t_batt = T("battery", "battery_green")
    t_paper = T("paper", "document_paper")
    t_orange = T("accent", "tool_orange")

    w, d, h = 1.20, 0.65, 0.78
    _table_frame(g, w, d, h, 0.045, t_steel, t_radio, shelf_z=0.15)

    # Main military transceiver unit
    bx(g, -0.46, -0.12, h, 0.12, 0.24, h + 0.30, t_radio)
    front_label(g, (-0.17, 0.06, h + 0.15), (0.50, 0.36, 0.22), t_screen, face="front")
    # Tuning knobs
    for kx in (-0.38, -0.26, -0.02, 0.06):
        tube(g, (kx, -0.12, h + 0.06), (kx, -0.15, h + 0.06), 0.024, 0.022, t_steel, seg=8)

    # Desk microphone / handset on right
    bx(g, 0.22, -0.16, h, 0.36, -0.02, h + 0.04, t_steel)
    tube(g, (0.29, -0.09, h + 0.04), (0.29, -0.12, h + 0.22), 0.012, 0.010, t_steel, seg=6)
    ball(g, (0.29, -0.13, h + 0.23), 0.032, t_orange, seg=6, rings=4)

    # Tall whip antenna mast at back corner
    tube(g, (-0.48, 0.22, h), (-0.48, 0.22, h + 0.95), 0.022, 0.006, t_steel, seg=6)
    if v >= 2:
        tube(g, (0.48, 0.22, h), (0.48, 0.22, h + 0.78), 0.018, 0.006, t_steel, seg=6)

    # Backup battery bank on lower shelf + log sheet on desk
    bx(g, -0.35, -0.18, 0.175, 0.05, 0.16, 0.38, t_batt)
    qpaint(g, (0.20, -0.26, h + 0.003), (0.50, -0.26, h + 0.003), (0.50, 0.08, h + 0.003), (0.20, 0.08, h + 0.003),
           t_paper)
    if v == 3:
        # Oscilloscope / amplifier stack on top of radio
        bx(g, -0.40, -0.08, h + 0.30, 0.06, 0.22, h + 0.48, t_steel)
        front_label(g, (-0.17, 0.07, h + 0.39), (0.36, 0.30, 0.12), t_screen, face="front")
    return g


@register(BUILDERS)
def craft_storage(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wood = T("wood", "wood_plank_worn")
    t_dark = T("dark", "wood_dark")
    t_steel = T("steel", "steel_brushed")
    t_green = T("crate", "painted_metal_green")
    t_tarp = T("tarp", "fabric_green")

    if v == 1:
        # Heavy reinforced supply chest + stacked crates
        bx(g, -0.60, -0.35, 0.0, 0.60, 0.35, 0.58, t_wood)
        bx(g, -0.62, -0.37, 0.54, 0.62, 0.37, 0.64, t_dark)
        # Metal corner bands & latches
        for sx in (-0.56, 0.56):
            bx(g, sx - 0.03, -0.36, 0.0, sx + 0.03, 0.36, 0.64, t_steel)
        for lx in (-0.25, 0.25):
            bx(g, lx - 0.04, -0.38, 0.46, lx + 0.04, -0.35, 0.58, t_steel)
        bx(g, -0.52, -0.24, 0.64, -0.05, 0.24, 0.96, t_green)
    else:
        # Multi-tier heavy survivor shelving rack with crates & tarp
        w, d, h = 1.40, 0.62, 1.65
        hx, hy = w / 2.0, d / 2.0
        for sx in (-1, 1):
            for sy in (-1, 1):
                x, y = sx * (hx - 0.03), sy * (hy - 0.03)
                bx(g, x - 0.025, y - 0.025, 0.0, x + 0.025, y + 0.025, h, t_steel)
        for z in (0.06, 0.58, 1.10, 1.60):
            bx(g, -hx, -hy, z, hx, hy, z + 0.04, t_wood)
        # Stored crates on shelves
        bx(g, -0.58, -0.24, 0.10, -0.06, 0.24, 0.52, t_dark)
        bx(g, 0.06, -0.22, 0.10, 0.58, 0.22, 0.48, t_green)
        bx(g, -0.54, -0.22, 0.62, 0.12, 0.24, 1.02, t_wood)
        bx(g, 0.18, -0.20, 0.62, 0.56, 0.20, 0.96, t_tarp)
        if v == 3:
            bx(g, -0.48, -0.22, 1.14, 0.42, 0.22, 1.50, t_tarp)
    return g


@register(BUILDERS)
def craft_bed(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_frame = T("frame", "wood_dark" if v == 2 else "steel_brushed")
    t_mat = T("mattress", "fabric_green" if v != 3 else "fabric_blue")
    t_blanket = T("blanket", "fabric_blue" if v != 3 else "leather_brown")
    t_pillow = T("pillow", "plastic_white")
    t_pack = T("pack", "leather_brown")

    w, d, h = 2.00, 0.92, 0.44
    hx, hy = w / 2.0, d / 2.0

    # Folding/bunk legs
    for sx in (-0.82, 0.0, 0.82):
        for sy in (-1, 1):
            y = sy * (hy - 0.05)
            bx(g, sx - 0.03, y - 0.03, 0.0, sx + 0.03, y + 0.03, h - 0.06, t_frame)
        bx(g, sx - 0.025, -hy + 0.05, 0.08, sx + 0.025, hy - 0.05, 0.12, t_frame)

    # Side rails + headboard
    bx(g, -hx, -hy, h - 0.08, hx, hy, h - 0.03, t_frame)
    bx(g, -hx, -hy, h - 0.03, -hx + 0.06, hy, h + 0.34, t_frame)
    if v >= 2:
        bx(g, hx - 0.06, -hy, h - 0.03, hx, hy, h + 0.22, t_frame)

    # Mattress, pillow, blanket
    bx(g, -hx + 0.06, -hy + 0.04, h - 0.03, hx - 0.06, hy - 0.04, h + 0.09, t_mat)
    bx(g, -hx + 0.12, -hy + 0.14, h + 0.09, -hx + 0.44, hy - 0.14, h + 0.17, t_pillow)
    bx(g, -0.25, -hy + 0.03, h + 0.08, hx - 0.08, hy - 0.03, h + 0.13, t_blanket)

    # Stashed survivor pack under cot
    bx(g, -0.35, -0.28, 0.0, 0.25, 0.22, 0.26, t_pack)
    if v == 3:
        # Makeshift canopy posts
        for sx in (-hx + 0.03, hx - 0.03):
            for sy in (-hy + 0.03, hy - 0.03):
                bx(g, sx - 0.025, sy - 0.025, h, sx + 0.025, sy + 0.025, 1.55, t_frame)
        bx(g, -hx, -hy, 1.52, hx, hy, 1.58, t_blanket)
    return g


def entries():
    out = []
    specs = [
        ("craft_workbench", "ENV_MAT_2K", [
            dict(wood="wood_plank_worn", frame="wood_dark", accent="painted_metal_green"),
            dict(wood="wood_light", frame="steel_brushed", accent="painted_metal_blue"),
            dict(wood="wood_dark", frame="rust_heavy", accent="painted_metal_green"),
        ]),
        ("craft_weaponbench", "ENV_MAT_2K", [
            dict(frame="painted_metal_green", top="wood_dark"),
            dict(frame="painted_metal_blue", top="steel_brushed"),
            dict(frame="rust_heavy", top="wood_plank_worn"),
        ]),
        ("craft_chem", "HERO_ITEMS_1K", [
            dict(steel="lab_steel", chem="chem_yellow"),
            dict(steel="lab_steel", chem="battery_green"),
            dict(steel="radio_olive", chem="chem_yellow"),
        ]),
        ("craft_medstation", "HERO_ITEMS_1K", [
            dict(steel="lab_steel", med="medkit_white"),
            dict(steel="lab_steel", med="medkit_white", bandage="document_paper"),
            dict(steel="lab_steel", med="medkit_white", red="canister_red"),
        ]),
        ("craft_purifier", "ENV_MAT_2K", [
            dict(tank="painted_metal_blue", wood="wood_plank_worn"),
            dict(tank="painted_metal_green", wood="wood_dark"),
            dict(tank="plastic_white", wood="wood_light"),
        ]),
        ("craft_stove", "ENV_MAT_2K", [
            dict(rust="rust_heavy", black="plastic_black"),
            dict(rust="steel_brushed", black="plastic_black"),
            dict(rust="rust_heavy", black="painted_metal_green"),
        ]),
        ("craft_generator", "HERO_ITEMS_1K", [
            dict(body="generator_orange", tank="canister_red"),
            dict(body="radio_olive", tank="canister_red"),
            dict(body="tool_orange", tank="generator_orange"),
        ]),
        ("craft_radio", "HERO_ITEMS_1K", [
            dict(radio="radio_olive", screen="screen_green"),
            dict(radio="lab_steel", screen="screen_green"),
            dict(radio="vehicle_armor", screen="screen_green"),
        ]),
        ("craft_storage", "ENV_MAT_2K", [
            dict(wood="wood_plank_worn", crate="painted_metal_green"),
            dict(wood="wood_dark", crate="painted_metal_blue"),
            dict(wood="wood_light", crate="rust_heavy"),
        ]),
        ("craft_bed", "ENV_MAT_2K", [
            dict(mattress="fabric_green", blanket="fabric_blue"),
            dict(mattress="fabric_blue", blanket="fabric_red"),
            dict(mattress="fabric_green", blanket="leather_brown"),
        ]),
    ]
    for bname, atlas, pals in specs:
        for i, pal in enumerate(pals, 1):
            eid = "%s_%02d" % (bname, i)
            out.append(mk(eid, "Crafting", bname, params=dict(variant=i, seed=i), atlas=atlas, palette=pal))
    return out
