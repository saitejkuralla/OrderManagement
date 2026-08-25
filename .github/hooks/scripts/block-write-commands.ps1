<#
  PreToolUse guardrail for read-only agents (testing-agent, security-agent).
  Denies shell tool calls whose command text matches a known write/destructive
  pattern (file writes, deletes, git commit/push, DB mutation, etc.), as a
  technical backstop to the "read-only" instructions in those agents' prompts.
#>

$ErrorActionPreference = 'Stop'

$rawInput = [Console]::In.ReadToEnd()

$decision = 'allow'
$reason = 'Read-only policy check passed.'

try {
    $payload = $rawInput | ConvertFrom-Json
} catch {
    $payload = $null
}

if ($null -ne $payload) {
    $toolName = [string]$payload.tool_name

    # Only inspect shell/terminal-style tool invocations; leave read/search/edit tools alone.
    if ($toolName -match '(?i)terminal|execute|shell|process|command') {
        $toolInputText = ''
        if ($payload.tool_input) {
            $toolInputText = ($payload.tool_input | ConvertTo-Json -Depth 10 -Compress)
        }

        $deniedPatterns = @(
            'rm\s+-rf',
            'Remove-Item',
            'del\s+/[sf]',
            'git\s+commit',
            'git\s+push',
            'git\s+add',
            'git\s+reset\s+--hard',
            'DROP\s+(TABLE|DATABASE)',
            'TRUNCATE\s+TABLE',
            'DELETE\s+FROM',
            'Set-Content',
            'Add-Content',
            'Out-File',
            'New-Item',
            'Move-Item',
            '>\s*\S',
            'mkfs',
            'dd\s+if=',
            'shutdown',
            'Restart-Computer'
        )

        foreach ($pattern in $deniedPatterns) {
            if ($toolInputText -match $pattern) {
                $decision = 'deny'
                $reason = "Blocked by read-only agent policy: command matches disallowed pattern '$pattern'."
                break
            }
        }
    }
}

$output = @{
    hookSpecificOutput = @{
        hookEventName            = 'PreToolUse'
        permissionDecision       = $decision
        permissionDecisionReason = $reason
    }
}

$output | ConvertTo-Json -Depth 10
exit 0
