using FluentValidation;
using HeuristicLogix.Shared.DTOs;

namespace HeuristicLogix.Modules.Inventory.Validators;

/// <summary>
/// FluentValidation validator for UnitOfMeasureUpsertDto.
/// Validates business rules before persisting data.
/// </summary>
public class UnitOfMeasureUpsertDtoValidator : AbstractValidator<UnitOfMeasureUpsertDto>
{
    public UnitOfMeasureUpsertDtoValidator()
    {
        RuleFor(x => x.UnitOfMeasureName)
            .NotEmpty()
            .WithMessage("El nombre de la unidad es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre no puede exceder 200 caracteres")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("El nombre no puede contener solo espacios en blanco");

        RuleFor(x => x.UnitOfMeasureSymbol)
            .NotEmpty()
            .WithMessage("El símbolo es requerido")
            .MaximumLength(20)
            .WithMessage("El símbolo no puede exceder 20 caracteres")
            .Must(symbol => !string.IsNullOrWhiteSpace(symbol))
            .WithMessage("El símbolo no puede contener solo espacios en blanco")
            .Matches("^[a-zA-Z0-9³²]+$")
            .WithMessage("El símbolo solo puede contener letras, números y caracteres especiales (³, ²)");
    }
}
