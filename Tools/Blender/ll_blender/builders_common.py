"""Shared helpers for all builders_*.py modules (pure python, no bpy).

Module protocol (every builders_<domain>.py):
    BUILDERS = {}                      # name -> fn(params:dict, pal:dict, atlas:str) -> Geo
    @register(BUILDERS)                # decorator, registers under fn.__name__
    def entries() -> list[Entry]       # all catalogue entries this module contributes (uses `mk`)
`catalog.py` collects every module's entries(); `builders.build_geo(entry)` looks the builder name up in the merged BUILDERS.

build_geo injects into params:  '_id' (entry id), '_scale' (entry.scale).  For RIGGED entries the builder must
build the mesh already at `_scale` (rig_spec.humanoid_bones(_scale)); for static entries build_geo bakes entry.scale.
"""
import math
import random
from dataclasses import dataclass, field

from . import atlas as A
from .geo import Geo
from . import rig_spec as R

CATEGORIES = ["Architecture", "Residential", "Office", "Mall", "Medical", "Gym", "Barricades", "Crafting",
              "Weapons", "Characters", "Zombies", "Loot", "City", "Vehicles", "Lab", "Executive"]
ANIM_STYLES = ("none", "shamble", "sprint", "hunch", "heavy", "boss", "survivor", "fps")

# tri budget classes (CONTRACT task statement)
BUDGET_DEFAULT = {"Architecture": 2500, "Residential": 1500, "Office": 1500, "Mall": 1500, "Medical": 1500,
                  "Gym": 1500, "Barricades": 1500, "Crafting": 1500, "Weapons": 3500, "Characters": 4500,
                  "Zombies": 4500, "Loot": 600, "City": 300, "Vehicles": 1200, "Lab": 1500, "Executive": 1500}


@dataclass
class Entry:
    id: str
    category: str
    builder: str
    params: dict = field(default_factory=dict)
    atlas: str = "ENV_MAT_2K"
    palette: dict = field(default_factory=dict)
    rig: str = "none"            # 'none' | 'humanoid' | 'fps_arms'
    scale: float = 1.0           # rig scale for rigged entries, baked uniform scale for static ones
    anim_style: str = "none"
    tri_budget: int = 1500


def mk(id, category, builder, params=None, atlas="ENV_MAT_2K", palette=None, rig="none", scale=1.0,
       anim="none", budget=None):
    assert category in CATEGORIES, category
    assert anim in ANIM_STYLES, anim
    return Entry(id, category, builder, dict(params or {}), atlas, dict(palette or {}), rig, scale, anim,
                 budget if budget is not None else BUDGET_DEFAULT[category])


def register(table):
    def deco(fn):
        table[fn.__name__] = fn
        return fn
    return deco


def rnd(params, salt=0):
    """Deterministic RNG from params['seed'] (and entry id hash-free salt)."""
    return random.Random(int(params.get("seed", 0)) * 1000003 + salt * 7919 + 12345)


class Tags:
    """Palette resolver: T(role, default_tag) -> tag guaranteed to exist in the atlas.
    entry.palette maps role -> tag; unknown/missing roles use the default (which must be valid)."""

    def __init__(self, atlas, pal):
        self.tiles = A.ATLASES[atlas]["tiles"]
        self.pal = pal or {}

    def __call__(self, role, default):
        t = self.pal.get(role, default)
        if t not in self.tiles:
            t = default
        assert t in self.tiles, "default tag %s not in atlas" % default
        return t


def geo_for(params, atlas, name=None):
    return Geo(atlas, name or str(params.get("_id", "geo")))


# --------------------------------------------------------------------------------------------- box helpers
_QUADS = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
# face order: bottom, top, front(-Y), right(+X), back(+Y), left(-X)


def _face_tags(tags):
    if isinstance(tags, str):
        return [tags] * 6
    if isinstance(tags, dict):
        s = tags.get("sides", tags.get("side"))
        base = tags.get("all")
        return [tags.get("bottom", base), tags.get("top", base), tags.get("front", s or base),
                tags.get("right", s or base), tags.get("back", s or base), tags.get("left", s or base)]
    t = list(tags)
    return t + [t[-1]] * (6 - len(t))


def _rot_euler(p, rx, ry, rz):
    x, y, z = p
    if rx:
        c, s = math.cos(rx), math.sin(rx)
        y, z = y * c - z * s, y * s + z * c
    if ry:
        c, s = math.cos(ry), math.sin(ry)
        x, z = x * c + z * s, -x * s + z * c
    if rz:
        c, s = math.cos(rz), math.sin(rz)
        x, y = x * c - y * s, x * s + y * c
    return (x, y, z)


