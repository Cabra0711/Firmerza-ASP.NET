using Firmeza.Data;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Response;
using Firmeza.Services.Interfaces;
using Firmeza.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


namespace Firmeza.Services;


public class LoginService : ILoginService
{
    private readonly ApplicationDbContext _context;
    private readonly CustomerValidator _customerValidator; 
    private readonly IConfiguration _configuration;

    public LoginService(ApplicationDbContext context, CustomerValidator customerValidator, IConfiguration configuration)
    {
        _customerValidator = customerValidator;
        _context = context;
        _configuration = configuration;
    }


    public async Task<ServiceResponse<Customer>> CreateCustomer(Customer customer)
    {
        
        
        var response = new ServiceResponse<Customer>();
        var customerValidator = _customerValidator.Validate(customer);
        if (!customerValidator.IsValid)
        {
            response.Success = false;
            return response;
        }
        
        var customerExists = await _context.Customers.FirstOrDefaultAsync(c=> c.Email == customer.Email);

        if (customerExists != null)
        {
            response.Message = "Este usuario ya existe en el sistema";
            response.Success = false;
            return response;
        }
        else
        {
            customer.Role = UserRole.Customer;
            customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            
            response.Message = "Usuario registrado con exito";
            response.Success = true;
            response.Data = customer;
            
            return response;
        }
    }

    public async Task<ServiceResponse<Customer>> Login(string username, string password)
    {
        var response = new ServiceResponse<Customer>();
        var customerExists = await _context.Customers.SingleOrDefaultAsync(c => EF.Functions.ILike(c.UserName, username));
        
        if (customerExists == null)
        {
            response.Success = false;
            response.Message = "Ingrese sus credenciales nuevamente";
            return response;
        }
        
        var verificationResult = BCrypt.Net.BCrypt.Verify(password, customerExists.Password);
        
        if (verificationResult == true)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var Key = System.Text.Encoding.UTF8.GetBytes(secretKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Key), SecurityAlgorithms.HmacSha256Signature);

            var Data = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, customerExists.UserName),
                    new Claim(ClaimTypes.Role, customerExists.Role.ToString()),
                }),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials,
            };
            
            var Object = tokenHandler.CreateToken(Data);
            var token = tokenHandler.WriteToken(Object);

            customerExists.Token = token;
            response.Success = true;
            response.Message = "Login Exitoso redireccionando...";
            response.Data = customerExists;
            
        }
        else
        {
            response.Success = false;
            response.Message = "Ingrese sus credenciales nuevamente";
        }
        return response;
    }
}