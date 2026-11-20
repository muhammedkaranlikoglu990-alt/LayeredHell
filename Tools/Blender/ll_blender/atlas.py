"""Texture atlas layout shared by the texture generator, the geometry kit and the Unity importer.

Three texture tiers (all compressed to ASTC 6x6 on Android by the Unity importer):
  * ENV_*   2048x2048  shared tiling atlases for walls/floors/furniture/city      (mode 'tile')
  * HERO_*  1024x1024  weapons, zombies, survivors, key items (close-up assets)    (mode 'cell')
  * PROP_*   512x512   cans, bottles, books, small loot                            (mode 'cell')

Every atlas is a 4x4 grid (16 cells). Cell index i -> column i%4, row (from top) i//4.
UV convention: Unity/Blender, v = 0 at the BOTTOM of the image.

mode 'tile': UV0 holds tiling coordinates (1.0 == `mpt` metres, may exceed [0,1]) and UV1 holds the cell
             origin; the LayeredHell/AtlasTiling shader computes  origin + frac(uv0) * cellSize.
mode 'cell': UV0 is already inside the cell rectangle (inset 3%), UV1 is unused/equal to UV0.
"""

GRID = 4

ATLASES = {
    # ------------------------------------------------------------------ 2K tiling atlases
    "ENV_STRUCT_2K": dict(size=2048, mode="tile", mpt=2.0, tiles=[
        "concrete_raw", "concrete_dark", "brick_red", "plaster_white",
        "plaster_dirty", "wallpaper_stripe", "wallpaper_floral", "tile_white",
        "tile_checker", "parquet_oak", "carpet_grey", "carpet_red",
        "metal_plate", "marble_white", "ceiling_panel", "wall_damaged"]),
    "ENV_MAT_2K": dict(size=2048, mode="tile", mpt=1.0, tiles=[
        "wood_dark", "wood_light", "wood_plank_worn", "fabric_blue",
        "fabric_red", "fabric_green", "leather_brown", "leather_black",
        "plastic_white", "plastic_black", "rubber", "glass_dirty",
        "steel_brushed", "rust_heavy", "painted_metal_green", "painted_metal_blue"]),
    "ENV_CITY_2K": dict(size=2048, mode="tile", mpt=4.0, tiles=[
        "asphalt", "facade_glass_blue", "facade_glass_dark", "facade_concrete",
        "facade_brick", "facade_stone", "paint_yellow", "paint_white",
        "car_red", "car_blue", "car_black", "car_white",
        "car_green", "roof_gravel", "grass_dead", "steel_dark"]),
    # ------------------------------------------------------------------ 1K hero atlases
    "HERO_WEAPONS_1K": dict(size=1024, mode="cell", mpt=0.5, tiles=[
        "gun_steel", "gun_blued", "gun_black_poly", "wood_stock",
        "wood_dark_stock", "brass", "rust_metal", "grip_rubber",
        "camo_green", "tan_polymer", "chrome", "scope_glass",
        "tape_wrap", "paint_red", "bat_wood", "pipe_grey"]),
    "HERO_ZOMBIE_1K": dict(size=1024, mode="cell", mpt=0.8, tiles=[
        "skin_pale", "skin_grey", "skin_green_sick", "skin_burnt",
        "face_walker", "face_runner", "face_spitter", "face_goliath",
        "cloth_suit", "cloth_casual", "cloth_labcoat", "cloth_security",
        "armor_plate", "blood_fresh", "blood_dried", "boil_flesh"]),
    "HERO_SURVIVOR_1K": dict(size=1024, mode="cell", mpt=0.8, tiles=[
        "skin_tan", "skin_light", "skin_dark", "face_aras",
        "face_selin", "face_kemal", "face_elif", "hair_black",
        "hair_brown", "hair_grey", "cloth_jacket_orange", "cloth_jeans",
        "cloth_labcoat_clean", "cloth_overalls", "glove_leather", "boot_black"]),
    "HERO_ITEMS_1K": dict(size=1024, mode="cell", mpt=0.6, tiles=[
        "keycard", "vehicle_armor", "helicopter_body", "radio_olive",
        "medkit_white", "generator_orange", "battery_green", "document_paper",
        "chem_yellow", "circuit_green", "canister_red", "glass_vial",
        "lab_steel", "screen_green", "tool_orange", "bandage"]),
    # ------------------------------------------------------------------ 512 prop atlases
    "PROP_A_512": dict(size=512, mode="cell", mpt=0.25, tiles=[
        "can_tomato", "can_beans", "can_tuna", "bottle_water",
        "bottle_soda", "book_red", "book_blue", "book_green",
        "outlet_white", "box_cardboard", "paper_note", "wrapper_snack",
        "battery_metal", "key_brass", "tape_duct", "coin_gold"]),
    "PROP_B_512": dict(size=512, mode="cell", mpt=0.25, tiles=[
        "pill_bottle", "syringe", "bandage_roll", "bulb_glass",
        "wire_copper", "screw_metal", "scrap_metal", "cloth_rag",
        "lighter", "matchbox", "flashlight_body", "radio_small",
        "photo_frame", "toy_plastic", "mug_ceramic", "cigarette_pack"]),
}

for _name, _a in ATLASES.items():
    assert len(_a["tiles"]) == GRID * GRID, _name
    assert len(set(_a["tiles"])) == GRID * GRID, _name


def material_name(atlas):
    """Blender material name == Unity material asset name."""
    return "M_" + atlas


def tile_index(atlas, tag):
    try:
        return ATLASES[atlas]["tiles"].index(tag)
    except ValueError:
        raise KeyError("tag '%s' not in atlas %s" % (tag, atlas))


def cell_rect(atlas, index):
    """(u0, v0, du, dv) of a cell. v measured from the bottom."""
    col = index % GRID
    row = index // GRID
    d = 1.0 / GRID
    return (col * d, 1.0 - (row + 1) * d, d, d)


def texture_filename(atlas):
    return atlas + ".png"
