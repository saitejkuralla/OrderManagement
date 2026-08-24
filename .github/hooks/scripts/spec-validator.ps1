$ErrorActionPreference = "Stop"

$inputJson = [Console]::In.ReadToEnd()
$specId = if ($env:SPEC_ID) { $env:SPEC_ID } else { "SPEC-1042" }

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../../..")).Path
$specFile = Join-Path $repoRoot "docs/specs/$specId-order-discount.md"

if (-not (Test-Path $specFile)) {
    @{
        decision = "block"
        reason = "SPEC-ID could not be resolved. Resolve the active SPEC before completing this subagent."
    } | ConvertTo-Json -Compress
    exit 0
}

@{ decision = "allow" } | ConvertTo-Json -Compress
