namespace OrderFlow.Api.Contracts.Requests;

public sealed class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
}
