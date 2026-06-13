using Firmeza.Enums;

namespace Firmeza.Models;

public class Customer : BaseEntity
{
    public string UserName { get; set; }
    public string Document { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }
    
}