using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Command to update an existing category in the Inventory schema.
/// Enforces business rules: category name must be unique (excluding current).
/// </summary>
/// <param name="Category">The category entity with updated values</param>
public sealed record UpdateCategoryCommand(Category Category) : IRequest<Category>;

/// <summary>
/// Handler for updating an existing category.
/// Validates duplicate names (excluding current category) before persisting.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class UpdateCategoryHandler(DbContext context) 
    : IRequestHandler<UpdateCategoryCommand, Category>
{
    public async Task<Category> Handle(
        UpdateCategoryCommand request, 
        CancellationToken cancellationToken)
    {
        // Strict validation: Check for duplicate category name (excluding current)
        var duplicateExists = await context.Set<Category>()
            .AnyAsync(
                c => c.CategoryName == request.Category.CategoryName 
                     && c.CategoryId != request.Category.CategoryId, 
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Another category with name '{request.Category.CategoryName}' already exists");
        }

        // Update and persist
        context.Set<Category>().Update(request.Category);
        await context.SaveChangesAsync(cancellationToken);

        return request.Category;
    }
}
