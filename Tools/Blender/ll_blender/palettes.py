"""Tag palettes (style grids) per atlas family. Builders take role->tag palettes; catalogue entries pick from these lists."""

MAT = dict(
    wood=["wood_dark", "wood_light", "wood_plank_worn"],
    fabric=["fabric_blue", "fabric_red", "fabric_green"],
    leather=["leather_brown", "leather_black"],
    plastic=["plastic_white", "plastic_black"],
    metal=["steel_brushed", "painted_metal_green", "painted_metal_blue", "rust_heavy"],
    paint=["painted_metal_green", "painted_metal_blue", "plastic_white", "plastic_black"],
    soft=["fabric_blue", "fabric_red", "fabric_green", "leather_brown", "leather_black"],
)
STRUCT = dict(
    wall=["plaster_white", "plaster_dirty", "wallpaper_stripe", "wallpaper_floral", "concrete_raw", "concrete_dark",
          "brick_red", "tile_white"],
    wall_res=["wallpaper_stripe", "wallpaper_floral", "plaster_white", "plaster_dirty"],
    wall_off=["plaster_white", "plaster_dirty", "concrete_raw", "tile_white"],
    wall_ind=["concrete_raw", "concrete_dark", "brick_red", "metal_plate"],
    floor=["parquet_oak", "tile_white", "tile_checker", "carpet_grey", "carpet_red", "concrete_dark", "marble_white",
           "concrete_raw"],
    ceiling=["ceiling_panel", "plaster_white", "plaster_dirty", "concrete_raw"],
    carpet=["carpet_grey", "carpet_red"],
)
CITY = dict(
    facade=["facade_glass_blue", "facade_glass_dark", "facade_concrete", "facade_brick", "facade_stone"],
    car=["car_red", "car_blue", "car_black", "car_white", "car_green"],
)
ZOMBIE_SKIN = ["skin_pale", "skin_grey", "skin_green_sick", "skin_burnt"]
ZOMBIE_CLOTH = ["cloth_suit", "cloth_casual", "cloth_labcoat", "cloth_security"]
SURV_SKIN = ["skin_tan", "skin_light", "skin_dark"]
SURV_HAIR = ["hair_black", "hair_brown", "hair_grey"]
WEAPON_METAL = ["gun_steel", "gun_blued", "gun_black_poly", "chrome", "rust_metal"]
WEAPON_GRIP = ["wood_stock", "wood_dark_stock", "grip_rubber", "tan_polymer", "camo_green", "gun_black_poly"]
PROP_A = dict(can=["can_tomato", "can_beans", "can_tuna"], book=["book_red", "book_blue", "book_green"])
