namespace OrderFlow.Application.Contracts.Results;

public sealed record ProductResult(Guid Id, string Name, string Sku, decimal Price, bool IsActive);
