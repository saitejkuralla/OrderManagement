using System.Net;
using System.Net.Http.Json;
using OrderFlow.Api.Contracts.Requests;
using OrderFlow.Api.Contracts.Responses;

namespace OrderFlow.IntegrationTests.Api;

public class OrdersApiTests : IClassFixture<OrderFlowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(OrderFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ThenRetrieve_ThenConfirm_Succeeds()
    {
        var customers = await _client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers");
        var products = await _client.GetFromJsonAsync<List<ProductResponse>>("/api/products");
        var customer = customers!.First(c => c.Tier == "VIP");
        var product = products!.First(p => p.Sku == "LAPTOP-001");

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);
        Assert.Equal("Pending", created!.Status);
        Assert.Equal(product.Price, created.Subtotal);

        var getResponse = await _client.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(created.Id, fetched!.Id);

        var confirmResponse = await _client.PostAsync($"/api/orders/{created.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal("Confirmed", confirmed!.Status);
    }

    [Fact]
    public async Task CreateOrder_ThenCancel_SetsStatusToCancelled()
    {
        var customers = await _client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers");
        var products = await _client.GetFromJsonAsync<List<ProductResponse>>("/api/products");
        var customer = customers!.First(c => c.Tier == "Standard");
        var product = products!.First(p => p.Sku == "MOUSE-001");

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = new List<CreateOrderItemRequest> { new() { ProductId = product.Id, Quantity = 2 } }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        var cancelResponse = await _client.PostAsync($"/api/orders/{created!.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal("Cancelled", cancelled!.Status);
    }

    [Fact]
    public async Task CreateOrder_UnknownCustomer_ReturnsNotFound()
    {
        var products = await _client.GetFromJsonAsync<List<ProductResponse>>("/api/products");
        var product = products!.First();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest> { new() { ProductId = product.Id, Quantity = 1 } }
        };

        var response = await _client.PostAsJsonAsync("/api/orders", createRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_NoItems_ReturnsBadRequest()
    {
        var customers = await _client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers");
        var createRequest = new CreateOrderRequest { CustomerId = customers!.First().Id, Items = new List<CreateOrderItemRequest>() };

        var response = await _client.PostAsJsonAsync("/api/orders", createRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
