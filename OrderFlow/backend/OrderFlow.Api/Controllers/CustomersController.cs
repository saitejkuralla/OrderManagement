using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrderFlow.Api.Contracts.Requests;
using OrderFlow.Api.Contracts.Responses;
using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IValidator<CreateCustomerRequest> _validator;

    public CustomersController(ICustomerService customerService, IValidator<CreateCustomerRequest> validator)
    {
        _customerService = customerService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        return Ok(customers.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(ToResponse(customer));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToModelState(validationResult));
        }

        var tier = Enum.Parse<CustomerTier>(request.Tier, ignoreCase: true);
        var command = new CreateCustomerCommand(request.Name, request.Email, tier);
        var created = await _customerService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
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

    private static CustomerResponse ToResponse(CustomerResult result) => new()
    {
        Id = result.Id,
        Name = result.Name,
        Email = result.Email,
        Tier = result.Tier.ToString(),
        OrderCount = result.OrderCount
    };
}
