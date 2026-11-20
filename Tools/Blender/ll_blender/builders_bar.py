"""Procedural Barricade & defensive cover builders (category 'Barricades', atlas ENV_MAT_2K / ENV_STRUCT_2K)."""
import math
from .builders_common import (
    register, mk, Tags, geo_for, bx, obox, tube, pick
)
from . import palettes as P

BUILDERS = {}
ATLAS_MAT = "ENV_MAT_2K"
ATLAS_STRUCT = "ENV_STRUCT_2K"


@register(BUILDERS)
def b_bar_planks(params, pal, atlas=ATLAS_MAT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.8))
    h = float(params.get("h", 1.35))
    style = int(params.get("style", 0))
    wood_t = T("wood", "wood_plank_worn")
    post_t = T("post", "wood_dark")
    metal_t = T("metal", "rust_heavy")

    hw = w / 2.0
    for sx in (-hw * 0.72, hw * 0.72):
        bx(g, sx - 0.05, -0.35, 0.0, sx + 0.05, 0.35, 0.07, post_t)
        bx(g, sx - 0.05, -0.06, 0.07, sx + 0.05, 0.02, h, post_t)
        obox(g, (sx, 0.16, h * 0.45), (0.08, 0.07, h * 0.92), post_t, rot=(0.36, 0, 0))

    n_planks = 5 + (style % 2)
    for i in range(n_planks):
        pz = 0.14 + i * (h - 0.24) / max(1, n_planks - 1)
        ang = ((i % 3) - 1) * 0.055
        obox(g, (0.0, -0.09, pz), (w, 0.04, 0.17), wood_t, rot=(0, ang, 0))
    obox(g, (-hw * 0.25, -0.13, h * 0.52), (w * 0.85, 0.035, 0.14), post_t, rot=(0, 0.48, 0))
    if style in (2, 4, 5):
        obox(g, (hw * 0.25, -0.13, h * 0.52), (w * 0.85, 0.035, 0.14), metal_t, rot=(0, -0.48, 0))
    return g


@register(BUILDERS)
def b_bar_sandbag(params, pal, atlas=ATLAS_MAT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.0))
    courses = int(params.get("courses", 5))
    style = int(params.get("style", 0))
    bag_t = T("bag", "wood_light")
    alt_t = T("alt", "leather_brown")

    bw, bd, bh = 0.46, 0.28, 0.15
    cols = max(3, int(round(w / bw)))
    total_w = cols * bw
    for r in range(courses):
        z0 = r * (bh * 0.92)
        stag = (r % 2) * (bw * 0.45)
        n_c = cols - (1 if (r == courses - 1 and style % 2 == 1) else 0)
        for c in range(n_c):
            if style == 3 and r >= courses - 2 and c == n_c // 2:
                continue
            cx = -total_w / 2.0 + (c + 0.5) * bw + (stag if c < n_c - 1 else 0.0)
            cy = ((c + r) % 2 - 0.5) * 0.03
            tag = bag_t if (r + c) % 2 == 0 else alt_t
            obox(g, (cx, cy, z0 + bh / 2.0), (bw * 0.94, bd, bh), tag,
                 rot=(0.0, 0.0, ((c - r) % 3 - 1) * 0.06), taper=0.88)
    return g


@register(BUILDERS)
def b_bar_debris(params, pal, atlas=ATLAS_MAT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.2))
    style = int(params.get("style", 0))
    wood_t = T("wood", "wood_plank_worn")
    metal_t = T("metal", "rust_heavy")
    dark_t = T("dark", "plastic_black")

    hw = w / 2.0
    bx(g, -hw * 0.85, -0.45, 0.0, hw * 0.85, 0.45, 0.28, dark_t)
    obox(g, (-hw * 0.35, -0.05, 0.48), (0.85, 0.52, 0.42), wood_t, rot=(0.15, 0.22, -0.25))
    obox(g, (hw * 0.32, 0.08, 0.44), (0.78, 0.48, 0.38), metal_t, rot=(-0.18, -0.20, 0.30))

    for k in range(5):
        px = -hw * 0.65 + k * (w * 0.32)
        ang_y = -0.35 + (k % 3) * 0.35
        obox(g, (px, -0.10 + (k % 2) * 0.2, 0.65), (0.14, 0.95, 0.10), wood_t, rot=(0.28, ang_y, 0.4 * k))
    tube(g, (-hw * 0.7, -0.25, 0.15), (hw * 0.65, 0.22, 0.95), 0.035, 0.035, metal_t, seg=6)
    tube(g, (hw * 0.6, -0.30, 0.12), (-hw * 0.5, 0.28, 0.88), 0.03, 0.03, T("pipe", "steel_brushed"), seg=6)
    return g


