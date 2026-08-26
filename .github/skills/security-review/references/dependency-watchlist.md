# Dependency Watchlist

OrderFlow has two dependency ecosystems: NuGet (backend) and npm (Angular
frontend). No Python/Java/PHP/Ruby/Rust/Go package ecosystems apply — those
sections from the original skill are intentionally omitted.

## How to Use
1. Read `**/*.csproj` for `<PackageReference>` versions and
   `frontend/orderflow-ui/package.json` / `package-lock.json` for npm
   versions.
2. Only flag a package if you have high confidence of a real, known issue for
   that specific version (e.g. from training knowledge of a documented CVE).
   Do not invent version numbers or CVE IDs. If unsure, report it as "worth a
   manual `dotnet list package --vulnerable` / `npm audit` check" rather than
   asserting a specific vulnerability.
3. This skill does not run `dotnet list package --vulnerable` or `npm audit`
   itself (no command execution) — recommend the human run it.

## NuGet — Things Worth Checking
- Any package pinned far below its current major version, especially
  `Microsoft.EntityFrameworkCore.*` (data-access surface) or
  `FluentValidation*` (input-validation surface).
- Transitive packages pulled in `obj/project.assets.json` that are much older
  than the direct reference — can indicate a floating/unintended downgrade.
- Preview/RC packages used in what should be a stable dependency chain.

## npm — Things Worth Checking
- Angular core packages (`@angular/*`) out of sync with each other (mixed
  major versions is itself a maintenance/security risk, not just a build
  issue).
- Any dependency with a known prototype-pollution or ReDoS advisory at the
  locked version in `package-lock.json`.
- Dev-only tooling accidentally listed under `dependencies` instead of
  `devDependencies` (increases production attack surface unnecessarily).

## Reporting Format for This Section
When flagging a dependency, state: package name, version found, why it's
suspect, and — if genuinely known — the fixed version to upgrade to. Never
state a CVE ID unless you are confident it is correct; prefer "has a known
issue in this version range" if uncertain.
