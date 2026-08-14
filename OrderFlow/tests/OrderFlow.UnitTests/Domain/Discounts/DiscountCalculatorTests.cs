using OrderFlow.Domain.Discounts;
using OrderFlow.Domain.Discounts.Rules;
using OrderFlow.Domain.Enums;
using Xunit;

namespace OrderFlow.UnitTests.Domain.Discounts;

public class DiscountCalculatorTests
{
    [Fact]
    public void Calculate_StandardCustomerSmallOrder_AppliesNoDiscount()
    {
        var calculator = new DiscountCalculator(new IDiscountRule[]
        {
            new CustomerTierDiscountRule(),
            new LargeOrderDiscountRule()
        });

        var result = calculator.Calculate(new DiscountContext(5_000m, CustomerTier.Standard));

        Assert.Empty(result.AppliedDiscounts);
        Assert.Equal(0m, result.TotalDiscountPercentage);
        Assert.Equal(0m, result.TotalDiscountAmount);
        Assert.Equal(5_000m, result.FinalTotal);
    }

    [Fact]
    public void Calculate_VIPCustomerLargeOrder_MatchesDocumentedExample()
    {
        // Subtotal 110,000: VIP 10% + Large order 5% = 15% total discount => final total 93,500.
        var calculator = new DiscountCalculator(new IDiscountRule[]
        {
            new CustomerTierDiscountRule(),
            new LargeOrderDiscountRule()
        });

        var result = calculator.Calculate(new DiscountContext(110_000m, CustomerTier.VIP));

        Assert.Equal(2, result.AppliedDiscounts.Count);
        Assert.Equal(0.15m, result.TotalDiscountPercentage);
        Assert.Equal(16_500m, result.TotalDiscountAmount);
        Assert.Equal(93_500m, result.FinalTotal);
    }

    [Fact]
    public void Calculate_CombinedDiscountsExceedingCap_IsCappedAt20Percent()
    {
        var calculator = new DiscountCalculator(new IDiscountRule[]
        {
            new FixedPercentageRule("Rule A", 0.15m),
            new FixedPercentageRule("Rule B", 0.10m)
        });

        var result = calculator.Calculate(new DiscountContext(10_000m, CustomerTier.VIP));

        Assert.Equal(DiscountCalculator.MaxDiscountPercentage, result.TotalDiscountPercentage);
        Assert.Equal(2_000m, result.TotalDiscountAmount);
        Assert.Equal(8_000m, result.FinalTotal);
    }

    /// <summary>Test double demonstrating the discount engine's extensibility for future discount types.</summary>
    private sealed class FixedPercentageRule : IDiscountRule
    {
        private readonly string _name;
        private readonly decimal _percentage;

        public FixedPercentageRule(string name, decimal percentage)
        {
            _name = name;
            _percentage = percentage;
        }

        public AppliedDiscount? Evaluate(DiscountContext context) =>
            new(_name, _percentage, Math.Round(context.Subtotal * _percentage, 2));
    }
}
