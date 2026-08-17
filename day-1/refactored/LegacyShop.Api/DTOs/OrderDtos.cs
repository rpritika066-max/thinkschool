using System.ComponentModel.DataAnnotations;

namespace LegacyShop.Api.DTOs;

public sealed class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }

    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; init; } = [];

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string ShippingState { get; init; } = string.Empty;

    public string? PromoCode { get; init; }
}

public sealed class OrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}

public sealed record OrderLineResponse(
    string Product,
    int Quantity,
    decimal LineTotal);

public sealed record CreateOrderResponse(
    int OrderId,
    string Customer,
    IReadOnlyList<OrderLineResponse> Items,
    decimal Subtotal,
    decimal Shipping,
    decimal Tax,
    decimal Total,
    int PointsEarned,
    string MembershipLevel);

public sealed record CreateOrderResult(
    bool Success,
    int StatusCode,
    string? Error,
    CreateOrderResponse? Response);