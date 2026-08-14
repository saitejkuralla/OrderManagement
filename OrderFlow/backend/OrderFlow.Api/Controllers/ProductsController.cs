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
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _validator;

    public ProductsController(IProductService productService, IValidator<CreateProductRequest> validator)
    {
        _productService = productService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(ToResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToModelState(validationResult));
        }

        var command = new CreateProductCommand(request.Name, request.Sku, request.Price);
        var created = await _productService.CreateAsync(command, cancellationToken);

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

    private static ProductResponse ToResponse(ProductResult result) => new()
    {
        Id = result.Id,
        Name = result.Name,
        Sku = result.Sku,
        Price = result.Price,
        IsActive = result.IsActive
    };
}
