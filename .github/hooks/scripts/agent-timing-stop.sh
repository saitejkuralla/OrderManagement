#!/usr/bin/env bash
set -euo pipefail

# subagentStop timing hook.
# Appends a real-clock "stop" event for the completed subagent to a
# per-session JSONL log, paired with the corresponding start event.

INPUT="$(cat || true)"
REPO_ROOT="$(pwd)"
LOG_DIR="$REPO_ROOT/.github/copilot-execution"
mkdir -p "$LOG_DIR"

HOOK_INPUT="$INPUT" python3 - "$LOG_DIR" <<'PY'
import datetime
import json
import os
import sys

log_dir = sys.argv[1]
raw = os.environ.get("HOOK_INPUT", "")
try:
    payload = json.loads(raw) if raw.strip() else {}
except Exception:
    payload = {}

session_id = payload.get("session_id") or "unknown-session"
agent_name = "unknown-agent"
for field in ("subagent_type", "subagentType", "agent_type", "agentName", "name"):
    if payload.get(field):
        agent_name = payload[field]
        break

entry = {
    "event": "stop",
    "agent": agent_name,
    "session_id": session_id,
    "timestamp": datetime.datetime.utcnow().isoformat() + "Z",
}

log_file = os.path.join(log_dir, f"{session_id}-timing.jsonl")
with open(log_file, "a") as f:
    f.write(json.dumps(entry) + "\n")

print(json.dumps({"decision": "allow"}))
PY
