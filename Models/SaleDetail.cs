namespace Firmeza.Models;

public class SaleDetail : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; }
}