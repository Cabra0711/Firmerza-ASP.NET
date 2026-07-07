using Firmeza.DTOs;
using Firmeza.Models;
using Firmeza.Services.Interfaces;
using Firmeza.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Controllers.Api;

[ApiController]
[Route("api/productos")]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ProductValidator _productValidator;

    public ProductsApiController(IProductService productService, ProductValidator productValidator)
    {
        _productService = productService;
        _productValidator = productValidator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var response = await _productService.GetAllProducts();
        return Ok(response.Data.Select(ToDto));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetOne(Guid id)
    {
        var response = await _productService.GetProduct(id);
        if (!response.Success || response.Data == null)
        {
            return NotFound(new { message = response.Message });
        }
        return Ok(ToDto(response.Data));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Sku = dto.Sku ?? string.Empty,
            Quantity = dto.Quantity,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
        };

        var validation = await _productValidator.ValidateAsync(product);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });
        }

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        var response = await _productService.CreateProduct(product);
        if (!response.Success || response.Data == null)
        {
            return BadRequest(new { message = response.Message });
        }

        return CreatedAtAction(nameof(GetOne), new { id = response.Data.Id }, ToDto(response.Data));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, ProductUpsertDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Sku = dto.Sku ?? string.Empty,
            Quantity = dto.Quantity,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
        };

        var validation = await _productValidator.ValidateAsync(product);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });
        }

        var response = await _productService.UpdateProduct(id, product);
        if (!response.Success || response.Data == null)
        {
            return NotFound(new { message = response.Message });
        }

        return Ok(ToDto(response.Data));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _productService.DeleteProduct(id);
        if (!response.Success)
        {
            return NotFound(new { message = response.Message });
        }
        return NoContent();
    }

    private static ProductDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Sku = product.Sku,
        Quantity = product.Quantity,
        Category = product.Category,
        ImageUrl = product.ImageUrl,
        Status = product.Status,
    };
}
