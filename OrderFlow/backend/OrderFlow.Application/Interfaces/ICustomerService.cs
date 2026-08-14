using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;

namespace OrderFlow.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResult>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerResult> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default);
}
