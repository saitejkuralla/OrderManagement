$ErrorActionPreference = "Stop"

$inputJson = [Console]::In.ReadToEnd()

try {
    $data = $inputJson | ConvertFrom-Json
} catch {
    @{ 
        permissionDecision = "deny"
        permissionDecisionReason = "Hook received malformed JSON input."
    } | ConvertTo-Json -Compress
    exit 0
}

$tool = [string]$data.toolName
$argsText = if ($null -ne $data.toolArgs) {
    $data.toolArgs | ConvertTo-Json -Compress -Depth 20
} else {
    ""
}

$readOnly = @("view", "grep", "rg", "glob", "web_fetch", "web_search", "ask_user")
if ($readOnly -contains $tool) {
    @{ permissionDecision = "allow" } | ConvertTo-Json -Compress
    exit 0
}

$writeTools = @("create", "edit", "apply_patch", "str_replace_editor")
if ($writeTools -notcontains $tool) {
    # Bash/powershell/task are not automatically denied in this first version.
    @{ permissionDecision = "allow" } | ConvertTo-Json -Compress
    exit 0
}

$allowed = @(
    "(^|[\/])OrderFlow[\/]backend[\/]OrderFlow\.Domain[\/]Entities[\/]",
    "(^|[\/])OrderFlow[\/]backend[\/]OrderFlow\.Domain[\/]Discounts[\/]",
    "(^|[\/])OrderFlow[\/]backend[\/]OrderFlow\.Application[\/]",
    "(^|[\/])OrderFlow[\/]tests[\/]OrderFlow\.UnitTests[\/]",
    "(^|[\/])OrderFlow[\/]specs[\/]"
)

foreach ($pattern in $allowed) {
    if ($argsText -match $pattern) {
        @{ permissionDecision = "allow" } | ConvertTo-Json -Compress
        exit 0
    }
}

@{
    permissionDecision = "deny"
    permissionDecisionReason = "SPEC-1042 scope enforcement blocked this write. Allowed write scope: OrderFlow/backend/OrderFlow.Domain/Entities/**, OrderFlow/backend/OrderFlow.Domain/Discounts/**, OrderFlow/backend/OrderFlow.Application/**, OrderFlow/tests/OrderFlow.UnitTests/**, OrderFlow/specs/**."
} | ConvertTo-Json -Compress
