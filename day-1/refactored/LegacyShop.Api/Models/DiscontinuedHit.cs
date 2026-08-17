namespace LegacyShop.Api.Models;

public class DiscontinuedHit
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public DateTime Hit { get; set; }
}