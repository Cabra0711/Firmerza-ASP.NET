using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Validators;

namespace Firmeza.Tests;

public class ProductValidatorTests
{
    private readonly ProductValidator _validator = new();

    private static Product ValidProduct() => new()
    {
        Name = "Teclado Mecánico",
        Description = "Teclado mecánico retroiluminado",
        Price = 49.99m,
        Sku = "",
        Quantity = 10,
        Category = ProductCategory.Electronics,
        ImageUrl = "https://example.com/img.jpg",
    };

    [Fact]
    public void Validate_WithValidProductAndDefaultCategory_Passes()
    {
        var product = ValidProduct();

        var result = _validator.Validate(product);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_Fails()
    {
        var product = ValidProduct();
        product.Name = "";

        var result = _validator.Validate(product);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNegativePrice_Fails()
    {
        var product = ValidProduct();
        product.Price = -5;

        var result = _validator.Validate(product);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithBlankSku_Passes()
    {
        var product = ValidProduct();
        product.Sku = "";

        var result = _validator.Validate(product);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithMalformedSku_Fails()
    {
        var product = ValidProduct();
        product.Sku = "not-a-valid-sku";

        var result = _validator.Validate(product);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithWellFormedSku_Passes()
    {
        var product = ValidProduct();
        product.Sku = "ABC-1234";

        var result = _validator.Validate(product);

        Assert.True(result.IsValid);
    }
}
