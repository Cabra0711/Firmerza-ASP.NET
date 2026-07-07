using Firmeza.Enums;

namespace Firmeza.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public ProductCategory Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
}

public class ProductUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public ProductCategory Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
