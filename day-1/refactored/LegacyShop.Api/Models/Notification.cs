namespace LegacyShop.Api.Models;

public class Notification
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
}