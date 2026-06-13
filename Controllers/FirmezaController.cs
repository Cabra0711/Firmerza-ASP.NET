using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Controllers;


[Route("firmeza")]
public class FirmezaController : Controller
{
    [AllowAnonymous]
    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }
    
    [Authorize] 
    [HttpGet("Admin")]
    public IActionResult Admin()
    {
        return View();
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