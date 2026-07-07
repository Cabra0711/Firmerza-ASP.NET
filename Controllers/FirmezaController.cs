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
    private readonly ISaleService _saleService;
    private readonly ICustomerService _customerService;
    private readonly IReportService _reportService;
    private readonly Validators.ProductValidator _productValidator;
    public FirmezaController(ILoginService loginService,  IProductService productService, ISaleService saleService, ICustomerService customerService, IReportService reportService, Validators.ProductValidator productValidator)
    {
        _productService = productService;
        _loginService = loginService;
        _saleService = saleService;
        _customerService = customerService;
        _reportService = reportService;
        _productValidator = productValidator;
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
        var weekStart = DateTime.UtcNow.AddDays(-7);
        
        return View(response);
    }

    [Authorize]
    [HttpPost("Admin/create")]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            TempData["ProductError"] = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction("Admin", "Firmeza");
        }

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
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            TempData["ProductError"] = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction("Admin", "Firmeza");
        }

        product.Id = id;
        product.UpdatedAt = DateTime.UtcNow;
        await _productService.UpdateProduct(product.Id, product);
        return RedirectToAction("Admin", "Firmeza");
    }
    
    
    [Authorize]
    [HttpPost("Admin/products/import")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ProductError"] = "Debe seleccionar un archivo Excel (.xlsx) para importar.";
            return RedirectToAction("Admin", "Firmeza");
        }

        await using var stream = file.OpenReadStream();
        var result = await _reportService.ImportProducts(stream);

        TempData["ProductError"] = result.Errors.Count > 0
            ? $"Importados {result.SuccessCount} de {result.TotalRows}. Errores: " + string.Join(" | ", result.Errors)
            : $"Importados {result.SuccessCount} de {result.TotalRows} productos con éxito.";

        return RedirectToAction("Admin", "Firmeza");
    }

    [Authorize]
    [HttpGet("Admin/products/export")]
    public async Task<IActionResult> ExportProducts(string format, ProductCategory? category, ProductStatus? status)
    {
        var response = await _productService.GetAllProducts();
        var products = (response.Data ?? Enumerable.Empty<Product>()).AsEnumerable();

        if (category.HasValue)
        {
            products = products.Where(p => p.Category == category.Value);
        }
        if (status.HasValue)
        {
            products = products.Where(p => p.Status == status.Value);
        }

        if (format == "pdf")
        {
            var pdfBytes = _reportService.ExportProductsToPdf(products);
            return File(pdfBytes, "application/pdf", "productos.pdf");
        }

        var excelBytes = _reportService.ExportProductsToExcel(products);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "productos.xlsx");
    }

    [Authorize]
    [HttpGet("Admin-Customer")]
    public async Task<IActionResult> Customer(string? search)
    {
        var response = await _customerService.GetAllCustomers(search);
        return View(response);
    }

    [Authorize]
    [HttpPost("Admin/customer/disable/{id}")]
    public async Task<IActionResult> DisableCustomer(Guid id)
    {
        await _customerService.SetActive(id, false);
        return RedirectToAction("Customer", "Firmeza");
    }

    [Authorize]
    [HttpPost("Admin/customer/enable/{id}")]
    public async Task<IActionResult> EnableCustomer(Guid id)
    {
        await _customerService.SetActive(id, true);
        return RedirectToAction("Customer", "Firmeza");
    }
    
    [Authorize]
    [HttpPost("Admin/customers/import")]
    public async Task<IActionResult> ImportCustomers(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ProductError"] = "Debe seleccionar un archivo Excel (.xlsx) para importar.";
            return RedirectToAction("Customer", "Firmeza");
        }

        await using var stream = file.OpenReadStream();
        var result = await _reportService.ImportCustomers(stream);

        TempData["ProductError"] = result.Errors.Count > 0
            ? $"Importados {result.SuccessCount} de {result.TotalRows}. Errores: " + string.Join(" | ", result.Errors)
            : $"Importados {result.SuccessCount} de {result.TotalRows} clientes con éxito.";

        return RedirectToAction("Customer", "Firmeza");
    }

    [Authorize]
    [HttpGet("Admin/customers/export")]
    public async Task<IActionResult> ExportCustomers(string format, string? search)
    {
        var response = await _customerService.GetAllCustomers(search);
        var customers = response.Data ?? Enumerable.Empty<Customer>();

        if (format == "pdf")
        {
            var pdfBytes = _reportService.ExportCustomersToPdf(customers);
            return File(pdfBytes, "application/pdf", "clientes.pdf");
        }

        var excelBytes = _reportService.ExportCustomersToExcel(customers);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "clientes.xlsx");
    }

    [Authorize]
    [HttpGet("Admin/sales/export")]
    public async Task<IActionResult> ExportSales(string format, DateTime? from, DateTime? to, SaleStatus? status)
    {
        var response = await _saleService.GetAllSales(from, to, status);
        var sales = response.Data ?? Enumerable.Empty<Sale>();

        if (format == "pdf")
        {
            var pdfBytes = _reportService.ExportSalesToPdf(sales);
            return File(pdfBytes, "application/pdf", "ventas.pdf");
        }

        var excelBytes = _reportService.ExportSalesToExcel(sales);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ventas.xlsx");
    }

    [Authorize]
    [HttpGet("Admin-Sells")]
    public async Task<IActionResult> Sells(DateTime? from, DateTime? to, SaleStatus? status)
    {
        var response = await _saleService.GetAllSales(from, to, status);
        return View(response);
    }
    
    [AllowAnonymous]
    [HttpGet("Landing")]
    public async Task<IActionResult> Landing()
    {
        var response = await _productService.GetAllProducts(); 
        return View(response);
    }
    
    [AllowAnonymous]
    [HttpGet("Error401")]
    public IActionResult Error()
    {
        Response.StatusCode = 401;
        var token = HttpContext.Session.GetString("JWToken");
        var username = HttpContext.Session.GetString("Username");

        if (!string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Landing", "Firmeza");
        }
        
        return View();
    }
    
    
}