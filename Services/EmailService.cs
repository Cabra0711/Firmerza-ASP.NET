using System.Net;
using System.Net.Mail;
using Firmeza.Models;
using Firmeza.Services.Interfaces;

namespace Firmeza.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSaleConfirmationAsync(string toEmail, string customerName, Sale sale)
    {
        var host = _configuration["SmtpSettings:Host"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogInformation("SMTP no configurado o correo vacío; se omite el envío de confirmación para la venta {SaleNumber}.", sale.SaleNumber);
            return;
        }

        var port = int.TryParse(_configuration["SmtpSettings:Port"], out var parsedPort) ? parsedPort : 587;
        var user = _configuration["SmtpSettings:User"];
        var password = _configuration["SmtpSettings:Password"];
        var from = _configuration["SmtpSettings:From"] ?? user ?? "no-reply@firmeza.com";
        var enableSsl = !bool.TryParse(_configuration["SmtpSettings:EnableSsl"], out var parsedSsl) || parsedSsl;

        using var message = new MailMessage(from, toEmail)
        {
            Subject = $"Firmeza - Confirmación de compra {sale.SaleNumber}",
            Body = $"Hola {customerName},\n\n" +
                   $"Tu compra {sale.SaleNumber} por un total de ${sale.Total:N2} fue registrada con éxito el {sale.SaleDate:yyyy-MM-dd HH:mm}.\n\n" +
                   "Gracias por comprar en Firmeza.",
            IsBodyHtml = false,
        };

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
        };

        if (!string.IsNullOrWhiteSpace(user))
        {
            client.Credentials = new NetworkCredential(user, password);
        }

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el correo de confirmación para la venta {SaleNumber}.", sale.SaleNumber);
        }
    }
}
