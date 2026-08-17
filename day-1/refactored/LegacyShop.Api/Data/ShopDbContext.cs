using LegacyShop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LegacyShop.Api.Data;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<DiscontinuedHit> DiscontinuedHits => Set<DiscontinuedHit>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.LoyaltyAccount)
            .WithOne(l => l.Customer)
            .HasForeignKey<LoyaltyAccount>(l => l.CustomerId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderLines)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId);

        modelBuilder.Entity<PromoCode>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<PromoCode>().HasData(
            new PromoCode
            {
                Id = 1,
                Code = "WELCOME10",
                IsActive = true,
                UsesRemaining = 100,
                DiscountAmount = 10m
            });

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                Name = "Test Customer",
                Status = "ACTIVE",
                MembershipLevel = "BASIC"
            });

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Keyboard",
                Price = 50m,
                StockQty = 20,
                IsDiscontinued = false
            },
            new Product
            {
                Id = 2,
                Name = "Mouse",
                Price = 25m,
                StockQty = 30,
                IsDiscontinued = false
            });
    }
}