#!/usr/bin/env python3
"""Builds EVERY catalogue entry and checks the contract. Writes Tools/Blender/catalog_manifest.json.

usage: python3 Tools/Verify/verify_catalog.py [--prefix zmb_] [--module builders_chars] [--quiet] [--no-manifest]
Filtered runs (--prefix/--module) skip the global count / prefix coverage checks and do not write the manifest.
Exit code 0 == all good.
"""
import json
import math
import os
import sys
import time
import traceback

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "Blender"))

from ll_blender import atlas as A            # noqa: E402
from ll_blender import rig_spec as R         # noqa: E402
from ll_blender import builders              # noqa: E402
from ll_blender.builders_common import BUDGET_DEFAULT, CATEGORIES, ANIM_STYLES  # noqa: E402

PREFIXES = """arch_wall_ arch_wallbroken_ arch_floor_ arch_ceiling_ arch_door_ arch_window_ arch_pillar_ arch_stair_
arch_elevator_ arch_vent_ res_bed_ res_sofa_ res_table_ res_chair_ res_wardrobe_ res_shelf_ res_tv_ res_fridge_
res_stove_ res_sink_ res_toilet_ res_bath_ res_lamp_ res_rug_ off_desk_ off_chair_ off_partition_ off_monitor_
off_cabinet_ off_printer_ off_whiteboard_ off_cooler_ mall_shelf_ mall_counter_ mall_mannequin_ mall_shopfront_
mall_cart_ mall_kiosk_ mall_atm_ mall_escalator_ med_bed_ med_ivstand_ med_monitor_ med_cabinet_ med_wheelchair_
med_lab_ gym_bench_ gym_rack_ gym_treadmill_ gym_weights_ bar_planks_ bar_sandbag_ bar_debris_ bar_crate_
bar_concrete_ bar_steelplate_ bar_wreck_ craft_workbench_ craft_weaponbench_ craft_chem_ craft_medstation_
craft_purifier_ craft_stove_ craft_generator_ craft_radio_ craft_storage_ craft_bed_ wpn_att_ wpn_mag_ wpn_ammo_
chr_npc_ zmb_walker_ zmb_runner_ zmb_spitter_ zmb_armored_ zmb_goliath_ city_building_ city_lamp_ city_tree_
veh_car_ lab_pod_ lab_tank_ lab_server_ lab_cage_ lab_pipe_ exe_chandelier_ exe_banquet_ exe_desk_ exe_safe_
exe_helipad_ exe_radar_""".split()
WEAPON_IDS = "pistol revolver shotgun smg rifle sniper pipe bat axe machete grenade molotov".split()
REQUIRED_IDS = (["chr_fps_arms_01", "chr_aras_01", "chr_selin_01", "chr_kemal_01", "chr_elif_01", "chr_murat_01",
                 "chr_nehir_01", "chr_tarik_01", "veh_armored_response_01", "veh_helicopter_01"]
                + ["wpn_%s_01" % w for w in WEAPON_IDS]
                + ["loot_%s_01" % n for n in "canned_beans canned_tomato canned_tuna water_bottle book outlet battery "
                   "key medkit bandage scrap_metal duct_tape journal radio".split()])
ATLAS_RULE = {
    "Weapons": {"HERO_WEAPONS_1K"},
    "Zombies": {"HERO_ZOMBIE_1K"},
    "Characters": {"HERO_SURVIVOR_1K"},
    "Loot": {"PROP_A_512", "PROP_B_512", "HERO_ITEMS_1K"},
    "Vehicles": {"ENV_CITY_2K", "HERO_ITEMS_1K"},
}
HERO_OK_ENV = {"Crafting", "Lab", "Executive", "Medical", "Residential", "Office", "Mall", "Gym", "Barricades"}