def obox(g, center, size, tags, bone=None, rot=(0, 0, 0), taper=1.0, skip=()):
    """Oriented box with per-face tags (str | 6-seq | dict top/bottom/sides/front/...), Euler rot (rx, ry, rz) radians
    applied about the box centre (X then Y then Z). `skip` = face indices to omit (0 bottom,1 top,2 front,3 right,4 back,5 left)."""
    ft = _face_tags(tags)
    cx, cy, cz = center
    sx, sy, sz = size[0] / 2.0, size[1] / 2.0, size[2] / 2.0
    rx, ry, rz = rot
    pts = []
    for k, zz in ((0, -sz), (1, sz)):
        t = 1.0 if k == 0 else taper
        for (x, y) in ((-sx * t, -sy * t), (sx * t, -sy * t), (sx * t, sy * t), (-sx * t, sy * t)):
            p = _rot_euler((x, y, zz), rx, ry, rz)
            pts.append((cx + p[0], cy + p[1], cz + p[2]))
    i = [g.add_vert(p, bone) for p in pts]
    for n, q in enumerate(_QUADS):
        if n in skip:
            continue
        g.add_face([i[k] for k in q], ft[n])
    return g


def bx(g, x0, y0, z0, x1, y1, z1, tags, bone=None, **kw):
    """Axis aligned box by min/max corners (handles swapped corners)."""
    a0, a1 = min(x0, x1), max(x0, x1)
    b0, b1 = min(y0, y1), max(y0, y1)
    c0, c1 = min(z0, z1), max(z0, z1)
    return obox(g, ((a0 + a1) / 2, (b0 + b1) / 2, (c0 + c1) / 2), (a1 - a0, b1 - b0, c1 - c0), tags, bone, **kw)


def prism(g, pts_xy, z0, z1, tag, cap_tag=None, bone=None):
    """Extruded CONVEX polygon (CCW seen from +Z) from z0 to z1."""
    n = len(pts_xy)
    cap_tag = cap_tag or tag
    ib = [g.add_vert((p[0], p[1], z0), bone) for p in pts_xy]
    it = [g.add_vert((p[0], p[1], z1), bone) for p in pts_xy]
    for k in range(n):
        k2 = (k + 1) % n
        g.add_face([ib[k], ib[k2], it[k2], it[k]], tag)
    g.add_face(list(reversed(ib)), cap_tag)
    g.add_face(list(it), cap_tag)
    return g


def _frame(a, b):
    d = (b[0] - a[0], b[1] - a[1], b[2] - a[2])
    L = math.sqrt(d[0] ** 2 + d[1] ** 2 + d[2] ** 2) or 1e-9
    f = (d[0] / L, d[1] / L, d[2] / L)
    ref = (0, 0, 1) if abs(f[2]) < 0.9 else (1, 0, 0)
    r = (ref[1] * f[2] - ref[2] * f[1], ref[2] * f[0] - ref[0] * f[2], ref[0] * f[1] - ref[1] * f[0])
    rl = math.sqrt(r[0] ** 2 + r[1] ** 2 + r[2] ** 2) or 1e-9
    r = (r[0] / rl, r[1] / rl, r[2] / rl)
    u = (f[1] * r[2] - f[2] * r[1], f[2] * r[0] - f[0] * r[2], f[0] * r[1] - f[1] * r[0])
    return f, r, u, L


def tube(g, a, b, r0, r1, tag, seg=6, bone=None, cap_a=True, cap_b=True, cap_tag=None, squash=(1.0, 1.0)):
    """Oriented frustum between points a and b (any direction): limbs, pipes, handles, barrels.
    r0 at a, r1 at b. `squash` scales the (right, up) radii (elliptic section)."""
    f, r, u, L = _frame(a, b)
    cap_tag = cap_tag or tag
    ra, rb = [], []
    for k in range(seg):
        th = 2 * math.pi * k / seg
        c, s = math.cos(th) * squash[0], math.sin(th) * squash[1]
        ra.append(g.add_vert((a[0] + (r[0] * c + u[0] * s) * r0, a[1] + (r[1] * c + u[1] * s) * r0,
                              a[2] + (r[2] * c + u[2] * s) * r0), bone))
        rb.append(g.add_vert((b[0] + (r[0] * c + u[0] * s) * r1, b[1] + (r[1] * c + u[1] * s) * r1,
                              b[2] + (r[2] * c + u[2] * s) * r1), bone))
    circ = 2 * math.pi * max(r0, r1, 1e-4)
    for k in range(seg):
        k2 = (k + 1) % seg
        u0, u1 = circ * k / seg, circ * (k + 1) / seg
        g.add_face([ra[k], ra[k2], rb[k2], rb[k]], tag, [(u0, 0), (u1, 0), (u1, L), (u0, L)])
    if cap_a and r0 > 1e-6:
        g.add_face(list(reversed(ra)), cap_tag)
    if cap_b and r1 > 1e-6:
        g.add_face(list(rb), cap_tag)
    return g


