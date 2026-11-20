"""Procedural 3D builders for Loot items (category 'Loot', 280 entries across 29 item types)."""
import math

from . import palettes as P
from .builders_common import (
    Tags, ball, bx, front_label, geo_for, label_cyl, mk, obox, pick, prism, qpaint, register, tube,
)

BUILDERS = {}


def _ground(g):
    lo, _ = g.bounds()
    if lo[2] < 0.001:
        g.transform(offset=(0.0, 0.0, 0.001 - lo[2]))
    return g


@register(BUILDERS)
def loot_can(params, pal, atlas):
    """Tin food can with wrapped label art (beans, tomato, tuna)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    kind = params.get("kind", "beans")
    t_label = T("label", "can_" + kind)
    t_metal = T("metal", "battery_metal")

    if kind == "tuna":
        r = 0.044 + (v % 3) * 0.003
        h = 0.038 + (v % 2) * 0.006
    else:
        r = 0.038 + (v % 3) * 0.003
        h = 0.105 + (v % 4) * 0.008

    # Bottom rim, label cylinder body, top rim
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.005), r * 1.03, r * 1.03, t_metal, seg=12)
    label_cyl(g, (0.0, 0.0, h * 0.5), r, h - 0.010, t_label, t_metal, seg=12)
    tube(g, (0.0, 0.0, h - 0.005), (0.0, 0.0, h), r * 1.03, r * 1.02, t_metal, seg=12)
    if v % 3 == 0:
        # Ring-pull tab on lid
        bx(g, -0.007, -0.015, h, 0.007, 0.010, h + 0.003, t_metal)
    return _ground(g)


@register(BUILDERS)
def loot_bottle(params, pal, atlas):
    """Water bottle or soda bottle/can."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    kind = params.get("kind", "water")
    t_label = T("label", "bottle_water" if kind == "water" else "bottle_soda")
    t_cap = T("cap", "book_blue" if kind == "water" else "book_red")
    t_metal = T("metal", "battery_metal")

    if kind == "soda" and (v % 2 == 1):
        # Aluminum soda can with bevel top/bottom
        r, h = 0.033, 0.122
        tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.008), r * 0.88, r, t_metal, seg=12)
        label_cyl(g, (0.0, 0.0, 0.061), r, 0.106, t_label, t_metal, seg=12)
        tube(g, (0.0, 0.0, 0.114), (0.0, 0.0, 0.122), r, r * 0.85, t_metal, seg=12)
    else:
        # PET bottle with wrapped label band, shoulder cone & screw cap
        r = 0.036 + (v % 3) * 0.003
        h = 0.19 + (v % 3) * 0.02
        label_cyl(g, (0.0, 0.0, h * 0.36), r, h * 0.72, t_label, t_label, seg=12)
        tube(g, (0.0, 0.0, h * 0.72), (0.0, 0.0, h * 0.88), r, r * 0.38, t_label, seg=10)
        tube(g, (0.0, 0.0, h * 0.88), (0.0, 0.0, h), r * 0.42, r * 0.42, t_cap, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_book(params, pal, atlas):
    """Hardcover book or survivor journal."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    kind = params.get("kind", "book")
    t_cover = T("cover", "book_red")
    t_pages = T("pages", "paper_note")
    t_strap = T("strap", "key_brass")

    w = 0.16 + (v % 3) * 0.015
    d = 0.22 + (v % 4) * 0.015
    h = 0.026 + (v % 3) * 0.008
    hx, hy = w / 2.0, d / 2.0

    # Bottom cover, spine (-X), top cover, page block
    bx(g, -hx, -hy, 0.0, hx, hy, 0.004, t_cover)
    bx(g, -hx, -hy, 0.0, -hx + 0.006, hy, h, t_cover)
    bx(g, -hx + 0.006, -hy + 0.005, 0.004, hx - 0.005, hy - 0.005, h - 0.004, t_pages)
    bx(g, -hx, -hy, h - 0.004, hx, hy, h, t_cover)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.94, d * 0.94, h), t_cover, face="top")

    if kind == "journal":
        # Elastic closure band or bookmark ribbon
        bx(g, hx * 0.52, -hy - 0.002, 0.0, hx * 0.66, hy + 0.002, h + 0.001, t_strap)
    return _ground(g)


@register(BUILDERS)
def loot_outlet(params, pal, atlas):
    """Wall electrical socket faceplate / multi-plug adapter."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_plate = T("plate", "outlet_white")
    t_back = T("back", "battery_metal")

    w = 0.086
    d = 0.125 if (v % 2 == 0) else 0.086
    h = 0.022
    bx(g, -w / 2 + 0.006, -d / 2 + 0.006, 0.0, w / 2 - 0.006, d / 2 - 0.006, 0.014, t_back)
    bx(g, -w / 2, -d / 2, 0.014, w / 2, d / 2, h, t_plate)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.92, d * 0.92, h), t_plate, face="top")
    return _ground(g)


