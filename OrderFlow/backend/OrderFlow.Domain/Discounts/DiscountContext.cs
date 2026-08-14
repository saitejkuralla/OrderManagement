using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Discounts;

/// <summary>Input the discount engine needs to evaluate applicable discounts.</summary>
public sealed record DiscountContext(decimal Subtotal, CustomerTier CustomerTier);
