using Firmeza.Models;
using FluentValidation;

namespace Firmeza.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public  ProductValidator()
    {
         RuleFor(p => p.Name)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .Length(5, 150).WithMessage("El nombre debe tener entre 5 y 150 caracteres.");

    
        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede pasar de 500 caracteres.");

       
        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("El precio debe ser un número mayor a cero.")
            .LessThanOrEqualTo(100000).WithMessage("El precio no puede exceder los $100,000.00.")
          
            .ScalePrecision(2, 10).WithMessage("El precio no puede tener más de 2 decimales.");

     
        RuleFor(p => p.Sku)
            .NotEmpty().WithMessage("El código SKU es obligatorio.")
         
            .Matches(@"^[A-Z]{3}-\d{4}$").WithMessage("El SKU debe cumplir el formato de 3 letras mayúsculas, un guion y 4 números (Ej: ABC-1234).");

       
        RuleFor(p => p.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("El inventario no puede ser un número negativo.")
           
            .Must((producto, cantidad) => cantidad >= 0).WithMessage("Cantidad inválida.");

       
        RuleFor(p => p.Category)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            
            .Must(BeAValidCategory).WithMessage("La categoría ingresada no está permitida. Usa: Electrónica, Ropa, Hogar o Deportes.");
    }
    private bool BeAValidCategory(string category)
    {
        var AllowedCategories = new List<string> { "Electrónica", "Ropa", "Hogar", "Deportes" };
        return AllowedCategories.Contains(category);
    }
}