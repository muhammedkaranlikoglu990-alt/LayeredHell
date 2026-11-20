"""Merged builder registry + build_geo(entry)."""
import importlib

from .builders_common import apply_scale_static

MODULES = ["builders_arch", "builders_res", "builders_office", "builders_mall", "builders_med", "builders_bar",
           "builders_craft", "builders_weapons", "builders_chars", "builders_loot", "builders_city", "builders_lab"]

_loaded = {}
BUILDERS = {}
MISSING = []


def load_modules():
    if _loaded:
        return
    for m in MODULES:
        try:
            mod = importlib.import_module("." + m, __package__)
        except ModuleNotFoundError as e:
            if e.name and e.name.endswith(m):
                MISSING.append(m)
                continue
            raise
        _loaded[m] = mod
        for k, fn in getattr(mod, "BUILDERS", {}).items():
            assert k not in BUILDERS, "duplicate builder name %s (module %s)" % (k, m)
            BUILDERS[k] = fn


def modules():
    load_modules()
    return _loaded


def build_geo(entry):
    """Entry -> Geo. Injects _id/_scale into params; bakes entry.scale for static (non-rigged) entries."""
    load_modules()
    fn = BUILDERS[entry.builder]
    params = dict(entry.params)
    params["_id"] = entry.id
    params["_scale"] = entry.scale
    g = fn(params, entry.palette, entry.atlas)
    g.name = entry.id
    if entry.rig == "none":
        apply_scale_static(g, entry.scale)
    return g
