using Firmeza.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Firmeza.Enums;
using Firmeza.Models;


namespace Firmeza.Controllers;


[Route("firmeza")]
public class FirmezaController : Controller
{
    private readonly IProductService _productService;
    private readonly ILoginService _loginService;
    public FirmezaController(ILoginService loginService,  IProductService productService)
    {
        _productService = productService;
        _loginService = loginService;
    }
    
    [AllowAnonymous]
    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        
        
        var response = await _loginService.Login(username, password);
        
        if (response.Success)
        {
            HttpContext.Session.SetString("Username", username);
            if (!string.IsNullOrEmpty(response.Data.Token))
            {
                HttpContext.Session.SetString("JWToken", response.Data.Token);
            }

            if (response.Data.Role == UserRole.Customer)
            {
                return RedirectToAction("Landing", "Firmeza");
            }
            else
            {
                return RedirectToAction("Admin", "Firmeza");
            }
        }
        ViewBag.Error = response?.Message ?? "Credenciales incorrectas o invalidas intente de nuevo porfavor.";
        return View();
    }
    
    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<IActionResult> Register(Customer customer)
    {
        var validator = new Validators.CustomerValidator();
        
        var validationResult = await validator.ValidateAsync(customer);
        
        if (!validationResult.IsValid)
        {

            ViewBag.ErrorValidation = string.Join("<br/>", validationResult.Errors.Select(e => e.ErrorMessage));
            return View("Login", customer); 
        }
        try
        {
            var response = await _loginService.CreateCustomer(customer);
            
            if (response.Success)
            {
                ViewBag.Success = "¡Cuenta creada! Ya podés iniciar sesión.";
                return View("Login");
            }
            
            ViewBag.Error = response.Message ?? "No se pudo crear el usuario, intente de nuevo.";
            return View("Login", customer);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") 
        {

            ViewBag.Error = "El correo electrónico o el nombre de usuario ya se encuentran registrados.";
            return View("Login", customer);
        }
        catch (Exception ex)
        {

            ViewBag.Error = $" Explotó el sistema: {ex.Message}";
            return View("Login", customer);
        }
    }
    
    
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JWToken");
        return RedirectToAction("Login", "Firmeza");
    }
    
    [Authorize] 
    [HttpGet("Admin")]
    public async Task<IActionResult> Admin()
    {
        var response = await _productService.GetAllProducts();
        return View(response);
    }

    [Authorize]
    [HttpPost("Admin/create")]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        product.CreatedAt =  DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await  _productService.CreateProduct(product);
        return RedirectToAction("Admin", "Firmeza");
        
    }

    [Authorize]
    [HttpPost("Admin/delete/{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProduct(id);
        return RedirectToAction("Admin", "Firmeza");
    }

    [Authorize]
    [HttpPost("Admin/edit/{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id ,Product product)
    {
        product.Id = id;
        product.UpdatedAt = DateTime.UtcNow;
        await _productService.UpdateProduct(product.Id, product);
        return RedirectToAction("Admin", "Firmeza");
    }
    
    
    [Authorize] 
    [HttpGet("Admin-Customer")]
    public IActionResult Customer()
    {
        return View();
    }
    
    [Authorize] 
    [HttpGet("Admin-Sells")]
    public IActionResult Sells()
    {
        return View();
    }
    
    [AllowAnonymous]
    [HttpGet("Landing")]
    public IActionResult Landing()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet("Error401")]
    public IActionResult Error()
    {
        Response.StatusCode = 401;
        return View();
    }
    
    
}