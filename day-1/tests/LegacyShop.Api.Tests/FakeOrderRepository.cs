using LegacyShop.Api.Models;
using LegacyShop.Api.Repositories;

namespace LegacyShop.Api.Tests;

public sealed class FakeOrderRepository : IOrderRepository
{
    public List<Customer> Customers { get; } = [];
    public List<Product> Products { get; } = [];
    public List<Order> Orders { get; } = [];
    public List<LoyaltyAccount> LoyaltyAccounts { get; } = [];
    public List<PromoCode> PromoCodes { get; } = [];
    public List<Notification> Notifications { get; } = [];
    public List<DiscontinuedHit> DiscontinuedHits { get; } = [];

    public Task<Customer?> GetCustomerAsync(
        int customerId,
        CancellationToken cancellationToken)
        => Task.FromResult(
            Customers.FirstOrDefault(x => x.Id == customerId));

    public Task<Product?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken)
        => Task.FromResult(
            Products.FirstOrDefault(x => x.Id == productId));

    public Task<LoyaltyAccount?> GetLoyaltyAccountAsync(
        int customerId,
        CancellationToken cancellationToken)
        => Task.FromResult(
            LoyaltyAccounts.FirstOrDefault(x => x.CustomerId == customerId));

    public Task<PromoCode?> GetPromoCodeAsync(
        string code,
        CancellationToken cancellationToken)
        => Task.FromResult(
            PromoCodes.FirstOrDefault(x => x.Code == code));

    public Task<List<Order>> GetCustomerOrdersAsync(
        int customerId,
        CancellationToken cancellationToken)
        => Task.FromResult(
            Orders.Where(x => x.CustomerId == customerId).ToList());

    public Task AddOrderAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        order.Id = Orders.Count + 1;
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task AddOrderLineAsync(
        OrderLine orderLine,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AddDiscontinuedHitAsync(
        DiscontinuedHit hit,
        CancellationToken cancellationToken)
    {
        DiscontinuedHits.Add(hit);
        return Task.CompletedTask;
    }

    public Task AddNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task AddLoyaltyAccountAsync(
        LoyaltyAccount account,
        CancellationToken cancellationToken)
    {
        account.Id = LoyaltyAccounts.Count + 1;
        LoyaltyAccounts.Add(account);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}