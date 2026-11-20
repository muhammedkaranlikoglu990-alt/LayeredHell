#!/usr/bin/env bash
# 45: Layered Hell - Headless Blender 4.2+ batch model & animation exporter
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
OUT_DIR="${1:-$PROJECT_ROOT/Assets/Art/Models}"
FORMAT="${2:-fbx}"
BLENDER_BIN="${BLENDER:-blender}"

if ! command -v "$BLENDER_BIN" >/dev/null 2>&1; then
    echo "[ERROR] Blender binary ('$BLENDER_BIN') not found in PATH. Set BLENDER=/path/to/blender." >&2
    exit 1
fi

echo "[45:Layered Hell] Running Blender pipeline with $BLENDER_BIN -> $OUT_DIR (format=$FORMAT)"
"$BLENDER_BIN" -b -P "$SCRIPT_DIR/run_blender.py" -- --out "$OUT_DIR" --format "$FORMAT"
