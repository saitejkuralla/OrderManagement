# SPEC-1043 — Promotional Coupon Discount — Execution Plan

## 1. SPEC Summary

Allow an Order creation request to optionally include a promotional coupon
code (`WELCOME10` = 10%, `SAVE20` = 20%, hardcoded, case-insensitive). When
a valid code is supplied, a `CouponDiscountAmount` is computed from the
Order subtotal and combined with (added to, not merged into) the existing
tier/large-order discount, which continues to be capped at 20% as today.
The coupon discount is capped independently at 20%. An unrecognized coupon
code must reject the Order creation request entirely. The coupon code is
never persisted; no schema changes are permitted. When no coupon is
supplied, behavior is byte-for-byte unchanged from today.

The main implementation challenge is that `OrderService.BuildOrderResult`
is shared between `CreateAsync` and `GetByIdAsync`/`GetAllAsync`, but the
coupon line item must appear only in the creation response (per SPEC
section 8, "Known Limitation") since the coupon code is not persisted and
must not be reconstructed on later retrieval.

---

## 2. Requirements

### Functional
- FR-01: Coupon code is optional on Order creation; absence leaves behavior unchanged.
- FR-02: Fixed, hardcoded, case-insensitive coupon catalog (`WELCOME10`=10%, `SAVE20`=20%).
- FR-03/FR-04: `CouponDiscountAmount = Subtotal * CouponPercentage / 100` when a known code is supplied.
- FR-05: Coupon discount is evaluated independently of tier/large-order discount; each is capped at 20% independently, then added.
- FR-06: `TotalDiscountAmount = TierAndLargeOrderDiscountAmount + CouponDiscountAmount`; `FinalTotal = Subtotal - TotalDiscountAmount`.
- FR-07: Unknown coupon code rejects the request; Order must not be created.
- FR-08: No coupon supplied ⇒ `CouponDiscountAmount = 0`, no change to existing calculation.

### Non-Functional / Constraints
- No database schema changes.
- Coupon code and percentages fixed in code (no config/DB).
- Coupon code is not persisted on the Order and is not retrievable after creation.
- Existing tier/large-order discount behavior must be unaffected when no coupon is supplied.
- No unrelated Order or discount-rule behavior changes.
- Implementation must be ready for independent testing and security review.

---

## 3. Task Breakdown