@register(BUILDERS)
def loot_battery(params, pal, atlas):
    """Dry-cell / 9V / lantern battery."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "battery_metal")
    t_term = T("term", "key_brass")

    if v % 3 == 0:
        # 9V rectangular battery with snap terminals
        bx(g, -0.014, -0.009, 0.0, 0.014, 0.009, 0.048, t_body)
        for sx in (-0.006, 0.006):
            tube(g, (sx, 0.0, 0.048), (sx, 0.0, 0.053), 0.003, 0.003, t_term, seg=6)
    else:
        # Cylindrical D/AA cell battery
        r = 0.016 if (v % 2 == 1) else 0.010
        h = 0.058 if (v % 2 == 1) else 0.050
        label_cyl(g, (0.0, 0.0, h * 0.5), r, h, t_body, t_term, seg=10)
        tube(g, (0.0, 0.0, h), (0.0, 0.0, h + 0.003), r * 0.35, r * 0.32, t_term, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_key(params, pal, atlas):
    """Brass door key / keyring."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_key = T("key", "key_brass" if v % 2 == 1 else "coin_gold")
    t_ring = T("ring", "battery_metal")

    # Keyring loop + key bow + shaft + bitting teeth
    tube(g, (0.0, 0.042, 0.003), (0.0, 0.042, 0.006), 0.014, 0.014, t_ring, seg=10)
    tube(g, (0.0, 0.022, 0.002), (0.0, 0.022, 0.007), 0.014, 0.014, t_key, seg=8)
    bx(g, -0.003, -0.036, 0.002, 0.003, 0.012, 0.006, t_key)
    for k in range(2 + (v % 3)):
        y = -0.034 + k * 0.008
        bx(g, 0.003, y, 0.002, 0.010 + (k % 2) * 0.003, y + 0.005, 0.006, t_key)
    return _ground(g)


