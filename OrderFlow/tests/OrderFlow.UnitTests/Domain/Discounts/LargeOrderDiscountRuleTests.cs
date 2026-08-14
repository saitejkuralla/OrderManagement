using OrderFlow.Domain.Discounts;
using OrderFlow.Domain.Discounts.Rules;
using OrderFlow.Domain.Enums;
using Xunit;

namespace OrderFlow.UnitTests.Domain.Discounts;

public class LargeOrderDiscountRuleTests
{
    private readonly LargeOrderDiscountRule _rule = new();

    [Fact]
    public void Evaluate_SubtotalAtOrBelowThreshold_ReturnsNoDiscount()
    {
        var result = _rule.Evaluate(new DiscountContext(10_000m, CustomerTier.Standard));

        Assert.Null(result);
    }

    [Fact]
    public void Evaluate_SubtotalAboveThreshold_Returns5PercentDiscount()
    {
        var result = _rule.Evaluate(new DiscountContext(11_000m, CustomerTier.Standard));

        Assert.NotNull(result);
        Assert.Equal(0.05m, result!.Percentage);
        Assert.Equal(550m, result.Amount);
    }
}
