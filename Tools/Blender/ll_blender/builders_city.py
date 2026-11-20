"""Procedural 3D builders for City background (36 entries) and Vehicles (16 entries)."""
import math

from . import palettes as P
from .builders_common import (
    Tags, ball, bx, geo_for, mk, obox, pick, prism, register, rnd, tube,
)

BUILDERS = {}


@register(BUILDERS)
def city_building(params, pal, atlas):
    """Low-poly skyline/street building (budget <= 300 tris, height 18..56 m)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    r = rnd(params, v)

    t_facade = T("facade", "facade_glass_blue")
    t_base = T("base", "facade_concrete")
    t_roof = T("roof", "roof_gravel")
    t_steel = T("steel", "steel_dark")

    w = 11.0 + (v % 5) * 2.2
    d = 10.0 + ((v * 3) % 4) * 2.0
    h = 18.0 + (v % 8) * 4.5
    hx, hy = w / 2.0, d / 2.0

    # Podium ground floor base
    bx(g, -hx - 0.3, -hy - 0.3, 0.0, hx + 0.3, hy + 0.3, 3.8, t_base)
    # Main tower shaft
    bx(g, -hx, -hy, 3.8, hx, hy, h, t_facade)
    # Roof parapet rim + gravel roof deck
    bx(g, -hx - 0.2, -hy - 0.2, h, hx + 0.2, hy + 0.2, h + 0.7, t_base)
    bx(g, -hx + 0.2, -hy + 0.2, h + 0.2, hx - 0.2, hy - 0.2, h + 0.75, t_roof)

    # Stepped upper tier or penthouse mechanical room
    if v % 3 == 0:
        h2 = h + 7.5
        bx(g, -hx * 0.65, -hy * 0.65, h + 0.7, hx * 0.65, hy * 0.65, h2, t_facade)
        bx(g, -hx * 0.60, -hy * 0.60, h2, hx * 0.60, hy * 0.60, h2 + 0.5, t_roof)
        top_z = h2 + 0.5
    else:
        bx(g, -hx * 0.45, -hy * 0.40, h + 0.7, hx * 0.35, hy * 0.45, h + 3.4, t_base)
        top_z = h + 3.4

    # Rooftop HVAC units / communication spire / water tower
    if v % 2 == 0:
        tube(g, (0.0, 0.0, top_z), (0.0, 0.0, top_z + 5.5), 0.18, 0.05, t_steel, seg=6)
    else:
        bx(g, hx * 0.15, -hy * 0.35, h + 0.75, hx * 0.65, hy * 0.15, h + 2.2, t_steel)
    return g


@register(BUILDERS)
def city_lamp(params, pal, atlas):
    """Urban streetlight / traffic light pole (budget <= 300 tris)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_steel = T("steel", "steel_dark")
    t_base = T("base", "facade_concrete")
    t_light = T("light", "paint_white")
    t_accent = T("accent", "paint_yellow")

    h = 5.4 + (v % 3) * 0.8
    # Concrete & cast iron pedestal base
    bx(g, -0.22, -0.22, 0.0, 0.22, 0.22, 0.45, t_base)
    tube(g, (0.0, 0.0, 0.45), (0.0, 0.0, h), 0.09, 0.055, t_steel, seg=6)

    if v in (1, 4, 7):
        # Single outreach cobra-head arm toward -Y
        tube(g, (0.0, 0.0, h - 0.15), (0.0, -1.45, h + 0.35), 0.045, 0.032, t_steel, seg=6)
        bx(g, -0.16, -1.75, h + 0.26, 0.16, -1.25, h + 0.42, t_steel)
        bx(g, -0.13, -1.70, h + 0.22, 0.13, -1.30, h + 0.27, t_light)
    elif v in (2, 5, 8):
        # Twin highway luminaire arms (-X and +X)
        for sx in (-1, 1):
            tube(g, (0.0, 0.0, h - 0.15), (sx * 1.4, 0.0, h + 0.35), 0.045, 0.032, t_steel, seg=6)
            bx(g, sx * 1.4 - 0.22, -0.15, h + 0.24, sx * 1.4 + 0.22, 0.15, h + 0.40, t_light)
    elif v == 3:
        # Pedestrian boulevard post with globe lantern top
        bx(g, -0.18, -0.18, h - 0.10, 0.18, 0.18, h + 0.05, t_steel)
        ball(g, (0.0, 0.0, h + 0.28), 0.24, t_light, seg=8, rings=4)
    else:
        # Traffic signal cantilever mast
        tube(g, (0.0, 0.0, h - 0.3), (-2.4, 0.0, h - 0.2), 0.055, 0.042, t_steel, seg=6)
        bx(g, -2.1, -0.16, h - 0.85, -1.75, 0.16, h + 0.05, t_accent)
        bx(g, -1.1, -0.16, h - 0.85, -0.75, 0.16, h + 0.05, t_accent)
    return g


