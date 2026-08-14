namespace OrderFlow.Application.Contracts.Commands;

public sealed record CreateOrderItemCommand(Guid ProductId, int Quantity);

public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyList<CreateOrderItemCommand> Items);
