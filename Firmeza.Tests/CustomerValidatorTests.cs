using Firmeza.Models;
using Firmeza.Validators;

namespace Firmeza.Tests;

public class CustomerValidatorTests
{
    private readonly CustomerValidator _validator = new();

    private static Customer ValidCustomer() => new()
    {
        UserName = "Juan Perez",
        Email = "juan@example.com",
        Password = "Abcdef1$",
        Document = "12345678",
    };

    [Fact]
    public void Validate_WithValidCustomer_Passes()
    {
        var result = _validator.Validate(ValidCustomer());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidEmail_Fails()
    {
        var customer = ValidCustomer();
        customer.Email = "not-an-email";

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithWeakPassword_Fails()
    {
        var customer = ValidCustomer();
        customer.Password = "weak";

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithUserNameContainingDigits_Fails()
    {
        var customer = ValidCustomer();
        customer.UserName = "Juan123";

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
    }
}
