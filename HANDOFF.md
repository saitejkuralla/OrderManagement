# OrderFlow — Session Handoff

Last updated: 2026-08-14 (post frontend build-out). This project is now **feature-complete and verified end-to-end**. This file is kept for historical/reference context.

## Status: DONE
- Backend: builds clean, 29/29 tests passing (19 unit + 10 integration).
- Frontend: `ng build` succeeds, Karma unit tests pass, app shell + all 6 screens (Dashboard, Customers, Products, Order List/Create/Detail) implemented and manually verified end-to-end in a live browser session against the running API (seeded data displayed correctly, order created with correct 15% VIP+large-order discount, confirm flow verified, status-based button visibility verified).
- `OrderFlow/README.md` written (architecture diagram, run commands, endpoints, seed data, discount rules, assumptions).
- Note: a couple of shared components (`tier-chip`, `empty-state`, `confirm-dialog`) already existed on disk with inline templates from earlier work not captured in prior summaries — reused as-is rather than duplicated; one pre-existing bug (`tier-chip` missing `CommonModule` for `ngClass`) was fixed during this session.

## Goal
Build a full-stack "OrderFlow" sample app per a detailed spec: .NET 8 Clean Architecture backend + Angular Material frontend, with seed data, tests, and a spec doc. No Copilot customization files should be added yet (that's a separate later phase). Actually build, build/run/test-verify everything — don't just scaffold.

Workspace root: `C:\Users\Saitej_Kuralla\Desktop\Working Project\OrderManagement`
App root: `OrderFlow\` (contains `backend\`, `frontend\`, `tests\`, `specs\`, `OrderFlow.slnx`, `README.md`)

## Backend — ✅ COMPLETE & VERIFIED (do not redo)
- Clean Architecture: `OrderFlow.Domain`, `OrderFlow.Application`, `OrderFlow.Infrastructure`, `OrderFlow.Api`.
- Build with **`dotnet build OrderFlow.slnx`** from `OrderFlow\` (SDK 10.0.302 creates `.slnx`, not `.sln` — always use `.slnx` in commands).
- EF Core 8 + SQLite, deterministic seeded data (4 customers: Alice/Standard, Bob/Silver, Charlie/Gold, David/VIP; 4 products: Laptop ₹80k, Monitor ₹25k, Keyboard ₹5k, Mouse ₹2k, all active).
- Discount engine: `IDiscountRule` extensibility, `CustomerTierDiscountRule` (Standard 0%/Silver 5%/Gold 10%/VIP 10%), `LargeOrderDiscountRule` (+5% if subtotal > ₹10,000), capped combined at 20% via `DiscountCalculator.MaxDiscountPercentage`. Documented example: Subtotal 110,000 + VIP → 15% discount → ₹93,500 final total.
- FluentValidation validators live in Api layer only (validate Request DTOs, not Application commands). Controllers manually build `ModelStateDictionary` from `ValidationResult.Errors` (no `ToModelStateDictionary()` extension — it doesn't exist in the installed package version).
- Global `ExceptionHandlingMiddleware`: `NotFoundException`→404, `BusinessRuleViolationException`→400.
- Endpoints: `/api/customers` (GET/POST, GET/{id}), `/api/products` (GET/POST, GET/{id}), `/api/orders` (GET/POST, GET/{id}, POST/{id}/confirm, POST/{id}/cancel).
- `Program.cs`: CORS policy `AngularDevCorsPolicy` allows `http://localhost:4200`; `EnsureCreated()` wrapped in try/catch ignoring "already exists" (fixes a `WebApplicationFactory` host-rebuild race in integration tests); `public partial class Program {}` for test access.
- Default `http` launch profile → `http://localhost:5048` (Swagger at `/swagger` in Development).
- Tests: 19 unit tests (`tests/OrderFlow.UnitTests`) + 10 integration tests (`tests/OrderFlow.IntegrationTests`) — **all 29 passing** via `dotnet test OrderFlow.slnx`.
- `specs/001-order-discount/spec.md` — fully written (business rules, functional/non-functional requirements, acceptance criteria).

## Frontend — 🚧 IN PROGRESS (verified ground truth as of this handoff)
Location: `OrderFlow\frontend\orderflow-ui\` (Angular CLI 18, standalone components, no SSR, SCSS).

### Done and confirmed on disk:
- `ng new` + `npm install` completed successfully.
- Angular Material installed (`@angular/material`, `@angular/cdk`, `@angular/animations` in `package.json`).
- `src/environments/environment.ts` + `environment.prod.ts` created, `apiUrl: 'http://localhost:5048/api'`.
- `src/app/app.config.ts` configured: `provideRouter(routes)`, `provideAnimationsAsync()`, `provideHttpClient()`.
- `src/app/app.routes.ts` configured with lazy-loaded routes for `dashboard`, `customers`, `products`, `orders`, `orders/new`, `orders/:id` — **these route targets reference component files that do not exist yet**, so the app will not currently compile/run until those components are created.
- `src/app/core/models/`: `customer.model.ts`, `product.model.ts`, `order.model.ts` — TS interfaces mirroring backend Response DTOs (`Customer`, `CreateCustomerRequest`, `Product`, `CreateProductRequest`, `Order`, `OrderSummary`, `OrderItem`, `AppliedDiscount`, `CreateOrderRequest`, string-union types `CustomerTier`/`OrderStatus`).
- `src/app/core/services/`: `customer.service.ts`, `product.service.ts` (getAll/getById/create), `order.service.ts` (getAll/getById/create/confirm/cancel) — all `HttpClient`-based, `providedIn: 'root'`.
- Empty (created but no files yet): `src/app/layout/`, `src/app/dashboard/`, `src/app/customers/`, `src/app/products/`, `src/app/orders/order-list/`, `src/app/orders/order-create/`, `src/app/orders/order-detail/`, `src/app/shared/components/confirm-dialog/`, `src/app/shared/components/tier-chip/`, `src/app/shared/components/empty-state/`.
- `src/app/app.component.ts` is still the **default CLI template** (just `RouterOutlet`) — no app shell/sidenav wired in yet.

### Not started:
- App shell (`layout/`) with Material sidenav + toolbar, nav links: Dashboard / Orders / Customers / Products.
- Shared components: `tier-chip` (badge for CustomerTier), `confirm-dialog` (generic confirm), `empty-state`.
- Dashboard screen (summary cards + recent orders table).
- Customers screen (list + search + create dialog + tier badges).
- Products screen (list + search + create/edit + active indicator).
- Orders: `order-list` (list + search + status filter + link to detail), `order-create` (customer select + product multi-select w/ quantity + live subtotal/discount/total calc mirroring the documented VIP/₹110k example), `order-detail` (full breakdown + Confirm/Cancel buttons, disabled based on status).
- `ng build` verification once screens exist.

## Next Steps (in order)
1. Build the app shell in `layout/` (e.g. `app-shell.component.ts`) with Material `mat-sidenav-container` + `mat-toolbar`, wire it into `app.component.html` in place of the bare `<router-outlet>`.
2. Build the 3 shared components (`tier-chip`, `confirm-dialog`, `empty-state`).
3. Build Dashboard, Customers, Products, Orders (list/create/detail) screens using the existing `core/models` and `core/services` — do not recreate those, they already exist and are correct.
4. Run `ng build` (from `frontend/orderflow-ui/`) and fix any compile errors.
5. Write top-level `OrderFlow\README.md`: overview, architecture diagram, project structure, run commands (`dotnet run` from `backend/OrderFlow.Api` → `http://localhost:5048`; `ng serve` from `frontend/orderflow-ui` → `http://localhost:4200`), `dotnet test OrderFlow.slnx` from `OrderFlow/`, API endpoint list, seed data, discount rules summary.
6. Final verification: rebuild/retest backend, `ng build` frontend, run both apps concurrently, confirm CORS/API calls work end-to-end from the browser, confirm `orderflow.db` is created with seeded rows.
7. Give the user a concise final report (structure, architecture, run commands, test results 29/29, endpoints, seed data, assumptions made).

## Key Gotchas Learned
- SDK 10.0.302's `dotnet new sln` produces `OrderFlow.slnx`, not `.sln` — use `.slnx` everywhere.
- `FluentValidation.AspNetCore` 11.3.0 / `FluentValidation` 11.5.1 (as installed) does **not** have `ToModelStateDictionary()` — build `ModelStateDictionary` manually from `ValidationResult.Errors`.
- No .NET 8 SDK installed (only 9.0.300, 10.0.302 + net8.0 runtime) — net8.0 projects still build/test fine via multi-targeting support.
- Integration tests intermittently hit "table already exists" SQLite errors from `WebApplicationFactory` host rebuilds — mitigated defensively in `Program.cs` by ignoring that specific `EnsureCreated()` exception message.
