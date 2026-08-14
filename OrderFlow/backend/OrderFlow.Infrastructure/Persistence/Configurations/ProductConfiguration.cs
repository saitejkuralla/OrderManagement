using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.HasData(
            new Product { Id = SeedIds.LaptopId, Name = "Laptop", Sku = "LAPTOP-001", Price = 80_000m, IsActive = true },
            new Product { Id = SeedIds.MonitorId, Name = "Monitor", Sku = "MONITOR-001", Price = 25_000m, IsActive = true },
            new Product { Id = SeedIds.KeyboardId, Name = "Keyboard", Sku = "KEYBOARD-001", Price = 5_000m, IsActive = true },
            new Product { Id = SeedIds.MouseId, Name = "Mouse", Sku = "MOUSE-001", Price = 2_000m, IsActive = true }
        );
    }
}