@register(BUILDERS)
def loot_medkit(params, pal, atlas):
    """First-aid kit box with red cross emblem (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_med = T("med", "medkit_white")
    t_red = T("red", "canister_red")
    t_steel = T("steel", "lab_steel")

    w = 0.24 + (v % 3) * 0.02
    d = 0.18 + (v % 2) * 0.02
    h = 0.085 + (v % 2) * 0.015
    hx, hy = w / 2.0, d / 2.0

    bx(g, -hx, -hy, 0.0, hx, hy, h, t_med if v % 2 == 1 else t_red)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.88, d * 0.88, h), t_med, face="top")
    # Handle & clasps on front (-Y)
    bx(g, -0.045, -hy - 0.018, h * 0.30, 0.045, -hy, h * 0.68, t_steel)
    for sx in (-hx * 0.6, hx * 0.6):
        bx(g, sx - 0.012, -hy - 0.006, h * 0.35, sx + 0.012, -hy + 0.004, h * 0.72, t_steel)
    return _ground(g)


@register(BUILDERS)
def loot_bandage(params, pal, atlas):
    """Rolled sterile gauze bandage (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_roll = T("roll", "bandage_roll")
    t_rag = T("rag", "cloth_rag")

    r = 0.028 + (v % 3) * 0.004
    h = 0.065 + (v % 2) * 0.015
    label_cyl(g, (0.0, 0.0, h * 0.5), r, h, t_roll, t_rag, seg=12)
    # Loose tail strip extending onto floor
    bx(g, 0.0, -r, 0.0, r + 0.035, -r + 0.004, h * 0.96, t_roll)
    return _ground(g)


@register(BUILDERS)
def loot_scrap_metal(params, pal, atlas):
    """Salvaged scrap metal plates, pipes, and gears (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_scrap = T("scrap", "scrap_metal")
    t_screw = T("screw", "screw_metal")

    # Bent metal plate + short pipe offcut + cog/bolt
    obox(g, (-0.02, 0.0, 0.012), (0.14, 0.09, 0.016), t_scrap, rot=(0.08, -0.10, 0.25 * v))
    tube(g, (-0.05, -0.04, 0.025), (0.06, 0.03, 0.032), 0.016, 0.016, t_screw, seg=8)
    tube(g, (0.03, -0.02, 0.018), (0.03, -0.02, 0.034), 0.026, 0.026, t_scrap, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_duct_tape(params, pal, atlas):
    """Silver industrial duct tape roll (atlas PROP_A_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_tape = T("tape", "tape_duct")
    t_core = T("core", "box_cardboard")

    r_out = 0.052 + (v % 3) * 0.004
    h = 0.046 + (v % 2) * 0.006
    label_cyl(g, (0.0, 0.0, h * 0.5), r_out, h, t_tape, t_tape, seg=12)
    # Inner cardboard core disc on top/bottom
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, h + 0.001), r_out * 0.56, r_out * 0.56, t_core, seg=10)
    return _ground(g)


@register(BUILDERS)
def loot_radio(params, pal, atlas):
    """Handheld walkie-talkie / portable emergency radio (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "radio_small")
    t_metal = T("metal", "screw_metal")
    t_dark = T("dark", "flashlight_body")

    w, d, h = 0.064, 0.038, 0.135
    bx(g, -w / 2, -d / 2, 0.0, w / 2, d / 2, h, t_dark)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.90, d, h * 0.90), t_body, face="front")
    # Stubby antenna + volume knob on top
    tube(g, (-w * 0.28, 0.0, h), (-w * 0.28, 0.0, h + 0.085 + (v % 2) * 0.03), 0.006, 0.004, t_dark, seg=6)
    tube(g, (w * 0.22, 0.0, h), (w * 0.22, 0.0, h + 0.016), 0.009, 0.008, t_metal, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_snack(params, pal, atlas):
    """Crinkled foil snack bag / energy bar wrapper (atlas PROP_A_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wrap = T("wrap", "wrapper_snack")
    t_foil = T("foil", "battery_metal")

    w = 0.12 + (v % 3) * 0.02
    d = 0.17 + (v % 2) * 0.02
    h = 0.034
    hx, hy = w / 2.0, d / 2.0

    # Puffed pillow pouch + crimped foil end seals
    obox(g, (0.0, 0.0, h * 0.5), (w, d - 0.02, h), t_wrap)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.92, d - 0.025, h), t_wrap, face="top")
    bx(g, -hx, -hy, h * 0.35, hx, -hy + 0.012, h * 0.55, t_foil)
    bx(g, -hx, hy - 0.012, h * 0.35, hx, hy, h * 0.55, t_foil)
    return _ground(g)


