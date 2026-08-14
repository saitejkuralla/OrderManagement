namespace OrderFlow.Domain.Discounts;

/// <summary>
/// A single, independently testable discount rule. Add new implementations to introduce
/// new discount types (e.g. coupon codes) without changing existing rules or the calculator.
/// </summary>
public interface IDiscountRule
{
    /// <summary>Returns the discount that applies for the given context, or null if the rule does not apply.</summary>
    AppliedDiscount? Evaluate(DiscountContext context);
}
