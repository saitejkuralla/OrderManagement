using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;
using OrderFlow.Application.Exceptions;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Discounts;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDiscountCalculator _discountCalculator;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IDiscountCalculator discountCalculator)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _discountCalculator = discountCalculator;
    }

    public async Task<IReadOnlyList<OrderSummaryResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryResult(o.Id, o.Customer.Name, o.CreatedAt, o.Status, o.Subtotal, o.DiscountAmount, o.Total))
            .ToList();
    }

    public async Task<OrderResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : BuildOrderResult(order);
    }

    public async Task<OrderResult> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Items.Count == 0)
        {
            throw new BusinessRuleViolationException("At least one order item is required.");
        }

        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), command.CustomerId);

        var orderItems = new List<OrderItem>();
        foreach (var itemCommand in command.Items)
        {
            if (itemCommand.Quantity <= 0)
            {
                throw new BusinessRuleViolationException("Order item quantity must be greater than zero.");
            }

            var product = await _productRepository.GetByIdAsync(itemCommand.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemCommand.ProductId);

            if (!product.IsActive)
            {
                throw new BusinessRuleViolationException($"Product '{product.Name}' is not active and cannot be ordered.");
            }

            var lineTotal = Math.Round(product.Price * itemCommand.Quantity, 2, MidpointRounding.AwayFromZero);
            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                Quantity = itemCommand.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            });
        }

        var subtotal = orderItems.Sum(i => i.LineTotal);
        var discountResult = _discountCalculator.Calculate(new DiscountContext(subtotal, customer.Tier));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Customer = customer,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Subtotal = subtotal,
            DiscountAmount = discountResult.TotalDiscountAmount,
            Total = discountResult.FinalTotal,
            Items = orderItems
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        return BuildOrderResult(order);
    }

    public async Task<OrderResult> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        if (order.Status != OrderStatus.Pending)
        {
            throw new BusinessRuleViolationException($"Only pending orders can be confirmed. Current status is '{order.Status}'.");
        }

        order.Status = OrderStatus.Confirmed;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        return BuildOrderResult(order);
    }

    public async Task<OrderResult> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        if (order.Status != OrderStatus.Pending)
        {
            throw new BusinessRuleViolationException($"Only pending orders can be cancelled. Current status is '{order.Status}'.");
        }

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        return BuildOrderResult(order);
    }

    /// <summary>Recomputes the discount breakdown for display; the order stores only the resulting totals.</summary>
    private OrderResult BuildOrderResult(Order order)
    {
        var discountResult = _discountCalculator.Calculate(new DiscountContext(order.Subtotal, order.Customer.Tier));

        var items = order.Items
            .Select(i => new OrderItemResult(i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice, i.LineTotal))
            .ToList();

        var appliedDiscounts = discountResult.AppliedDiscounts
            .Select(d => new AppliedDiscountResult(d.Name, d.Percentage, d.Amount))
            .ToList();

        return new OrderResult(
            order.Id,
            order.CustomerId,
            order.Customer.Name,
            order.Customer.Tier,
            order.CreatedAt,
            order.Status,
            order.Subtotal,
            order.DiscountAmount,
            order.Total,
            items,
            appliedDiscounts);
    }
}
