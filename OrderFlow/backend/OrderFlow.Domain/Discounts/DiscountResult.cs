namespace OrderFlow.Domain.Discounts;

/// <summary>The full outcome of running the discount engine against an order subtotal.</summary>
public sealed record DiscountResult(
    decimal Subtotal,
    IReadOnlyList<AppliedDiscount> AppliedDiscounts,
    decimal TotalDiscountPercentage,
    decimal TotalDiscountAmount,
    decimal FinalTotal);
