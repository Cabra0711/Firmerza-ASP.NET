using Firmeza.Models;
using Firmeza.Response;

namespace Firmeza.Services.Interfaces;

public interface ILoginService
{
    public Task<ServiceResponse<Customer>> Login(string name, string password);
    public Task<ServiceResponse<Customer>> CreateCustomer(Customer customer);
}