# SPEC-1042 — Order Discount

## 1. Objective

Add support for applying a percentage discount to an Order.

The discount must be calculated only when the order is eligible.

## 2. Functional Requirements

### FR-01 — Discount percentage

An order may have a discount percentage between 0 and 30 inclusive.

### FR-02 — Discount calculation

The discount amount must be calculated from the order subtotal.

Formula:

DiscountAmount = Subtotal * DiscountPercentage / 100

### FR-03 — Final total

FinalTotal must be:

FinalTotal = Subtotal - DiscountAmount

### FR-04 — Invalid discount

A discount percentage below 0 or above 30 must be rejected.

### FR-05 — Zero discount

A discount percentage of 0 must leave the order total unchanged.

## 3. Implementation Scope

Allowed:

- Order domain/model code
- Discount calculation code
- Order-related tests
- Discount-related tests
- SPEC documentation

Expected areas:

src/Orders/**
src/Discounts/**
tests/Orders/**
tests/Discounts/**

## 4. Out of Scope

Do not modify:

- Payment processing
- Customer management
- Shipping
- Infrastructure/deployment
- Authentication
- Database schema unless explicitly required by the existing design

Expected forbidden examples:

src/Payments/**
src/Customers/**
infrastructure/**
deployment/**

## 5. Testing Requirements

Tests must cover:

1. Valid discount
2. Zero discount
3. Maximum discount
4. Negative discount
5. Discount greater than 30
6. Final total calculation

## 6. Completion Criteria

The implementation is complete only when:

- Functional requirements are implemented.
- Required tests are present.
- Tests pass.
- No out-of-scope files are modified.
- The implementation can be reviewed against this SPEC.

## 7. SPEC Identifier

SPEC-ID: SPEC-1042