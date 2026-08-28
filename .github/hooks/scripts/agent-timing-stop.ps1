<#
  subagentStop timing hook.
  Appends a real-clock "stop" event for the completed subagent to a
  per-session JSONL log, paired with the corresponding start event.
#>

$ErrorActionPreference = "Stop"

$rawInput = [Console]::In.ReadToEnd()

try {
    $payload = $rawInput | ConvertFrom-Json
} catch {
    $payload = $null
}

$sessionId = "unknown-session"
if ($payload -and $payload.session_id) { $sessionId = [string]$payload.session_id }

$agentName = "unknown-agent"
foreach ($field in @("subagent_type", "subagentType", "agent_type", "agentName", "name")) {
    if ($payload -and $payload.$field) {
        $agentName = [string]$payload.$field
        break
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../../..")).Path
$logDir = Join-Path $repoRoot ".github/copilot-execution"
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}
$logFile = Join-Path $logDir "$sessionId-timing.jsonl"

$entry = @{
    event      = "stop"
    agent      = $agentName
    session_id = $sessionId
    timestamp  = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json -Compress

Add-Content -Path $logFile -Value $entry

@{ decision = "allow" } | ConvertTo-Json -Compress
