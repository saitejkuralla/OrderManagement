# SPEC-1043 — Promotional Coupon Discount

## 1. Objective

Enable an Order to receive an additional percentage-based discount when a
valid promotional coupon code is supplied at order creation.

The coupon discount must stack with any existing customer-tier and
large-order discounts already applied to the order, and must not require
any database schema changes.

---

## 2. Functional Requirements

### FR-01 — Coupon Code Input

An Order creation request may optionally include a coupon code.

When no coupon code is supplied, existing Order behavior is unchanged.

### FR-02 — Known Coupon Codes

The system maintains a fixed, hardcoded catalog of valid coupon codes and
their discount percentages:

- WELCOME10 = 10%
- SAVE20 = 20%

Coupon code matching is case-insensitive.

### FR-03 — Apply Coupon Discount

When a known coupon code is supplied, its discount percentage must be
used, together with the Order subtotal, to calculate an additional
coupon discount amount.

### FR-04 — Coupon Discount Amount

Formula:

CouponDiscountAmount = Subtotal * CouponPercentage / 100

### FR-05 — Combined Discount With Existing Rules

The coupon discount is evaluated independently of the existing
customer-tier and large-order discount rules.

- The customer-tier and large-order discounts remain combined and capped
  at a maximum of 20%, as before.
- The coupon discount is capped independently at a maximum of 20%.
- The two capped amounts are added together to produce the Order's total
  discount amount.

### FR-06 — Final Total

Formula:

TotalDiscountAmount = TierAndLargeOrderDiscountAmount + CouponDiscountAmount

FinalTotal = Subtotal - TotalDiscountAmount

Where TierAndLargeOrderDiscountAmount is already capped at 20% by the
existing discount engine, and CouponDiscountAmount is capped at 20%
independently (see FR-05).

### FR-07 — Invalid Coupon Code

A coupon code that does not match a known code must be rejected.

The Order must not be created when an invalid coupon code is supplied.

### FR-08 — No Coupon Supplied

When no coupon code is supplied, CouponDiscountAmount = 0, and existing
Order discount and total calculations are unaffected.

---

## 3. Acceptance Criteria

### AC-01 — Valid Coupon, No Other Discounts

Given a Standard-tier customer's Order with a subtotal of 1000, and no
large-order discount applies,

When coupon code "WELCOME10" is applied,

Then:

- CouponDiscountAmount = 100
- FinalTotal = 900

### AC-02 — Valid Coupon Stacking With Tier Discount

Given a Gold-tier customer's Order with a subtotal of 5000
(tier discount = 10% = 500),

When coupon code "SAVE20" is applied,

Then:

- Tier/large-order discount amount = 500 (10%, under the 20% cap)
- Coupon discount amount = 1000 (20%)
- Total discount amount = 1500
- FinalTotal = 3500

### AC-03 — Unknown Coupon Code Rejected

Given an Order,

When an unrecognized coupon code is supplied,

Then the request must be rejected and the Order must not be created.

### AC-04 — Case-Insensitive Match

Given an Order,

When coupon code "welcome10" (lowercase) is supplied,

Then it is treated the same as "WELCOME10" and a 10% coupon discount is
applied.

### AC-05 — No Coupon Supplied

Given an Order with a subtotal of 1000 and no coupon code supplied,

Then:

- CouponDiscountAmount = 0
- FinalTotal reflects only the existing tier/large-order discount
  behavior, unchanged.

### AC-06 — Combined Cap Behavior

Given an Order where the tier and large-order discount alone already
totals 20% (capped), with a subtotal of 2500,

When coupon code "SAVE20" is applied,

Then:

- Tier/large-order discount amount = 500 (20% of 2500)
- Coupon discount amount = 500 (20% of 2500)
- Total discount amount = 1000
- FinalTotal = 1500

---

## 4. Constraints

- The existing Order behavior (tier and large-order discounts) must
  continue to work unchanged when no coupon code is supplied.
- Coupon codes and their percentages are fixed in code; no configuration
  file or database changes are required or permitted for this feature.
- The coupon code itself is not persisted; it is not stored on the Order
  and cannot be retrieved or reapplied after order creation.
- Invalid coupon codes must not be applied, and must not result in an
  Order being created.
- The implementation must not change unrelated Order or discount-rule
  behavior.

---

## 5. Testing Requirements

The implementation must be validated for:

- valid coupon code with no other discounts applying
- valid coupon code stacking with an existing tier and/or large-order
  discount
- unknown/invalid coupon code rejection
- case-insensitive coupon code matching
- no coupon code supplied (regression / unchanged behavior)
- combined discount amount and final total calculation
- coupon discount capped independently at 20%
- existing tier/large-order discount cap (20%) unaffected by coupon logic

---

## 6. Completion Criteria

The feature is complete when:

- All functional requirements are implemented.
- All acceptance criteria are satisfied.
- Required tests are present and passing.
- Existing relevant tests continue to pass.
- No database schema changes are introduced.
- The implementation is ready for independent testing and security
  review.

---

## 7. SPEC Identifier

SPEC-ID: SPEC-1043

---

## 8. Known Limitation

Since the coupon code is not persisted, the coupon discount line item
appears in the Order's discount breakdown only in the response returned
at creation time. Subsequent retrieval of the Order will not reapply or
re-display the coupon in the discount breakdown; however, the Order's
stored DiscountAmount and Total already include the coupon discount and
remain accurate.

---

## 9. Demo Scenario

This feature exists to showcase promotional discounts in a live demo.
A suggested walkthrough, using only the seed data already defined above:

1. Create an Order for a Standard-tier customer with a subtotal of 1000
   and no coupon code — show the baseline FinalTotal = 1000 (no discount)
   is unchanged from today's behavior (AC-05).
2. Re-create the same Order with coupon code `WELCOME10` — show
   CouponDiscountAmount = 100 and FinalTotal = 900 (AC-01).
3. Create an Order for a Gold-tier customer with a subtotal of 5000 and
   coupon code `SAVE20` — show both the tier discount and the coupon
   discount applied together, FinalTotal = 3500 (AC-02).
4. Attempt to create an Order with an unrecognized coupon code (e.g.
   `BADCODE`) — show the request is rejected (AC-03).
5. Repeat step 2 using lowercase `welcome10` — show the same 10% discount
   is applied, demonstrating case-insensitive matching (AC-04).
