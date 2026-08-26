# .NET 8 / ASP.NET Core / Angular Detection Patterns

Stack-specific patterns for OrderFlow. Load during Step 1/4 of the workflow.
Only the ecosystems OrderFlow actually uses are covered — no Python, Java,
PHP, Go, Ruby, or Rust content (out of scope for this repo).

---

## ASP.NET Core / EF Core (backend: `OrderFlow.Api`, `.Application`, `.Infrastructure`)

### SQL Injection
```csharp
// VULNERABLE — raw SQL with string interpolation
var orders = dbContext.Database
    .SqlQueryRaw<Order>($"SELECT * FROM Orders WHERE CustomerId = '{customerId}'");

// SAFE — parameterized
var orders = dbContext.Database
    .SqlQueryRaw<Order>("SELECT * FROM Orders WHERE CustomerId = {0}", customerId);

// SAFEST — LINQ (what OrderFlow's repositories already use)
var orders = await dbContext.Orders
    .Where(o => o.CustomerId == customerId)
    .ToListAsync(cancellationToken);
```
Flag any `FromSqlRaw`, `ExecuteSqlRaw`, or `SqlQueryRaw` call that concatenates
or interpolates a variable directly into the query text. OrderFlow's existing
repositories use LINQ exclusively — a raw-SQL call appearing in a diff is
itself worth a second look even before checking for interpolation.

### Missing Authorization
OrderFlow currently has no authentication scheme configured (see SKILL.md
Ground Rules) — do not flag this repeatedly. If authentication is added later,
apply the normal check: every controller action that mutates state should
have `[Authorize]` (or an explicit, commented reason why not) once a scheme
exists.

### Mass Assignment / Over-Posting
```csharp
// VULNERABLE — binding the full entity from the request body
[HttpPut("{id:guid}")]
public async Task<IActionResult> Update(Guid id, Customer customer) { ... }

// SAFE — bind to a request DTO and map only allowed fields (OrderFlow's existing pattern)
[HttpPut("{id:guid}")]
public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request) { ... }
```
OrderFlow already uses dedicated `Contracts/Requests` DTOs — flag any new
endpoint that binds a domain entity or Application command directly from the
HTTP body instead.

### Deserialization
```csharp
// VULNERABLE
var obj = new BinaryFormatter().Deserialize(stream); // BinaryFormatter is obsolete/unsafe

// SAFE
var obj = JsonSerializer.Deserialize<T>(json);
```

### XML Processing (XXE)
```csharp
// VULNERABLE — external entities enabled
var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };

// SAFE (default in modern .NET, but verify if settings are overridden)
var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
```

### Command Injection
```csharp
// VULNERABLE
Process.Start("cmd.exe", $"/c {userInput}");

// SAFE — avoid shell invocation; use argument arrays and an allowlist if a
// process must be started at all
```
OrderFlow has no legitimate reason to shell out — any `Process.Start`/`Process.Run`
call touching user input is a high-severity finding.

### Weak Cryptography / Randomness
```csharp
// VULNERABLE
var hash = MD5.HashData(passwordBytes);
var token = new Random().Next().ToString();

// SAFE
var token = RandomNumberGenerator.GetBytes(32);
// Passwords: use ASP.NET Core Identity's PasswordHasher (PBKDF2) or Argon2/BCrypt
```

### Secrets in Configuration
```jsonc
// VULNERABLE — real credential committed
{ "ConnectionStrings": { "OrderFlow": "Data Source=prod.db;Password=hunter2" } }

// SAFE — no embedded credential (OrderFlow's current SQLite connection string
// has none) or sourced from environment/user-secrets/Key Vault
```

### Logging
```csharp
// VULNERABLE
_logger.LogInformation("Created order for {Email} with card {Card}", email, cardNumber);

// SAFE — log identifiers, not sensitive payloads
_logger.LogInformation("Created order {OrderId} for customer {CustomerId}", order.Id, customerId);
```

### CORS
```csharp
// VULNERABLE — wide open, would allow any origin to call the API with credentials
policy.AllowAnyOrigin().AllowCredentials();

// SAFE — OrderFlow's existing pattern: explicit origin allowlist
policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
```
Flag any change that widens the existing `AngularDevCorsPolicy` beyond an
explicit origin list, especially combined with `AllowCredentials()`.

---

## Angular / TypeScript (frontend: `frontend/orderflow-ui`)

### Unsafe HTML Rendering
```typescript
// VULNERABLE
this.sanitizer.bypassSecurityTrustHtml(userSuppliedContent);
```
```html
<!-- VULNERABLE -->
<div [innerHTML]="userSuppliedContent"></div>
```
```typescript
// SAFE — Angular's default interpolation auto-escapes
// {{ userSuppliedContent }}
```
Flag any `bypassSecurityTrust*` call or `[innerHTML]` binding fed by data that
ultimately came from an API response or user input, unless sanitized first
(e.g. with DOMPurify).

### HTTP Client Usage
```typescript
// VULNERABLE — building URLs by string concatenation with user input
this.http.get(`${this.baseUrl}/` + userInput);

// SAFE — OrderFlow's existing pattern: typed path segments, no raw concatenation of untrusted input
this.http.get<Order>(`${this.baseUrl}/${id}`);
```
This is a low-severity note for OrderFlow specifically, since Angular's
`HttpClient` does not execute the URL as a query — the real risk surface is
server-side (see EF Core section above), but still confirm IDs are validated
as GUIDs/expected shape before use.

### Secrets in Frontend Config
```typescript
// VULNERABLE — a backend secret bundled into the client build
export const environment = { apiUrl: '...', stripeSecretKey: 'sk_live_...' };

// SAFE — only public, non-sensitive config (OrderFlow's current environment.ts pattern)
export const environment = { apiUrl: 'http://localhost:5048/api' };
```

### Auth Token Storage (if authentication is added later)
```typescript
// AVOID
localStorage.setItem('token', jwt);

// PREFER
// httpOnly cookie set by the server, or in-memory storage with silent refresh
```

---

## Notes
- OrderFlow has no CI/CD workflows or Dockerfiles at this time — GitHub
  Actions / Docker / Terraform secret patterns are intentionally omitted from
  this reference. Revisit if CI/CD is introduced.
- xUnit and Moq are test-only tooling; they are not a security review surface
  and are intentionally not covered here.
