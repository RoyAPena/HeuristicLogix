using FluentValidation;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Modules.Inventory.Validators;

/// <summary>
/// FluentValidation validator for Category entity.
/// </summary>
public class CategoryValidator : AbstractValidator<Category>
{
    public CategoryValidator()
    {
        RuleFor(c => c.CategoryName)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(300)
            .WithMessage("Category name cannot exceed 300 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Category name cannot be only whitespace");
    }
}
