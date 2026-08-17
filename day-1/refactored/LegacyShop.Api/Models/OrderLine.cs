namespace LegacyShop.Api.Models;

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal LineTotal { get; set; }

    public Order? Order { get; set; }
}