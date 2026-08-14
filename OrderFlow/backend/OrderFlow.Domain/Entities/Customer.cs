using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CustomerTier Tier { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