@register(BUILDERS)
def city_tree(params, pal, atlas):
    """Urban sidewalk/park tree in planter curb (budget <= 300 tris)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    r = rnd(params, v)

    t_curb = T("curb", "facade_concrete")
    t_trunk = T("trunk", "roof_gravel")
    t_leaf = T("leaf", "grass_dead" if (v % 2 == 1) else "car_green")

    # Sidewalk tree pit curb & soil bed
    bx(g, -0.85, -0.85, 0.0, 0.85, 0.85, 0.22, t_curb)
    bx(g, -0.68, -0.68, 0.22, 0.68, 0.68, 0.26, t_trunk)

    # Trunk
    h_tr = 2.4 + (v % 3) * 0.45
    lean_x = (v % 3 - 1) * 0.14
    tube(g, (0.0, 0.0, 0.22), (lean_x, 0.0, h_tr), 0.22, 0.13, t_trunk, seg=6)

    # Primary branches & low-poly foliage clusters (or bare winter limbs on v==4,8)
    branches = [
        ((lean_x, 0.0, h_tr - 0.2), (lean_x - 1.1, -0.7, h_tr + 1.4), 1.15),
        ((lean_x, 0.0, h_tr - 0.1), (lean_x + 1.0, -0.5, h_tr + 1.5), 1.10),
        ((lean_x, 0.0, h_tr), (lean_x + 0.2, 1.0, h_tr + 1.6), 1.20),
        ((lean_x, 0.0, h_tr), (lean_x, 0.0, h_tr + 2.1), 1.25),
    ]
    for p0, p1, rad in branches:
        tube(g, p0, p1, 0.10, 0.04, t_trunk, seg=5)
        if v not in (4, 8):
            ball(g, p1, rad, t_leaf, seg=6, rings=4, scale=(1.0, 1.0, 0.72))
        else:
            # Bare secondary twigs
            tube(g, p1, (p1[0] * 1.25, p1[1] * 1.25, p1[2] + 0.65), 0.035, 0.012, t_trunk, seg=4)
    return g


@register(BUILDERS)
def veh_car(params, pal, atlas):
    """Passenger car / SUV / van / police cruiser facing -Y (budget <= 1200 tris)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))

    t_body = T("body", "car_red")
    t_glass = T("glass", "facade_glass_dark")
    t_tire = T("tire", "asphalt")
    t_rim = T("rim", "facade_concrete")
    t_steel = T("steel", "steel_dark")
    t_head = T("headlight", "paint_white")
    t_tail = T("taillight", "car_red")
    t_warn = T("warn", "paint_yellow")

    is_suv = v in (4, 5, 9)
    is_van = v in (6, 10)
    is_pickup = v in (7, 11)
    is_police = v in (8, 12)

    w = 1.86 if not is_van else 1.96
    L = 4.40 if not is_van else 4.75
    hx, hy = w / 2.0, L / 2.0
    r_wheel = 0.34

    # 4 Wheels (axis along X)
    for sx in (-1, 1):
        x0 = sx * (hx - 0.18)
        x1 = sx * (hx + 0.03)
        for wy in (-1.34, 1.32):
            tube(g, (x0, wy, r_wheel), (x1, wy, r_wheel), r_wheel, r_wheel, t_tire, seg=10, cap_tag=t_rim)

    # Lower chassis & bumpers
    bx(g, -hx, -hy, 0.22, hx, hy, 0.88, t_body)
    bx(g, -hx - 0.03, -hy - 0.06, 0.20, hx + 0.03, -hy + 0.18, 0.48, t_steel)
    bx(g, -hx - 0.03, hy - 0.18, 0.22, hx + 0.03, hy + 0.06, 0.50, t_steel)

    # Front grille + headlights (-Y) + rear taillights (+Y)
    bx(g, -0.48, -hy - 0.02, 0.48, 0.48, -hy + 0.04, 0.78, t_steel)
    for sx in (-1, 1):
        bx(g, sx * 0.68 - 0.16, -hy - 0.02, 0.58, sx * 0.68 + 0.16, -hy + 0.04, 0.78, t_head)
        bx(g, sx * 0.68 - 0.18, hy - 0.04, 0.58, sx * 0.68 + 0.18, hy + 0.02, 0.80, t_tail)

    # Upper greenhouse cabin
    if is_van:
        cab_y0, cab_y1, cab_z1 = -1.55, hy - 0.12, 1.88
    elif is_suv:
        cab_y0, cab_y1, cab_z1 = -0.85, hy - 0.18, 1.66
    elif is_pickup:
        cab_y0, cab_y1, cab_z1 = -0.85, 0.45, 1.64
        # Open truck bed walls at rear
        bx(g, -hx, 0.48, 0.88, -hx + 0.12, hy, 1.12, t_body)
        bx(g, hx - 0.12, 0.48, 0.88, hx, hy, 1.12, t_body)
        bx(g, -hx, hy - 0.12, 0.88, hx, hy, 1.12, t_body)
    else:
        cab_y0, cab_y1, cab_z1 = -0.82, 1.18, 1.46

    bx(g, -hx + 0.12, cab_y0, 0.88, hx - 0.12, cab_y1, cab_z1, t_body)
    # Windshield (-Y), rear window (+Y), and side window bands
    bx(g, -hx + 0.20, cab_y0 - 0.02, 0.94, hx - 0.20, cab_y0 + 0.12, cab_z1 - 0.07, t_glass)
    bx(g, -hx + 0.22, cab_y1 - 0.12, 0.94, hx - 0.22, cab_y1 + 0.02, cab_z1 - 0.07, t_glass)
    bx(g, -hx + 0.10, cab_y0 + 0.18, 0.94, hx - 0.10, cab_y1 - 0.18, cab_z1 - 0.08, t_glass)

    # Side mirrors
    for sx in (-1, 1):
        bx(g, sx * (hx - 0.02), cab_y0 + 0.15, 0.92, sx * (hx + 0.12), cab_y0 + 0.25, 1.04, t_body)

    if is_police:
        # Emergency roof lightbar + push bumper
        bx(g, -0.52, -0.18, cab_z1, 0.52, 0.12, cab_z1 + 0.12, t_head)
        bx(g, -0.54, -0.19, cab_z1 + 0.02, -0.12, 0.13, cab_z1 + 0.13, t_tail)
        bx(g, 0.12, -0.19, cab_z1 + 0.02, 0.54, 0.13, cab_z1 + 0.13, T("blue", "car_blue"))
        bx(g, -0.45, -hy - 0.14, 0.28, 0.45, -hy - 0.04, 0.84, t_steel)
    elif v == 3:
        # Taxi roof sign
        bx(g, -0.28, -0.14, cab_z1, 0.28, 0.14, cab_z1 + 0.16, t_warn)
    return g


