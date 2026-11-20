"""CATALOG: list[Entry] gathered from every builders_<domain>.entries()."""
from .builders_common import Entry, mk, CATEGORIES, BUDGET_DEFAULT  # noqa: F401 (re-exported)
from . import builders

CATALOG = []
_seen = set()
for _name, _mod in builders.modules().items():
    for _e in _mod.entries():
        assert _e.id not in _seen, "duplicate catalogue id " + _e.id
        _seen.add(_e.id)
        CATALOG.append(_e)
CATALOG.sort(key=lambda e: (CATEGORIES.index(e.category), e.id))
BY_ID = {e.id: e for e in CATALOG}
