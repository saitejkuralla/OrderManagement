$ErrorActionPreference = "Stop"

$inputJson = [Console]::In.ReadToEnd()
$specId = if ($env:SPEC_ID) { $env:SPEC_ID } else { "SPEC-1042" }

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../../..")).Path
$specFile = Join-Path $repoRoot "OrderFlow/specs/$specId-order-discount/$specId-order-discount.md"

if (-not (Test-Path $specFile)) {
    @{
        additionalContext = "SPEC-ID $specId could not be resolved. Do not modify files until the SPEC-ID is resolved."
    } | ConvertTo-Json -Compress
    exit 0
}

$content = Get-Content -Raw -Path $specFile

@{
    additionalContext = @"
ACTIVE SPECIFICATION: $specId

The following specification is the governing contract for this task.
Respect its requirements, allowed scope, forbidden scope and completion criteria.

$content
"@
} | ConvertTo-Json -Compress