def ball(g, center, radius, tag, seg=6, rings=4, bone=None, scale=(1, 1, 1)):
    return g.sphere(center, radius, tag, seg=seg, rings=rings, bone=bone, scale=scale)


# ------------------------------------------------------------------------------------- painted-cell helpers
def qpaint(g, p0, p1, p2, p3, tag, frac=(0.0, 0.0, 1.0, 1.0), bone=None):
    """Quad (CCW from outside: p0 bottom-left, p1 bottom-right, p2 top-right, p3 top-left as seen by the viewer)
    textured with the sub-rectangle frac=(u0,v0,u1,v1) (0..1 fractions of the atlas cell/tile). Use for faces,
    labels, screens, keycards, documents, wall posters."""
    mpt = A.ATLASES[g.atlas]["mpt"]
    u0, v0, u1, v1 = frac
    i = [g.add_vert(p, bone) for p in (p0, p1, p2, p3)]
    g.add_face(i, tag, [(u0 * mpt, v0 * mpt), (u1 * mpt, v0 * mpt), (u1 * mpt, v1 * mpt), (u0 * mpt, v1 * mpt)])
    return g


def front_label(g, center, size, tag, face="front", bone=None, frac=(0.0, 0.0, 1.0, 1.0), off=0.0005):
    """Painted quad lying on the front(-Y)/back(+Y)/right(+X)/left(-X)/top(+Z) face plane of a box with the given
    centre/size (offset outwards by `off`). Full cell by default."""
    cx, cy, cz = center
    hx, hy, hz = size[0] / 2.0, size[1] / 2.0, size[2] / 2.0
    if face == "front":
        y = cy - hy - off
        return qpaint(g, (cx - hx, y, cz - hz), (cx + hx, y, cz - hz), (cx + hx, y, cz + hz), (cx - hx, y, cz + hz),
                      tag, frac, bone)
    if face == "back":
        y = cy + hy + off
        return qpaint(g, (cx + hx, y, cz - hz), (cx - hx, y, cz - hz), (cx - hx, y, cz + hz), (cx + hx, y, cz + hz),
                      tag, frac, bone)
    if face == "right":
        x = cx + hx + off
        return qpaint(g, (x, cy - hy, cz - hz), (x, cy + hy, cz - hz), (x, cy + hy, cz + hz), (x, cy - hy, cz + hz),
                      tag, frac, bone)
    if face == "left":
        x = cx - hx - off
        return qpaint(g, (x, cy + hy, cz - hz), (x, cy - hy, cz - hz), (x, cy - hy, cz + hz), (x, cy + hy, cz + hz),
                      tag, frac, bone)
    # top
    z = cz + hz + off
    return qpaint(g, (cx - hx, cy - hy, z), (cx + hx, cy - hy, z), (cx + hx, cy + hy, z), (cx - hx, cy + hy, z),
                  tag, frac, bone)


def label_cyl(g, center, radius, height, tag, cap_tag, seg=10, axis="z", bone=None, radius_top=None,
              frac=(0.0, 0.05, 1.0, 0.95)):
    """Cylinder whose side wraps ONE FULL CELL (u 0..1 around, v=frac range along height); caps use cap_tag.
    For cans / bottles / batteries / pill bottles."""
    mpt = A.ATLASES[g.atlas]["mpt"]
    rt = radius if radius_top is None else radius_top
    ring_b, ring_t = [], []
    for k in range(seg):
        a = 2 * math.pi * k / seg
        ca, sa = math.cos(a), math.sin(a)
        ring_b.append(Geo._axis_pt(center, axis, radius * ca, radius * sa, -height / 2))
        ring_t.append(Geo._axis_pt(center, axis, rt * ca, rt * sa, height / 2))
    ib = [g.add_vert(p, bone) for p in ring_b]
    it = [g.add_vert(p, bone) for p in ring_t]
    for k in range(seg):
        k2 = (k + 1) % seg
        u0, u1 = mpt * 0.98 * k / seg, mpt * 0.98 * (k + 1) / seg
        v0, v1 = frac[1] * mpt, frac[3] * mpt
        g.add_face([ib[k], ib[k2], it[k2], it[k]], tag, [(u0, v0), (u1, v0), (u1, v1), (u0, v1)])
    if radius > 1e-6:
        g.add_face(list(reversed(ib)), cap_tag)
    if rt > 1e-6:
        g.add_face(list(it), cap_tag)
    return g


def apply_scale_static(g, s):
    if abs(s - 1.0) > 1e-9:
        g.transform(scale=(s, s, s))
    return g


def pick(seq, i):
    return seq[i % len(seq)]


def lerp(a, b, t):
    return a + (b - a) * t
