using LegacyShop.Api.DTOs;
using LegacyShop.Api.Models;
using LegacyShop.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace LegacyShop.Api.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_WhenCustomerDoesNotExist_ReturnsNotFound()
    {
        var repository = new FakeOrderRepository();
        var service = new OrderService(
            repository,
            NullLogger<OrderService>.Instance);

        var request = new CreateOrderRequest
        {
            CustomerId = 999,
            ShippingState = "CA",
            Items =
            [
                new OrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1
                }
            ]
        };

        var result = await service.CreateOrderAsync(
            request,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Customer not found.", result.Error);
    }

    [Fact]
    public async Task CreateOrder_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var repository = new FakeOrderRepository();

        repository.Customers.Add(new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Status = "ACTIVE",
            MembershipLevel = "BASIC"
        });

        var service = new OrderService(
            repository,
            NullLogger<OrderService>.Instance);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            ShippingState = "CA",
            Items =
            [
                new OrderItemRequest
                {
                    ProductId = 999,
                    Quantity = 1
                }
            ]
        };

        var result = await service.CreateOrderAsync(
            request,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_CreatesOrderAndCalculatesTotal()
    {
        var repository = new FakeOrderRepository();

        repository.Customers.Add(new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Status = "ACTIVE",
            MembershipLevel = "BASIC"
        });

        repository.Products.Add(new Product
        {
            Id = 1,
            Name = "Keyboard",
            Price = 50m,
            StockQty = 20,
            IsDiscontinued = false
        });

        var service = new OrderService(
            repository,
            NullLogger<OrderService>.Instance);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            ShippingState = "CA",
            Items =
            [
                new OrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1
                }
            ]
        };

        var result = await service.CreateOrderAsync(
            request,
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Response);
        Assert.Equal(1, result.Response.OrderId);
        Assert.Equal(50m, result.Response.Subtotal);
        Assert.Equal(4.99m, result.Response.Shipping);
        Assert.Equal(4.536675m, result.Response.Tax);
        Assert.Equal(59.53m, result.Response.Total);   }
}