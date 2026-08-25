#!/usr/bin/env bash
set -euo pipefail

INPUT="$(cat || true)"
SPEC_ID="${SPEC_ID:-SPEC-1042}"
REPO_ROOT="$(pwd)"
SPEC_FILE="$REPO_ROOT/OrderFlow/specs/${SPEC_ID}-order-discount/${SPEC_ID}-order-discount.md"

if [[ ! -f "$SPEC_FILE" ]]; then
  python3 - <<'PY'
import json
print(json.dumps({
    "decision": "block",
    "reason": "SPEC-ID could not be resolved. Resolve the active SPEC before completing this subagent."
}))
PY
  exit 0
fi

# This hook validates the existence of the active specification.
# Full requirement/test/diff validation can be added once the repository's
# actual build/test commands are confirmed.
python3 - <<'PY'
import json
print(json.dumps({"decision": "allow"}))
PY
