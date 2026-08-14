using OrderFlow.Application.Contracts.Commands;
using OrderFlow.Application.Contracts.Results;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(ToResult).ToList();
    }

    public async Task<ProductResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToResult(product);
    }

    public async Task<ProductResult> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Sku = command.Sku,
            Price = command.Price,
            IsActive = true
        };

        await _productRepository.AddAsync(product, cancellationToken);
        return ToResult(product);
    }

    private static ProductResult ToResult(Product product) =>
        new(product.Id, product.Name, product.Sku, product.Price, product.IsActive);
}
