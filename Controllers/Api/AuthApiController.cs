using Firmeza.DTOs;
using Firmeza.Models;
using Firmeza.Services.Interfaces;
using Firmeza.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly CustomerValidator _customerValidator;

    public AuthApiController(ILoginService loginService, CustomerValidator customerValidator)
    {
        _loginService = loginService;
        _customerValidator = customerValidator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var response = await _loginService.Login(request.Username, request.Password);

        if (!response.Success || response.Data == null)
        {
            return Unauthorized(new { message = response.Message });
        }

        return Ok(new LoginResponseDto
        {
            Token = response.Data.Token ?? string.Empty,
            Username = response.Data.UserName,
            Role = response.Data.Role.ToString(),
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<CustomerDto>> Register(RegisterRequestDto request)
    {
        var customer = new Customer
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password,
            Document = request.Document,
        };

        var validation = await _customerValidator.ValidateAsync(customer);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });
        }

        var response = await _loginService.CreateCustomer(customer);
        if (!response.Success || response.Data == null)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new CustomerDto
        {
            Id = response.Data.Id,
            UserName = response.Data.UserName,
            Email = response.Data.Email,
            Document = response.Data.Document,
            Role = response.Data.Role,
            IsActive = response.Data.IsActive,
        });
    }
}
