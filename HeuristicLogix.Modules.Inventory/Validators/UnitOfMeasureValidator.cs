using FluentValidation;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Modules.Inventory.Validators;

/// <summary>
/// FluentValidation validator for UnitOfMeasure entity.
/// </summary>
public class UnitOfMeasureValidator : AbstractValidator<UnitOfMeasure>
{
    public UnitOfMeasureValidator()
    {
        RuleFor(u => u.UnitOfMeasureName)
            .NotEmpty()
            .WithMessage("Unit of measure name is required")
            .MaximumLength(200)
            .WithMessage("Unit of measure name cannot exceed 200 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Unit of measure name cannot be only whitespace");

        RuleFor(u => u.UnitOfMeasureSymbol)
            .NotEmpty()
            .WithMessage("Unit of measure symbol is required")
            .MaximumLength(20)
            .WithMessage("Unit of measure symbol cannot exceed 20 characters")
            .Must(symbol => !string.IsNullOrWhiteSpace(symbol))
            .WithMessage("Unit of measure symbol cannot be only whitespace")
            .Matches("^[a-zA-Z0-9³²]+$")
            .WithMessage("Unit of measure symbol can only contain letters, numbers, and special characters (³, ²)");
    }
}
