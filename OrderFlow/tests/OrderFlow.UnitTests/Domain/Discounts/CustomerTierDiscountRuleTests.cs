using OrderFlow.Domain.Discounts;
using OrderFlow.Domain.Discounts.Rules;
using OrderFlow.Domain.Enums;
using Xunit;

namespace OrderFlow.UnitTests.Domain.Discounts;

public class CustomerTierDiscountRuleTests
{
    private readonly CustomerTierDiscountRule _rule = new();

    [Fact]
    public void Evaluate_StandardCustomer_ReturnsNoDiscount()
    {
        var result = _rule.Evaluate(new DiscountContext(5_000m, CustomerTier.Standard));

        Assert.Null(result);
    }

    [Fact]
    public void Evaluate_SilverCustomer_Returns5PercentDiscount()
    {
        var result = _rule.Evaluate(new DiscountContext(5_000m, CustomerTier.Silver));

        Assert.NotNull(result);
        Assert.Equal(0.05m, result!.Percentage);
        Assert.Equal(250m, result.Amount);
    }

    [Fact]
    public void Evaluate_GoldCustomer_Returns10PercentDiscount()
    {
        var result = _rule.Evaluate(new DiscountContext(5_000m, CustomerTier.Gold));

        Assert.NotNull(result);
        Assert.Equal(0.10m, result!.Percentage);
        Assert.Equal(500m, result.Amount);
    }

    [Fact]
    public void Evaluate_VIPCustomer_Returns10PercentDiscount()
    {
        var result = _rule.Evaluate(new DiscountContext(5_000m, CustomerTier.VIP));

        Assert.NotNull(result);
        Assert.Equal(0.10m, result!.Percentage);
        Assert.Equal(500m, result.Amount);
    }
}
