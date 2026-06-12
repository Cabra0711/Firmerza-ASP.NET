using Firmeza.Models;
using FluentValidation;

namespace Firmeza.Validators;

public class CustomerValidator : AbstractValidator<Customer>
{
    public  CustomerValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");

        // Email
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo no puede superar los 150 caracteres.");

        // Password
        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(32).WithMessage("La contraseña no puede superar los 32 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe tener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe tener al menos una letra minúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe tener al menos un número.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe tener al menos un carácter especial (ej: @, #, $, %).");
    }
}