# Vulnerability Categories

Language-agnostic categories to reason through during Step 4 (Vulnerability
Deep Scan), with examples grounded in OrderFlow's actual layers. Use
`dotnet-angular-patterns.md` for concrete syntax-level signals; use this file
for the conceptual checklist.

## 1. Injection
- **SQL/query injection** — string-built EF Core raw SQL, or any future
  Dapper/ADO.NET query built with interpolation.
- **Log injection** — unescaped user input written to logs, enabling log
  forging or breaking downstream log parsing.

## 2. Broken Authentication / Authorization
- Missing or incorrect authorization checks on state-changing endpoints
  (`OrdersController`, `CustomersController`, `ProductsController`).
- Insecure Direct Object Reference (IDOR) — e.g. `GetById(Guid id)` returning
  another customer's order without an ownership check, once auth exists.
- Note: OrderFlow has no auth scheme today — see SKILL.md Ground Rules before
  raising this category as a finding.

## 3. Sensitive Data Exposure
- Secrets committed to `appsettings*.json`, `environment*.ts`, or source.
- Sensitive fields (pricing internals, customer PII) returned in API
  responses beyond what the `Contracts/Responses` DTO should expose.
- Verbose exception details leaking stack traces to the client — check
  `Middleware/ExceptionHandlingMiddleware.cs` for this.

## 4. Security Misconfiguration
- Overly permissive CORS (`AllowAnyOrigin` + `AllowCredentials`).
- Swagger/OpenAPI UI exposed outside `Development` (`Program.cs` currently
  gates it correctly behind `IsDevelopment()` — flag if that check is
  removed).
- Debug/detailed error pages left enabled outside Development.

## 5. Cryptographic Failures
- Weak hashing (`MD5`, `SHA1`) used for anything security-relevant.
- Non-cryptographic RNG (`System.Random`) used for tokens, IDs meant to be
  unguessable, or discount codes.

## 6. Business Logic Flaws
OrderFlow-specific: this is a discount/pricing engine, so logic bugs are a
real security surface even without classic injection.
- Discount rules (`OrderFlow.Domain.Discounts`) applied more than once, or
  applied using client-supplied data instead of server-recalculated values.
- Order total computed client-side and trusted by the API instead of
  recalculated server-side from `Product` prices.
- Negative quantities, zero-price edge cases, or race conditions in stock/
  order-status transitions (e.g. `Confirm` / `Cancel` actions in
  `OrdersController`).

## 7. Vulnerable Dependencies
- See `dependency-watchlist.md`.

## 8. Insufficient Input Validation
- Missing or bypassable `FluentValidation` rules in `OrderFlow.Api/Validation`
  — check that every mutating endpoint's request type actually has a
  registered validator, and that the validator covers the fields that matter
  (quantities, prices, IDs), not just presence checks.

## 9. Client-Side Risks (Angular)
- `[innerHTML]` / `bypassSecurityTrustHtml` — see `dotnet-angular-patterns.md`.
- Sensitive data placed in `environment.ts` (bundled into the client build,
  effectively public).