@register(BUILDERS)
def loot_pills(params, pal, atlas):
    """Prescription pill bottle (antibiotics or painkillers, atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_bottle = T("bottle", "pill_bottle")
    t_cap = T("cap", "bandage_roll")

    r = 0.020 + (v % 2) * 0.003
    h = 0.068 + (v % 3) * 0.008
    label_cyl(g, (0.0, 0.0, h * 0.45), r, h * 0.85, t_bottle, t_bottle, seg=10)
    # Child-proof ribbed white cap
    tube(g, (0.0, 0.0, h * 0.85), (0.0, 0.0, h), r * 1.08, r * 1.08, t_cap, seg=10)
    return _ground(g)


@register(BUILDERS)
def loot_wire(params, pal, atlas):
    """Spool / coil of copper electrical wire (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_wire = T("wire", "wire_copper")
    t_flange = T("flange", "screw_metal")

    r = 0.042 + (v % 2) * 0.006
    h = 0.048
    tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.006), r * 1.18, r * 1.18, t_flange, seg=10)
    label_cyl(g, (0.0, 0.0, h * 0.5), r, h - 0.012, t_wire, t_flange, seg=10)
    tube(g, (0.0, 0.0, h - 0.006), (0.0, 0.0, h), r * 1.18, r * 1.18, t_flange, seg=10)
    return _ground(g)


