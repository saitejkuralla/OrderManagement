# Secret & Credential Detection Patterns

Signals to look for during Step 3 (Secrets & Exposure Scan), scoped to what's
actually relevant in OrderFlow. CI/CD-provider-specific patterns (GitHub
Actions secrets, Docker build args, Terraform state, cloud-provider access
keys) are intentionally omitted — this repo has no CI/CD workflows or IaC at
this time. Revisit this file if those are introduced.

## High-Confidence Signals
- A connection string containing `Password=`, `Pwd=`, or `User Id=` with a
  literal (non-placeholder) value, in `appsettings*.json` or any `.cs` file.
  - **Exception**: OrderFlow's SQLite connection string
    (`Data Source=orderflow.db`) is file-based with no credential — this is
    not a finding.
- API keys / tokens matching common vendor formats (e.g. `sk_live_`, `AKIA`,
  a 40-character hex string assigned to a variable literally named `secret`,
  `apiKey`, `token`, etc.) hardcoded in `.cs` or `.ts` source rather than read
  from configuration/environment.
- A JWT-looking string (`eyJ...`) hardcoded as a constant rather than
  generated/received at runtime.
- Private key material (`-----BEGIN PRIVATE KEY-----` or similar) committed
  to source.

## Where to Look in This Repo
- `backend/OrderFlow.Api/appsettings.json` and `appsettings.Development.json`
- Any `.cs` file under `Infrastructure` (`InfrastructureServiceCollectionExtensions.cs`
  and friends) — connection strings and options binding live here.
- `frontend/orderflow-ui/src/environments/*.ts` — this is bundled into the
  client and publicly visible; anything here that looks like a *backend*
  secret (not a public API base URL) is a finding regardless of how it looks.

## Known-Safe Values (do not flag)
- `Data Source=orderflow.db` and similar file-based SQLite connection strings
  with no `Password=`/`User Id=` component.
- Placeholder-looking values clearly meant to be replaced (`<your-key-here>`,
  `CHANGE_ME`, `TODO`) — note these as an INFO-level reminder, not a secret
  leak.

## Reporting
For each match: file path + line, the redacted value (show only enough to
confirm the pattern, e.g. `sk_live_****`), and why it's a finding. Never print
the full secret value in the report.