@register(BUILDERS)
def b_bar_crate(params, pal, atlas=ATLAS_MAT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.1))
    d = float(params.get("d", 0.95))
    h = float(params.get("h", 0.95))
    style = int(params.get("style", 0))
    panel_t = T("panel", "wood_light")
    frame_t = T("frame", "wood_dark")
    metal_t = T("corner", "rust_heavy")

    def _crate(cx, cy, cz, cw, cd, ch, rz=0.0):
        obox(g, (cx, cy, cz + ch / 2.0), (cw - 0.06, cd - 0.06, ch - 0.04), panel_t, rot=(0, 0, rz))
        obox(g, (cx, cy, cz + 0.05), (cw, cd, 0.10), frame_t, rot=(0, 0, rz))
        obox(g, (cx, cy, cz + ch - 0.05), (cw, cd, 0.10), frame_t, rot=(0, 0, rz))
        for sx in (-1, 1):
            for sy in (-1, 1):
                px = cx + sx * (cw / 2.0 - 0.05)
                py = cy + sy * (cd / 2.0 - 0.05)
                if abs(rz) < 1e-4:
                    bx(g, px - 0.05, py - 0.05, cz + 0.10, px + 0.05, py + 0.05, cz + ch - 0.10, metal_t)

    if style in (2, 4, 5):
        _crate(-w * 0.42, 0.0, 0.0, w * 0.82, d * 0.85, h * 0.82)
        _crate(w * 0.44, 0.04, 0.0, w * 0.78, d * 0.82, h * 0.78)
        _crate(0.0, 0.02, h * 0.80, w * 0.76, d * 0.74, h * 0.70, rz=0.12)
    else:
        _crate(0.0, 0.0, 0.0, w, d, h)
    return g


@register(BUILDERS)
def b_bar_concrete(params, pal, atlas=ATLAS_STRUCT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.2))
    h = float(params.get("h", 0.98))
    style = int(params.get("style", 0))
    conc_t = T("concrete", "concrete_raw")
    dark_t = T("base", "concrete_dark")
    metal_t = T("rebar", "metal_plate")

    hw = w / 2.0
    bx(g, -hw, -0.32, 0.0, hw, 0.32, 0.14, dark_t)
    obox(g, (0, 0, 0.28), (w, 0.56, 0.28), conc_t, taper=0.62)
    obox(g, (0, 0, 0.42 + (h - 0.42) / 2.0), (w, 0.34, h - 0.42), conc_t, taper=0.68)

    for sx in (-hw * 0.48, hw * 0.48):
        bx(g, sx - 0.16, -0.33, 0.0, sx + 0.16, 0.33, 0.08, dark_t)

    if style in (1, 3, 5):
        for sx in (-hw * 0.55, 0.0, hw * 0.55):
            tube(g, (sx, -0.04, h - 0.02), (sx, 0.04, h + 0.18), 0.018, 0.016, metal_t, seg=5)
    return g


@register(BUILDERS)
def b_bar_steelplate(params, pal, atlas=ATLAS_MAT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.8))
    h = float(params.get("h", 1.30))
    style = int(params.get("style", 0))
    plate_t = T("plate", "rust_heavy")
    frame_t = T("frame", "steel_brushed")

    hw = w / 2.0
    for sx in (-hw * 0.72, hw * 0.72):
        bx(g, sx - 0.05, -0.25, 0.0, sx + 0.05, 0.48, 0.06, frame_t)
        obox(g, (sx, 0.22, h * 0.45), (0.07, 0.07, h * 0.92), frame_t, rot=(0.42, 0, 0))

    obox(g, (0, -0.06, h * 0.42), (w, 0.05, h * 0.76), plate_t, rot=(-0.15, 0, 0))
    for sx in (-hw * 0.52, hw * 0.52):
        obox(g, (sx, -0.14, h * 0.88), (w * 0.42, 0.05, h * 0.24), frame_t, rot=(-0.15, 0, 0))
    return g


