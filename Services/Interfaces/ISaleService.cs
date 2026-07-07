using Firmeza.DTOs;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Response;

namespace Firmeza.Services.Interfaces;

public interface ISaleService
{
    public Task<ServiceResponse<IEnumerable<Sale>>> GetAllSales(DateTime? from, DateTime? to, SaleStatus? status);
    public Task<ServiceResponse<Sale>> GetSale(Guid id);
    public Task<ServiceResponse<IEnumerable<Sale>>> GetSalesByCustomer(Guid customerId);
    public Task<ServiceResponse<Sale>> CreateSale(Guid customerId, List<SaleItemRequest> items);
}
