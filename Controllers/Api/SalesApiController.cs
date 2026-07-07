using Firmeza.DTOs;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Controllers.Api;

[ApiController]
[Route("api/ventas")]
[Authorize]
public class SalesApiController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ICustomerService _customerService;
    private readonly IEmailService _emailService;

    public SalesApiController(ISaleService saleService, ICustomerService customerService, IEmailService emailService)
    {
        _saleService = saleService;
        _customerService = customerService;
        _emailService = emailService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll(DateTime? from, DateTime? to, SaleStatus? status)
    {
        var response = await _saleService.GetAllSales(from, to, status);
        return Ok(response.Data.Select(ToDto));
    }

    [HttpGet("mis-compras")]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetMine()
    {
        var customerId = GetCustomerId();
        if (customerId == null)
        {
            return Unauthorized();
        }

        var response = await _saleService.GetSalesByCustomer(customerId.Value);
        return Ok(response.Data.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDto>> GetOne(Guid id)
    {
        var response = await _saleService.GetSale(id);
        if (!response.Success || response.Data == null)
        {
            return NotFound(new { message = response.Message });
        }

        if (!User.IsInRole("Admin"))
        {
            var customerId = GetCustomerId();
            if (customerId == null || response.Data.CustomerId != customerId.Value)
            {
                return Forbid();
            }
        }

        return Ok(ToDto(response.Data));
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(SaleCreateDto dto)
    {
        var customerId = GetCustomerId();
        if (customerId == null)
        {
            return Unauthorized();
        }

        var response = await _saleService.CreateSale(customerId.Value, dto.Items);
        if (!response.Success || response.Data == null)
        {
            return BadRequest(new { message = response.Message });
        }

        var customerResponse = await _customerService.GetCustomer(customerId.Value);
        if (customerResponse.Success && customerResponse.Data != null)
        {
            await _emailService.SendSaleConfirmationAsync(customerResponse.Data.Email, customerResponse.Data.UserName, response.Data);
        }

        return CreatedAtAction(nameof(GetOne), new { id = response.Data.Id }, ToDto(response.Data));
    }

    private Guid? GetCustomerId()
    {
        var claim = User.FindFirst("customerId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static SaleDto ToDto(Sale sale) => new()
    {
        Id = sale.Id,
        SaleNumber = sale.SaleNumber,
        SaleDate = sale.SaleDate,
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.UserName ?? string.Empty,
        SubTotal = sale.SubTotal,
        Tax = sale.Tax,
        Total = sale.Total,
        Status = sale.Status,
        Details = sale.SaleDetails.Select(sd => new SaleDetailDto
        {
            ProductId = sd.ProductId,
            ProductName = sd.Product?.Name ?? string.Empty,
            Quantity = sd.Quantity,
            UnitPrice = sd.UnitPrice,
            SubTotal = sd.SubTotal,
        }).ToList(),
    };
}
