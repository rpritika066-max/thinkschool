using LegacyShop.Api.Models;

namespace LegacyShop.Api.Repositories;

public interface IOrderRepository
{
    Task<Customer?> GetCustomerAsync(
        int customerId,
        CancellationToken cancellationToken);

    Task<Product?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken);

    Task<LoyaltyAccount?> GetLoyaltyAccountAsync(
        int customerId,
        CancellationToken cancellationToken);

    Task<PromoCode?> GetPromoCodeAsync(
        string code,
        CancellationToken cancellationToken);

    Task<List<Order>> GetCustomerOrdersAsync(
        int customerId,
        CancellationToken cancellationToken);

    Task AddOrderAsync(
        Order order,
        CancellationToken cancellationToken);

    Task AddOrderLineAsync(
        OrderLine orderLine,
        CancellationToken cancellationToken);

    Task AddDiscontinuedHitAsync(
        DiscontinuedHit hit,
        CancellationToken cancellationToken);

    Task AddNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken);

    Task AddLoyaltyAccountAsync(
        LoyaltyAccount account,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}