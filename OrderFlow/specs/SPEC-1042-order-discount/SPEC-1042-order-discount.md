# SPEC-1042 — Order Discount

## 1. Objective

Enable an Order to have a percentage-based discount.

The discount must affect the Order's final total according to the
configured discount percentage.

---

## 2. Functional Requirements

### FR-01 — Discount Percentage

An Order can have a discount percentage from 0% through 30% inclusive.

The default discount percentage is 0%.

### FR-02 — Apply Discount

A discount percentage can be applied to an Order.

The supplied percentage must be used when calculating the Order's
discount and final total.

### FR-03 — Discount Amount

The discount amount must be calculated from the Order subtotal.

Formula:

DiscountAmount = Subtotal * DiscountPercentage / 100

### FR-04 — Final Total

The Order's final total must be calculated as:

FinalTotal = Subtotal - DiscountAmount

### FR-05 — Invalid Discount

A discount percentage below 0% or above 30% must be rejected.

The Order must not apply an invalid discount.

### FR-06 — Zero Discount

A discount percentage of 0% must result in:

DiscountAmount = 0

The Order's final total must remain equal to its subtotal.

---

## 3. Acceptance Criteria

### AC-01 — Valid Discount

Given an Order with a subtotal of 1000,

When a 10% discount is applied,

Then:

- DiscountAmount = 100
- FinalTotal = 900

### AC-02 — Zero Discount

Given an Order with a subtotal of 1000,

When a 0% discount is applied,

Then:

- DiscountAmount = 0
- FinalTotal = 1000

### AC-03 — Maximum Discount

Given an Order with a subtotal of 1000,

When a 30% discount is applied,

Then:

- DiscountAmount = 300
- FinalTotal = 700

### AC-04 — Negative Discount

Given an Order,

When a discount below 0% is applied,

Then the discount must be rejected.

### AC-05 — Discount Above Maximum

Given an Order,

When a discount above 30% is applied,

Then the discount must be rejected.

### AC-06 — Final Total Calculation

Given an Order with:

- Subtotal = 2500
- DiscountPercentage = 20%

Then:

- DiscountAmount = 500
- FinalTotal = 2000

---

## 4. Constraints

- The existing Order behavior must continue to work when no discount
  is applied.
- Invalid discount percentages must not be applied.
- The implementation must not change unrelated Order behavior.

---

## 5. Testing Requirements

The implementation must be validated for:

- valid discounts
- zero discount
- maximum discount
- negative discount
- discount above 30%
- discount amount calculation
- final total calculation
- existing Order behavior without a discount

---

## 6. Completion Criteria

The feature is complete when:

- All functional requirements are implemented.
- All acceptance criteria are satisfied.
- Required tests are present and passing.
- Existing relevant tests continue to pass.
- The implementation is ready for independent testing and security review.

---

## 7. SPEC Identifier

SPEC-ID: SPEC-1042