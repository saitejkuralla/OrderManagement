namespace OrderFlow.Domain.Discounts;

/// <summary>A single named discount that was applied, before any overall cap is enforced.</summary>
public sealed record AppliedDiscount(string Name, decimal Percentage, decimal Amount);
