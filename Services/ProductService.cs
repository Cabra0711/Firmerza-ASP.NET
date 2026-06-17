using Firmeza.Data;
using Firmeza.Models;
using Firmeza.Response;
using Firmeza.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<IEnumerable<Product>>> GetAllProducts()
    {
        

        var products = await _context.Products.ToListAsync();

        return new ServiceResponse<IEnumerable<Product>>()
        {
            Data = products,
            Success = true,
        };
    }

    public async Task<ServiceResponse<Product>> GetProduct(Guid id)
    {
        var response = new ServiceResponse<Product>();
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product != null)
        {
            response.Data = product;
            response.Success = true;
            response.Message = "Libro Encontrado en el sistema...";
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "No se encontro el producto...";
            return response;
        }   
        
    }
    
    public async Task<ServiceResponse<Product>> CreateProduct(Product product)
    {
        var response = new ServiceResponse<Product>();
        var products = await _context.Products.FirstOrDefaultAsync(p => p.Sku == product.Sku);

        if (products != null)
        {
            response.Success = false;
            response.Message = "El libro digitado con ese SKU ya existe en el sistema";
            return response;
        }
        else
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            
            response.Data = product;
            response.Success = true;
            response.Message = "Libro Añadido a el sistema";
            return response;
        }
    }

    public async Task<ServiceResponse<Product>> UpdateProduct(Guid id, Product product)
    {
        var response = new ServiceResponse<Product>();
        var productExists = await _context.Products.FindAsync(id);

        if (productExists != null)
        {
            productExists.Name = product.Name;
            productExists.Category = product.Category;
            productExists.Price = product.Price;
            productExists.Description = product.Description;
            productExists.Quantity = product.Quantity;
            productExists.Status = product.Status;
            productExists.UpdatedAt = DateTime.UtcNow;
            
            response.Data = productExists;
            response.Success = true;
            response.Message = "Libro Actualizado en el sistema";
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "No se encontro el producto...";
            return response;
        }
        
    }

    public async Task<ServiceResponse<Product>> DeleteProduct(Guid id)
    {
        var response = new ServiceResponse<Product>();
        var productExists = await _context.Products.FindAsync(id);

        if (productExists != null)
        {
            _context.Products.Remove(productExists);
            await _context.SaveChangesAsync();
            
            response.Data = productExists;
            response.Success = true;
            response.Message = "Producto Eliminado con exito..";
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "No se encontro el producto...";
            return response;
        }
        
    }
}