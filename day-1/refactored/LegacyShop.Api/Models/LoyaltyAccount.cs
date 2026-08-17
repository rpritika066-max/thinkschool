namespace LegacyShop.Api.Models;

public class LoyaltyAccount
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int Points { get; set; }

    public Customer? Customer { get; set; }
}