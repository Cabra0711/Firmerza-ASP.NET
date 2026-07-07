using Firmeza.Data;
using Firmeza.DTOs;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Response;
using Firmeza.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Services;

public class SaleService : ISaleService
{
    private const decimal TaxRate = 0.19m;
    private readonly ApplicationDbContext _context;

    public SaleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<IEnumerable<Sale>>> GetAllSales(DateTime? from, DateTime? to, SaleStatus? status)
    {
        var query = _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(s => s.SaleDate >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(s => s.SaleDate <= to.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();

        return new ServiceResponse<IEnumerable<Sale>>
        {
            Data = sales,
            Success = true,
        };
    }

    public async Task<ServiceResponse<Sale>> GetSale(Guid id)
    {
        var response = new ServiceResponse<Sale>();
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
        {
            response.Success = false;
            response.Message = "No se encontró la venta...";
            return response;
        }

        response.Data = sale;
        response.Success = true;
        return response;
    }

    public async Task<ServiceResponse<IEnumerable<Sale>>> GetSalesByCustomer(Guid customerId)
    {
        var sales = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<Sale>>
        {
            Data = sales,
            Success = true,
        };
    }

    public async Task<ServiceResponse<Sale>> CreateSale(Guid customerId, List<SaleItemRequest> items)
    {
        var response = new ServiceResponse<Sale>();

        if (items == null || items.Count == 0)
        {
            response.Success = false;
            response.Message = "La venta debe incluir al menos un producto.";
            return response;
        }

        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            response.Success = false;
            response.Message = "No se encontró el cliente...";
            return response;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var saleDetails = new List<SaleDetail>();
        decimal subTotal = 0;

        foreach (var item in items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                response.Success = false;
                response.Message = $"No se encontró el producto {item.ProductId}.";
                return response;
            }
            if (item.Quantity <= 0)
            {
                response.Success = false;
                response.Message = $"La cantidad para {product.Name} debe ser mayor a cero.";
                return response;
            }
            if (product.Quantity < item.Quantity)
            {
                response.Success = false;
                response.Message = $"Stock insuficiente para {product.Name}. Disponible: {product.Quantity}.";
                return response;
            }

            var detailSubTotal = product.Price * item.Quantity;
            subTotal += detailSubTotal;

            saleDetails.Add(new SaleDetail
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                SubTotal = detailSubTotal,
            });

            product.Quantity -= item.Quantity;
            product.Status = product.Quantity switch
            {
                0 => ProductStatus.OutOfStock,
                <= 30 => ProductStatus.LowStock,
                > 30 => ProductStatus.InStock
            };
            product.UpdatedAt = DateTime.UtcNow;
        }

        var tax = Math.Round(subTotal * TaxRate, 2);
        var sale = new Sale
        {
            SaleNumber = GenerateSaleNumber(),
            SaleDate = DateTime.UtcNow,
            CustomerId = customerId,
            SubTotal = subTotal,
            Tax = tax,
            Total = subTotal + tax,
            Status = SaleStatus.Active,
            SaleDetails = saleDetails,
        };

        await _context.Sales.AddAsync(sale);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        response.Data = sale;
        response.Success = true;
        response.Message = "Venta registrada con éxito";
        return response;
    }

    private static string GenerateSaleNumber()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"VTA-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
    }
}
