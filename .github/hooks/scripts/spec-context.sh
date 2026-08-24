#!/usr/bin/env bash
set -euo pipefail

INPUT="$(cat || true)"
SPEC_ID="${SPEC_ID:-SPEC-1042}"
REPO_ROOT="$(cd ../.. && pwd)"
SPEC_DIR="$REPO_ROOT/../../docs/specs"
SPEC_FILE="$SPEC_DIR/${SPEC_ID}-order-discount.md"

if [[ ! -f "$SPEC_FILE" ]]; then
  echo "SPEC resolution failed: $SPEC_ID" >&2
  printf '{"additionalContext":"SPEC-ID %s could not be resolved. Do not modify files until the SPEC-ID is resolved."}\n' "$SPEC_ID"
  exit 0
fi

SPEC_CONTENT="$(cat "$SPEC_FILE")"

python3 - "$SPEC_ID" "$SPEC_CONTENT" <<'PY'
import json
import sys

spec_id = sys.argv[1]
content = sys.argv[2]

print(json.dumps({
    "additionalContext":
        f"ACTIVE SPECIFICATION: {spec_id}\n\n"
        "The following specification is the governing contract for this task. "
        "Respect its requirements, allowed scope, forbidden scope and completion criteria.\n\n"
        + content
}))
PY