@register(BUILDERS)
def veh_armored_response(params, pal, atlas):
    """6x6 tactical armored response APC facing -Y (atlas HERO_ITEMS_1K, budget <= 1200)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))

    t_armor = T("armor", "vehicle_armor")
    t_steel = T("steel", "lab_steel")
    t_glass = T("glass", "glass_vial")
    t_orange = T("accent", "tool_orange")
    t_red = T("red", "canister_red")
    t_olive = T("olive", "radio_olive")

    w, L = 2.40, 5.80
    hx, hy = w / 2.0, L / 2.0
    rw = 0.48

    # 6 Heavy off-road wheels (3 axles)
    for sx in (-1, 1):
        x0 = sx * (hx - 0.26)
        x1 = sx * (hx + 0.04)
        for wy in (-1.85, 0.0, 1.85):
            tube(g, (x0, wy, rw), (x1, wy, rw), rw, rw, t_armor, seg=10, cap_tag=t_steel)

    # Lower armored hull tub + upper sloped troop hull
    bx(g, -hx, -hy + 0.20, 0.38, hx, hy, 1.32, t_armor)
    obox(g, (0.0, 0.10, 1.72), (w - 0.18, L - 0.55, 0.82), t_armor, taper=0.86)

    # Heavy V-shaped bullbar ram at front (-Y)
    bx(g, -hx + 0.10, -hy - 0.18, 0.42, hx - 0.10, -hy + 0.22, 1.24, t_steel)
    for sx in (-0.75, 0.75):
        bx(g, sx - 0.16, -hy - 0.20, 0.96, sx + 0.16, -hy + 0.10, 1.16, t_orange)

    # Armored ballistic viewport slits & side firing ports
    for sx in (-0.52, 0.52):
        bx(g, sx - 0.34, -hy + 0.30, 1.50, sx + 0.34, -hy + 0.46, 1.84, t_glass)
        bx(g, sx - 0.36, -hy + 0.28, 1.84, sx + 0.36, -hy + 0.48, 1.92, t_steel)

    # Roof gun turret cupola & barrel
    tube(g, (0.0, -0.35, 2.13), (0.0, -0.35, 2.48), 0.52, 0.44, t_olive if v == 2 else t_armor, seg=8)
    bx(g, -0.18, -0.75, 2.26, 0.18, -0.25, 2.46, t_steel)
    tube(g, (0.0, -1.65, 2.36), (0.0, -0.75, 2.36), 0.045, 0.055, t_steel, seg=6)

    # External fuel jerrycans at rear (+Y) & whip antenna
    for sx in (-0.65, 0.65):
        bx(g, sx - 0.20, hy - 0.05, 1.05, sx + 0.20, hy + 0.14, 1.55, t_red)
    tube(g, (hx - 0.22, hy - 0.35, 2.10), (hx - 0.22, hy - 0.35, 3.35), 0.02, 0.006, t_steel, seg=5)
    return g


@register(BUILDERS)
def veh_helicopter(params, pal, atlas):
    """Rescue/evacuation helicopter facing -Y (atlas HERO_ITEMS_1K, budget <= 1200)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))

    t_body = T("body", "helicopter_body")
    t_armor = T("armor", "vehicle_armor")
    t_steel = T("steel", "lab_steel")
    t_glass = T("glass", "glass_vial")
    t_accent = T("accent", "tool_orange" if v == 1 else "canister_red")

    # Landing skids (z >= 0.0) & struts
    for sx in (-1, 1):
        x = sx * 1.05
        tube(g, (x, -1.95, 0.06), (x, 1.65, 0.06), 0.055, 0.055, t_steel, seg=6)
        tube(g, (x, -1.95, 0.06), (x, -2.20, 0.24), 0.05, 0.04, t_steel, seg=6)
        for sy in (-1.05, 0.85):
            tube(g, (x, sy, 0.06), (sx * 0.72, sy, 0.62), 0.045, 0.045, t_steel, seg=6)

    # Main fuselage cabin + rescue stripe
    bx(g, -0.88, -1.65, 0.56, 0.88, 1.55, 2.05, t_body)
    bx(g, -0.90, -1.20, 1.10, 0.90, 1.40, 1.38, t_accent)

    # Tapered nose & glazed cockpit canopy (-Y)
    obox(g, (0.0, -2.20, 1.12), (1.56, 1.15, 1.05), t_body, taper=0.72)
    obox(g, (0.0, -2.02, 1.48), (1.42, 1.10, 0.72), t_glass, taper=0.68)

    # Top turbine engine cowling + exhausts
    obox(g, (0.0, 0.15, 2.30), (1.15, 2.15, 0.52), t_armor, taper=0.75)
    for sx in (-0.32, 0.32):
        tube(g, (sx, 1.20, 2.32), (sx, 1.55, 2.32), 0.14, 0.16, t_steel, seg=8)

    # Main rotor mast + 4 rotor blades
    tube(g, (0.0, -0.10, 2.55), (0.0, -0.10, 2.95), 0.12, 0.10, t_steel, seg=8)
    bx(g, -4.80, -0.24, 2.88, 4.80, 0.04, 2.93, t_armor)
    bx(g, -0.14, -4.90, 2.88, 0.14, 4.70, 2.93, t_armor)

    # Tail boom extending toward +Y + vertical fin + tail rotor
    tube(g, (0.0, 1.50, 1.62), (0.0, 5.20, 1.86), 0.42, 0.16, t_body, seg=8)
    bx(g, -0.95, 3.80, 1.72, 0.95, 4.15, 1.80, t_accent)
    prism(g, [(-0.06, 4.85), (0.06, 4.85), (0.04, 5.55), (-0.04, 5.55)], 1.75, 2.95, t_accent)
    # Tail rotor hub & blades on +X side of tail fin
    tube(g, (0.05, 5.25, 2.65), (0.22, 5.25, 2.65), 0.07, 0.07, t_steel, seg=6)
    bx(g, 0.20, 4.55, 2.58, 0.24, 5.95, 2.72, t_armor)
    return g


