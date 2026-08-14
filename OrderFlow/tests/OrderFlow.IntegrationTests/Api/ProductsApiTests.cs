using System.Net;
using System.Net.Http.Json;
using OrderFlow.Api.Contracts.Requests;
using OrderFlow.Api.Contracts.Responses;

namespace OrderFlow.IntegrationTests.Api;

public class ProductsApiTests : IClassFixture<OrderFlowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(OrderFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededProducts()
    {
        var products = await _client.GetFromJsonAsync<List<ProductResponse>>("/api/products");

        Assert.NotNull(products);
        Assert.True(products!.Count >= 4);
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreated()
    {
        var request = new CreateProductRequest
        {
            Name = "Test Webcam",
            Sku = $"WEBCAM-{Guid.NewGuid():N}"[..20],
            Price = 3_000m
        };

        var response = await _client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.True(created!.IsActive);
    }

    [Fact]
    public async Task Create_InvalidProduct_ReturnsBadRequest()
    {
        var request = new CreateProductRequest { Name = string.Empty, Sku = string.Empty, Price = 0 };

        var response = await _client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