@register(BUILDERS)
def b_bar_wreck(params, pal, atlas=ATLAS_MAT):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.4))
    style = int(params.get("style", 0))
    rust_t = T("rust", "rust_heavy")
    steel_t = T("steel", "steel_brushed")
    tire_t = T("tire", "rubber")
    paint_t = T("paint", "painted_metal_green")

    hw = w / 2.0
    bx(g, -hw * 0.65, -0.38, 0.0, hw * 0.45, 0.36, 0.78, paint_t)
    dx = hw * 0.68
    tube(g, (dx, -0.05, 0.0), (dx, -0.05, 0.88), 0.28, 0.28, rust_t, seg=12)
    for rz in (0.28, 0.60):
        tube(g, (dx, -0.05, rz), (dx, -0.05, rz + 0.03), 0.295, 0.295, steel_t, seg=12)
    tx = -hw * 0.72
    for tz in (0.0, 0.22, 0.44):
        tube(g, (tx, 0.08, tz), (tx, 0.08, tz + 0.21), 0.30, 0.30, tire_t, seg=12)
    obox(g, (-0.1, -0.22, 0.92), (w * 0.88, 0.08, 0.45), rust_t, rot=(-0.18, 0.08, 0.05))
    return g


def entries():
    out = []
    woods = P.MAT["wood"]
    metals = P.MAT["metal"]
    paints = P.MAT["paint"]

    # 1. bar_planks_01..06
    for i in range(6):
        out.append(mk(f"bar_planks_{i + 1:02d}", "Barricades", "b_bar_planks",
                      params={"w": 1.6 + (i % 3) * 0.3, "h": 1.25 + (i % 2) * 0.2, "style": i},
                      atlas=ATLAS_MAT,
                      palette={"wood": pick(woods, i), "post": pick(woods, i + 1), "metal": pick(metals, i)}))

    # 2. bar_sandbag_01..06
    for i in range(6):
        out.append(mk(f"bar_sandbag_{i + 1:02d}", "Barricades", "b_bar_sandbag",
                      params={"w": 1.8 + (i % 3) * 0.4, "courses": 4 + (i % 3), "style": i},
                      atlas=ATLAS_MAT,
                      palette={"bag": pick(["wood_light", "leather_brown", "fabric_green"], i),
                               "alt": pick(["wood_plank_worn", "leather_brown"], i)}))

    # 3. bar_debris_01..06
    for i in range(6):
        out.append(mk(f"bar_debris_{i + 1:02d}", "Barricades", "b_bar_debris",
                      params={"w": 2.0 + (i % 3) * 0.3, "style": i},
                      atlas=ATLAS_MAT,
                      palette={"wood": pick(woods, i), "metal": pick(metals, i), "dark": "plastic_black"}))

    # 4. bar_crate_01..06
    for i in range(6):
        out.append(mk(f"bar_crate_{i + 1:02d}", "Barricades", "b_bar_crate",
                      params={"w": 1.0 + (i % 2) * 0.2, "d": 0.90, "h": 0.90, "style": i},
                      atlas=ATLAS_MAT,
                      palette={"panel": pick(woods + paints, i), "frame": pick(woods, i + 1),
                               "corner": pick(metals, i)}))

    # 5. bar_concrete_01..06 (ENV_STRUCT_2K)
    for i in range(6):
        out.append(mk(f"bar_concrete_{i + 1:02d}", "Barricades", "b_bar_concrete",
                      params={"w": 2.0 + (i % 3) * 0.3, "h": 0.95 + (i % 2) * 0.15, "style": i},
                      atlas=ATLAS_STRUCT,
                      palette={"concrete": pick(["concrete_raw", "concrete_dark", "wall_damaged"], i),
                               "base": "concrete_dark", "rebar": "metal_plate"}))

    # 6. bar_steelplate_01..06
    for i in range(6):
        out.append(mk(f"bar_steelplate_{i + 1:02d}", "Barricades", "b_bar_steelplate",
                      params={"w": 1.6 + (i % 3) * 0.25, "h": 1.25 + (i % 2) * 0.15, "style": i},
                      atlas=ATLAS_MAT,
                      palette={"plate": pick(["rust_heavy", "steel_brushed", "painted_metal_green", "painted_metal_blue"], i),
                               "frame": "steel_brushed"}))

    # 7. bar_wreck_01..06
    for i in range(6):
        out.append(mk(f"bar_wreck_{i + 1:02d}", "Barricades", "b_bar_wreck",
                      params={"w": 2.2 + (i % 3) * 0.3, "style": i},
                      atlas=ATLAS_MAT,
                      palette={"rust": "rust_heavy", "steel": "steel_brushed", "tire": "rubber",
                               "paint": pick(paints, i)}))

    return out