def entries():
    out = []
    # City buildings (20)
    bases = ["facade_concrete", "facade_brick", "facade_stone"]
    for i in range(1, 21):
        pal = dict(
            facade=pick(P.CITY["facade"], i - 1),
            base=pick(bases, i - 1),
            roof="roof_gravel",
            steel="steel_dark",
        )
        out.append(mk("city_building_%02d" % i, "City", "city_building",
                      params=dict(variant=i, seed=i), atlas="ENV_CITY_2K", palette=pal))

    # City lamps (8)
    for i in range(1, 9):
        pal = dict(
            steel="steel_dark",
            base=pick(bases, i - 1),
            light="paint_white" if (i % 2 == 1) else "paint_yellow",
            accent="paint_yellow",
        )
        out.append(mk("city_lamp_%02d" % i, "City", "city_lamp",
                      params=dict(variant=i, seed=30 + i), atlas="ENV_CITY_2K", palette=pal))

    # City trees (8)
    for i in range(1, 9):
        pal = dict(
            curb=pick(bases, i - 1),
            trunk="roof_gravel",
            leaf="grass_dead" if (i % 2 == 1) else "car_green",
        )
        out.append(mk("city_tree_%02d" % i, "City", "city_tree",
                      params=dict(variant=i, seed=60 + i), atlas="ENV_CITY_2K", palette=pal))

    # Vehicles: Passenger cars (12)
    for i in range(1, 13):
        cbody = "car_white" if i in (8, 12) else ("paint_yellow" if i == 3 else pick(P.CITY["car"], i - 1))
        pal = dict(body=cbody, glass="facade_glass_dark", tire="asphalt", rim="facade_concrete")
        out.append(mk("veh_car_%02d" % i, "Vehicles", "veh_car",
                      params=dict(variant=i, seed=90 + i), atlas="ENV_CITY_2K", palette=pal))

    # Vehicles: Armored response APC (2)
    out.append(mk("veh_armored_response_01", "Vehicles", "veh_armored_response",
                  params=dict(variant=1, seed=201), atlas="HERO_ITEMS_1K",
                  palette=dict(armor="vehicle_armor", steel="lab_steel")))
    out.append(mk("veh_armored_response_02", "Vehicles", "veh_armored_response",
                  params=dict(variant=2, seed=202), atlas="HERO_ITEMS_1K",
                  palette=dict(armor="radio_olive", steel="vehicle_armor")))

    # Vehicles: Helicopter (2)
    out.append(mk("veh_helicopter_01", "Vehicles", "veh_helicopter",
                  params=dict(variant=1, seed=211), atlas="HERO_ITEMS_1K",
                  palette=dict(body="helicopter_body", armor="vehicle_armor", accent="tool_orange")))
    out.append(mk("veh_helicopter_02", "Vehicles", "veh_helicopter",
                  params=dict(variant=2, seed=212), atlas="HERO_ITEMS_1K",
                  palette=dict(body="vehicle_armor", armor="lab_steel", accent="canister_red")))
    return out
