using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Contracts.Results;

public sealed record AppliedDiscountResult(string Name, decimal Percentage, decimal Amount);

public sealed record OrderItemResult(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record OrderResult(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    CustomerTier CustomerTier,
    DateTime CreatedAt,
    OrderStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    IReadOnlyList<OrderItemResult> Items,
    IReadOnlyList<AppliedDiscountResult> AppliedDiscounts);

public sealed record OrderSummaryResult(
    Guid Id,
    string CustomerName,
    DateTime CreatedAt,
    OrderStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total);
