using Firmeza.Models;
using Firmeza.Response;
using Microsoft.AspNetCore.Identity.Data;

namespace Firmeza.Services.Interfaces;

public interface ILoginService
{
    public Task<ServiceResponse<Customer>> Login(string name, string password);
    public Task<ServiceResponse<Customer>> CreateCustomer(Customer customer);
}