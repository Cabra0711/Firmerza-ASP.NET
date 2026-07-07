using Firmeza.Data;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Services;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class ProductServiceTests
{
    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateProduct_WithoutSku_GeneratesSkuAndStatus()
    {
        await using var context = NewContext();
        var service = new ProductService(context);
        var product = new Product { Name = "Mouse", Description = "Mouse inalámbrico", Price = 20, Quantity = 5, Category = ProductCategory.Electronics };

        var response = await service.CreateProduct(product);

        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Data!.Sku));
        Assert.Equal(ProductStatus.LowStock, response.Data.Status);
    }

    [Fact]
    public async Task CreateProduct_WithZeroQuantity_SetsOutOfStock()
    {
        await using var context = NewContext();
        var service = new ProductService(context);
        var product = new Product { Name = "Monitor", Description = "Monitor 24 pulgadas", Price = 200, Quantity = 0, Category = ProductCategory.Electronics };

        var response = await service.CreateProduct(product);

        Assert.Equal(ProductStatus.OutOfStock, response.Data!.Status);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_Fails()
    {
        await using var context = NewContext();
        var service = new ProductService(context);
        await service.CreateProduct(new Product { Name = "Teclado", Description = "Teclado", Price = 10, Quantity = 5, Sku = "ABC-1234", Category = ProductCategory.Electronics });

        var response = await service.CreateProduct(new Product { Name = "Teclado 2", Description = "Otro teclado", Price = 15, Quantity = 5, Sku = "ABC-1234", Category = ProductCategory.Electronics });

        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateProduct_RecalculatesStatusFromQuantity_NotFromSubmittedValue()
    {
        await using var context = NewContext();
        var service = new ProductService(context);
        var created = await service.CreateProduct(new Product { Name = "Silla", Description = "Silla ergonómica", Price = 100, Quantity = 50, Category = ProductCategory.Home });

        // Simulate an edit form that doesn't submit Status (defaults to 0 = InStock) while dropping quantity to a low-stock level.
        var update = new Product { Name = "Silla", Description = "Silla ergonómica", Price = 100, Quantity = 5, Category = ProductCategory.Home, Status = default };
        var response = await service.UpdateProduct(created.Data!.Id, update);

        Assert.True(response.Success);
        Assert.Equal(ProductStatus.LowStock, response.Data!.Status);
    }

    [Fact]
    public async Task DeleteProduct_RemovesProduct()
    {
        await using var context = NewContext();
        var service = new ProductService(context);
        var created = await service.CreateProduct(new Product { Name = "Lámpara", Description = "Lámpara LED", Price = 15, Quantity = 5, Category = ProductCategory.Home });

        var response = await service.DeleteProduct(created.Data!.Id);
        var all = await service.GetAllProducts();

        Assert.True(response.Success);
        Assert.Empty(all.Data!);
    }
}
