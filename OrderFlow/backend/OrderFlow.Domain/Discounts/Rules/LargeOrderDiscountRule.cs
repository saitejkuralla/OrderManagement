namespace OrderFlow.Domain.Discounts.Rules;

/// <summary>Additional discount for large orders: +5% when the subtotal exceeds ₹10,000.</summary>
public sealed class LargeOrderDiscountRule : IDiscountRule
{
    public const decimal Threshold = 10_000m;
    public const decimal Percentage = 0.05m;

    public AppliedDiscount? Evaluate(DiscountContext context)
    {
        if (context.Subtotal <= Threshold)
        {
            return null;
        }

        var amount = Math.Round(context.Subtotal * Percentage, 2, MidpointRounding.AwayFromZero);
        return new AppliedDiscount("Large Order Discount", Percentage, amount);
    }
}
