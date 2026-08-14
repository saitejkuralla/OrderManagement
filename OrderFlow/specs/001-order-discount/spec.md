# Order Discount

## Problem

Customers need discounts based on customer tier and order value so that loyal and
high-value customers are rewarded consistently, while keeping the calculation easy
to reason about, test, and extend as new discount types are introduced.

## Business Rules

1. Standard tier customers receive a 0% customer-tier discount.
2. Silver tier customers receive a 5% customer-tier discount.
3. Gold tier customers receive a 10% customer-tier discount.
4. VIP tier customers receive a 10% customer-tier discount.
5. Orders with a subtotal greater than ₹10,000 receive an additional 5% large-order discount.
6. The combined discount percentage across all applicable rules is capped at a maximum
   of 20%, regardless of how many individual discounts would otherwise apply.
7. All monetary calculations (prices, subtotals, discounts, totals) use `decimal`.
   `double` and `float` must never be used for monetary values.
8. Discount calculation is a pure function of `(subtotal, customer tier)` and must be
   independently testable without a database, an HTTP pipeline, or any other
   infrastructure dependency.
9. Discount logic must not be implemented inside API controllers. Controllers only
   translate HTTP requests into application use cases and application results into
   HTTP responses.
10. The discount implementation must be extensible: adding a new discount type (for
    example, a promotional coupon) must be possible by adding a new rule
    implementation, without modifying existing rules or the calculator that combines
    them.

## Functional Requirements

- The system must calculate an order's subtotal as the sum of each order item's line
  total (`quantity * unit price`).
- The system must calculate the customer-tier discount based on the tier of the
  customer placing the order.
- The system must calculate the large-order discount based on the order subtotal.
- The system must combine all applicable discounts into a single total discount
  percentage and total discount amount, applying the 20% maximum cap described above.
- The system must calculate the final total as `subtotal - total discount amount`.
- The system must expose, for every order, all of the following values:
  - Subtotal
  - Each individual applied discount (name, percentage, amount)
  - Total discount percentage
  - Total discount amount
  - Final total
- The discount breakdown must be available both at order-creation time and whenever
  an existing order is retrieved.

## Non-Functional Requirements

- **Monetary precision**: all monetary values are represented as `decimal` and rounded
  to 2 decimal places using `MidpointRounding.AwayFromZero` to avoid floating-point
  rounding errors.
- **Testability**: the discount engine (`IDiscountRule`, `IDiscountCalculator`) must be
  unit-testable in isolation, with no dependency on EF Core, ASP.NET Core, or the file
  system.
- **Maintainability**: each discount rule is a small, single-responsibility class.
  Combining rules and enforcing the maximum discount cap is the sole responsibility of
  the calculator, keeping each piece easy to understand and change independently.
- **API separation**: discount rules live in the Domain layer; orchestration (fetching
  the order's customer/products and invoking the calculator) lives in the Application
  layer; the Api layer only maps HTTP requests/responses.
- **Performance considerations**: discount calculation is an in-memory, CPU-bound
  operation over a small, fixed number of rules and order items, so no caching or
  asynchronous processing is required.

## Acceptance Criteria

1. Given a Standard tier customer with an order subtotal of ₹5,000, when the order is
   calculated, then the total discount is ₹0 and the final total equals the subtotal.
2. Given a Silver tier customer with an order subtotal of ₹5,000, when the order is
   calculated, then the customer-tier discount is 5% (₹250) and the final total is
   ₹4,750.
3. Given a Gold tier customer with an order subtotal of ₹5,000, when the order is
   calculated, then the customer-tier discount is 10% (₹500) and the final total is
   ₹4,500.
4. Given a VIP tier customer with an order subtotal of ₹5,000, when the order is
   calculated, then the customer-tier discount is 10% (₹500) and the final total is
   ₹4,500.
5. Given any customer with an order subtotal greater than ₹10,000, when the order is
   calculated, then an additional 5% large-order discount is applied on top of the
   customer-tier discount.
6. Given a VIP tier customer with an order subtotal of ₹110,000, when the order is
   calculated, then the total discount is 15% (₹16,500) and the final total is
   ₹93,500.
7. Given a combination of discounts whose combined percentage would exceed 20%, when
   the order is calculated, then the total discount percentage applied is capped at
   20%.
8. Given an order with multiple line items, when the subtotal is calculated, then it
   equals the sum of every line item's `quantity * unit price`.
9. Given an order item with a quantity of zero or less, when the order is submitted,
   then the system rejects the order with a business rule violation.
10. Given an order with no items, a non-existent customer, a non-existent product, or
    an inactive product, when the order is submitted, then the system rejects the
    order with an appropriate error (business rule violation or not found).
