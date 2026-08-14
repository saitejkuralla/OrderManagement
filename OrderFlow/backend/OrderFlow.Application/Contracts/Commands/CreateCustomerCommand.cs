using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Contracts.Commands;

public sealed record CreateCustomerCommand(string Name, string Email, CustomerTier Tier);
