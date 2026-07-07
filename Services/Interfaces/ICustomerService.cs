using Firmeza.Models;
using Firmeza.Response;

namespace Firmeza.Services.Interfaces;

public interface ICustomerService
{
    public Task<ServiceResponse<IEnumerable<Customer>>> GetAllCustomers(string? search);
    public Task<ServiceResponse<Customer>> GetCustomer(Guid id);
    public Task<ServiceResponse<Customer>> SetActive(Guid id, bool isActive);
}
