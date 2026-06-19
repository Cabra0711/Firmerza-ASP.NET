using System.ComponentModel.DataAnnotations.Schema;
using Firmeza.Enums;

namespace Firmeza.Models;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } =  string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public ProductCategory Category { get; set; }
    [Column("ImageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public ICollection<SaleDetail> SaleDetails { get; set; }
}