using System.Net;
using System.Net.Http.Json;
using LegacyShop.Api.DTOs;

namespace LegacyShop.Api.Tests;

public sealed class OrderIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostOrder_WithInvalidRequest_ReturnsValidationProblemDetails()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 0,
            ShippingState = "",
            Items = []
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("CustomerId", body);
        Assert.Contains("ShippingState", body);
        Assert.Contains("Items", body);
    }
}
