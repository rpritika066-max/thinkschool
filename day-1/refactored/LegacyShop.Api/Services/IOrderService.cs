using LegacyShop.Api.DTOs;

namespace LegacyShop.Api.Services;

public interface IOrderService
{
    Task<CreateOrderResult> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken);
}