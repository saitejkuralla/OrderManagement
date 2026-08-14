using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrderFlow.Api.Contracts.Requests;
using OrderFlow.Api.Contracts.Responses;
using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _validator;

    public OrdersController(IOrderService orderService, IValidator<CreateOrderRequest> validator)
    {
        _orderService = orderService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return Ok(orders.Select(ToSummaryResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(ToResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToModelState(validationResult));
        }

        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Items.Select(i => new CreateOrderItemCommand(i.ProductId, i.Quantity)).ToList());

        var created = await _orderService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<OrderResponse>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.ConfirmAsync(id, cancellationToken);
        return Ok(ToResponse(order));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.CancelAsync(id, cancellationToken);
        return Ok(ToResponse(order));
    }

    private static ModelStateDictionary ToModelState(FluentValidation.Results.ValidationResult validationResult)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in validationResult.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return modelState;
    }

    private static OrderSummaryResponse ToSummaryResponse(OrderSummaryResult result) => new()
    {
        Id = result.Id,
        CustomerName = result.CustomerName,
        CreatedAt = result.CreatedAt,
        Status = result.Status.ToString(),
        Subtotal = result.Subtotal,
        DiscountAmount = result.DiscountAmount,
        Total = result.Total
    };

    private static OrderResponse ToResponse(OrderResult result) => new()
    {
        Id = result.Id,
        CustomerId = result.CustomerId,
        CustomerName = result.CustomerName,
        CustomerTier = result.CustomerTier.ToString(),
        CreatedAt = result.CreatedAt,
        Status = result.Status.ToString(),
        Subtotal = result.Subtotal,
        DiscountAmount = result.DiscountAmount,
        Total = result.Total,
        Items = result.Items.Select(i => new OrderItemResponse
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.LineTotal
        }).ToList(),
        AppliedDiscounts = result.AppliedDiscounts.Select(d => new AppliedDiscountResponse
        {
            Name = d.Name,
            Percentage = d.Percentage,
            Amount = d.Amount
        }).ToList()
    };
}
