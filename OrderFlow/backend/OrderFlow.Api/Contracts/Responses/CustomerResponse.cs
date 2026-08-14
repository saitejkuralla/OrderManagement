namespace OrderFlow.Api.Contracts.Responses;

public sealed class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}
