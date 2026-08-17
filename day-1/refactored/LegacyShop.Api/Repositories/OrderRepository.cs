using LegacyShop.Api.Data;
using LegacyShop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LegacyShop.Api.Repositories;

public class OrderRepository(ShopDbContext db) : IOrderRepository
{
    public Task<Customer?> GetCustomerAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        return db.Customers
            .FirstOrDefaultAsync(
                c => c.Id == customerId,
                cancellationToken);
    }

    public Task<Product?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        return db.Products
            .FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken);
    }

    public Task<LoyaltyAccount?> GetLoyaltyAccountAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        return db.LoyaltyAccounts
            .FirstOrDefaultAsync(
                l => l.CustomerId == customerId,
                cancellationToken);
    }

    public Task<PromoCode?> GetPromoCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        return db.PromoCodes
            .FirstOrDefaultAsync(
                p => p.Code == code,
                cancellationToken);
    }

    public Task<List<Order>> GetCustomerOrdersAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        return db.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddOrderAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await db.Orders.AddAsync(order, cancellationToken);
    }

    public async Task AddOrderLineAsync(
        OrderLine orderLine,
        CancellationToken cancellationToken)
    {
        await db.OrderLines.AddAsync(orderLine, cancellationToken);
    }

    public async Task AddDiscontinuedHitAsync(
        DiscontinuedHit hit,
        CancellationToken cancellationToken)
    {
        await db.DiscontinuedHits.AddAsync(hit, cancellationToken);
    }

    public async Task AddNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        await db.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task AddLoyaltyAccountAsync(
        LoyaltyAccount account,
        CancellationToken cancellationToken)
    {
        await db.LoyaltyAccounts.AddAsync(account, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}