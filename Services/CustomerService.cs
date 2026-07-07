using Firmeza.Data;
using Firmeza.Models;
using Firmeza.Response;
using Firmeza.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<IEnumerable<Customer>>> GetAllCustomers(string? search)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.UserName, $"%{search}%") ||
                EF.Functions.ILike(c.Email, $"%{search}%"));
        }

        var customers = await query.OrderBy(c => c.UserName).ToListAsync();

        return new ServiceResponse<IEnumerable<Customer>>
        {
            Data = customers,
            Success = true,
        };
    }

    public async Task<ServiceResponse<Customer>> GetCustomer(Guid id)
    {
        var response = new ServiceResponse<Customer>();
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            response.Success = false;
            response.Message = "No se encontró el cliente...";
            return response;
        }

        response.Data = customer;
        response.Success = true;
        return response;
    }

    public async Task<ServiceResponse<Customer>> SetActive(Guid id, bool isActive)
    {
        var response = new ServiceResponse<Customer>();
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            response.Success = false;
            response.Message = "No se encontró el cliente...";
            return response;
        }

        customer.IsActive = isActive;
        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        response.Data = customer;
        response.Success = true;
        response.Message = isActive ? "Cliente habilitado" : "Cliente deshabilitado";
        return response;
    }
}
