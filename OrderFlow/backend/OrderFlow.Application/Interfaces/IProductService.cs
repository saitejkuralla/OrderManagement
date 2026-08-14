using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;

namespace OrderFlow.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductResult>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResult> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default);
}
