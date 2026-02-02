using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Command to create a new category in the Inventory schema.
/// Enforces business rules: category name must be unique.
/// </summary>
/// <param name="Category">The category entity to create</param>
public sealed record CreateCategoryCommand(Category Category) : IRequest<Category>;

/// <summary>
/// Handler for creating a new category.
/// Validates duplicate names before persisting.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class CreateCategoryHandler(DbContext context) 
    : IRequestHandler<CreateCategoryCommand, Category>
{
    public async Task<Category> Handle(
        CreateCategoryCommand request, 
        CancellationToken cancellationToken)
    {
        // Strict validation: Check for duplicate category name
        var categoryExists = await context.Set<Category>()
            .AnyAsync(
                c => c.CategoryName == request.Category.CategoryName, 
                cancellationToken);

        if (categoryExists)
        {
            throw new InvalidOperationException(
                $"Category with name '{request.Category.CategoryName}' already exists");
        }

        // Add and persist
        context.Set<Category>().Add(request.Category);
        await context.SaveChangesAsync(cancellationToken);

        return request.Category;
    }
}
