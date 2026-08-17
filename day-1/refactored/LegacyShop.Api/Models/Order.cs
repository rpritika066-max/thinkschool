namespace LegacyShop.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "PENDING";
    public int ItemCount { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}