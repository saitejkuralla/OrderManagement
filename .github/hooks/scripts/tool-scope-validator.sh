#!/usr/bin/env bash
set -euo pipefail

INPUT="$(cat || true)"

python3 - "$INPUT" <<'PY'
import json, sys, re

raw = sys.argv[1]
try:
    data = json.loads(raw) if raw.strip() else {}
except Exception:
    print(json.dumps({
        "permissionDecision": "deny",
        "permissionDecisionReason": "Hook received malformed JSON input."
    }))
    sys.exit(0)

tool = data.get("toolName", "")
args = data.get("toolArgs", {})

# Read-only tools are not blocked by the scope policy.
read_only = {"view", "grep", "rg", "glob", "web_fetch", "web_search", "ask_user"}
if tool in read_only:
    print(json.dumps({"permissionDecision": "allow"}))
    sys.exit(0)

# Convert tool arguments to text so we can inspect paths/commands.
text = json.dumps(args, ensure_ascii=False)

# Write-capable tool calls.
write_tools = {"create", "edit", "apply_patch", "str_replace_editor"}
if tool not in write_tools:
    # For bash/powershell/task we don't deny automatically here.
    # Command-level safety can be added after observing actual payloads.
    print(json.dumps({"permissionDecision": "allow"}))
    sys.exit(0)

allowed = [
    r"OrderFlow[\\/]backend[\\/]OrderFlow\.Domain[\\/]Entities[\\/]",
    r"OrderFlow[\\/]backend[\\/]OrderFlow\.Domain[\\/]Discounts[\\/]",
    r"OrderFlow[\\/]backend[\\/]OrderFlow\.Application[\\/]",`r`n    r"OrderFlow[\\/]backend[\\/]OrderFlow\.Api[\\/]",`r`n    r"OrderFlow[\\/]backend[\\/]OrderFlow\.Infrastructure[\\/]Environment[/]",
    r"OrderFlow[\\/]tests[\\/]OrderFlow\.UnitTests[/]",
    r"(^|[\\/])OrderFlow[\\/]specs[\\/]"
]

for pattern in allowed:
    if re.search(pattern, text, re.IGNORECASE):
        print(json.dumps({"permissionDecision": "allow"}))
        sys.exit(0)

# If this is a write tool and no allowed path is visible, block.
print(json.dumps({
    "permissionDecision": "deny",
    "permissionDecisionReason":
        "SPEC-1043 scope enforcement blocked this write. "
        "Allowed write scope: OrderFlow/backend/OrderFlow.Domain/Entities/**, "
        "OrderFlow/backend/OrderFlow.Domain/Discounts/**, OrderFlow/backend/OrderFlow.Application/**, "
        "OrderFlow/tests/OrderFlow.UnitTests/**, OrderFlow/specs/**."
}))
PY
