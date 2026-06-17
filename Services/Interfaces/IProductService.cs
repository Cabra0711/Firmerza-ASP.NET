using Firmeza.Models;
using Firmeza.Response;

namespace Firmeza.Services.Interfaces;

public interface IProductService
{
    public Task<ServiceResponse<IEnumerable<Product>>> GetAllProducts();
    public Task<ServiceResponse<Product>> GetProduct(Guid id);
    public Task<ServiceResponse<Product>> CreateProduct(Product product);
    public Task<ServiceResponse<Product>> UpdateProduct(Guid id, Product product);
    public Task<ServiceResponse<Product>> DeleteProduct(Guid id);
}