namespace OrderFlow.Domain.Discounts.Rules;

/// <summary>
/// Applies every registered <see cref="IDiscountRule"/> and caps the combined discount at
/// <see cref="MaxDiscountPercentage"/> so no future rule combination can exceed the business limit.
/// </summary>
public sealed class DiscountCalculator : IDiscountCalculator
{
    public const decimal MaxDiscountPercentage = 0.20m;

    private readonly IEnumerable<IDiscountRule> _rules;

    public DiscountCalculator(IEnumerable<IDiscountRule> rules)
    {
        _rules = rules;
    }

    public DiscountResult Calculate(DiscountContext context)
    {
        var appliedDiscounts = new List<AppliedDiscount>();
        decimal totalPercentage = 0m;

        foreach (var rule in _rules)
        {
            var discount = rule.Evaluate(context);
            if (discount is null)
            {
                continue;
            }

            appliedDiscounts.Add(discount);
            totalPercentage += discount.Percentage;
        }

        var cappedPercentage = Math.Min(totalPercentage, MaxDiscountPercentage);
        var totalDiscountAmount = Math.Round(context.Subtotal * cappedPercentage, 2, MidpointRounding.AwayFromZero);
        var finalTotal = context.Subtotal - totalDiscountAmount;

        return new DiscountResult(context.Subtotal, appliedDiscounts, cappedPercentage, totalDiscountAmount, finalTotal);
    }
}