| ID | Objective | Affected Area/Files | Dependencies | Expected Output | Role |
|----|-----------|----------------------|---------------|------------------|------|
| T1 | Add hardcoded coupon catalog and an independent coupon discount calculator in the Domain layer (own 20% cap, separate from `DiscountCalculator`'s combined cap). | `backend/OrderFlow.Domain/Discounts/` — new: `CouponCatalog.cs`, `ICouponDiscountCalculator.cs`, `CouponDiscountCalculator.cs`, `CouponDiscountOutcome.cs` (or similar record) | None | New Domain types compiling independently, exposing case-insensitive lookup and a `Calculate(subtotal, couponCode)` outcome (supplied/recognized/amount) | implementer |
| T2 | Register the new coupon calculator for DI. | `backend/OrderFlow.Infrastructure/InfrastructureServiceCollectionExtensions.cs` | T1 | `ICouponDiscountCalculator` resolvable at runtime alongside existing `IDiscountCalculator` | implementer |
| T3 | Add optional `CouponCode` to the Application command. | `backend/OrderFlow.Application/Contracts/Commands/CreateOrderCommand.cs` | None | `CreateOrderCommand` carries `string? CouponCode` | implementer |
| T4 | Add optional `CouponCode` to the API request contract. | `backend/OrderFlow.Api/Contracts/Requests/CreateOrderRequest.cs` | None | `CreateOrderRequest` carries `CouponCode` property | implementer |
| T5 | Wire the controller to pass `CouponCode` from request to command. | `backend/OrderFlow.Api/Controllers/OrdersController.cs` (`Create` action) | T3, T4 | Coupon code flows from HTTP request into the command | implementer |
| T6 | Add lightweight input-shape validation for `CouponCode` (e.g. max length) in the request validator; catalog/business validation stays server-side in Domain/Application, not duplicated here. | `backend/OrderFlow.Api/Validation/CreateOrderRequestValidator.cs` | T4 | Validator rejects malformed input without duplicating the coupon catalog | implementer |
| T7 | Integrate coupon evaluation into `OrderService.CreateAsync`: reject unknown codes via `BusinessRuleViolationException` (FR-07, surfaces as HTTP 400 via existing `ExceptionHandlingMiddleware`), combine capped coupon amount with capped tier/large-order amount per FR-06, persist `Order.DiscountAmount`/`Order.Total` inclusive of the coupon, and include the coupon as an `AppliedDiscount` line item **only in the creation response** (not in `BuildOrderResult` used by `GetByIdAsync`/`GetAllAsync`, since the coupon code is not persisted). | `backend/OrderFlow.Application/Services/OrderService.cs` | T1, T2, T3 | `CreateAsync` produces correct `OrderResult` per AC-01/02/05/06; invalid coupon throws before persistence (AC-03); `GetByIdAsync` behavior unchanged | implementer |
| T8 | Unit tests for the Domain coupon calculator: valid codes, unknown code, case-insensitivity, independent 20% cap, no-code path. | `tests/OrderFlow.UnitTests/Domain/Discounts/` (new test file) | T1 | Passing tests covering FR-02–FR-05, FR-08 in isolation | tester |
| T9 | Unit tests for `OrderService.CreateAsync` coupon integration: AC-01, AC-02, AC-04, AC-05, AC-06, and invalid-coupon rejection (AC-03) with no order persisted. | `tests/OrderFlow.UnitTests/Application/Services/OrderServiceTests.cs` | T7 | Passing tests covering all acceptance criteria at the service layer | tester |
| T10 | Integration tests exercising the HTTP API: `POST /api/orders` with a valid coupon (response includes coupon breakdown, correct totals), with an invalid coupon (400 ProblemDetails, no order created), and without a coupon (regression — unchanged response). | `tests/OrderFlow.IntegrationTests/Api/OrdersApiTests.cs` | T5, T6, T7 | Passing end-to-end tests confirming the documented "Known Limitation" (coupon not shown on subsequent `GET`) | tester |
| T11 | Full regression test run across unit and integration suites to confirm existing tier/large-order discount behavior and unrelated Order flows (confirm/cancel/list) are unaffected. | Whole solution (`dotnet test`) | T8, T9, T10 | Green test run; no regressions in pre-existing tests | tester |
| T12 | Security/constraint review: confirm coupon code is never persisted or logged in a sensitive way, comparison uses a safe case-insensitive strategy (e.g. `Ordinal`/`OrdinalIgnoreCase`, not culture-sensitive), invalid-coupon error responses do not leak internals (rely on existing `ProblemDetails` middleware), and input length/format is bounded to prevent abuse. | Domain/Application/Api coupon code paths | T7, T10 | Security review notes with no open findings, or documented follow-ups | security reviewer |
| T13 | Final review against the SPEC: verify all FRs and ACs are met, no schema changes were introduced, and existing behavior is unchanged when no coupon is supplied. | Whole feature diff | T11, T12 | Sign-off that Completion Criteria (SPEC section 6) are satisfied | reviewer |

---

## 4. Sequential Execution Plan

```
T1 → T3 → T4 → T2 → T5 → T6 → T7 → T8 → T9 → T10 → T11 → T12 → T13
```

Rationale for ordering:

- **T1 before T2**: the coupon calculator type must exist before it can be registered in DI.
- **T3, T4 before T5**: both the command and the request DTO must carry `CouponCode` before the controller can map one to the other.
- **T2 before T7**: although unit tests can new-up `OrderService` directly, the DI registration must exist before any component resolved through the container (controller/integration tests) can use the new calculator; placing it before T7 keeps the service layer's runtime dependency graph consistent as soon as it starts consuming `ICouponDiscountCalculator`.
- **T5, T6 before T7 is not required by compilation, but T7 is the core logic task and depends directly on T1–T3** (calculator, DI, command shape) to compile and behave correctly; T5/T6 (API plumbing) are independent of T7's internals but must exist before end-to-end (T10) testing is possible.
- **T7 before T8 is not required** (T8 only needs T1), but is listed after T2 in the linear chain per topological ordering of the table; T8 could execute in parallel with T2–T7 if the orchestrator supports parallel branches.
- **T7 before T9**: service-level tests require the integrated coupon logic in `OrderService`.
- **T5, T6, T7 before T10**: full HTTP round-trip tests require the request contract, validator, and service logic all in place.
- **T8, T9, T10 before T11**: the regression run should follow all new tests being authored so failures are attributable to the full change set.
- **T7, T10 before T12**: security review needs the final implementation and observable HTTP behavior (error responses) to assess.
- **T11, T12 before T13**: final sign-off requires both a green regression run and a completed security review.

---

## 5. Agent Responsibilities

| Role | Tasks |
|------|-------|
| implementer | T1, T2, T3, T4, T5, T6, T7 |
| tester | T8, T9, T10, T11 |
| security reviewer | T12 |
| reviewer | T13 |

---

## 6. Risks

- **Shared-state risk (high)**: `OrderService.BuildOrderResult` is reused by `CreateAsync`, `GetByIdAsync`, and `GetAllAsync`. Adding coupon-aware logic incorrectly into this shared method would either leak coupon behavior into `GetById`/`GetAll` (contradicting the documented Known Limitation and FR constraint that the coupon isn't persisted) or fail to surface it in the create response (violating AC-01/AC-02/AC-06). The implementer must isolate the coupon line item to the create-time response path only.
- **Double-counting risk**: the coupon discount must not be added into the same rule pipeline (`IDiscountRule`/`DiscountCalculator`) that caps tier/large-order discounts together, or it will be capped jointly instead of independently, violating FR-05/AC-02/AC-06. It must remain a separate calculation added after both caps are applied.
- **Persistence risk**: `Order.DiscountAmount`/`Order.Total` must include the coupon amount (so stored totals stay accurate per SPEC section 8), while the coupon code itself must not be added to the `Order` entity or persistence model — a schema change would violate the SPEC's explicit constraint.
- **Validation-layer duplication risk**: placing the coupon catalog in the FluentValidation validator (API layer) as well as the service layer would create two sources of truth; the catalog should live once, in the Domain layer.
- **Rejection semantics risk**: FR-07 requires the Order to not be created for invalid codes. The implementer must ensure the check happens before any repository write, and that it surfaces as a client (4xx) error, consistent with the existing `BusinessRuleViolationException` → HTTP 400 mapping, not a 500.
- **Case-sensitivity/culture risk**: string comparison for coupon codes must use an ordinal, non-culture-sensitive comparison to avoid locale-dependent matching bugs (security reviewer to confirm in T12).
- **Regression risk**: existing tier/large-order discount tests and unrelated Order flows (confirm/cancel/list) must be re-run (T11) to confirm no unintended behavior change, since `OrderService` and `CreateOrderRequest`/`CreateOrderCommand` are shared, actively-used types.
- **Testing gap risk**: integration tests (T10) are the only place that can verify the documented Known Limitation (coupon absent on subsequent `GET`) end-to-end; skipping this would leave that specific SPEC behavior unverified.

---

## 7. Definition of Done

- All functional requirements FR-01 through FR-08 are implemented as described.
- All acceptance criteria AC-01 through AC-06 pass via automated tests.
- Coupon logic is fully covered by unit tests (Domain calculator and `OrderService`) and integration tests (HTTP API), per the SPEC's Testing Requirements (section 5).
- No changes to the database schema or persistence model.
- No coupon code is stored on the `Order` entity or retrievable after creation.
- Existing tests for tier/large-order discounts and other Order flows continue to pass unmodified.
- Security review (T12) has no open findings related to input handling, comparison logic, or error response disclosure.
- Final review (T13) confirms the Completion Criteria in SPEC section 6 are met.
