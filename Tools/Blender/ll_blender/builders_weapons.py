"""Procedural 3D builders for Weapons, attachments, magazines and ammo (category 'Weapons', 48 entries)."""
import math

from . import palettes as P
from .builders_common import (
    Tags, ball, bx, geo_for, mk, obox, pick, prism, register, tube,
)

BUILDERS = {}


def _ground(g):
    lo, _ = g.bounds()
    if lo[2] < 0.002:
        g.transform(offset=(0.0, 0.0, 0.002 - lo[2]))
    return g


@register(BUILDERS)
def wpn_pistol(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_slide = T("metal", "gun_blued")
    t_frame = T("frame", "gun_black_poly")
    t_grip = T("grip", "grip_rubber")
    t_steel = T("steel", "gun_steel")
    t_accent = T("accent", "paint_red")

    # Grip (angled slightly back in +Y)
    obox(g, (0.0, 0.055, 0.048), (0.028, 0.042, 0.095), t_grip, rot=(-0.22, 0, 0))
    # Magazine baseplate
    obox(g, (0.0, 0.066, 0.004), (0.030, 0.046, 0.010), t_frame)
    # Receiver / Lower frame + rail
    bx(g, -0.015, -0.10, 0.085, 0.015, 0.08, 0.112, t_frame)
    # Trigger guard & trigger
    bx(g, -0.008, -0.02, 0.055, 0.008, 0.03, 0.062, t_frame)
    bx(g, -0.008, -0.02, 0.055, 0.008, -0.012, 0.085, t_frame)
    bx(g, -0.004, 0.00, 0.066, 0.004, 0.010, 0.085, t_steel)
    # Slide
    bx(g, -0.016, -0.11, 0.112, 0.016, 0.082, 0.144, t_slide)
    # Barrel protruding slightly
    tube(g, (0.0, -0.125, 0.127), (0.0, -0.09, 0.127), 0.007, 0.007, t_steel, seg=8)
    # Front and rear iron sights
    bx(g, -0.004, -0.102, 0.144, 0.004, -0.090, 0.151, t_accent)
    bx(g, -0.010, 0.062, 0.144, 0.010, 0.076, 0.151, t_steel)

    if v >= 2:
        # Underbarrel tactical light
        bx(g, -0.013, -0.095, 0.068, 0.013, -0.035, 0.085, t_steel)
    if v == 3:
        # Threaded compensator / suppressor stub + micro red-dot
        tube(g, (0.0, -0.21, 0.127), (0.0, -0.11, 0.127), 0.014, 0.014, t_frame, seg=8)
        bx(g, -0.014, 0.015, 0.144, 0.014, 0.052, 0.168, t_frame)
    return _ground(g)


@register(BUILDERS)
def wpn_revolver(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_metal = T("metal", "chrome")
    t_grip = T("grip", "wood_dark_stock")
    t_steel = T("steel", "gun_steel")
    t_brass = T("brass", "brass")

    # Wood grip
    obox(g, (0.0, 0.085, 0.048), (0.030, 0.044, 0.098), t_grip, rot=(-0.28, 0, 0))
    # Frame & trigger guard
    bx(g, -0.014, -0.035, 0.085, 0.014, 0.085, 0.138, t_metal)
    bx(g, -0.007, -0.005, 0.058, 0.007, 0.045, 0.085, t_metal)
    # 6-shot revolving cylinder
    tube(g, (0.0, -0.030, 0.114), (0.0, 0.032, 0.114), 0.023, 0.023, t_steel, seg=8)
    # Hammer spur
    obox(g, (0.0, 0.086, 0.140), (0.008, 0.022, 0.020), t_steel, rot=(0.4, 0, 0))
    # Barrel + underlug + top rib
    blen = -0.17 - 0.03 * (v - 1)
    tube(g, (0.0, blen, 0.126), (0.0, -0.030, 0.126), 0.010, 0.011, t_metal, seg=8)
    bx(g, -0.008, blen + 0.005, 0.106, 0.008, -0.032, 0.118, t_metal)
    bx(g, -0.006, blen, 0.135, 0.006, -0.030, 0.142, t_metal)
    bx(g, -0.003, blen + 0.005, 0.142, 0.003, blen + 0.022, 0.151, t_brass)
    if v == 3:
        # Scoped hunting revolver
        tube(g, (0.0, -0.08, 0.160), (0.0, 0.05, 0.160), 0.014, 0.014, t_steel, seg=8)
    return _ground(g)


@register(BUILDERS)
def wpn_shotgun(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_metal = T("metal", "gun_blued")
    t_wood = T("grip", "wood_stock")
    t_steel = T("steel", "gun_steel")
    t_shell = T("accent", "paint_red")

    # Buttstock (unless sawed-off variant 3)
    if v != 3:
        obox(g, (0.0, 0.28, 0.068), (0.038, 0.28, 0.085), t_wood, rot=(-0.16, 0, 0))
        bx(g, -0.020, 0.40, 0.02, 0.020, 0.42, 0.11, T("pad", "grip_rubber"))
    else:
        obox(g, (0.0, 0.20, 0.065), (0.036, 0.13, 0.065), t_wood, rot=(-0.32, 0, 0))

    # Receiver
    bx(g, -0.020, -0.04, 0.082, 0.020, 0.16, 0.138, t_metal)
    # Trigger guard
    bx(g, -0.008, 0.06, 0.056, 0.008, 0.12, 0.082, t_steel)
    # Barrel + mag tube underneath
    blen = -0.52 if v != 3 else -0.36
    tube(g, (0.0, blen, 0.124), (0.0, -0.04, 0.124), 0.012, 0.013, t_metal, seg=8)
    tube(g, (0.0, blen + 0.05, 0.098), (0.0, -0.04, 0.098), 0.011, 0.011, t_steel, seg=8)
    # Ribbed pump forend
    tube(g, (0.0, -0.32, 0.098), (0.0, -0.14, 0.098), 0.021, 0.021, t_wood, seg=8)
    if v >= 2:
        # Side saddle shell holder with 4 red shells
        bx(g, 0.020, 0.0, 0.090, 0.026, 0.12, 0.130, t_steel)
        for k in range(4):
            y = 0.015 + k * 0.028
            tube(g, (0.032, y, 0.088), (0.032, y, 0.132), 0.007, 0.007, t_shell, seg=6)
    return _ground(g)


@register(BUILDERS)
def wpn_smg(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("metal", "gun_black_poly")
    t_steel = T("steel", "gun_steel")
    t_grip = T("grip", "grip_rubber")

    # Compact upper & lower receiver
    bx(g, -0.021, -0.16, 0.115, 0.021, 0.12, 0.172, t_body)
    bx(g, -0.018, -0.10, 0.085, 0.018, 0.10, 0.115, t_steel)
    # Pistol grip
    obox(g, (0.0, 0.065, 0.045), (0.028, 0.040, 0.090), t_grip, rot=(-0.20, 0, 0))
    # Forward vertical magazine (slightly curved/angled)
    obox(g, (0.0, -0.075, 0.042), (0.020, 0.032, 0.115), t_steel, rot=(0.12, 0, 0))
    # Barrel + shroud
    tube(g, (0.0, -0.25, 0.142), (0.0, -0.16, 0.142), 0.016, 0.016, t_steel, seg=8)
    tube(g, (0.0, -0.29, 0.142), (0.0, -0.24, 0.142), 0.009, 0.009, t_steel, seg=8)
    # Collapsible wire/folding stock
    for sx in (-0.018, 0.018):
        bx(g, sx - 0.003, 0.12, 0.135, sx + 0.003, 0.28, 0.144, t_steel)
    bx(g, -0.020, 0.275, 0.095, 0.020, 0.295, 0.165, t_body)
    # Top rail sights
    bx(g, -0.010, -0.14, 0.172, 0.010, -0.12, 0.190, t_steel)
    bx(g, -0.012, 0.08, 0.172, 0.012, 0.10, 0.190, t_steel)
    if v >= 2:
        # Foregrip
        bx(g, -0.012, -0.15, 0.050, 0.012, -0.125, 0.115, t_grip)
    if v == 3:
        # Attached suppressor + optic
        tube(g, (0.0, -0.41, 0.142), (0.0, -0.26, 0.142), 0.018, 0.018, t_body, seg=8)
        bx(g, -0.016, -0.03, 0.172, 0.016, 0.03, 0.205, t_body)
    return _ground(g)


@register(BUILDERS)
def wpn_rifle(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_metal = T("metal", "gun_steel")
    t_stock = T("grip", "wood_stock")
    t_poly = T("poly", "gun_black_poly")
    t_mag = T("mag", "rust_metal" if v == 3 else "gun_blued")

    # Receiver
    bx(g, -0.022, -0.08, 0.110, 0.022, 0.14, 0.168, t_metal)
    # Dust cover sloped top
    prism(g, [(-0.018, -0.06), (0.018, -0.06), (0.018, 0.13), (-0.018, 0.13)], 0.168, 0.182, t_metal)
    # Buttstock
    obox(g, (0.0, 0.27, 0.120), (0.036, 0.26, 0.078), t_stock, rot=(-0.14, 0, 0))
    # Pistol grip
    obox(g, (0.0, 0.095, 0.060), (0.026, 0.038, 0.098), t_poly, rot=(-0.24, 0, 0))
    # Curved banana magazine (2 segments)
    obox(g, (0.0, -0.025, 0.065), (0.022, 0.055, 0.090), t_mag, rot=(0.22, 0, 0))
    obox(g, (0.0, -0.048, 0.018), (0.022, 0.055, 0.070), t_mag, rot=(0.45, 0, 0))
    # Handguard (lower + upper gas tube cover)
    bx(g, -0.022, -0.28, 0.118, 0.022, -0.08, 0.152, t_stock)
    bx(g, -0.017, -0.26, 0.152, 0.017, -0.08, 0.174, t_stock)
    # Barrel + gas tube + front sight triangle + muzzle brake
    tube(g, (0.0, -0.48, 0.138), (0.0, -0.28, 0.138), 0.009, 0.010, t_metal, seg=8)
    tube(g, (0.0, -0.34, 0.162), (0.0, -0.26, 0.162), 0.008, 0.008, t_metal, seg=6)
    bx(g, -0.005, -0.45, 0.138, 0.005, -0.42, 0.188, t_metal)
    tube(g, (0.0, -0.52, 0.138), (0.0, -0.48, 0.138), 0.012, 0.012, t_metal, seg=8)

    if v >= 2:
        # Side-mounted tactical rail + reflex sight
        bx(g, -0.016, -0.02, 0.182, 0.016, 0.05, 0.214, t_poly)
    if v == 3:
        # Tape wrap around stock and mag
        bx(g, -0.020, 0.22, 0.085, 0.020, 0.28, 0.155, T("tape", "tape_wrap"))
    return _ground(g)


@register(BUILDERS)
def wpn_sniper(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("grip", "camo_green")
    t_metal = T("metal", "gun_steel")
    t_dark = T("dark", "gun_black_poly")
    t_glass = T("glass", "scope_glass")

    # Chassis & thumbhole/cheek-rest stock
    bx(g, -0.022, -0.28, 0.105, 0.022, 0.16, 0.152, t_body)
    bx(g, -0.022, 0.16, 0.075, 0.022, 0.42, 0.148, t_body)
    bx(g, -0.020, 0.24, 0.148, 0.020, 0.36, 0.172, t_dark)
    # Grip & detachable box magazine
    obox(g, (0.0, 0.12, 0.058), (0.026, 0.040, 0.095), t_dark, rot=(-0.20, 0, 0))
    bx(g, -0.016, -0.05, 0.045, 0.016, 0.04, 0.105, t_metal)
    # Upper receiver & bolt handle
    bx(g, -0.020, -0.10, 0.152, 0.020, 0.14, 0.180, t_metal)
    tube(g, (0.020, 0.08, 0.162), (0.055, 0.08, 0.150), 0.005, 0.008, t_metal, seg=6)
    ball(g, (0.058, 0.08, 0.148), 0.011, t_dark, seg=6, rings=4)
    # Long heavy barrel + fluted muzzle brake
    tube(g, (0.0, -0.68, 0.158), (0.0, -0.10, 0.158), 0.010, 0.014, t_metal, seg=8)
    bx(g, -0.014, -0.73, 0.146, 0.014, -0.68, 0.170, t_dark)
    # High-magnification scope + mount rings + objective lens
    bx(g, -0.012, -0.06, 0.180, 0.012, -0.04, 0.202, t_dark)
    bx(g, -0.012, 0.06, 0.180, 0.012, 0.08, 0.202, t_dark)
    tube(g, (0.0, -0.12, 0.210), (0.0, 0.13, 0.210), 0.015, 0.015, t_dark, seg=8)
    tube(g, (0.0, -0.18, 0.210), (0.0, -0.12, 0.210), 0.024, 0.015, t_dark, seg=10, cap_tag=t_glass)
    tube(g, (0.0, 0.13, 0.210), (0.0, 0.17, 0.210), 0.015, 0.020, t_dark, seg=8, cap_tag=t_glass)
    # Deployed bipod legs at front
    if v >= 2:
        for sx in (-1, 1):
            tube(g, (sx * 0.015, -0.26, 0.105), (sx * 0.085, -0.28, 0.005), 0.007, 0.006, t_dark, seg=6)
    if v == 3:
        # Heavy suppressor on sniper
        tube(g, (0.0, -0.86, 0.158), (0.0, -0.68, 0.158), 0.020, 0.020, t_dark, seg=8)
    return _ground(g)


@register(BUILDERS)
def wpn_pipe(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_pipe = T("metal", "pipe_grey")
    t_rust = T("rust", "rust_metal")
    t_tape = T("grip", "tape_wrap")

    # Main vertical pipe shaft
    tube(g, (0.0, 0.0, 0.02), (0.0, 0.0, 0.68), 0.018, 0.018, t_pipe, seg=8)
    # Tape-wrapped grip near bottom
    tube(g, (0.0, 0.0, 0.05), (0.0, 0.0, 0.24), 0.021, 0.021, t_tape, seg=8)
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.03), 0.025, 0.025, t_rust, seg=8)
    # Heavy T-joint / elbow flange at striking head
    tube(g, (0.0, 0.0, 0.62), (0.0, 0.0, 0.72), 0.027, 0.027, t_rust, seg=8)
    tube(g, (0.0, -0.09, 0.67), (0.0, 0.07, 0.67), 0.025, 0.025, t_rust, seg=8)
    if v >= 2:
        # Valve wheel / gear welded to top
        tube(g, (0.0, -0.11, 0.67), (0.0, -0.09, 0.67), 0.055, 0.055, t_pipe, seg=8)
    if v == 3:
        # Reinforcing bolts/spikes through head
        for z in (0.56, 0.62, 0.71):
            tube(g, (-0.055, 0.0, z), (0.055, 0.0, z), 0.006, 0.002, t_pipe, seg=5)
    return _ground(g)


@register(BUILDERS)
def wpn_bat(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wood = T("wood", "bat_wood")
    t_tape = T("grip", "tape_wrap")
    t_metal = T("metal", "gun_steel")

    # Pommel knob
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.025), 0.026, 0.022, t_wood, seg=8)
    # Taped handle
    tube(g, (0.0, 0.0, 0.025), (0.0, 0.0, 0.25), 0.016, 0.018, t_tape, seg=8)
    # Tapered barrel transition
    tube(g, (0.0, 0.0, 0.25), (0.0, 0.0, 0.48), 0.018, 0.028, t_wood, seg=10)
    # Thick hitting barrel + rounded cap
    tube(g, (0.0, 0.0, 0.48), (0.0, 0.0, 0.80), 0.028, 0.032, t_wood, seg=10)
    ball(g, (0.0, 0.0, 0.80), 0.032, t_wood, seg=8, rings=4, scale=(1, 1, 0.5))

    if v >= 2:
        # Metal reinforcement bands & driven nails
        for z in (0.55, 0.70):
            tube(g, (0.0, 0.0, z), (0.0, 0.0, z + 0.025), 0.034, 0.034, t_metal, seg=8)
    if v == 3:
        for k in range(6):
            ang = k * math.pi / 3.0
            z = 0.54 + (k % 3) * 0.08
            dx, dy = math.cos(ang), math.sin(ang)
            tube(g, (dx * 0.02, dy * 0.02, z), (dx * 0.068, dy * 0.068, z + 0.01), 0.004, 0.001, t_metal, seg=5)
    return _ground(g)


@register(BUILDERS)
def wpn_axe(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_haft = T("grip", "wood_stock")
    t_head = T("metal", "paint_red" if v == 1 else "gun_steel")
    t_edge = T("edge", "chrome")
    t_tape = T("tape", "grip_rubber")

    # Handle shaft (elliptical)
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.76), 0.018, 0.016, t_haft, seg=8, squash=(0.75, 1.25))
    tube(g, (0.0, 0.0, 0.02), (0.0, 0.0, 0.22), 0.019, 0.019, t_tape, seg=8, squash=(0.78, 1.28))

    # Axe head collar around top of shaft
    bx(g, -0.018, -0.032, 0.62, 0.018, 0.032, 0.73, t_head)
    # Forward blade cheek (wedges toward -Y)
    prism(g, [(-0.014, -0.03), (0.014, -0.03), (0.005, -0.14), (-0.005, -0.14)], 0.60, 0.75, t_head)
    # Razor cutting edge strip at front (-Y)
    prism(g, [(-0.005, -0.14), (0.005, -0.14), (0.001, -0.175), (-0.001, -0.175)], 0.58, 0.77, t_edge)
    # Rear pick spike (fireman axe style) or poll
    if v != 2:
        prism(g, [(-0.012, 0.03), (0.012, 0.03), (0.002, 0.13), (-0.002, 0.13)], 0.65, 0.70, t_head)
    else:
        bx(g, -0.016, 0.03, 0.63, 0.016, 0.065, 0.72, t_head)
    return _ground(g)


@register(BUILDERS)
def wpn_machete(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_blade = T("metal", "gun_steel")
    t_edge = T("edge", "chrome")
    t_grip = T("grip", "wood_dark_stock")
    t_guard = T("guard", "brass")

    # Ergonomic handle + hand guard
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.15), 0.017, 0.015, t_grip, seg=8, squash=(0.65, 1.3))
    bx(g, -0.014, -0.038, 0.145, 0.014, 0.034, 0.160, t_guard)

    # Spine + sharpened edge along blade (z = 0.16 .. 0.58)
    bx(g, -0.003, -0.022, 0.16, 0.003, 0.022, 0.52, t_blade)
    prism(g, [(-0.003, -0.022), (0.003, -0.022), (0.0008, -0.044), (-0.0008, -0.044)], 0.16, 0.52, t_edge)
    # Swept clip-point tip
    prism(g, [(-0.0025, -0.038), (0.0025, -0.038), (0.0025, 0.015), (-0.0025, 0.015)], 0.52, 0.60, t_blade)

    if v >= 2:
        # Sawback teeth along spine (+Y edge)
        for k in range(5):
            z0 = 0.24 + k * 0.05
            bx(g, -0.003, 0.020, z0, 0.003, 0.030, z0 + 0.025, t_blade)
    return _ground(g)


@register(BUILDERS)
def wpn_grenade(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "camo_green")
    t_steel = T("metal", "gun_steel")
    t_accent = T("accent", "brass")

    #Segmented pineapple/M67 ovoid body
    ball(g, (0.0, 0.0, 0.055), 0.042, t_body, seg=8, rings=5, scale=(1.0, 1.0, 1.22))
    if v >= 2:
        # Band rings around body
        tube(g, (0.0, 0.0, 0.038), (0.0, 0.0, 0.072), 0.044, 0.044, t_accent, seg=8)
    # Fuse neck
    tube(g, (0.0, 0.0, 0.100), (0.0, 0.0, 0.125), 0.014, 0.012, t_steel, seg=8)
    # Spoon lever curving down side
    bx(g, -0.008, -0.018, 0.120, 0.008, 0.035, 0.130, t_steel)
    obox(g, (0.0, 0.042, 0.082), (0.012, 0.006, 0.082), t_steel, rot=(0.20, 0, 0))
    # Pull pin ring on side
    tube(g, (0.016, 0.0, 0.116), (0.024, 0.0, 0.116), 0.016, 0.016, t_accent, seg=8)
    return _ground(g)


@register(BUILDERS)
def wpn_molotov(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_glass = T("glass", "scope_glass" if v == 2 else "wood_dark_stock")
    t_rag = T("rag", "tape_wrap")
    t_band = T("band", "paint_red")

    # Bottle base & body
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.15), 0.038, 0.038, t_glass, seg=10)
    # Label / tape wrap around middle
    tube(g, (0.0, 0.0, 0.045), (0.0, 0.0, 0.11), 0.0395, 0.0395, t_band, seg=10)
    # Bottle shoulder cone
    tube(g, (0.0, 0.0, 0.15), (0.0, 0.0, 0.20), 0.038, 0.015, t_glass, seg=10)
    # Bottle neck
    tube(g, (0.0, 0.0, 0.20), (0.0, 0.0, 0.26), 0.015, 0.014, t_glass, seg=8)
    # Stuffed cloth rag wick
    tube(g, (0.0, 0.0, 0.25), (0.008, -0.015, 0.30), 0.016, 0.012, t_rag, seg=6)
    obox(g, (0.018, -0.032, 0.26), (0.018, 0.035, 0.075), t_rag, rot=(0.45, 0.25, 0))
    return _ground(g)


@register(BUILDERS)
def wpn_att(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("metal", "gun_black_poly")
    t_steel = T("steel", "gun_steel")
    t_glass = T("glass", "scope_glass")
    t_red = T("accent", "paint_red")

    if v == 1:
        # Cylindrical rifle suppressor
        tube(g, (0.0, -0.11, 0.024), (0.0, 0.11, 0.024), 0.022, 0.022, t_body, seg=10)
        tube(g, (0.0, 0.11, 0.024), (0.0, 0.135, 0.024), 0.016, 0.016, t_steel, seg=8)
    elif v == 2:
        # Reflex red-dot sight
        bx(g, -0.022, -0.045, 0.0, 0.022, 0.045, 0.020, t_steel)
        bx(g, -0.024, -0.040, 0.020, 0.024, 0.030, 0.058, t_body)
        bx(g, -0.018, -0.042, 0.026, 0.018, -0.036, 0.052, t_glass)
    elif v == 3:
        # High-power rifle scope
        bx(g, -0.018, -0.06, 0.0, 0.018, -0.03, 0.025, t_steel)
        bx(g, -0.018, 0.03, 0.0, 0.018, 0.06, 0.025, t_steel)
        tube(g, (0.0, -0.11, 0.042), (0.0, 0.11, 0.042), 0.018, 0.018, t_body, seg=10)
        tube(g, (0.0, -0.16, 0.042), (0.0, -0.11, 0.042), 0.026, 0.018, t_body, seg=10, cap_tag=t_glass)
        tube(g, (0.0, 0.0, 0.060), (0.0, 0.0, 0.072), 0.012, 0.012, t_steel, seg=8)
    else:
        # Tactical laser + flashlight module
        bx(g, -0.024, -0.055, 0.0, 0.024, 0.055, 0.032, t_body)
        tube(g, (-0.012, -0.075, 0.016), (-0.012, -0.055, 0.016), 0.012, 0.010, t_steel, seg=8, cap_tag=t_glass)
        tube(g, (0.012, -0.068, 0.016), (0.012, -0.055, 0.016), 0.006, 0.006, t_red, seg=6)
    return _ground(g)


@register(BUILDERS)
def wpn_mag(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("metal", "gun_blued")
    t_poly = T("poly", "gun_black_poly")
    t_brass = T("brass", "brass")

    if v == 1:
        # Pistol double-stack magazine
        obox(g, (0.0, 0.0, 0.065), (0.024, 0.036, 0.125), t_body, rot=(-0.14, 0, 0))
        bx(g, -0.014, -0.014, 0.0, 0.014, 0.026, 0.010, t_poly)
        tube(g, (0.0, -0.018, 0.128), (0.0, 0.014, 0.128), 0.005, 0.005, t_brass, seg=6)
    elif v == 2:
        # Curved 30-round rifle banana magazine
        obox(g, (0.0, 0.0, 0.055), (0.024, 0.062, 0.105), t_body, rot=(0.18, 0, 0))
        obox(g, (0.0, -0.024, 0.145), (0.024, 0.060, 0.095), t_poly, rot=(0.36, 0, 0))
    elif v == 3:
        # High-capacity drum magazine
        tube(g, (0.0, -0.025, 0.075), (0.0, 0.025, 0.075), 0.072, 0.072, t_body, seg=12)
        bx(g, -0.016, -0.024, 0.135, 0.016, 0.024, 0.195, t_poly)
    else:
        # Heavy sniper box magazine
        bx(g, -0.016, -0.048, 0.0, 0.016, 0.048, 0.095, t_body)
        tube(g, (0.0, -0.038, 0.097), (0.0, 0.036, 0.097), 0.006, 0.006, t_brass, seg=6)
    return _ground(g)


@register(BUILDERS)
def wpn_ammo(params, pal, atlas):
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_box = T("box", "camo_green")
    t_red = T("red", "paint_red")
    t_brass = T("brass", "brass")
    t_steel = T("steel", "gun_steel")
    t_lead = T("lead", "chrome")

    if v == 1:
        # Pistol 9mm carton + 3 standing cartridges
        bx(g, -0.065, -0.040, 0.0, 0.025, 0.040, 0.048, t_box)
        for k in range(3):
            x = 0.042 + k * 0.016
            tube(g, (x, -0.01, 0.0), (x, -0.01, 0.022), 0.005, 0.005, t_brass, seg=6)
            tube(g, (x, -0.01, 0.022), (x, -0.01, 0.032), 0.005, 0.002, t_lead, seg=6)
    elif v == 2:
        # 12-gauge shotgun shell box + shells
        bx(g, -0.07, -0.045, 0.0, 0.03, 0.045, 0.068, t_red)
        for k in range(3):
            x = 0.048 + k * 0.022
            tube(g, (x, 0.0, 0.0), (x, 0.0, 0.014), 0.009, 0.009, t_brass, seg=8)
            tube(g, (x, 0.0, 0.014), (x, 0.0, 0.062), 0.009, 0.009, t_red, seg=8)
    elif v == 3:
        # Military surplus metal ammo can
        bx(g, -0.14, -0.075, 0.0, 0.14, 0.075, 0.17, t_box)
        bx(g, -0.148, -0.082, 0.165, 0.148, 0.082, 0.195, t_box)
        bx(g, -0.154, -0.03, 0.13, -0.14, 0.03, 0.185, t_steel)
        tube(g, (-0.06, 0.0, 0.21), (0.06, 0.0, 0.21), 0.008, 0.008, t_steel, seg=6)
    else:
        # Sniper match-grade cartridge case + long brass rounds
        bx(g, -0.09, -0.055, 0.0, 0.03, 0.055, 0.045, t_steel)
        for k in range(3):
            x = 0.050 + k * 0.018
            tube(g, (x, 0.0, 0.0), (x, 0.0, 0.052), 0.006, 0.005, t_brass, seg=6)
            tube(g, (x, 0.0, 0.052), (x, 0.0, 0.076), 0.0045, 0.001, t_lead, seg=6)
    return _ground(g)


def entries():
    out = []
    wpn_pals = {
        "pistol": [
            dict(metal="gun_blued", frame="gun_black_poly", grip="grip_rubber"),
            dict(metal="chrome", frame="gun_steel", grip="wood_dark_stock"),
            dict(metal="tan_polymer", frame="gun_black_poly", grip="grip_rubber"),
        ],
        "revolver": [
            dict(metal="chrome", grip="wood_dark_stock"),
            dict(metal="gun_blued", grip="wood_stock"),
            dict(metal="gun_steel", grip="grip_rubber"),
        ],
        "shotgun": [
            dict(metal="gun_blued", grip="wood_stock"),
            dict(metal="gun_black_poly", grip="grip_rubber"),
            dict(metal="rust_metal", grip="wood_dark_stock"),
        ],
        "smg": [
            dict(metal="gun_black_poly", steel="gun_steel", grip="grip_rubber"),
            dict(metal="tan_polymer", steel="gun_blued", grip="grip_rubber"),
            dict(metal="camo_green", steel="gun_black_poly", grip="grip_rubber"),
        ],
        "rifle": [
            dict(metal="gun_steel", grip="wood_stock", mag="gun_blued"),
            dict(metal="gun_black_poly", grip="tan_polymer", mag="gun_black_poly"),
            dict(metal="rust_metal", grip="wood_dark_stock", mag="rust_metal"),
        ],
        "sniper": [
            dict(grip="camo_green", metal="gun_steel"),
            dict(grip="tan_polymer", metal="gun_black_poly"),
            dict(grip="wood_dark_stock", metal="gun_blued"),
        ],
        "pipe": [
            dict(metal="pipe_grey", rust="rust_metal", grip="tape_wrap"),
            dict(metal="rust_metal", rust="pipe_grey", grip="grip_rubber"),
            dict(metal="gun_steel", rust="rust_metal", grip="paint_red"),
        ],
        "bat": [
            dict(wood="bat_wood", grip="tape_wrap"),
            dict(wood="wood_dark_stock", grip="grip_rubber"),
            dict(wood="bat_wood", grip="paint_red"),
        ],
        "axe": [
            dict(metal="paint_red", grip="wood_stock"),
            dict(metal="gun_steel", grip="wood_dark_stock"),
            dict(metal="rust_metal", grip="tape_wrap"),
        ],
        "machete": [
            dict(metal="gun_steel", grip="wood_dark_stock"),
            dict(metal="gun_blued", grip="grip_rubber"),
            dict(metal="rust_metal", grip="tape_wrap"),
        ],
        "grenade": [
            dict(body="camo_green", metal="gun_steel"),
            dict(body="gun_black_poly", metal="chrome"),
            dict(body="tan_polymer", metal="gun_steel"),
        ],
        "molotov": [
            dict(glass="wood_dark_stock", rag="tape_wrap", band="paint_red"),
            dict(glass="scope_glass", rag="tape_wrap", band="camo_green"),
            dict(glass="camo_green", rag="paint_red", band="tape_wrap"),
        ],
    }
    for w, pals in wpn_pals.items():
        bname = "wpn_" + w
        for i, pal in enumerate(pals, 1):
            out.append(mk("%s_%02d" % (bname, i), "Weapons", bname,
                          params=dict(variant=i, seed=i), atlas="HERO_WEAPONS_1K", palette=pal))

    for bname, pals in (
        ("wpn_att", [
            dict(metal="gun_black_poly"),
            dict(metal="gun_steel"),
            dict(metal="camo_green"),
            dict(metal="tan_polymer"),
        ]),
        ("wpn_mag", [
            dict(metal="gun_blued"),
            dict(metal="gun_black_poly"),
            dict(metal="rust_metal"),
            dict(metal="tan_polymer"),
        ]),
        ("wpn_ammo", [
            dict(box="camo_green"),
            dict(red="paint_red"),
            dict(box="camo_green", steel="gun_steel"),
            dict(steel="gun_steel", brass="brass"),
        ]),
    ):
        for i, pal in enumerate(pals, 1):
            out.append(mk("%s_%02d" % (bname, i), "Weapons", bname,
                          params=dict(variant=i, seed=i), atlas="HERO_WEAPONS_1K", palette=pal))
    return out
