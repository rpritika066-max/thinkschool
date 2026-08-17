namespace LegacyShop.Api.Models;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UsesRemaining { get; set; }
    public decimal DiscountAmount { get; set; }
}