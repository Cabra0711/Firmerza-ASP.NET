using Firmeza.DTOs;
using Firmeza.Models;
using Firmeza.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Controllers.Api;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = "Admin")]
public class CustomersApiController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersApiController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(string? search)
    {
        var response = await _customerService.GetAllCustomers(search);
        return Ok(response.Data.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetOne(Guid id)
    {
        var response = await _customerService.GetCustomer(id);
        if (!response.Success || response.Data == null)
        {
            return NotFound(new { message = response.Message });
        }
        return Ok(ToDto(response.Data));
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id)
    {
        var response = await _customerService.SetActive(id, false);
        if (!response.Success)
        {
            return NotFound(new { message = response.Message });
        }
        return Ok(ToDto(response.Data!));
    }

    [HttpPost("{id:guid}/enable")]
    public async Task<IActionResult> Enable(Guid id)
    {
        var response = await _customerService.SetActive(id, true);
        if (!response.Success)
        {
            return NotFound(new { message = response.Message });
        }
        return Ok(ToDto(response.Data!));
    }

    private static CustomerDto ToDto(Customer customer) => new()
    {
        Id = customer.Id,
        UserName = customer.UserName,
        Email = customer.Email,
        Document = customer.Document,
        Role = customer.Role,
        IsActive = customer.IsActive,
    };
}
