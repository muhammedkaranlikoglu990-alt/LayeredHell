#!/usr/bin/env python3
"""Procedural PNG texture atlas generator for '45: Layered Hell' (pure Python stdlib).

Generates all 9 atlases defined in `Tools/Blender/ll_blender/atlas.py` into
`Assets/Art/Textures/<ATLAS>.png` plus `Assets/Art/Textures/atlas_manifest.json`
following `Tools/Textures/CELL_CONVENTIONS.md`.
"""
import json
import math
import multiprocessing
import os
import random
import struct
import sys
import time
import zlib

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
sys.path.insert(0, os.path.join(ROOT, "Tools", "Blender"))

from ll_blender import atlas as A  # noqa: E402


def _clamp(x):
    return 0 if x < 0 else (255 if x > 255 else int(x))


def _rgb(r, g, b):
    return bytes((_clamp(r), _clamp(g), _clamp(b)))


def _lerp_c(c0, c1, t):
    return (
        _clamp(c0[0] + (c1[0] - c0[0]) * t),
        _clamp(c0[1] + (c1[1] - c0[1]) * t),
        _clamp(c0[2] + (c1[2] - c0[2]) * t),
    )


# --------------------------------------------------------------------------- Fast 2D canvas per cell
class CellCanvas:
    """Fast bytearray RGB canvas of size `cs x cs` with UV-space drawing helpers (v=0 bottom, v=1 top)."""

    def __init__(self, cs, base_rgb=(128, 128, 128)):
        self.cs = cs
        self.buf = bytearray(_rgb(*base_rgb) * (cs * cs))

    def fill_base_noise(self, base_rgb, amp=14, freq=8, mode="isotropic", seed=1, c2=None):
        """Generate a seamless 64x64 procedural noise/pattern grid and upscale into cs x cs."""
        cs = self.cs
        N = 64
        rng = random.Random(seed * 9973 + 17)
        grid = [rng.randint(-128, 127) for _ in range(N * N)]

        # Smooth 3x3 wrap-around box filter on 64x64 grid
        sm = [0] * (N * N)
        mask = N - 1
        for y in range(N):
            ym = ((y - 1) & mask) * N
            y0 = y * N
            yp = ((y + 1) & mask) * N
            for x in range(N):
                xm = (x - 1) & mask
                xp = (x + 1) & mask
                sm[y0 + x] = (
                    grid[ym + xm] + grid[ym + x] * 2 + grid[ym + xp]
                    + grid[y0 + xm] * 2 + grid[y0 + x] * 4 + grid[y0 + xp] * 2
                    + grid[yp + xm] + grid[yp + x] * 2 + grid[yp + xp]
                ) >> 4

        br, bg, bb = base_rgb
        c2r, c2g, c2b = c2 if c2 else base_rgb
        tile_row = [b""] * N
        for y in range(N):
            row_ba = bytearray(N * 3)
            y0 = y * N
            v = y / float(N)
            for x in range(N):
                u = x / float(N)
                n1 = sm[y0 + x]
                n2 = grid[((y * 3) & mask) * N + ((x * 3) & mask)] >> 1
                val = (n1 + n2) / 128.0
                if mode == "wood":
                    ring = math.sin((u * freq + val * 0.35) * 2.0 * math.pi)
                    d = int((ring * 0.65 + val * 0.35) * amp)
                    r, g, b = br + d, bg + int(d * 0.8), bb + int(d * 0.6)
                elif mode == "brushed":
                    streak = sm[y0 + ((x // 4) & mask)] / 128.0
                    d = int((streak * 0.75 + val * 0.25) * amp)
                    r, g, b = br + d, bg + d, bb + d
                elif mode == "marble":
                    vein = abs(math.sin((u * 2.0 + v * 1.0 + val * 0.6) * math.pi * 2.0))
                    t = vein ** 0.35
                    r = int(c2r + (br - c2r) * t)
                    g = int(c2g + (bg - c2g) * t)
                    b = int(c2b + (bb - c2b) * t)
                elif mode == "camo":
                    blob = math.sin(u * 4 * math.pi + val * 2.2) + math.cos(v * 4 * math.pi - val * 2.2)
                    if blob > 0.45:
                        r, g, b = c2r, c2g, c2b
                    elif blob < -0.45:
                        r, g, b = int(br * 0.65), int(bg * 0.65), int(bb * 0.65)
                    else:
                        d = int(val * (amp * 0.5))
                        r, g, b = br + d, bg + d, bb + d
                elif mode == "rust":
                    t = max(0.0, min(1.0, 0.5 + val * 0.85))
                    r = int(br + (c2r - br) * t)
                    g = int(bg + (c2g - bg) * t)
                    b = int(bb + (c2b - bb) * t)
                elif mode == "fabric":
                    weave = ((x ^ y) & 1) * 6 - 3
                    d = int(val * (amp * 0.6)) + weave
                    r, g, b = br + d, bg + d, bb + d
                else:
                    d = int(val * amp)
                    r, g, b = br + d, bg + d, bb + d
                i3 = x * 3
                row_ba[i3] = _clamp(r)
                row_ba[i3 + 1] = _clamp(g)
                row_ba[i3 + 2] = _clamp(b)
            tile_row[y] = bytes(row_ba)

        # Tile/scale the N x N pattern cleanly into cs x cs
        buf = self.buf
        stride = cs * 3
        if cs >= N and cs % N == 0:
            rep = cs // N
            # Repeat horizontally & vertically for seamless tiling
            for y in range(cs):
                src = tile_row[(y * 2 // rep) & mask]
                # expand each pixel horizontally
                if rep == 2:
                    expanded = tile_row[y & mask] + tile_row[(y + 17) & mask]
                elif rep == 4:
                    r0 = tile_row[y & mask]
                    r1 = tile_row[(y + 11) & mask]
                    expanded = r0 + r1 + r0 + r1
                else:
                    expanded = src * rep
                buf[y * stride:(y + 1) * stride] = expanded[:stride]

    def rect_uv(self, u0, v0, u1, v1, rgb):
        """Fill UV rectangle [u0..u1, v0..v1] (v=0 bottom, v=1 top)."""
        cs = self.cs
        x0 = max(0, min(cs, int(round(min(u0, u1) * cs))))
        x1 = max(0, min(cs, int(round(max(u0, u1) * cs))))
        # v=1 -> y=0, v=0 -> y=cs
        y0 = max(0, min(cs, int(round((1.0 - max(v0, v1)) * cs))))
        y1 = max(0, min(cs, int(round((1.0 - min(v0, v1)) * cs))))
        if x1 <= x0 or y1 <= y0:
            return
        span = _rgb(*rgb) * (x1 - x0)
        stride = cs * 3
        s0, s1 = x0 * 3, x1 * 3
        buf = self.buf
        for y in range(y0, y1):
            base = y * stride
            buf[base + s0:base + s1] = span

    def frame_uv(self, u0, v0, u1, v1, thick, rgb):
        self.rect_uv(u0, v0, u1, v0 + thick, rgb)
        self.rect_uv(u0, v1 - thick, u1, v1, rgb)
        self.rect_uv(u0, v0, u0 + thick, v1, rgb)
        self.rect_uv(u1 - thick, v0, u1, v1, rgb)

    def disc_uv(self, uc, vc, ru, rv, rgb):
        """Fill ellipse centred at (uc, vc) with radii (ru, rv)."""
        cs = self.cs
        yc = (1.0 - vc) * cs
        xc = uc * cs
        rx = max(1.0, ru * cs)
        ry = max(1.0, rv * cs)
        y0 = max(0, int(yc - ry))
        y1 = min(cs, int(yc + ry) + 1)
        px = _rgb(*rgb)
        stride = cs * 3
        buf = self.buf
        for y in range(y0, y1):
            dy = (y + 0.5 - yc) / ry
            if abs(dy) > 1.0:
                continue
            dx = math.sqrt(max(0.0, 1.0 - dy * dy)) * rx
            x0 = max(0, int(xc - dx))
            x1 = min(cs, int(xc + dx))
            if x1 > x0:
                base = y * stride
                buf[base + x0 * 3:base + x1 * 3] = px * (x1 - x0)

    def text_lines_uv(self, u0, v0, u1, v1, n_lines, rgb, seed=1):
        """Draw stylized horizontal text/barcode blocks inside a UV box."""
        rng = random.Random(seed)
        dv = (v1 - v0) / max(1, n_lines)
        h = dv * 0.48
        for k in range(n_lines):
            vb = v1 - (k + 0.8) * dv
            cur_u = u0
            while cur_u < u1 - 0.04:
                w = min(u1 - cur_u, rng.uniform(0.06, 0.22))
                self.rect_uv(cur_u, vb, cur_u + w, vb + h, rgb)
                cur_u += w + rng.uniform(0.02, 0.05)


# --------------------------------------------------------------------------- Special painted cell artists
def _paint_face(c, tag, seed):
    """Paint character / zombie front portrait per CELL_CONVENTIONS.md."""
    profiles = {
        # zombies
        "face_walker": dict(skin=(158, 162, 148), hair=(55, 58, 52), sclera=(45, 42, 45),
                            pupil=(225, 230, 220), mouth=(70, 22, 24), Kind="walker"),
        "face_runner": dict(skin=(142, 134, 130), hair=(42, 36, 36), sclera=(165, 25, 25),
                            pupil=(245, 60, 40), mouth=(145, 18, 20), Kind="runner"),
        "face_spitter": dict(skin=(118, 152, 104), hair=(48, 64, 42), sclera=(40, 95, 35),
                             pupil=(135, 250, 65), mouth=(75, 185, 45), Kind="spitter"),
        "face_goliath": dict(skin=(125, 112, 108), hair=(50, 44, 42), sclera=(48, 38, 32),
                             pupil=(245, 215, 45), mouth=(85, 24, 22), Kind="goliath"),
        # survivors
        "face_aras": dict(skin=(198, 152, 118), hair=(36, 30, 28), sclera=(240, 240, 238),
                          pupil=(52, 38, 28), mouth=(152, 92, 82), Kind="aras"),
        "face_selin": dict(skin=(224, 184, 156), hair=(32, 28, 34), sclera=(242, 242, 242),
                           pupil=(48, 64, 52), mouth=(185, 98, 98), Kind="selin"),
        "face_kemal": dict(skin=(154, 114, 86), hair=(128, 130, 134), sclera=(236, 236, 232),
                           pupil=(44, 34, 26), mouth=(122, 74, 66), Kind="kemal"),
        "face_elif": dict(skin=(220, 178, 150), hair=(98, 62, 38), sclera=(242, 242, 240),
                          pupil=(62, 82, 54), mouth=(182, 96, 92), Kind="elif"),
    }[tag]

    skin = profiles["skin"]
    hair = profiles["hair"]
    kind = profiles["Kind"]
    c.fill_base_noise(skin, amp=10, mode="isotropic", seed=seed)

    # Forehead / hair band (v > 0.78), keeping 6% side margin so cell border stays skin colour
    c.rect_uv(0.06, 0.80, 0.94, 0.95, hair)
    if kind in ("selin", "elif"):
        # Side hair framing temples
        c.rect_uv(0.06, 0.48, 0.16, 0.85, hair)
        c.rect_uv(0.84, 0.48, 0.94, 0.85, hair)
    if kind == "kemal":
        # Grey stubble beard around jaw/chin (v 0.08..0.34)
        stubble = _lerp_c(skin, hair, 0.55)
        c.rect_uv(0.20, 0.08, 0.80, 0.33, stubble)

    # Cheek shading & brow ridge
    shadow = _lerp_c(skin, (30, 20, 20), 0.25)
    c.rect_uv(0.20, 0.64, 0.40, 0.68, hair if kind in ("aras", "selin", "kemal", "elif") else shadow)
    c.rect_uv(0.60, 0.64, 0.80, 0.68, hair if kind in ("aras", "selin", "kemal", "elif") else shadow)

    # Eyes at v=0.58, centred u=0.30 and u=0.70, ~0.12 wide
    for uc in (0.30, 0.70):
        if kind in ("walker", "runner", "spitter", "goliath"):
            c.disc_uv(uc, 0.58, 0.085, 0.065, (38, 28, 32))
        c.disc_uv(uc, 0.58, 0.062, 0.042, profiles["sclera"])
        pr = 0.022 if kind == "goliath" else 0.030
        c.disc_uv(uc, 0.58, pr, pr, profiles["pupil"])

    # Nose at v~0.42, u=0.50
    c.rect_uv(0.46, 0.40, 0.54, 0.55, shadow)
    c.rect_uv(0.44, 0.39, 0.56, 0.43, _lerp_c(skin, (20, 10, 10), 0.32))

    # Mouth at v=0.25, centre u=0.50, ~0.34 wide
    if kind == "spitter":
        c.rect_uv(0.32, 0.16, 0.68, 0.31, (35, 55, 25))
        c.rect_uv(0.36, 0.10, 0.64, 0.26, profiles["mouth"])
        c.disc_uv(0.22, 0.36, 0.045, 0.045, (165, 220, 65))
    elif kind in ("walker", "runner", "goliath"):
        mh = 0.07 if kind != "goliath" else 0.10
        c.rect_uv(0.33, 0.25 - mh * 0.5, 0.67, 0.25 + mh * 0.5, profiles["mouth"])
        # Teeth bars
        for k in range(5):
            tu = 0.35 + k * 0.065
            c.rect_uv(tu, 0.24, tu + 0.04, 0.28, (215, 205, 180))
        if kind == "walker":
            # Facial wound / stitch line on cheek
            c.rect_uv(0.66, 0.34, 0.84, 0.37, (115, 22, 24))
        elif kind == "runner":
            # Blood drips from mouth to chin
            c.rect_uv(0.42, 0.08, 0.58, 0.22, (145, 16, 18))
    else:
        c.rect_uv(0.35, 0.23, 0.65, 0.28, profiles["mouth"])
        c.rect_uv(0.37, 0.25, 0.63, 0.265, _lerp_c(profiles["mouth"], (40, 15, 15), 0.35))


def _paint_label(c, bg_rgb, band_rgb, accent_rgb, emblem="circle", seed=1):
    """Wrap-around product label art for cans, bottles, books, snack wrappers, boxes."""
    c.fill_base_noise(bg_rgb, amp=8, seed=seed)
    # Top and bottom wrap bands
    c.rect_uv(0.04, 0.78, 0.96, 0.92, band_rgb)
    c.rect_uv(0.04, 0.08, 0.96, 0.20, band_rgb)
    # Central label panel + emblem
    c.rect_uv(0.08, 0.24, 0.92, 0.74, (242, 238, 228))
    if emblem == "tomato":
        c.disc_uv(0.50, 0.50, 0.16, 0.16, (210, 35, 30))
        c.rect_uv(0.47, 0.64, 0.53, 0.70, (40, 135, 45))
    elif emblem == "beans":
        for uc, vc in ((0.42, 0.52), (0.52, 0.46), (0.58, 0.54)):
            c.disc_uv(uc, vc, 0.07, 0.05, (155, 68, 32))
    elif emblem == "fish":
        c.disc_uv(0.48, 0.50, 0.17, 0.08, (35, 95, 155))
        c.rect_uv(0.62, 0.44, 0.70, 0.56, (35, 95, 155))
    elif emblem == "drop":
        c.disc_uv(0.50, 0.46, 0.11, 0.11, accent_rgb)
        c.rect_uv(0.46, 0.52, 0.54, 0.64, accent_rgb)
    elif emblem == "cross":
        c.rect_uv(0.44, 0.34, 0.56, 0.66, accent_rgb)
        c.rect_uv(0.34, 0.44, 0.66, 0.56, accent_rgb)
    else:
        c.disc_uv(0.50, 0.50, 0.14, 0.14, accent_rgb)
    c.text_lines_uv(0.14, 0.27, 0.86, 0.38, 2, (45, 45, 50), seed=seed)


def render_cell(atlas_name, tag, cs, seed):
    """Render one cell/tile of size `cs x cs` and return a `CellCanvas`."""
    c = CellCanvas(cs)

    # ========================================================== ENV_STRUCT_2K (tile)
    if tag == "concrete_raw":
        c.fill_base_noise((138, 140, 136), amp=18, seed=seed)
    elif tag == "concrete_dark":
        c.fill_base_noise((78, 82, 84), amp=14, seed=seed)
    elif tag == "brick_red":
        c.fill_base_noise((148, 62, 48), amp=16, seed=seed)
        mortar = (178, 172, 162)
        rows = 8
        for r in range(rows):
            v0 = r / float(rows)
            c.rect_uv(0.0, v0, 1.0, v0 + 0.018, mortar)
            off = 0.125 if (r & 1) else 0.0
            for k in range(4):
                u = (off + k * 0.25) % 1.0
                c.rect_uv(u, v0, min(1.0, u + 0.016), v0 + 1.0 / rows, mortar)
    elif tag == "plaster_white":
        c.fill_base_noise((226, 224, 218), amp=10, seed=seed)
    elif tag == "plaster_dirty":
        c.fill_base_noise((176, 168, 152), amp=22, mode="rust", c2=(122, 114, 98), seed=seed)
    elif tag == "wallpaper_stripe":
        c.fill_base_noise((212, 204, 186), amp=8, seed=seed)
        for k in range(8):
            u0 = k / 8.0 + 0.03
            c.rect_uv(u0, 0.0, u0 + 0.05, 1.0, (168, 152, 126))
    elif tag == "wallpaper_floral":
        c.fill_base_noise((196, 206, 188), amp=9, seed=seed)
        for gy in range(4):
            for gx in range(4):
                uc = (gx + 0.5) * 0.25
                vc = (gy + 0.5) * 0.25
                c.disc_uv(uc, vc, 0.045, 0.045, (162, 118, 124))
                c.disc_uv(uc, vc, 0.018, 0.018, (224, 205, 130))
    elif tag == "tile_white":
        c.fill_base_noise((232, 236, 238), amp=7, seed=seed)
        grout = (165, 170, 174)
        for k in range(4):
            p = k * 0.25
            c.rect_uv(0.0, p, 1.0, p + 0.015, grout)
            c.rect_uv(p, 0.0, p + 0.015, 1.0, grout)
    elif tag == "tile_checker":
        c.fill_base_noise((226, 226, 222), amp=6, seed=seed)
        for gy in range(4):
            for gx in range(4):
                if (gx + gy) & 1:
                    c.rect_uv(gx * 0.25, gy * 0.25, (gx + 1) * 0.25, (gy + 1) * 0.25, (48, 52, 56))
    elif tag == "parquet_oak":
        c.fill_base_noise((164, 116, 68), amp=16, mode="wood", freq=10, seed=seed)
        seam = (102, 68, 38)
        for k in range(8):
            v0 = k * 0.125
            c.rect_uv(0.0, v0, 1.0, v0 + 0.008, seam)
            u_j = 0.5 if (k & 1) else 0.25
            c.rect_uv(u_j, v0, u_j + 0.008, v0 + 0.125, seam)
    elif tag == "carpet_grey":
        c.fill_base_noise((112, 116, 122), amp=14, mode="fabric", seed=seed)
    elif tag == "carpet_red":
        c.fill_base_noise((142, 36, 38), amp=14, mode="fabric", seed=seed)
    elif tag == "metal_plate":
        c.fill_base_noise((126, 132, 138), amp=12, mode="brushed", seed=seed)
        c.frame_uv(0.0, 0.0, 1.0, 1.0, 0.015, (86, 92, 96))
    elif tag == "marble_white":
        c.fill_base_noise((236, 234, 230), amp=10, mode="marble", c2=(158, 160, 166), seed=seed)
    elif tag == "ceiling_panel":
        c.fill_base_noise((218, 220, 216), amp=10, seed=seed)
        for k in range(2):
            p = k * 0.5
            c.rect_uv(0.0, p, 1.0, p + 0.02, (150, 154, 156))
            c.rect_uv(p, 0.0, p + 0.02, 1.0, (150, 154, 156))
    elif tag == "wall_damaged":
        c.fill_base_noise((168, 160, 148), amp=24, mode="rust", c2=(112, 72, 54), seed=seed)

    # ========================================================== ENV_MAT_2K (tile)
    elif tag == "wood_dark":
        c.fill_base_noise((92, 58, 34), amp=14, mode="wood", freq=8, seed=seed)
    elif tag == "wood_light":
        c.fill_base_noise((188, 144, 94), amp=14, mode="wood", freq=8, seed=seed)
    elif tag == "wood_plank_worn":
        c.fill_base_noise((134, 102, 72), amp=18, mode="wood", freq=7, seed=seed)
        for k in range(4):
            c.rect_uv(0.0, k * 0.25, 1.0, k * 0.25 + 0.012, (68, 48, 32))
    elif tag == "fabric_blue":
        c.fill_base_noise((54, 86, 138), amp=12, mode="fabric", seed=seed)
    elif tag == "fabric_red":
        c.fill_base_noise((158, 46, 46), amp=12, mode="fabric", seed=seed)
    elif tag == "fabric_green":
        c.fill_base_noise((62, 108, 68), amp=12, mode="fabric", seed=seed)
    elif tag == "leather_brown":
        c.fill_base_noise((108, 64, 36), amp=12, seed=seed)
    elif tag == "leather_black":
        c.fill_base_noise((38, 38, 42), amp=8, seed=seed)
    elif tag == "plastic_white":
        c.fill_base_noise((235, 236, 238), amp=5, seed=seed)
    elif tag == "plastic_black":
        c.fill_base_noise((34, 36, 40), amp=6, seed=seed)
    elif tag == "rubber":
        c.fill_base_noise((44, 46, 48), amp=9, seed=seed)
    elif tag == "glass_dirty":
        c.fill_base_noise((154, 182, 188), amp=12, seed=seed)
    elif tag == "steel_brushed":
        c.fill_base_noise((162, 168, 174), amp=14, mode="brushed", seed=seed)
    elif tag in ("rust_heavy", "rust_metal"):
        c.fill_base_noise((124, 64, 38), amp=22, mode="rust", c2=(78, 48, 36), seed=seed)
    elif tag == "painted_metal_green":
        c.fill_base_noise((64, 104, 74), amp=11, mode="brushed", seed=seed)
    elif tag == "painted_metal_blue":
        c.fill_base_noise((58, 92, 132), amp=11, mode="brushed", seed=seed)

    # ========================================================== ENV_CITY_2K (tile)
    elif tag == "asphalt":
        c.fill_base_noise((58, 60, 64), amp=14, seed=seed)
    elif tag in ("facade_glass_blue", "facade_glass_dark", "facade_concrete", "facade_brick", "facade_stone"):
        pal_f = {
            "facade_glass_blue": ((72, 104, 132), (42, 74, 108)),
            "facade_glass_dark": ((56, 62, 70), (28, 34, 42)),
            "facade_concrete": ((156, 158, 154), (52, 62, 72)),
            "facade_brick": ((142, 68, 54), (48, 54, 62)),
            "facade_stone": ((178, 168, 148), (58, 64, 72)),
        }[tag]
        c.fill_base_noise(pal_f[0], amp=12, seed=seed)
        win = pal_f[1]
        for gy in range(4):
            for gx in range(4):
                u0, v0 = gx * 0.25 + 0.04, gy * 0.25 + 0.05
                c.rect_uv(u0, v0, u0 + 0.17, v0 + 0.15, win)
    elif tag == "paint_yellow":
        c.fill_base_noise((228, 184, 36), amp=8, seed=seed)
    elif tag == "paint_white":
        c.fill_base_noise((236, 236, 232), amp=6, seed=seed)
    elif tag == "car_red":
        c.fill_base_noise((182, 34, 36), amp=8, mode="brushed", seed=seed)
    elif tag == "car_blue":
        c.fill_base_noise((38, 82, 158), amp=8, mode="brushed", seed=seed)
    elif tag == "car_black":
        c.fill_base_noise((32, 34, 38), amp=6, mode="brushed", seed=seed)
    elif tag == "car_white":
        c.fill_base_noise((232, 234, 238), amp=6, mode="brushed", seed=seed)
    elif tag == "car_green":
        c.fill_base_noise((48, 112, 62), amp=9, mode="brushed", seed=seed)
    elif tag == "roof_gravel":
        c.fill_base_noise((108, 104, 98), amp=18, seed=seed)
    elif tag == "grass_dead":
        c.fill_base_noise((118, 112, 68), amp=18, seed=seed)
    elif tag == "steel_dark":
        c.fill_base_noise((68, 74, 80), amp=10, mode="brushed", seed=seed)

    # ========================================================== HERO_WEAPONS_1K (cell)
    elif tag == "gun_steel":
        c.fill_base_noise((118, 124, 130), amp=12, mode="brushed", seed=seed)
    elif tag == "gun_blued":
        c.fill_base_noise((54, 62, 74), amp=10, mode="brushed", seed=seed)
    elif tag == "gun_black_poly":
        c.fill_base_noise((36, 38, 42), amp=7, seed=seed)
    elif tag == "wood_stock":
        c.fill_base_noise((142, 92, 48), amp=14, mode="wood", freq=7, seed=seed)
    elif tag == "wood_dark_stock":
        c.fill_base_noise((86, 52, 30), amp=12, mode="wood", freq=7, seed=seed)
    elif tag == "brass":
        c.fill_base_noise((198, 164, 68), amp=12, mode="brushed", seed=seed)
    elif tag == "grip_rubber":
        c.fill_base_noise((42, 44, 46), amp=10, mode="fabric", seed=seed)
    elif tag == "camo_green":
        c.fill_base_noise((78, 98, 64), amp=12, mode="camo", c2=(116, 104, 74), seed=seed)
    elif tag == "tan_polymer":
        c.fill_base_noise((176, 156, 122), amp=9, seed=seed)
    elif tag == "chrome":
        c.fill_base_noise((208, 214, 220), amp=12, mode="brushed", seed=seed)
    elif tag == "scope_glass":
        c.fill_base_noise((48, 118, 136), amp=10, seed=seed)
    elif tag == "tape_wrap":
        c.fill_base_noise((184, 176, 156), amp=12, mode="fabric", seed=seed)
    elif tag == "paint_red":
        c.fill_base_noise((188, 38, 36), amp=9, seed=seed)
    elif tag == "bat_wood":
        c.fill_base_noise((178, 132, 82), amp=12, mode="wood", freq=6, seed=seed)
    elif tag == "pipe_grey":
        c.fill_base_noise((104, 108, 112), amp=14, mode="brushed", seed=seed)

    # ========================================================== HERO_ZOMBIE_1K & HERO_SURVIVOR_1K
    elif tag.startswith("face_"):
        _paint_face(c, tag, seed)
    elif tag == "skin_pale":
        c.fill_base_noise((158, 162, 148), amp=10, seed=seed)
    elif tag == "skin_grey":
        c.fill_base_noise((132, 134, 130), amp=10, seed=seed)
    elif tag == "skin_green_sick":
        c.fill_base_noise((118, 152, 104), amp=12, seed=seed)
    elif tag == "skin_burnt":
        c.fill_base_noise((82, 66, 60), amp=14, mode="rust", c2=(125, 72, 58), seed=seed)
    elif tag == "cloth_suit":
        c.fill_base_noise((58, 62, 72), amp=10, mode="fabric", seed=seed)
    elif tag == "cloth_casual":
        c.fill_base_noise((88, 106, 122), amp=12, mode="fabric", seed=seed)
    elif tag == "cloth_labcoat":
        c.fill_base_noise((186, 184, 174), amp=14, mode="rust", c2=(132, 98, 86), seed=seed)
    elif tag == "cloth_security":
        c.fill_base_noise((44, 58, 78), amp=10, mode="fabric", seed=seed)
    elif tag == "armor_plate":
        c.fill_base_noise((64, 70, 76), amp=12, mode="brushed", seed=seed)
    elif tag == "blood_fresh":
        c.fill_base_noise((142, 20, 22), amp=14, mode="rust", c2=(96, 12, 14), seed=seed)
    elif tag == "blood_dried":
        c.fill_base_noise((78, 28, 24), amp=12, seed=seed)
    elif tag == "boil_flesh":
        c.fill_base_noise((164, 188, 68), amp=18, mode="rust", c2=(138, 92, 48), seed=seed)
    elif tag == "skin_tan":
        c.fill_base_noise((198, 152, 118), amp=8, seed=seed)
    elif tag == "skin_light":
        c.fill_base_noise((224, 184, 156), amp=8, seed=seed)
    elif tag == "skin_dark":
        c.fill_base_noise((154, 114, 86), amp=8, seed=seed)
    elif tag == "hair_black":
        c.fill_base_noise((34, 30, 30), amp=8, mode="brushed", seed=seed)
    elif tag == "hair_brown":
        c.fill_base_noise((96, 62, 38), amp=10, mode="brushed", seed=seed)
    elif tag == "hair_grey":
        c.fill_base_noise((136, 138, 142), amp=10, mode="brushed", seed=seed)
    elif tag == "cloth_jacket_orange":
        c.fill_base_noise((204, 102, 38), amp=11, mode="fabric", seed=seed)
    elif tag == "cloth_jeans":
        c.fill_base_noise((58, 84, 118), amp=11, mode="fabric", seed=seed)
    elif tag == "cloth_labcoat_clean":
        c.fill_base_noise((232, 234, 236), amp=7, mode="fabric", seed=seed)
    elif tag == "cloth_overalls":
        c.fill_base_noise((76, 88, 72), amp=11, mode="fabric", seed=seed)
    elif tag == "glove_leather":
        c.fill_base_noise((92, 58, 36), amp=10, seed=seed)
    elif tag == "boot_black":
        c.fill_base_noise((36, 36, 40), amp=8, seed=seed)

    # ========================================================== HERO_ITEMS_1K (cell)
    elif tag == "keycard":
        # Card face: magnetic stripe v .72-.88, gold chip u .15-.32 v .40-.62, white text lines, colour band
        c.fill_base_noise((232, 236, 240), amp=5, seed=seed)
        c.rect_uv(0.04, 0.08, 0.96, 0.24, (32, 108, 172))
        c.rect_uv(0.04, 0.72, 0.96, 0.88, (28, 30, 34))
        c.rect_uv(0.15, 0.40, 0.32, 0.62, (214, 176, 64))
        c.frame_uv(0.15, 0.40, 0.32, 0.62, 0.015, (148, 116, 36))
        c.text_lines_uv(0.40, 0.38, 0.90, 0.64, 3, (52, 62, 74), seed=seed)
    elif tag == "vehicle_armor":
        c.fill_base_noise((62, 68, 64), amp=11, mode="brushed", seed=seed)
    elif tag == "helicopter_body":
        c.fill_base_noise((216, 220, 224), amp=9, mode="brushed", seed=seed)
    elif tag == "radio_olive":
        c.fill_base_noise((78, 92, 64), amp=10, mode="brushed", seed=seed)
    elif tag == "medkit_white":
        c.fill_base_noise((238, 240, 242), amp=5, seed=seed)
        c.rect_uv(0.40, 0.20, 0.60, 0.80, (206, 34, 34))
        c.rect_uv(0.20, 0.40, 0.80, 0.60, (206, 34, 34))
    elif tag == "generator_orange":
        c.fill_base_noise((218, 116, 34), amp=10, mode="brushed", seed=seed)
    elif tag == "battery_green":
        _paint_label(c, (48, 128, 68), (28, 34, 30), (228, 196, 44), emblem="circle", seed=seed)
    elif tag in ("document_paper", "paper_note"):
        c.fill_base_noise((238, 230, 208), amp=7, seed=seed)
        c.text_lines_uv(0.10, 0.16, 0.90, 0.86, 8, (72, 76, 84), seed=seed)
        c.rect_uv(0.62, 0.14, 0.88, 0.26, (186, 46, 42))
    elif tag == "chem_yellow":
        c.fill_base_noise((224, 188, 42), amp=10, seed=seed)
    elif tag == "circuit_green":
        c.fill_base_noise((28, 104, 54), amp=8, seed=seed)
        copper = (196, 168, 78)
        for k in range(6):
            p = 0.12 + k * 0.14
            c.rect_uv(0.08, p, 0.92, p + 0.022, copper)
            c.rect_uv(p, 0.08, p + 0.022, 0.92, copper)
            c.disc_uv(p, p, 0.04, 0.04, (214, 192, 110))
        c.rect_uv(0.34, 0.36, 0.66, 0.64, (36, 38, 42))
    elif tag == "canister_red":
        c.fill_base_noise((192, 38, 34), amp=10, mode="brushed", seed=seed)
    elif tag in ("glass_vial", "bulb_glass"):
        c.fill_base_noise((174, 214, 224), amp=9, seed=seed)
        c.rect_uv(0.22, 0.10, 0.34, 0.90, (228, 246, 250))
    elif tag == "lab_steel":
        c.fill_base_noise((176, 182, 188), amp=10, mode="brushed", seed=seed)
    elif tag == "screen_green":
        c.fill_base_noise((16, 48, 26), amp=6, seed=seed)
        for k in range(16):
            v0 = k / 16.0
            c.rect_uv(0.06, v0, 0.94, v0 + 0.018, (32, 118, 56))
        c.frame_uv(0.08, 0.10, 0.92, 0.90, 0.03, (96, 238, 128))
        c.text_lines_uv(0.16, 0.22, 0.84, 0.80, 5, (112, 248, 142), seed=seed)
    elif tag == "tool_orange":
        c.fill_base_noise((226, 98, 28), amp=10, seed=seed)
    elif tag in ("bandage", "bandage_roll"):
        c.fill_base_noise((232, 226, 212), amp=8, mode="fabric", seed=seed)
        c.rect_uv(0.06, 0.44, 0.94, 0.56, (198, 52, 48))

    # ========================================================== PROP_A_512 & PROP_B_512 (cell)
    elif tag == "can_tomato":
        _paint_label(c, (192, 44, 36), (132, 26, 22), (212, 38, 32), emblem="tomato", seed=seed)
    elif tag == "can_beans":
        _paint_label(c, (206, 116, 42), (124, 62, 24), (156, 72, 32), emblem="beans", seed=seed)
    elif tag == "can_tuna":
        _paint_label(c, (46, 118, 176), (26, 68, 114), (36, 96, 156), emblem="fish", seed=seed)
    elif tag == "bottle_water":
        _paint_label(c, (172, 216, 236), (36, 112, 184), (42, 134, 212), emblem="drop", seed=seed)
    elif tag == "bottle_soda":
        _paint_label(c, (196, 34, 38), (238, 236, 232), (218, 168, 36), emblem="circle", seed=seed)
    elif tag in ("book_red", "book_blue", "book_green"):
        bcol = {
            "book_red": ((156, 38, 38), (214, 176, 74)),
            "book_blue": ((38, 74, 138), (218, 186, 86)),
            "book_green": ((42, 108, 64), (214, 182, 78)),
        }[tag]
        c.fill_base_noise(bcol[0], amp=10, mode="fabric", seed=seed)
        c.frame_uv(0.10, 0.10, 0.90, 0.90, 0.03, bcol[1])
        c.rect_uv(0.18, 0.62, 0.82, 0.78, bcol[1])
        c.text_lines_uv(0.22, 0.24, 0.78, 0.52, 3, bcol[1], seed=seed)
    elif tag == "outlet_white":
        c.fill_base_noise((236, 236, 232), amp=5, seed=seed)
        c.frame_uv(0.08, 0.08, 0.92, 0.92, 0.03, (188, 188, 182))
        for vc in (0.32, 0.68):
            c.disc_uv(0.50, vc, 0.16, 0.13, (172, 172, 168))
            c.rect_uv(0.41, vc - 0.05, 0.45, vc + 0.05, (28, 28, 32))
            c.rect_uv(0.55, vc - 0.05, 0.59, vc + 0.05, (28, 28, 32))
    elif tag == "box_cardboard":
        c.fill_base_noise((174, 136, 92), amp=12, seed=seed)
        c.rect_uv(0.06, 0.46, 0.94, 0.54, (132, 98, 62))
        c.text_lines_uv(0.18, 0.18, 0.82, 0.38, 2, (74, 54, 36), seed=seed)
    elif tag == "wrapper_snack":
        _paint_label(c, (218, 152, 32), (176, 36, 34), (192, 42, 36), emblem="circle", seed=seed)
    elif tag == "battery_metal":
        _paint_label(c, (154, 160, 166), (36, 40, 46), (218, 164, 38), emblem="circle", seed=seed)
    elif tag in ("key_brass", "coin_gold"):
        c.fill_base_noise((212, 174, 66), amp=12, mode="brushed", seed=seed)
        c.disc_uv(0.50, 0.50, 0.34, 0.34, (228, 192, 84))
    elif tag == "tape_duct":
        c.fill_base_noise((146, 150, 154), amp=10, mode="brushed", seed=seed)
    elif tag == "pill_bottle":
        _paint_label(c, (218, 124, 34), (236, 236, 236), (198, 38, 38), emblem="cross", seed=seed)
    elif tag == "syringe":
        c.fill_base_noise((212, 228, 234), amp=7, seed=seed)
        for k in range(6):
            v0 = 0.18 + k * 0.11
            c.rect_uv(0.30, v0, 0.70, v0 + 0.02, (48, 54, 60))
    elif tag == "wire_copper":
        c.fill_base_noise((184, 104, 56), amp=14, mode="brushed", seed=seed)
    elif tag == "screw_metal":
        c.fill_base_noise((136, 142, 148), amp=12, mode="brushed", seed=seed)
    elif tag == "scrap_metal":
        c.fill_base_noise((108, 98, 92), amp=18, mode="rust", c2=(126, 68, 42), seed=seed)
    elif tag == "cloth_rag":
        c.fill_base_noise((168, 158, 142), amp=14, mode="fabric", seed=seed)
    elif tag in ("lighter", "matchbox", "cigarette_pack"):
        col = (204, 44, 36) if tag == "lighter" else ((186, 142, 78) if tag == "matchbox" else (218, 214, 206))
        _paint_label(c, col, (38, 36, 38), (218, 174, 46), emblem="circle", seed=seed)
    elif tag == "flashlight_body":
        c.fill_base_noise((46, 50, 56), amp=10, mode="brushed", seed=seed)
    elif tag == "radio_small":
        c.fill_base_noise((52, 56, 60), amp=8, seed=seed)
        # Speaker grille slats + dial window
        for k in range(6):
            v0 = 0.14 + k * 0.07
            c.rect_uv(0.14, v0, 0.86, v0 + 0.03, (24, 26, 28))
        c.rect_uv(0.14, 0.64, 0.86, 0.84, (176, 198, 168))
        c.rect_uv(0.46, 0.66, 0.52, 0.82, (204, 42, 38))
    elif tag == "photo_frame":
        c.fill_base_noise((116, 78, 46), amp=10, mode="wood", seed=seed)
        c.rect_uv(0.12, 0.12, 0.88, 0.88, (182, 198, 212))
        c.rect_uv(0.12, 0.12, 0.88, 0.38, (96, 128, 86))
        c.disc_uv(0.50, 0.58, 0.14, 0.14, (214, 172, 142))
    elif tag == "toy_plastic":
        _paint_label(c, (44, 124, 198), (224, 184, 38), (208, 46, 42), emblem="circle", seed=seed)
    elif tag == "mug_ceramic":
        _paint_label(c, (232, 228, 218), (46, 98, 156), (196, 54, 46), emblem="circle", seed=seed)
    else:
        c.fill_base_noise((140, 140, 140), amp=12, seed=seed)

    return c


def _write_png_rgb(path, width, height, rows_iter):
    """Write standard 8-bit RGB PNG from an iterable of row bytearrays/bytes."""
    raw = bytearray()
    for row in rows_iter:
        raw.append(0)  # Filter type 0 (None)
        raw.extend(row)

    def chunk(tag, data):
        crc = zlib.crc32(tag + data) & 0xFFFFFFFF
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", crc)

    ihdr = struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)
    idat = zlib.compress(bytes(raw), level=3)
    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", ihdr) + chunk(b"IDAT", idat) + chunk(b"IEND", b"")
    with open(path, "wb") as f:
        f.write(png)
    return len(png)


def _generate_one_atlas(args):
    name, spec, out_dir = args
    t0 = time.time()
    size = spec["size"]
    grid = A.GRID
    cs = size // grid
    tiles = spec["tiles"]

    cells = []
    for idx, tag in enumerate(tiles):
        cells.append(render_cell(name, tag, cs, seed=(idx + 1) * 131 + hash(name) % 997))

    # Stitch 4x4 grid of cells row-by-row (PNG row 0 = top = cell row 0)
    row_stride = cs * 3

    def iter_rows():
        for gr in range(grid):
            c0 = cells[gr * grid + 0].buf
            c1 = cells[gr * grid + 1].buf
            c2 = cells[gr * grid + 2].buf
            c3 = cells[gr * grid + 3].buf
            for cy in range(cs):
                s0 = cy * row_stride
                s1 = s0 + row_stride
                yield c0[s0:s1] + c1[s0:s1] + c2[s0:s1] + c3[s0:s1]

    out_path = os.path.join(out_dir, A.texture_filename(name))
    nbytes = _write_png_rgb(out_path, size, size, iter_rows())
    dt = time.time() - t0

    tile_entries = []
    for idx, tag in enumerate(tiles):
        col = idx % grid
        row = idx // grid
        u0, v0, du, dv = A.cell_rect(name, idx)
        tile_entries.append(dict(
            index=idx,
            tag=tag,
            col=col,
            row_from_top=row,
            uv_rect=[round(u0, 4), round(v0, 4), round(du, 4), round(dv, 4)],
            pixel_rect=[col * cs, row * cs, cs, cs],
        ))

    info = dict(
        atlas=name,
        file=A.texture_filename(name),
        material=A.material_name(name),
        size=size,
        grid=grid,
        cell_size=cs,
        mode=spec["mode"],
        mpt=spec["mpt"],
        bytes=nbytes,
        seconds=round(dt, 3),
        tiles=tile_entries,
    )
    return info


def main():
    t0 = time.time()
    out_dir = os.path.join(ROOT, "Assets", "Art", "Textures")
    os.makedirs(out_dir, exist_ok=True)

    tasks = [(name, spec, out_dir) for name, spec in A.ATLASES.items()]
    workers = min(len(tasks), max(1, (os.cpu_count() or 4)))
    with multiprocessing.Pool(workers) as pool:
        results = pool.map(_generate_one_atlas, tasks)

    for r in results:
        print("%-18s %4dx%-4d (%-4s, cell=%3dpx) -> %7d bytes  (%.2fs)" % (
            r["file"], r["size"], r["size"], r["mode"], r["cell_size"], r["bytes"], r["seconds"]))

    manifest = dict(
        count=len(results),
        grid=A.GRID,
        atlases={r["atlas"]: r for r in results},
    )
    for mpath in (
        os.path.join(out_dir, "atlas_manifest.json"),
        os.path.join(HERE, "atlas_manifest.json"),
    ):
        with open(mpath, "w") as f:
            json.dump(manifest, f, indent=2)

    dt = time.time() - t0
    print("Generated %d PNG atlases in %.2fs -> %s" % (len(results), dt, out_dir))
    return 0


if __name__ == "__main__":
    sys.exit(main())
