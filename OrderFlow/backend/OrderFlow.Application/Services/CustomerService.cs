using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<CustomerResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(ToResult).ToList();
    }

    public async Task<CustomerResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : ToResult(customer);
    }

    public async Task<CustomerResult> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            Tier = command.Tier
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        return ToResult(customer);
    }

    private static CustomerResult ToResult(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, customer.Tier, customer.Orders.Count);
}
