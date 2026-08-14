using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderFlowDbContext _dbContext;

    public ProductRepository(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
