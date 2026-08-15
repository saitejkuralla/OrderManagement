---
name: discount-calculation
description: Apply OrderFlow discount calculation rules and testing practices
---

# Discount Calculation Skill

Use this skill when implementing, reviewing, or testing
OrderFlow discount calculations.

Business rules:

1. Standard customers receive 0%.
2. Silver customers receive 5%.
3. Gold customers receive 10%.
4. VIP customers receive 10%.
5. Orders above ₹10,000 receive an additional 5%.
6. Maximum total discount is 20%.
7. Monetary calculations must use decimal.

When modifying discount logic:

- Identify all affected rules.
- Check boundary conditions.
- Add or update unit tests.
- Preserve existing behavior unless the specification explicitly changes it.