namespace LegacyShop.Api.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "ACTIVE";
    public string MembershipLevel { get; set; } = "BASIC";

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public LoyaltyAccount? LoyaltyAccount { get; set; }
}