@register(BUILDERS)
def loot_chemicals(params, pal, atlas):
    """Laboratory chemical bottle / hazardous reagent jug (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_chem = T("chem", "chem_yellow")
    t_glass = T("glass", "glass_vial")
    t_cap = T("cap", "canister_red")

    if v % 2 == 1:
        # Rectangular hazard jug with carry handle
        bx(g, -0.055, -0.040, 0.0, 0.055, 0.040, 0.145, t_chem)
        tube(g, (-0.025, 0.0, 0.145), (-0.025, 0.0, 0.175), 0.016, 0.016, t_cap, seg=8)
        bx(g, 0.010, -0.012, 0.145, 0.048, 0.012, 0.168, t_chem)
    else:
        # Glass reagent flask with yellow fluid & stopper
        tube(g, (0.0, 0.0, 0.0), (0.0, 0.0, 0.095), 0.045, 0.045, t_chem, seg=10)
        tube(g, (0.0, 0.0, 0.095), (0.0, 0.0, 0.140), 0.045, 0.016, t_glass, seg=10)
        tube(g, (0.0, 0.0, 0.140), (0.0, 0.0, 0.165), 0.018, 0.018, t_cap, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_circuit(params, pal, atlas):
    """Printed circuit board module with IC chips & capacitors (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_pcb = T("pcb", "circuit_green")
    t_chip = T("chip", "vehicle_armor")
    t_steel = T("steel", "lab_steel")
    t_cap = T("cap", "tool_orange")

    w, d = 0.14, 0.10
    bx(g, -w / 2, -d / 2, 0.0, w / 2, d / 2, 0.005, t_pcb)
    front_label(g, (0.0, 0.0, 0.0025), (w * 0.95, d * 0.95, 0.005), t_pcb, face="top")
    # Mounted IC chips & electrolytic capacitors
    bx(g, -0.035, -0.022, 0.005, 0.012, 0.022, 0.014, t_chip)
    bx(g, -w / 2, -0.028, 0.005, -w / 2 + 0.016, 0.028, 0.022, t_steel)
    for k in range(2 + (v % 2)):
        tube(g, (0.038, -0.025 + k * 0.024, 0.005), (0.038, -0.025 + k * 0.024, 0.024), 0.008, 0.008, t_cap, seg=6)
    return _ground(g)


@register(BUILDERS)
def loot_fuel(params, pal, atlas):
    """Red metal fuel jerrycan (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "canister_red" if v % 2 == 1 else "generator_orange")
    t_steel = T("steel", "lab_steel")

    w, d, h = 0.16, 0.28, 0.36
    hx, hy = w / 2.0, d / 2.0
    bx(g, -hx, -hy, 0.0, hx, hy, h * 0.82, t_body)
    obox(g, (0.0, 0.0, h * 0.90), (w * 0.85, d * 0.88, h * 0.18), t_body, taper=0.72)
    # Top carry handle bars & angled pour spout cap
    bx(g, -0.022, -0.04, h * 0.96, 0.022, 0.10, h + 0.03, t_body)
    tube(g, (0.0, -0.09, h * 0.92), (0.0, -0.13, h + 0.04), 0.024, 0.022, t_steel, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_lockpick(params, pal, atlas):
    """Survivor lockpick & tension wrench roll kit (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_roll = T("roll", "cloth_rag")
    t_steel = T("steel", "screw_metal")
    t_handle = T("handle", "scrap_metal")

    # Open cloth pouch pad
    bx(g, -0.065, -0.055, 0.0, 0.065, 0.055, 0.004, t_roll)
    # 3 slender lockpicks + L-shaped tension wrench
    for k in range(3):
        x = -0.040 + k * 0.024
        bx(g, x - 0.006, 0.0, 0.004, x + 0.006, 0.062, 0.010, t_handle)
        bx(g, x - 0.0015, -0.065, 0.004, x + 0.0015, 0.0, 0.008, t_steel)
    bx(g, 0.032, -0.055, 0.004, 0.038, 0.045, 0.009, t_steel)
    bx(g, 0.032, -0.055, 0.004, 0.056, -0.048, 0.009, t_steel)
    return _ground(g)


@register(BUILDERS)
def loot_keycard(params, pal, atlas):
    """High-security magnetic/chip access keycard (atlas HERO_ITEMS_1K)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    t_card = T("card", "keycard")
    t_clip = T("clip", "lab_steel")

    w, d, h = 0.086, 0.054, 0.004
    bx(g, -w / 2, -d / 2, 0.0, w / 2, d / 2, h, t_card)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.96, d * 0.96, h), t_card, face="top")
    bx(g, -0.012, d / 2 - 0.004, 0.0, 0.012, d / 2 + 0.008, h + 0.002, t_clip)
    return _ground(g)


@register(BUILDERS)
def loot_mug(params, pal, atlas):
    """Ceramic coffee mug with C-handle (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_mug = T("mug", "mug_ceramic")

    r = 0.042
    h = 0.096 + (v % 2) * 0.010
    label_cyl(g, (0.0, 0.0, h * 0.5), r, h, t_mug, t_mug, seg=12)
    # Handle loop on +X side
    bx(g, r - 0.002, -0.006, h * 0.72, r + 0.026, 0.006, h * 0.82, t_mug)
    bx(g, r - 0.002, -0.006, h * 0.18, r + 0.026, 0.006, h * 0.28, t_mug)
    bx(g, r + 0.018, -0.006, h * 0.18, r + 0.028, 0.006, h * 0.82, t_mug)
    return _ground(g)


@register(BUILDERS)
def loot_box(params, pal, atlas):
    """Cardboard supply/parcel box taped shut (atlas PROP_A_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_box = T("box", "box_cardboard")
    t_tape = T("tape", "tape_duct")
    t_note = T("note", "paper_note")

    w = 0.24 + (v % 3) * 0.04
    d = 0.20 + (v % 2) * 0.04
    h = 0.16 + (v % 3) * 0.03
    hx, hy = w / 2.0, d / 2.0

    bx(g, -hx, -hy, 0.0, hx, hy, h, t_box)
    # Packing tape seam across top + shipping label on front
    bx(g, -hx - 0.001, -0.022, h * 0.75, hx + 0.001, 0.022, h + 0.002, t_tape)
    front_label(g, (0.0, 0.0, h * 0.5), (w * 0.55, d, h * 0.50), t_note, face="front")
    return _ground(g)


@register(BUILDERS)
def loot_lighter(params, pal, atlas):
    """Flip-top pocket lighter or matchbox (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_body = T("body", "lighter" if v % 2 == 1 else "matchbox")
    t_metal = T("metal", "screw_metal")

    w, d, h = 0.038, 0.014, 0.058
    bx(g, -w / 2, -d / 2, 0.0, w / 2, d / 2, h * 0.72, t_body)
    front_label(g, (0.0, 0.0, h * 0.36), (w * 0.90, d, h * 0.65), t_body, face="front")
    # Windscreen chimney & flint wheel
    bx(g, -w * 0.42, -d * 0.40, h * 0.72, w * 0.15, d * 0.40, h, t_metal)
    tube(g, (w * 0.26, -d * 0.45, h * 0.78), (w * 0.26, d * 0.45, h * 0.78), 0.007, 0.007, t_metal, seg=8)
    return _ground(g)


@register(BUILDERS)
def loot_photo(params, pal, atlas):
    """Framed family photograph with rear kickstand (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    t_photo = T("photo", "photo_frame")
    t_back = T("back", "screw_metal")

    w, d, h = 0.15, 0.016, 0.19
    obox(g, (0.0, 0.0, h * 0.48), (w, d, h), t_photo, rot=(-0.18, 0, 0))
    front_label(g, (0.0, -0.002, h * 0.48), (w * 0.84, d, h * 0.84), t_photo, face="front")
    obox(g, (0.0, 0.042, h * 0.25), (0.028, 0.008, h * 0.52), t_back, rot=(0.45, 0, 0))
    return _ground(g)


@register(BUILDERS)
def loot_toy(params, pal, atlas):
    """Child's articulated block-robot / plush keepsake toy (atlas PROP_B_512)."""
    g = geo_for(params, atlas)
    T = Tags(atlas, pal)
    v = int(params.get("variant", 1))
    t_toy = T("toy", "toy_plastic")
    t_acc = T("acc", "bulb_glass")

    # Legs, torso with painted front, head, arms
    for sx in (-0.022, 0.022):
        bx(g, sx - 0.012, -0.016, 0.0, sx + 0.012, 0.016, 0.055, t_toy)
    bx(g, -0.036, -0.022, 0.055, 0.036, 0.022, 0.125, t_toy)
    front_label(g, (0.0, 0.0, 0.090), (0.064, 0.044, 0.060), t_toy, face="front")
    for sx in (-0.048, 0.048):
        bx(g, sx - 0.010, -0.012, 0.062, sx + 0.010, 0.012, 0.120, t_toy)
    ball(g, (0.0, 0.0, 0.155), 0.028 + (v % 2) * 0.004, t_toy, seg=8, rings=4)
    for sx in (-0.012, 0.012):
        ball(g, (sx, -0.024, 0.158), 0.006, t_acc, seg=5, rings=3)
    return _ground(g)


def entries():
    out = []
    # (prefix, count, builder, atlas, base_params, palettes_list)
    books = ["book_red", "book_blue", "book_green"]
    specs = [
        ("loot_canned_beans", 12, "loot_can", "PROP_A_512", dict(kind="beans"),
         [dict(label="can_beans")]),
        ("loot_canned_tomato", 12, "loot_can", "PROP_A_512", dict(kind="tomato"),
         [dict(label="can_tomato")]),
        ("loot_canned_tuna", 12, "loot_can", "PROP_A_512", dict(kind="tuna"),
         [dict(label="can_tuna")]),
        ("loot_water_bottle", 12, "loot_bottle", "PROP_A_512", dict(kind="water"),
         [dict(label="bottle_water", cap="book_blue"), dict(label="bottle_water", cap="outlet_white")]),
        ("loot_book", 15, "loot_book", "PROP_A_512", dict(kind="book"),
         [dict(cover=b, pages="paper_note") for b in books]),
        ("loot_outlet", 8, "loot_outlet", "PROP_A_512", {},
         [dict(plate="outlet_white")]),
        ("loot_battery", 10, "loot_battery", "PROP_A_512", {},
         [dict(body="battery_metal", term="key_brass"), dict(body="battery_metal", term="coin_gold")]),
        ("loot_key", 10, "loot_key", "PROP_A_512", {},
         [dict(key="key_brass"), dict(key="coin_gold")]),
        ("loot_medkit", 10, "loot_medkit", "HERO_ITEMS_1K", {},
         [dict(med="medkit_white", red="canister_red"), dict(med="medkit_white", red="tool_orange")]),
        ("loot_bandage", 10, "loot_bandage", "PROP_B_512", {},
         [dict(roll="bandage_roll", rag="cloth_rag")]),
        ("loot_scrap_metal", 12, "loot_scrap_metal", "PROP_B_512", {},
         [dict(scrap="scrap_metal", screw="screw_metal"), dict(scrap="screw_metal", screw="scrap_metal")]),
        ("loot_duct_tape", 10, "loot_duct_tape", "PROP_A_512", {},
         [dict(tape="tape_duct", core="box_cardboard")]),
        ("loot_journal", 12, "loot_book", "PROP_A_512", dict(kind="journal"),
         [dict(cover=b, pages="paper_note", strap="key_brass") for b in books]),
        ("loot_radio", 8, "loot_radio", "PROP_B_512", {},
         [dict(body="radio_small", dark="flashlight_body")]),
        ("loot_soda", 10, "loot_bottle", "PROP_A_512", dict(kind="soda"),
         [dict(label="bottle_soda", cap="book_red"), dict(label="bottle_soda", cap="book_green")]),
        ("loot_snack", 10, "loot_snack", "PROP_A_512", {},
         [dict(wrap="wrapper_snack")]),
        ("loot_antibiotic", 8, "loot_pills", "PROP_B_512", dict(kind="antibiotic"),
         [dict(bottle="pill_bottle", cap="bandage_roll")]),
        ("loot_painkiller", 8, "loot_pills", "PROP_B_512", dict(kind="painkiller"),
         [dict(bottle="pill_bottle", cap="lighter")]),
        ("loot_wire", 8, "loot_wire", "PROP_B_512", {},
         [dict(wire="wire_copper", flange="screw_metal")]),
        ("loot_chemicals", 8, "loot_chemicals", "HERO_ITEMS_1K", {},
         [dict(chem="chem_yellow", cap="canister_red"), dict(chem="battery_green", cap="tool_orange")]),
        ("loot_circuit", 8, "loot_circuit", "HERO_ITEMS_1K", {},
         [dict(pcb="circuit_green", cap="tool_orange")]),
        ("loot_fuel", 8, "loot_fuel", "HERO_ITEMS_1K", {},
         [dict(body="canister_red"), dict(body="generator_orange")]),
        ("loot_lockpick", 8, "loot_lockpick", "PROP_B_512", {},
         [dict(roll="cloth_rag", steel="screw_metal")]),
        ("loot_keycard", 8, "loot_keycard", "HERO_ITEMS_1K", {},
         [dict(card="keycard", clip="lab_steel")]),
        ("loot_mug", 10, "loot_mug", "PROP_B_512", {},
         [dict(mug="mug_ceramic")]),
        ("loot_box", 10, "loot_box", "PROP_A_512", {},
         [dict(box="box_cardboard", tape="tape_duct")]),
        ("loot_lighter", 8, "loot_lighter", "PROP_B_512", {},
         [dict(body="lighter"), dict(body="matchbox")]),
        ("loot_photo", 8, "loot_photo", "PROP_B_512", {},
         [dict(photo="photo_frame")]),
        ("loot_toy", 7, "loot_toy", "PROP_B_512", {},
         [dict(toy="toy_plastic")]),
    ]
    for prefix, count, bname, atlas, bparams, pals in specs:
        for i in range(1, count + 1):
            p = dict(bparams)
            p["variant"] = i
            p["seed"] = i
            out.append(mk("%s_%02d" % (prefix, i), "Loot", bname,
                          params=p, atlas=atlas, palette=pick(pals, i - 1)))
    return out
