"""Procedural Mall & retail builders (category 'Mall', atlas ENV_MAT_2K)."""
import math
from .builders_common import (
    register, mk, Tags, geo_for, bx, obox, tube, ball, pick
)
from . import palettes as P

BUILDERS = {}
ATLAS = "ENV_MAT_2K"


@register(BUILDERS)
def b_mall_shelf(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.8))
    d = float(params.get("d", 0.85))
    h = float(params.get("h", 1.75))
    tiers = int(params.get("tiers", 4))
    style = int(params.get("style", 0))
    frame_t = T("frame", "steel_brushed")
    shelf_t = T("shelf", "plastic_white")
    goods_t = T("goods", "fabric_red")

    hw, hd = w / 2.0, d / 2.0
    if style == 5:
        # Circular retail display carousel rack
        r = min(hw, hd) * 0.9
        tube(g, (0, 0, 0.0), (0, 0, 0.08), r, r, frame_t, seg=12)
        tube(g, (0, 0, 0.08), (0, 0, h), 0.06, 0.06, frame_t, seg=8)
        for t in range(3):
            zt = 0.45 + t * 0.45
            tube(g, (0, 0, zt), (0, 0, zt + 0.04), r * (1.0 - 0.12 * t), r * (1.0 - 0.12 * t), shelf_t, seg=12)
        return g

    # Gondola base kickplate & central spine wall
    bx(g, -hw, -hd, 0.0, hw, hd, 0.14, frame_t)
    double_sided = style in (0, 3, 4)
    spine_y0 = -0.03 if double_sided else hd - 0.06
    bx(g, -hw, spine_y0, 0.14, hw, spine_y0 + 0.06, h, frame_t)
    # Upright side posts
    for sx in (-hw, 0.0, hw - 0.04):
        bx(g, sx, spine_y0 - 0.02, 0.14, sx + 0.04, spine_y0 + 0.08, h, frame_t)

    for t in range(1, tiers):
        zt = 0.14 + t * (h - 0.25) / tiers
        bx(g, -hw, -hd + 0.04, zt, hw, spine_y0, zt + 0.035, shelf_t)
        if double_sided:
            bx(g, -hw, spine_y0 + 0.06, zt, hw, hd - 0.04, zt + 0.035, shelf_t)
        if (t + style) % 2 == 1:
            # Boxed merchandise on shelf
            bx(g, -hw * 0.7, -hd * 0.75, zt + 0.035, -0.05, spine_y0 - 0.04, zt + 0.22, goods_t)
            bx(g, 0.08, -hd * 0.75, zt + 0.035, hw * 0.75, spine_y0 - 0.04, zt + 0.18, T("alt", "painted_metal_blue"))
    return g


