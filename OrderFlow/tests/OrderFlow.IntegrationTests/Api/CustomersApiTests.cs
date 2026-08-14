using System.Net;
using System.Net.Http.Json;
using OrderFlow.Api.Contracts.Requests;
using OrderFlow.Api.Contracts.Responses;

namespace OrderFlow.IntegrationTests.Api;

public class CustomersApiTests : IClassFixture<OrderFlowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomersApiTests(OrderFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededCustomers()
    {
        var customers = await _client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers");

        Assert.NotNull(customers);
        Assert.True(customers!.Count >= 4);
    }

    [Fact]
    public async Task Create_ValidCustomer_ReturnsCreated()
    {
        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = $"test-{Guid.NewGuid():N}@orderflow.test",
            Tier = "Silver"
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(created);
        Assert.Equal(request.Name, created!.Name);
        Assert.Equal("Silver", created.Tier);
    }

    [Fact]
    public async Task Create_InvalidCustomer_ReturnsBadRequest()
    {
        var request = new CreateCustomerRequest { Name = string.Empty, Email = "not-an-email", Tier = "Unknown" };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
