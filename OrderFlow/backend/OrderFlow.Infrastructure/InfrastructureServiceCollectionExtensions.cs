using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Discounts;
using OrderFlow.Domain.Discounts.Rules;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Repositories;

namespace OrderFlow.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderFlow") ?? "Data Source=orderflow.db";

        services.AddDbContext<OrderFlowDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddSingleton<IDiscountRule, CustomerTierDiscountRule>();
        services.AddSingleton<IDiscountRule, LargeOrderDiscountRule>();
        services.AddSingleton<IDiscountCalculator, DiscountCalculator>();

        return services;
    }
}