@register(BUILDERS)
def b_mall_counter(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.2))
    d = float(params.get("d", 0.85))
    h = float(params.get("h", 0.92))
    style = int(params.get("style", 0))
    body_t = T("body", "wood_dark")
    top_t = T("top", "steel_brushed")
    belt_t = T("belt", "rubber")
    glass_t = T("glass", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    bx(g, -hw, -hd, 0.0, hw, hd, 0.08, top_t)

    if style in (1, 5, 6):
        # Glass display showcase counter (jewelry / bakery)
        bx(g, -hw, -hd, 0.08, hw, hd, h * 0.52, body_t)
        bx(g, -hw, -hd, h * 0.52, hw, -hd + 0.04, h, glass_t)
        bx(g, -hw, -hd, h - 0.04, hw, hd, h, glass_t)
        bx(g, -hw, -hd, h * 0.52, -hw + 0.04, hd, h, glass_t)
        bx(g, hw - 0.04, -hd, h * 0.52, hw, hd, h, glass_t)
        # Interior glass shelf
        bx(g, -hw + 0.05, -hd + 0.05, h * 0.74, hw - 0.05, hd - 0.05, h * 0.77, glass_t)
    else:
        # Supermarket checkout counter with conveyor belt & POS register
        bx(g, -hw, -hd, 0.08, hw, hd, h - 0.05, body_t)
        bx(g, -hw - 0.02, -hd - 0.02, h - 0.05, hw + 0.02, hd + 0.02, h, top_t)
        bx(g, -hw + 0.08, -hd + 0.10, h, hw * 0.25, hd - 0.10, h + 0.02, belt_t)
        # POS cash register & pole screen
        bx(g, hw * 0.35, -0.18, h, hw * 0.75, 0.18, h + 0.14, belt_t)
        tube(g, (hw * 0.55, 0.08, h + 0.14), (hw * 0.55, 0.08, h + 0.34), 0.02, 0.02, top_t, seg=6)
        bx(g, hw * 0.40, 0.04, h + 0.28, hw * 0.70, 0.10, h + 0.48, belt_t)
        if style == 3:
            # Sneeze guard glass
            bx(g, -hw + 0.1, -hd, h, hw - 0.1, -hd + 0.03, h + 0.38, glass_t)
    return g


@register(BUILDERS)
def b_mall_mannequin(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    body_t = T("body", "plastic_white")
    cloth_t = T("cloth", "fabric_red")
    base_t = T("base", "steel_brushed")

    # Pedestal base & support rod
    bx(g, -0.22, -0.22, 0.0, 0.22, 0.22, 0.03, base_t)
    tube(g, (0, 0.05, 0.03), (0, 0.05, 0.85), 0.02, 0.02, base_t, seg=6)

    if style in (1, 6):
        # Tailor's torso dress form on tall pole
        tube(g, (0, 0, 0.82), (0, 0, 1.12), 0.14, 0.11, cloth_t, seg=10, squash=(1.2, 0.8))
        tube(g, (0, 0, 1.12), (0, 0, 1.44), 0.11, 0.17, cloth_t, seg=10, squash=(1.25, 0.82))
        tube(g, (0, 0, 1.44), (0, 0, 1.54), 0.055, 0.05, body_t, seg=8)
    else:
        # Full-body mannequin: legs, pelvis, torso, arms, head
        stride = 0.08 if style in (2, 3) else 0.0
        for s in (-1, 1):
            lx = s * 0.09
            ly = s * stride
            tube(g, (lx, ly, 0.03), (lx, ly * 0.5, 0.46), 0.045, 0.058, body_t, seg=8)
            tube(g, (lx, ly * 0.5, 0.46), (lx, 0.0, 0.88), 0.058, 0.072, cloth_t, seg=8)
        # Pelvis & torso
        tube(g, (0, 0, 0.86), (0, 0, 1.12), 0.14, 0.11, cloth_t, seg=10, squash=(1.2, 0.8))
        tube(g, (0, 0, 1.12), (0, 0, 1.44), 0.11, 0.17, cloth_t, seg=10, squash=(1.25, 0.82))
        # Arms
        for s in (-1, 1):
            ax = s * 0.21
            tube(g, (ax, 0, 1.40), (ax * 1.15, -0.04 * s, 1.10), 0.046, 0.04, cloth_t, seg=6)
            tube(g, (ax * 1.15, -0.04 * s, 1.10), (ax * 1.1, -0.10, 0.82), 0.038, 0.03, body_t, seg=6)
        # Neck & oval head
        tube(g, (0, 0, 1.44), (0, 0, 1.53), 0.048, 0.045, body_t, seg=8)
        ball(g, (0, 0, 1.64), 0.095, body_t, seg=8, rings=6, scale=(0.85, 0.95, 1.15))
    return g


@register(BUILDERS)
def b_mall_shopfront(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 4.0))
    h = float(params.get("h", 3.0))
    style = int(params.get("style", 0))
    frame_t = T("frame", "steel_brushed")
    sign_t = T("sign", "painted_metal_blue")
    glass_t = T("glass", "glass_dirty")
    plinth_t = T("plinth", "wood_dark")

    hw = w / 2.0
    fd = 0.24
    # Side mullions & top signage fascia box
    bx(g, -hw, -fd / 2, 0.0, -hw + 0.18, fd / 2, h, frame_t)
    bx(g, hw - 0.18, -fd / 2, 0.0, hw, fd / 2, h, frame_t)
    bx(g, -hw, -fd / 2 - 0.04, h - 0.58, hw, fd / 2 + 0.04, h, sign_t)

    # Central double-door opening (width 1.4m) + flanking display windows
    dw = 0.70
    for s in (-1, 1):
        # Door frame upright
        mx = s * dw
        bx(g, mx - 0.04, -fd / 2, 0.0, mx + 0.04, fd / 2, h - 0.58, frame_t)
        # Flanking display window plinth & glass pane
        wx0 = (-hw + 0.18) if s == -1 else (dw + 0.04)
        wx1 = (-dw - 0.04) if s == -1 else (hw - 0.18)
        bx(g, wx0, -fd / 2, 0.0, wx1, fd / 2, 0.35, plinth_t)
        bx(g, wx0, -0.02, 0.35, wx1, 0.02, h - 0.58, glass_t)

    if style == 2:
        # Security roller shutter half-lowered over entrance
        bx(g, -dw, -0.04, 1.05, dw, 0.04, h - 0.58, frame_t)
    else:
        # Glass entrance doors
        for s in (-1, 1):
            x0 = (-dw + 0.04) if s == -1 else 0.01
            x1 = -0.01 if s == -1 else (dw - 0.04)
            bx(g, x0, -0.025, 0.0, x1, 0.025, h - 0.62, glass_t)
            hx = s * 0.08
            bx(g, hx - 0.015, -0.06, 0.90, hx + 0.015, 0.06, 1.25, frame_t)
    return g


@register(BUILDERS)
def b_mall_cart(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    style = int(params.get("style", 0))
    metal_t = T("metal", "steel_brushed")
    trim_t = T("handle", "fabric_red")
    wheel_t = T("wheel", "rubber")

    w, d = 0.56, 0.88
    hw, hd = w / 2.0, d / 2.0
    # 4 caster wheels sitting on z=0.0
    for sx in (-hw + 0.05, hw - 0.05):
        for sy in (-hd + 0.08, hd - 0.08):
            tube(g, (sx - 0.018, sy, 0.045), (sx + 0.018, sy, 0.045), 0.045, 0.045, wheel_t, seg=8)
            bx(g, sx - 0.015, sy - 0.02, 0.045, sx + 0.015, sy + 0.02, 0.11, metal_t)

    # Lower chassis frame
    bx(g, -hw + 0.03, -hd + 0.06, 0.11, hw - 0.03, hd - 0.06, 0.14, metal_t)

    if style == 1:
        # Flatbed warehouse bulk cart
        bx(g, -hw, -hd, 0.14, hw, hd, 0.20, T("deck", "wood_plank_worn"))
        for sx in (-hw + 0.03, hw - 0.03):
            tube(g, (sx, hd - 0.05, 0.20), (sx, hd - 0.05, 0.96), 0.022, 0.022, metal_t, seg=6)
        tube(g, (-hw + 0.03, hd - 0.05, 0.95), (hw - 0.03, hd - 0.05, 0.95), 0.022, 0.022, trim_t, seg=6)
    else:
        # Rear uprights & push handle
        for sx in (-hw + 0.03, hw - 0.03):
            tube(g, (sx, hd - 0.06, 0.14), (sx, hd + 0.04, 0.96), 0.02, 0.02, metal_t, seg=6)
        tube(g, (-hw, hd + 0.06, 0.96), (hw, hd + 0.06, 0.96), 0.022, 0.022, trim_t, seg=6)
        # Wire basket walls (hollow 4-sided basket)
        z_bot, z_top = 0.42, 0.88
        bx(g, -hw, -hd, z_bot, hw, hd - 0.04, z_bot + 0.03, metal_t)
        bx(g, -hw, -hd, z_bot, -hw + 0.03, hd - 0.04, z_top, metal_t)
        bx(g, hw - 0.03, -hd, z_bot, hw, hd - 0.04, z_top, metal_t)
        bx(g, -hw, -hd, z_bot, hw, -hd + 0.03, z_top - 0.06, metal_t)
        bx(g, -hw, hd - 0.07, z_bot, hw, hd - 0.04, z_top, metal_t)
        # Top bumper rim
        bx(g, -hw - 0.01, -hd - 0.01, z_top - 0.03, hw + 0.01, hd - 0.03, z_top, trim_t)
    return g


@register(BUILDERS)
def b_mall_kiosk(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 2.4))
    d = float(params.get("d", 2.0))
    h = float(params.get("h", 2.3))
    style = int(params.get("style", 0))
    body_t = T("body", "wood_dark")
    top_t = T("top", "plastic_white")
    sign_t = T("canopy", "painted_metal_blue")
    metal_t = T("pole", "steel_brushed")

    hw, hd = w / 2.0, d / 2.0
    if style in (3, 4):
        # Digital wayfinding directory / ticket totem kiosk
        bx(g, -0.45, -0.32, 0.0, 0.45, 0.32, 0.08, metal_t)
        bx(g, -0.38, -0.14, 0.08, 0.38, 0.14, 1.95, body_t)
        bx(g, -0.32, -0.155, 0.75, 0.32, -0.135, 1.78, T("screen", "glass_dirty"))
        return g

    # 4-sided hollow counter island
    cw = 0.38
    ch = 0.92
    bx(g, -hw, -hd, 0.0, hw, -hd + cw, ch, body_t)
    bx(g, -hw, hd - cw, 0.0, hw, hd, ch, body_t)
    bx(g, -hw, -hd + cw, 0.0, -hw + cw, hd - cw, ch, body_t)
    bx(g, hw - cw, -hd + cw, 0.0, hw, hd - cw, ch, body_t)
    # Countertop overhang rim
    bx(g, -hw - 0.04, -hd - 0.04, ch, hw + 0.04, -hd + cw + 0.02, ch + 0.05, top_t)
    bx(g, -hw - 0.04, hd - cw - 0.02, ch, hw + 0.04, hd + 0.04, ch + 0.05, top_t)
    bx(g, -hw - 0.04, -hd + cw, ch, -hw + cw + 0.02, hd - cw, ch + 0.05, top_t)
    bx(g, hw - cw - 0.02, -hd + cw, ch, hw + 0.04, hd - cw, ch + 0.05, top_t)

    # Corner canopy poles & overhead sign roof
    for sx in (-hw + 0.15, hw - 0.15):
        for sy in (-hd + 0.15, hd - 0.15):
            tube(g, (sx, sy, ch + 0.05), (sx, sy, h - 0.25), 0.032, 0.032, metal_t, seg=6)
    bx(g, -hw - 0.06, -hd - 0.06, h - 0.25, hw + 0.06, hd + 0.06, h, sign_t)
    return g


@register(BUILDERS)
def b_mall_atm(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 0.82))
    d = float(params.get("d", 0.72))
    h = float(params.get("h", 1.78))
    style = int(params.get("style", 0))
    body_t = T("body", "steel_brushed")
    trim_t = T("trim", "painted_metal_blue")
    dark_t = T("dark", "plastic_black")
    scr_t = T("screen", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    # Lower safe pedestal
    bx(g, -hw, -hd, 0.0, hw, hd, 0.88, body_t)
    # Side privacy wings & upper housing
    bx(g, -hw, -hd, 0.88, -hw + 0.08, hd, h - 0.22, trim_t)
    bx(g, hw - 0.08, -hd, 0.88, hw, hd, h - 0.22, trim_t)
    bx(g, -hw + 0.08, -hd + 0.18, 0.88, hw - 0.08, hd, h - 0.22, body_t)
    # Illuminated header topper box
    bx(g, -hw - 0.02, -hd - 0.04, h - 0.22, hw + 0.02, hd, h, trim_t)
    # Keypad shelf, sloped screen & cash dispenser slot
    bx(g, -hw + 0.08, -hd - 0.05, 0.86, hw - 0.08, -hd + 0.20, 0.92, dark_t)
    obox(g, (0, -hd + 0.16, 1.24), (w * 0.56, 0.04, 0.34), scr_t, rot=(-0.22, 0, 0))
    bx(g, -0.14, -hd - 0.02, 0.70, 0.14, -hd, 0.76, dark_t)
    bx(g, 0.16, -hd + 0.14, 1.15, 0.26, -hd + 0.18, 1.24, trim_t)
    return g


@register(BUILDERS)
def b_mall_escalator(params, pal, atlas=ATLAS):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    w = float(params.get("w", 1.5))
    d = float(params.get("d", 4.2))
    h = float(params.get("h", 2.4))
    steps = int(params.get("steps", 12))
    style = int(params.get("style", 0))
    truss_t = T("truss", "steel_brushed")
    tread_t = T("tread", "plastic_black")
    rail_t = T("rail", "rubber")
    glass_t = T("balustrade", "glass_dirty")

    hw, hd = w / 2.0, d / 2.0
    # Bottom & top landing floor plates (z >= 0.0)
    land_d = 0.65
    bx(g, -hw, -hd, 0.0, hw, -hd + land_d, 0.18, truss_t)
    bx(g, -hw, hd - land_d, h - 0.18, hw, hd, h, truss_t)

    # Stepped treads along incline
    run_d = d - 2 * land_d
    sd = run_d / steps
    sh = h / steps
    iw = w - 0.28
    for i in range(steps):
        y0 = -hd + land_d + i * sd
        y1 = y0 + sd
        z1 = 0.18 + (i + 1) * ((h - 0.18) / steps)
        z0 = max(0.0, z1 - sh * 1.35)
        bx(g, -iw / 2, y0, z0, iw / 2, y1, z1, tread_t)

    # Balustrade side trusses & moving handrails
    for side in (-1, 1):
        sx0 = -hw if side == -1 else (hw - 0.14)
        sx1 = sx0 + 0.14
        rx = (sx0 + sx1) / 2.0
        # Bottom & top newel posts
        bx(g, sx0, -hd, 0.18, sx1, -hd + land_d, 1.02, glass_t)
        bx(g, sx0, hd - land_d, h, sx1, hd, h + 0.84, glass_t)
        # Slanted truss beam + handrail loop
        tube(g, (rx, -hd + land_d, 0.20), (rx, hd - land_d, h - 0.05), 0.10, 0.10, truss_t, seg=6)
        tube(g, (rx, -hd + land_d, 0.60), (rx, hd - land_d, h + 0.42), 0.07, 0.07, glass_t if style % 2 == 0 else truss_t, seg=6)
        tube(g, (rx, -hd + 0.05, 1.02), (rx, -hd + land_d, 1.02), 0.05, 0.05, rail_t, seg=6)
        tube(g, (rx, -hd + land_d, 1.02), (rx, hd - land_d, h + 0.84), 0.05, 0.05, rail_t, seg=6)
        tube(g, (rx, hd - land_d, h + 0.84), (rx, hd - 0.05, h + 0.84), 0.05, 0.05, rail_t, seg=6)
    return g


def entries():
    out = []
    woods = P.MAT["wood"]
    fabrics = P.MAT["fabric"]
    metals = P.MAT["metal"]
    paints = P.MAT["paint"]

    # 1. mall_shelf_01..07
    for i in range(7):
        out.append(mk(f"mall_shelf_{i + 1:02d}", "Mall", "b_mall_shelf",
                      params={"w": 1.6 + (i % 3) * 0.3, "d": 0.85, "h": 1.70, "tiers": 4 + (i % 2), "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(metals, i), "shelf": pick(["plastic_white", "wood_light", "steel_brushed"], i),
                               "goods": pick(fabrics, i)}))

    # 2. mall_counter_01..07
    for i in range(7):
        out.append(mk(f"mall_counter_{i + 1:02d}", "Mall", "b_mall_counter",
                      params={"w": 2.0 + (i % 3) * 0.3, "d": 0.85, "h": 0.92, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(woods + paints, i), "top": pick(["steel_brushed", "plastic_white", "wood_light"], i),
                               "belt": "rubber", "glass": "glass_dirty"}))

    # 3. mall_mannequin_01..07
    for i in range(7):
        out.append(mk(f"mall_mannequin_{i + 1:02d}", "Mall", "b_mall_mannequin",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["plastic_white", "plastic_black", "wood_light"], i),
                               "cloth": pick(fabrics + ["leather_black", "leather_brown"], i),
                               "base": "steel_brushed"}))

    # 4. mall_shopfront_01..07
    for i in range(7):
        out.append(mk(f"mall_shopfront_{i + 1:02d}", "Mall", "b_mall_shopfront",
                      params={"w": 4.0, "h": 3.0, "style": i},
                      atlas=ATLAS,
                      palette={"frame": pick(["steel_brushed", "plastic_black", "wood_dark"], i),
                               "sign": pick(paints + fabrics, i),
                               "glass": "glass_dirty", "plinth": pick(woods + ["plastic_black"], i)}))

    # 5. mall_cart_01..07
    for i in range(7):
        out.append(mk(f"mall_cart_{i + 1:02d}", "Mall", "b_mall_cart",
                      params={"style": i},
                      atlas=ATLAS,
                      palette={"metal": pick(["steel_brushed", "rust_heavy", "painted_metal_blue"], i),
                               "handle": pick(fabrics + ["plastic_black"], i),
                               "wheel": "rubber"}))

    # 6. mall_kiosk_01..07
    for i in range(7):
        out.append(mk(f"mall_kiosk_{i + 1:02d}", "Mall", "b_mall_kiosk",
                      params={"w": 2.4, "d": 2.0, "h": 2.3, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(woods + ["plastic_black"], i),
                               "top": pick(["plastic_white", "steel_brushed", "wood_light"], i),
                               "canopy": pick(paints + fabrics, i), "pole": "steel_brushed"}))

    # 7. mall_atm_01..07
    for i in range(7):
        out.append(mk(f"mall_atm_{i + 1:02d}", "Mall", "b_mall_atm",
                      params={"w": 0.82, "d": 0.72, "h": 1.78, "style": i},
                      atlas=ATLAS,
                      palette={"body": pick(["steel_brushed", "plastic_white", "plastic_black"], i),
                               "trim": pick(["painted_metal_blue", "painted_metal_green", "fabric_red"], i),
                               "dark": "plastic_black", "screen": "glass_dirty"}))

    # 8. mall_escalator_01..07
    for i in range(7):
        out.append(mk(f"mall_escalator_{i + 1:02d}", "Mall", "b_mall_escalator",
                      params={"w": 1.4 + (i % 2) * 0.4, "d": 4.2, "h": 2.4, "steps": 12, "style": i},
                      atlas=ATLAS,
                      palette={"truss": pick(["steel_brushed", "plastic_black"], i),
                               "tread": "plastic_black", "rail": "rubber", "balustrade": "glass_dirty"}))

    return out
