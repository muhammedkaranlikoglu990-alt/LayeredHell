"""Pure-python (no bpy) procedural geometry container + primitives.

Builders create a `Geo`; the bpy layer converts it to a Blender mesh. Because this module has no bpy
dependency the whole 800+ model catalogue can be validated (poly budget, UV range, bone tags) on any machine.

Conventions: metres, Blender axes (Z up). Characters/props FACE -Y (Blender front). Origin sits on the
floor at the object's footprint centre (props) so Unity prefabs drop straight onto the floor.
"""
import math
from . import atlas as A


def _sub(a, b): return (a[0] - b[0], a[1] - b[1], a[2] - b[2])
def _cross(a, b): return (a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0])
def _norm(v):
    l = math.sqrt(v[0] ** 2 + v[1] ** 2 + v[2] ** 2) or 1.0
    return (v[0] / l, v[1] / l, v[2] / l)


class Geo:
    def __init__(self, atlas, name="geo"):
        assert atlas in A.ATLASES, atlas
        self.atlas = atlas
        self.name = name
        self.verts = []        # (x, y, z)
        self.vbone = []        # bone name or None per vertex (rigid skinning)
        self.faces = []        # tuples of vertex indices (3..n), CCW seen from outside
        self.fuv0 = []         # per face: list of (u, v) per loop
        self.fuv1 = []         # per face: list of (u, v) per loop
        self.ftag = []         # per face: tile tag
        self.default_bone = None

    # ------------------------------------------------------------------ low level
    def add_vert(self, p, bone=None):
        self.verts.append((float(p[0]), float(p[1]), float(p[2])))
        self.vbone.append(bone if bone is not None else self.default_bone)
        return len(self.verts) - 1

    def add_face(self, idx, tag, uv_m=None):
        """idx: vertex indices; uv_m: optional list of (u,v) in METRES per loop (else planar projection)."""
        pts = [self.verts[i] for i in idx]
        if uv_m is None:
            uv_m = self._planar(pts)
        u0, u1 = self._map_uv(uv_m, tag)
        self.faces.append(tuple(idx))
        self.fuv0.append(u0)
        self.fuv1.append(u1)
        self.ftag.append(tag)

    @staticmethod
    def _normal(pts):
        nx = ny = nz = 0.0
        n = len(pts)
        for i in range(n):
            a, b = pts[i], pts[(i + 1) % n]
            nx += (a[1] - b[1]) * (a[2] + b[2])
            ny += (a[2] - b[2]) * (a[0] + b[0])
            nz += (a[0] - b[0]) * (a[1] + b[1])
        return _norm((nx, ny, nz))

    def _planar(self, pts):
        n = self._normal(pts)
        ax, ay, az = abs(n[0]), abs(n[1]), abs(n[2])
        if az >= ax and az >= ay:
            return [(p[0], p[1]) for p in pts]
        if ay >= ax:
            return [(p[0], p[2]) for p in pts]
        return [(p[1], p[2]) for p in pts]

    def _map_uv(self, uv_m, tag):
        a = A.ATLASES[self.atlas]
        mpt = a["mpt"]
        idx = A.tile_index(self.atlas, tag)
        cu, cv, du, dv = A.cell_rect(self.atlas, idx)
        us = [p[0] / mpt for p in uv_m]
        vs = [p[1] / mpt for p in uv_m]
        ou, ov = math.floor(min(us)), math.floor(min(vs))
        us = [u - ou for u in us]
        vs = [v - ov for v in vs]
        if a["mode"] == "tile":
            return [(u, v) for u, v in zip(us, vs)], [(cu, cv)] * len(us)
        su = 1.0 / max(1.0, max(us))
        sv = 1.0 / max(1.0, max(vs))
        s = min(su, sv)
        uv = [(cu + du * (0.03 + 0.94 * u * s), cv + dv * (0.03 + 0.94 * v * s)) for u, v in zip(us, vs)]
        return uv, uv

    # ------------------------------------------------------------------ primitives
    def box(self, center, size, tag, bone=None, rot_z=0.0, taper=1.0, uv_scale=1.0):
        """Axis aligned box (optionally yawed). taper scales the TOP face footprint (frustum)."""
        cx, cy, cz = center
        sx, sy, sz = size[0] / 2.0, size[1] / 2.0, size[2] / 2.0
        c, s = math.cos(rot_z), math.sin(rot_z)
        corners = []
        for k, zz in ((0, -sz), (1, sz)):
            t = 1.0 if k == 0 else taper
            for (x, y) in ((-sx * t, -sy * t), (sx * t, -sy * t), (sx * t, sy * t), (-sx * t, sy * t)):
                corners.append((cx + x * c - y * s, cy + x * s + y * c, cz + zz))
        i = [self.add_vert(p, bone) for p in corners]
        quads = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
        for q in quads:
            self.add_face([i[k] for k in q], tag)
        return self

    def cylinder(self, center, radius, height, tag, seg=8, axis="z", bone=None, radius_top=None,
                 cap_bottom=True, cap_top=True, cap_tag=None):
        """Cylinder/cone/frustum centred at `center`. axis in 'x','y','z'."""
        rt = radius if radius_top is None else radius_top
        cap_tag = cap_tag or tag
        ring_b, ring_t = [], []
        for k in range(seg):
            a = 2 * math.pi * k / seg
            ca, sa = math.cos(a), math.sin(a)
            ring_b.append(self._axis_pt(center, axis, radius * ca, radius * sa, -height / 2))
            ring_t.append(self._axis_pt(center, axis, rt * ca, rt * sa, height / 2))
        ib = [self.add_vert(p, bone) for p in ring_b]
        it = [self.add_vert(p, bone) for p in ring_t]
        circ = 2 * math.pi * max(radius, rt)
        for k in range(seg):
            k2 = (k + 1) % seg
            u0, u1 = circ * k / seg, circ * (k + 1) / seg
            self.add_face([ib[k], ib[k2], it[k2], it[k]], tag, [(u0, 0), (u1, 0), (u1, height), (u0, height)])
        if cap_bottom and radius > 1e-6:
            self.add_face(list(reversed(ib)), cap_tag)
        if cap_top and rt > 1e-6:
            self.add_face(list(it), cap_tag)
        return self

    @staticmethod
    def _axis_pt(center, axis, a, b, h):
        cx, cy, cz = center
        if axis == "z":
            return (cx + a, cy + b, cz + h)
        if axis == "x":
            return (cx + h, cy + a, cz + b)
        return (cx + b, cy + h, cz + a)  # 'y' (cyclic permutation keeps winding)

    def sphere(self, center, radius, tag, seg=8, rings=5, bone=None, scale=(1, 1, 1)):
        cx, cy, cz = center
        rows = []
        top = self.add_vert((cx, cy, cz + radius * scale[2]), bone)
        for r in range(1, rings):
            phi = math.pi * r / rings
            row = []
            for k in range(seg):
                th = 2 * math.pi * k / seg
                row.append(self.add_vert((cx + radius * scale[0] * math.sin(phi) * math.cos(th),
                                          cy + radius * scale[1] * math.sin(phi) * math.sin(th),
                                          cz + radius * scale[2] * math.cos(phi)), bone))
            rows.append(row)
        bot = self.add_vert((cx, cy, cz - radius * scale[2]), bone)
        for k in range(seg):
            k2 = (k + 1) % seg
            self.add_face([top, rows[0][k], rows[0][k2]], tag)
        for r in range(len(rows) - 1):
            for k in range(seg):
                k2 = (k + 1) % seg
                self.add_face([rows[r][k], rows[r + 1][k], rows[r + 1][k2], rows[r][k2]], tag)
        for k in range(seg):
            k2 = (k + 1) % seg
            self.add_face([bot, rows[-1][k2], rows[-1][k]], tag)
        return self

    def wedge(self, center, size, tag, bone=None, rot_z=0.0):
        """Triangular prism (ramp): high side at +Y of the local box, slopes down to -Y."""
        cx, cy, cz = center
        sx, sy, sz = size[0] / 2.0, size[1] / 2.0, size[2] / 2.0
        c, s = math.cos(rot_z), math.sin(rot_z)
        loc = [(-sx, -sy, -sz), (sx, -sy, -sz), (sx, sy, -sz), (-sx, sy, -sz), (-sx, sy, sz), (sx, sy, sz)]
        i = [self.add_vert((cx + x * c - y * s, cy + x * s + y * c, cz + z), bone) for x, y, z in loc]
        self.add_face([i[0], i[3], i[2], i[1]], tag)                 # bottom
        self.add_face([i[0], i[1], i[5], i[4]], tag)                 # slope
        self.add_face([i[3], i[4], i[5], i[2]], tag)                 # back
        self.add_face([i[0], i[4], i[3]], tag)                       # left
        self.add_face([i[1], i[2], i[5]], tag)                       # right
        return self

    def quad(self, p0, p1, p2, p3, tag, bone=None):
        i = [self.add_vert(p, bone) for p in (p0, p1, p2, p3)]
        self.add_face(i, tag)
        return self

    def hbar(self, a, b, thickness, tag, bone=None):
        """Square-section bar between two points (pipes, legs, rails)."""
        ax, ay, az = a
        bx, by, bz = b
        d = (bx - ax, by - ay, bz - az)
        L = math.sqrt(d[0] ** 2 + d[1] ** 2 + d[2] ** 2)
        if L < 1e-6:
            return self
        # build local frame
        fwd = (d[0] / L, d[1] / L, d[2] / L)
        up = (0, 0, 1) if abs(fwd[2]) < 0.9 else (1, 0, 0)
        right = _norm(_cross(fwd, up))
        up2 = _norm(_cross(fwd, right))
        h = thickness / 2.0
        pts = []
        for base in (a, b):
            for (r, u) in ((-h, -h), (h, -h), (h, h), (-h, h)):
                pts.append((base[0] + right[0] * r + up2[0] * u, base[1] + right[1] * r + up2[1] * u,
                            base[2] + right[2] * r + up2[2] * u))
        i = [self.add_vert(p, bone) for p in pts]
        for q in [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]:
            self.add_face([i[k] for k in q], tag)
        return self

    # ------------------------------------------------------------------ transforms / queries
    def merge(self, other, offset=(0, 0, 0), rot_z=0.0, bone=None):
        """Append another Geo (same atlas) with optional transform."""
        assert other.atlas == self.atlas
        base = len(self.verts)
        c, s = math.cos(rot_z), math.sin(rot_z)
        for p, b in zip(other.verts, other.vbone):
            self.verts.append((p[0] * c - p[1] * s + offset[0], p[0] * s + p[1] * c + offset[1], p[2] + offset[2]))
            self.vbone.append(bone if bone is not None else b)
        for f, u0, u1, t in zip(other.faces, other.fuv0, other.fuv1, other.ftag):
            self.faces.append(tuple(i + base for i in f))
            self.fuv0.append(list(u0)); self.fuv1.append(list(u1)); self.ftag.append(t)
        return self

    def transform(self, scale=(1, 1, 1), offset=(0, 0, 0), rot_z=0.0):
        c, s = math.cos(rot_z), math.sin(rot_z)
        out = []
        for p in self.verts:
            x, y, z = p[0] * scale[0], p[1] * scale[1], p[2] * scale[2]
            out.append((x * c - y * s + offset[0], x * s + y * c + offset[1], z + offset[2]))
        self.verts = out
        return self

    def mirror_x_copy(self, from_bone=None, to_bone=None):
        """Append a mirrored (x -> -x) copy of everything; reverses winding. Bones with _L<->_R swapped."""
        base = len(self.verts)
        n = len(self.verts)
        for k in range(n):
            p = self.verts[k]
            b = self.vbone[k]
            if b and b.endswith("_L"): b = b[:-2] + "_R"
            elif b and b.endswith("_R"): b = b[:-2] + "_L"
            self.verts.append((-p[0], p[1], p[2])); self.vbone.append(b)
        nf = len(self.faces)
        for k in range(nf):
            self.faces.append(tuple(i + base for i in reversed(self.faces[k])))
            self.fuv0.append(list(reversed(self.fuv0[k]))); self.fuv1.append(list(reversed(self.fuv1[k])))
            self.ftag.append(self.ftag[k])
        return self

    def bounds(self):
        xs = [p[0] for p in self.verts]; ys = [p[1] for p in self.verts]; zs = [p[2] for p in self.verts]
        return (min(xs), min(ys), min(zs)), (max(xs), max(ys), max(zs))

    def tri_count(self):
        return sum(len(f) - 2 for f in self.faces)

    def used_bones(self):
        return sorted({b for b in self.vbone if b})

    def validate(self):
        """Raises AssertionError on corrupt data; returns stats dict."""
        n = len(self.verts)
        assert n > 0 and self.faces, "empty geometry: " + self.name
        for f, u0, u1 in zip(self.faces, self.fuv0, self.fuv1):
            assert len(f) >= 3 and len(set(f)) == len(f), "degenerate face"
            assert all(0 <= i < n for i in f), "bad index"
            assert len(u0) == len(f) and len(u1) == len(f), "uv/loop mismatch"
        if A.ATLASES[self.atlas]["mode"] == "cell":
            for u0 in self.fuv0:
                for (u, v) in u0:
                    assert -1e-6 <= u <= 1 + 1e-6 and -1e-6 <= v <= 1 + 1e-6, "cell uv out of range"
        return dict(verts=n, faces=len(self.faces), tris=self.tri_count())
