# Report Format

Use this structure for every security review report. Findings only — this
skill never edits files or runs commands; any command shown below is
**illustrative for a human to run manually**, never to be executed by the
assistant.

## Summary Table (always first)

| # | Severity | Category | Location | Title |
|---|----------|----------|----------|-------|
| 1 | CRITICAL | Injection | `OrderFlow.Infrastructure/Repositories/OrderRepository.cs:42` | Raw SQL built with string interpolation |

## Per-Finding Detail

For each finding:

```
### [n] <Title>
Severity: CRITICAL | HIGH | MEDIUM | LOW | INFO
Category: <from vuln-categories.md>
Location: <file>:<line>
Confidence: High | Medium | Low

Description:
<what the issue is and why it matters, in terms of this codebase>

Evidence:
<the relevant code snippet>

Suggested Fix (not applied — for human review):
<before/after snippet, consistent with OrderFlow's existing style/layering>

Manual verification command (illustrative only — do not run automatically):
<e.g. `dotnet list package --vulnerable` or `npm audit>=high`>
```

## Closing Notes
- If no findings: say so plainly. Do not manufacture low-value findings to
  appear thorough.
- If auth-related findings would normally apply but OrderFlow has no auth
  scheme configured, state that once as an architectural note rather than a
  per-endpoint finding (see SKILL.md Ground Rules).
- End every report with: **"No files were modified and no commands were
  executed as part of this review."**
