using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;

namespace OrderFlow.Application.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderSummaryResult>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResult> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken = default);
    Task<OrderResult> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResult> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
