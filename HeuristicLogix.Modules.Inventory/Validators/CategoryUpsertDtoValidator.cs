using FluentValidation;
using HeuristicLogix.Shared.DTOs;

namespace HeuristicLogix.Modules.Inventory.Validators;

/// <summary>
/// FluentValidation validator for CategoryUpsertDto.
/// Validates business rules before persisting data.
/// </summary>
public class CategoryUpsertDtoValidator : AbstractValidator<CategoryUpsertDto>
{
    public CategoryUpsertDtoValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .WithMessage("El nombre de la categoría es requerido")
            .MaximumLength(300)
            .WithMessage("El nombre no puede exceder 300 caracteres")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("El nombre no puede contener solo espacios en blanco");
    }
}
