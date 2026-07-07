using Firmeza.Data;
using Firmeza.DTOs;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Firmeza.Tests;

public class SaleServiceTests
{
    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(Customer customer, Product product)> Seed(ApplicationDbContext context, int stock = 10, decimal price = 100)
    {
        var customer = new Customer { UserName = "Cliente Test", Email = "cliente@test.com", Password = "hash", Document = "111", IsActive = true, Role = UserRole.Customer };
        var product = new Product { Name = "Producto Test", Description = "Descripción", Price = price, Sku = "ABC-1111", Quantity = stock, Category = ProductCategory.Electronics, Status = ProductStatus.InStock };
        await context.Customers.AddAsync(customer);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        return (customer, product);
    }

    [Fact]
    public async Task CreateSale_WithSufficientStock_ComputesTotalsAndDecrementsStock()
    {
        await using var context = NewContext();
        var (customer, product) = await Seed(context, stock: 10, price: 100);
        var service = new SaleService(context);

        var response = await service.CreateSale(customer.Id, new List<SaleItemRequest>
        {
            new() { ProductId = product.Id, Quantity = 3 },
        });

        Assert.True(response.Success);
        Assert.Equal(300, response.Data!.SubTotal);
        Assert.Equal(357, response.Data.Total); // 300 + 19% tax
        Assert.Equal(7, (await context.Products.FindAsync(product.Id))!.Quantity);
    }

    [Fact]
    public async Task CreateSale_WithInsufficientStock_Fails()
    {
        await using var context = NewContext();
        var (customer, product) = await Seed(context, stock: 2);
        var service = new SaleService(context);

        var response = await service.CreateSale(customer.Id, new List<SaleItemRequest>
        {
            new() { ProductId = product.Id, Quantity = 5 },
        });

        Assert.False(response.Success);
    }

    [Fact]
    public async Task CreateSale_WithEmptyItems_Fails()
    {
        await using var context = NewContext();
        var (customer, _) = await Seed(context);
        var service = new SaleService(context);

        var response = await service.CreateSale(customer.Id, new List<SaleItemRequest>());

        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetAllSales_FiltersByStatus()
    {
        await using var context = NewContext();
        var (customer, product) = await Seed(context, stock: 20);
        var service = new SaleService(context);
        await service.CreateSale(customer.Id, new List<SaleItemRequest> { new() { ProductId = product.Id, Quantity = 1 } });

        var activeSales = await service.GetAllSales(null, null, SaleStatus.Active);
        var pendingSales = await service.GetAllSales(null, null, SaleStatus.Pending);

        Assert.Single(activeSales.Data!);
        Assert.Empty(pendingSales.Data!);
    }
}
