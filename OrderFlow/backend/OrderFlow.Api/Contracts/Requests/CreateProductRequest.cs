namespace OrderFlow.Api.Contracts.Requests;

public sealed class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
