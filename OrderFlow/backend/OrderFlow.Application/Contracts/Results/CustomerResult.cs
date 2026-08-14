using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Contracts.Results;

public sealed record CustomerResult(Guid Id, string Name, string Email, CustomerTier Tier, int OrderCount);
