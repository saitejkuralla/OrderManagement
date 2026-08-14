using Moq;
using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Exceptions;
using OrderFlow.Application.Interfaces;
using OrderFlow.Application.Services;
using OrderFlow.Domain.Discounts;
using OrderFlow.Domain.Discounts.Rules;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using Xunit;

namespace OrderFlow.UnitTests.Application.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly IDiscountCalculator _discountCalculator = new DiscountCalculator(new IDiscountRule[]
    {
        new CustomerTierDiscountRule(),
        new LargeOrderDiscountRule()
    });

    private readonly Customer _vipCustomer = new()
    {
        Id = Guid.NewGuid(),
        Name = "David",
        Email = "david@orderflow.test",
        Tier = CustomerTier.VIP
    };

    private readonly Product _laptop = new() { Id = Guid.NewGuid(), Name = "Laptop", Sku = "LAPTOP-001", Price = 80_000m, IsActive = true };
    private readonly Product _monitor = new() { Id = Guid.NewGuid(), Name = "Monitor", Sku = "MONITOR-001", Price = 25_000m, IsActive = true };
    private readonly Product _keyboard = new() { Id = Guid.NewGuid(), Name = "Keyboard", Sku = "KEYBOARD-001", Price = 5_000m, IsActive = true };

    private OrderService CreateSut()
    {
        _customerRepository.Setup(r => r.GetByIdAsync(_vipCustomer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_vipCustomer);
        _productRepository.Setup(r => r.GetByIdAsync(_laptop.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_laptop);
        _productRepository.Setup(r => r.GetByIdAsync(_monitor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_monitor);
        _productRepository.Setup(r => r.GetByIdAsync(_keyboard.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_keyboard);

        return new OrderService(_orderRepository.Object, _customerRepository.Object, _productRepository.Object, _discountCalculator);
    }

    [Fact]
    public async Task CreateAsync_MultipleItems_CalculatesSubtotalAsSumOfLineTotals()
    {
        var sut = CreateSut();
        var command = new CreateOrderCommand(_vipCustomer.Id, new[]
        {
            new CreateOrderItemCommand(_laptop.Id, 1),
            new CreateOrderItemCommand(_monitor.Id, 1),
            new CreateOrderItemCommand(_keyboard.Id, 1)
        });

        var result = await sut.CreateAsync(command);

        Assert.Equal(110_000m, result.Subtotal);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_VIPCustomerLargeOrder_MatchesDocumentedExample()
    {
        var sut = CreateSut();
        var command = new CreateOrderCommand(_vipCustomer.Id, new[]
        {
            new CreateOrderItemCommand(_laptop.Id, 1),
            new CreateOrderItemCommand(_monitor.Id, 1),
            new CreateOrderItemCommand(_keyboard.Id, 1)
        });

        var result = await sut.CreateAsync(command);

        Assert.Equal(110_000m, result.Subtotal);
        Assert.Equal(16_500m, result.DiscountAmount);
        Assert.Equal(93_500m, result.Total);
        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NoItems_ThrowsBusinessRuleViolationException()
    {
        var sut = CreateSut();
        var command = new CreateOrderCommand(_vipCustomer.Id, Array.Empty<CreateOrderItemCommand>());

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => sut.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_InvalidQuantity_ThrowsBusinessRuleViolationException()
    {
        var sut = CreateSut();
        var command = new CreateOrderCommand(_vipCustomer.Id, new[]
        {
            new CreateOrderItemCommand(_laptop.Id, 0)
        });

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => sut.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_UnknownCustomer_ThrowsNotFoundException()
    {
        var sut = CreateSut();
        var command = new CreateOrderCommand(Guid.NewGuid(), new[]
        {
            new CreateOrderItemCommand(_laptop.Id, 1)
        });

        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_UnknownProduct_ThrowsNotFoundException()
    {
        var sut = CreateSut();
        var command = new CreateOrderCommand(_vipCustomer.Id, new[]
        {
            new CreateOrderItemCommand(Guid.NewGuid(), 1)
        });

        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_InactiveProduct_ThrowsBusinessRuleViolationException()
    {
        var inactiveProduct = new Product { Id = Guid.NewGuid(), Name = "Old Mouse", Sku = "MOUSE-OLD", Price = 1_000m, IsActive = false };
        _productRepository.Setup(r => r.GetByIdAsync(inactiveProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inactiveProduct);

        var sut = CreateSut();
        var command = new CreateOrderCommand(_vipCustomer.Id, new[]
        {
            new CreateOrderItemCommand(inactiveProduct.Id, 1)
        });

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => sut.CreateAsync(command));
    }

    [Fact]
    public async Task ConfirmAsync_PendingOrder_SetsStatusToConfirmed()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _vipCustomer.Id,
            Customer = _vipCustomer,
            Status = OrderStatus.Pending,
            Subtotal = 1_000m,
            Total = 1_000m,
            Items = new List<OrderItem>()
        };
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();
        var result = await sut.ConfirmAsync(order.Id);

        Assert.Equal(OrderStatus.Confirmed, result.Status);
        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConfirmedOrder_ThrowsBusinessRuleViolationException()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _vipCustomer.Id,
            Customer = _vipCustomer,
            Status = OrderStatus.Confirmed,
            Subtotal = 1_000m,
            Total = 1_000m,
            Items = new List<OrderItem>()
        };
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => sut.ConfirmAsync(order.Id));
    }

    [Fact]
    public async Task CancelAsync_PendingOrder_SetsStatusToCancelled()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _vipCustomer.Id,
            Customer = _vipCustomer,
            Status = OrderStatus.Pending,
            Subtotal = 1_000m,
            Total = 1_000m,
            Items = new List<OrderItem>()
        };
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();
        var result = await sut.CancelAsync(order.Id);

        Assert.Equal(OrderStatus.Cancelled, result.Status);
    }
}
