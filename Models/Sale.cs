using Firmeza.Enums;

namespace Firmeza.Models;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public SaleStatus Status { get; set; }
    public int CustomerId { get; set; }
    public ICollection<Customer> Customers { get; set; }
}