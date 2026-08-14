namespace OrderFlow.Domain.Discounts;

/// <summary>Combines all applicable <see cref="IDiscountRule"/> results into a single, capped outcome.</summary>
public interface IDiscountCalculator
{
    DiscountResult Calculate(DiscountContext context);
}
