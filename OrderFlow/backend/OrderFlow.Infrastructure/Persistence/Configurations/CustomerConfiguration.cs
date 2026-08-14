using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Tier).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(c => c.Email).IsUnique();

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Customer { Id = SeedIds.AliceId, Name = "Alice", Email = "alice@orderflow.test", Tier = CustomerTier.Standard },
            new Customer { Id = SeedIds.BobId, Name = "Bob", Email = "bob@orderflow.test", Tier = CustomerTier.Silver },
            new Customer { Id = SeedIds.CharlieId, Name = "Charlie", Email = "charlie@orderflow.test", Tier = CustomerTier.Gold },
            new Customer { Id = SeedIds.DavidId, Name = "David", Email = "david@orderflow.test", Tier = CustomerTier.VIP }
        );
    }
}
