using Firmeza.Models;

namespace Firmeza.Services.Interfaces;

public interface IEmailService
{
    public Task SendSaleConfirmationAsync(string toEmail, string customerName, Sale sale);
}
