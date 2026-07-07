using Firmeza.Data;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Services;
using Firmeza.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Firmeza.Tests;

public class LoginServiceTests
{
    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IConfiguration NewConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKeyForUnitTestsOnly1234567890",
            })
            .Build();
    }

    private static Customer NewCustomerRequest() => new()
    {
        UserName = "Maria Lopez",
        Email = "maria@example.com",
        Password = "Abcdef1$",
        Document = "87654321",
    };

    [Fact]
    public async Task CreateCustomer_HashesPasswordAndActivatesAccount()
    {
        await using var context = NewContext();
        var service = new LoginService(context, new CustomerValidator(), NewConfiguration());

        var response = await service.CreateCustomer(NewCustomerRequest());

        Assert.True(response.Success);
        Assert.NotEqual("Abcdef1$", response.Data!.Password);
        Assert.True(response.Data.IsActive);
        Assert.Equal(UserRole.Customer, response.Data.Role);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_Fails()
    {
        await using var context = NewContext();
        var service = new LoginService(context, new CustomerValidator(), NewConfiguration());
        await service.CreateCustomer(NewCustomerRequest());

        var response = await service.CreateCustomer(NewCustomerRequest());

        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        await using var context = NewContext();
        var service = new LoginService(context, new CustomerValidator(), NewConfiguration());
        await service.CreateCustomer(NewCustomerRequest());

        var response = await service.Login("Maria Lopez", "Abcdef1$");

        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Data!.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Fails()
    {
        await using var context = NewContext();
        var service = new LoginService(context, new CustomerValidator(), NewConfiguration());
        await service.CreateCustomer(NewCustomerRequest());

        var response = await service.Login("Maria Lopez", "WrongPassword1$");

        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_WithDisabledAccount_Fails()
    {
        await using var context = NewContext();
        var service = new LoginService(context, new CustomerValidator(), NewConfiguration());
        var created = await service.CreateCustomer(NewCustomerRequest());
        created.Data!.IsActive = false;
        await context.SaveChangesAsync();

        var response = await service.Login("Maria Lopez", "Abcdef1$");

        Assert.False(response.Success);
    }
}
