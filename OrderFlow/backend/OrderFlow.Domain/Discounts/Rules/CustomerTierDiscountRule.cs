using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Discounts.Rules;

/// <summary>Discount based on the customer's loyalty tier: Standard 0%, Silver 5%, Gold 10%, VIP 10%.</summary>
public sealed class CustomerTierDiscountRule : IDiscountRule
{
    private static readonly IReadOnlyDictionary<CustomerTier, decimal> Percentages = new Dictionary<CustomerTier, decimal>
    {
        [CustomerTier.Standard] = 0.00m,
        [CustomerTier.Silver] = 0.05m,
        [CustomerTier.Gold] = 0.10m,
        [CustomerTier.VIP] = 0.10m
    };

    public AppliedDiscount? Evaluate(DiscountContext context)
    {
        var percentage = Percentages.GetValueOrDefault(context.CustomerTier, 0m);
        if (percentage <= 0m)
        {
            return null;
        }

        var amount = Math.Round(context.Subtotal * percentage, 2, MidpointRounding.AwayFromZero);
        return new AppliedDiscount($"{context.CustomerTier} Discount", percentage, amount);
    }
}
