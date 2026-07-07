using Firmeza.Enums;

namespace Firmeza.DTOs;

public class SaleDetailDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class SaleDto
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public SaleStatus Status { get; set; }
    public List<SaleDetailDto> Details { get; set; } = new();
}

public class SaleCreateDto
{
    public List<SaleItemRequest> Items { get; set; } = new();
}
