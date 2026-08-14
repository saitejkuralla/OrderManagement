namespace OrderFlow.Application.Contracts.Commands;

public sealed record CreateProductCommand(string Name, string Sku, decimal Price);