def main():
    args = sys.argv[1:]
    quiet = "--quiet" in args
    prefix = args[args.index("--prefix") + 1] if "--prefix" in args else None
    module = args[args.index("--module") + 1] if "--module" in args else None
    filtered = bool(prefix or module)
    errors, warns = [], []

    builders.load_modules()
    if builders.MISSING:
        (warns if filtered else errors).append("missing builder modules: %s" % builders.MISSING)
    entries = []
    seen = {}
    for mname, mod in builders.modules().items():
        if module and mname != module:
            continue
        for e in mod.entries():
            if prefix and not e.id.startswith(prefix):
                continue
            if e.id in seen:
                errors.append("duplicate id %s (%s & %s)" % (e.id, seen[e.id], mname))
            seen[e.id] = mname
            entries.append(e)
    if not filtered:
        # cross-check with catalog.py
        from ll_blender import catalog
        if len(catalog.CATALOG) != len(entries):
            errors.append("catalog.CATALOG size %d != collected %d" % (len(catalog.CATALOG), len(entries)))

    t0 = time.time()
    stats = {}
    manifest = []
    rows = []
    for e in entries:
        try:
            assert e.id == e.id.lower() and e.id.replace("_", "").isalnum(), "id must be lowercase [a-z0-9_]"
            assert e.category in CATEGORIES, "bad category"
            assert e.atlas in A.ATLASES, "bad atlas"
            assert e.anim_style in ANIM_STYLES, "bad anim_style"
            assert e.builder in builders.BUILDERS, "unknown builder " + e.builder
            assert e.rig in ("none", "humanoid", "fps_arms"), "bad rig"
            assert e.tri_budget <= BUDGET_DEFAULT[e.category] or (e.rig == "fps_arms" and e.tri_budget <= 3000), \
                "tri_budget %d over class cap %d" % (e.tri_budget, BUDGET_DEFAULT[e.category])
            rule = ATLAS_RULE.get(e.category)
            if rule:
                assert e.atlas in rule, "atlas %s not allowed for %s (%s)" % (e.atlas, e.category, sorted(rule))
            elif e.category in HERO_OK_ENV:
                assert e.atlas.startswith("ENV_") or e.atlas == "HERO_ITEMS_1K", "bad atlas tier"
            else:
                assert e.atlas.startswith("ENV_"), "env category needs ENV atlas"
            for role, tag in e.palette.items():
                assert tag in A.ATLASES[e.atlas]["tiles"], "palette tag %s not in %s" % (tag, e.atlas)
            g = builders.build_geo(e)
            st = g.validate()
            assert g.atlas == e.atlas, "geo atlas != entry atlas"
            tris = st["tris"]
            assert tris <= e.tri_budget, "tris %d > budget %d" % (tris, e.tri_budget)
            for t in set(g.ftag):
                assert t in A.ATLASES[g.atlas]["tiles"], "tag " + t
            lo, hi = g.bounds()
            for k in range(3):
                assert math.isfinite(lo[k]) and math.isfinite(hi[k]), "non-finite bounds"
            assert max(hi[k] - lo[k] for k in range(3)) < 120, "model absurdly large"
            if e.rig == "none":
                assert not g.used_bones(), "static model has bones"
                assert lo[2] > -0.06, "model below floor (%.3f)" % lo[2]
            elif e.rig == "humanoid":
                assert all(b for b in g.vbone), "unskinned vertex in humanoid"
                bad = [b for b in g.used_bones() if b not in R.HUMANOID_BONES]
                assert not bad, "bad bones %s" % bad
                assert len(g.used_bones()) >= 14, "too few bones used"
                assert 1.0 * e.scale < hi[2] - lo[2] < 2.4 * max(1.0, e.scale), "odd humanoid height %.2f" % (hi[2] - lo[2])
                assert lo[2] > -0.1, "humanoid below floor"
                assert e.anim_style != "none" and e.anim_style != "fps", "humanoid needs an anim_style"
            else:
                assert all(b for b in g.vbone), "unskinned vertex in arms"
                bad = [b for b in g.used_bones() if b not in R.FPS_BONES]
                assert not bad, "bad fps bones %s" % bad
                assert e.anim_style == "fps", "fps arms need anim_style fps"
            mode = A.ATLASES[g.atlas]["mode"]
            for u0, u1 in zip(g.fuv0, g.fuv1):
                for (u, v) in u0:
                    assert math.isfinite(u) and math.isfinite(v), "uv non-finite"
                    if mode == "tile":
                        assert abs(u) < 64 and abs(v) < 64, "uv0 tile coords too large"
                for (u, v) in u1:
                    assert 0 <= u <= 1 and 0 <= v <= 1, "uv1 out of range"
            # degenerate (zero area) faces
            zero = 0
            for f in g.faces:
                p = [g.verts[i] for i in f]
                ar = 0.0
                for k in range(1, len(p) - 1):
                    a, b, c = p[0], p[k], p[k + 1]
                    ux, uy, uz = b[0] - a[0], b[1] - a[1], b[2] - a[2]
                    vx, vy, vz = c[0] - a[0], c[1] - a[1], c[2] - a[2]
                    ar += math.sqrt((uy * vz - uz * vy) ** 2 + (uz * vx - ux * vz) ** 2 + (ux * vy - uy * vx) ** 2) / 2
                if ar < 1e-9:
                    zero += 1
            if zero > max(2, len(g.faces) // 10):
                warns.append("%s: %d zero-area faces" % (e.id, zero))
            clips = (list(R.HUMANOID_CLIPS.keys()) if e.rig == "humanoid"
                     else list(R.FPS_CLIPS.keys()) if e.rig == "fps_arms" else [])
            manifest.append(dict(id=e.id, category=e.category, atlas=e.atlas, material=A.material_name(e.atlas),
                                 rig=e.rig, scale=e.scale, anim_style=e.anim_style, tris=tris, verts=st["verts"],
                                 tri_budget=e.tri_budget, bounds=[[round(x, 3) for x in lo], [round(x, 3) for x in hi]],
                                 clips=clips, bones=g.used_bones()))
            s = stats.setdefault(e.category, dict(n=0, tmin=10 ** 9, tmax=0, tsum=0, budget=0))
            s["n"] += 1
            s["tmin"] = min(s["tmin"], tris)
            s["tmax"] = max(s["tmax"], tris)
            s["tsum"] += tris
            s["budget"] = max(s["budget"], e.tri_budget)
        except Exception as ex:  # noqa: BLE001
            msg = "%s: %s: %s" % (e.id, type(ex).__name__, ex)
            errors.append(msg)
            if not quiet:
                tb = traceback.format_exc().strip().splitlines()
                if not isinstance(ex, AssertionError):
                    msg += " | " + tb[-3].strip()
    dt = time.time() - t0

    ids = set(seen)
    if not filtered:
        if len(entries) < 840:
            errors.append("entry count %d < 840" % len(entries))
        for p in PREFIXES:
            if not any(i.startswith(p) for i in ids):
                errors.append("missing prefix " + p)
        for p in ["arch_wall_", "arch_wallbroken_", "arch_floor_", "arch_ceiling_", "arch_door_", "arch_window_"]:
            pass
        for r in REQUIRED_IDS:
            if r not in ids:
                errors.append("missing required id " + r)
        for w in WEAPON_IDS:
            if not any(i.startswith("wpn_%s_" % w) for i in ids):
                errors.append("missing weapon " + w)
        for a in "walker runner spitter armored".split():
            n = sum(1 for i in ids if i.startswith("zmb_%s_" % a))
            if n < 5:
                errors.append("zmb_%s variants %d < 5" % (a, n))
        if not any(i.startswith("zmb_goliath_") for i in ids):
            errors.append("no goliath")
        for i in ids:
            if not i[-2:].isdigit() or i[-3] != "_":
                errors.append("id does not end with _NN: " + i)
        # every atlas tag used at least by some model (info only)
        used = {}
        for m in manifest:
            used.setdefault(m["atlas"], 0)
            used[m["atlas"]] += 1
        for a in A.ATLASES:
            if a not in used:
                warns.append("atlas %s unused" % a)

    # table
    print("%-13s %5s %7s %7s %7s %7s" % ("category", "n", "triMin", "triAvg", "triMax", "budget"))
    for c in CATEGORIES:
        s = stats.get(c)
        if s:
            print("%-13s %5d %7d %7d %7d %7d" % (c, s["n"], s["tmin"], s["tsum"] // s["n"], s["tmax"], s["budget"]))
    print("TOTAL entries: %d   built ok: %d   (%.1fs)" % (len(entries), len(manifest), dt))
    per_mod = {}
    for i, m in seen.items():
        per_mod[m] = per_mod.get(m, 0) + 1
    print("per module:", ", ".join("%s=%d" % kv for kv in sorted(per_mod.items())))
    for w in warns[:30]:
        print("WARN", w)
    for er in errors[:80]:
        print("ERROR", er)
    if len(errors) > 80:
        print("... %d more errors" % (len(errors) - 80))

    if not filtered and not errors:
        out = os.path.join(HERE, "..", "Blender", "catalog_manifest.json")
        with open(out, "w") as f:
            json.dump(dict(count=len(manifest), models=manifest), f, indent=1)
        print("wrote", os.path.normpath(out))
    print("RESULT:", "PASS" if not errors else "FAIL (%d errors)" % len(errors))
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
