using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Command to delete a category from the Inventory schema.
/// Enforces referential integrity: prevents deletion if category is in use by items.
/// </summary>
/// <param name="CategoryId">The identifier of the category to delete</param>
public sealed record DeleteCategoryCommand(int CategoryId) : IRequest<bool>;

/// <summary>
/// Handler for deleting a category.
/// Validates referential integrity before deletion.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class DeleteCategoryHandler(DbContext context) 
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCategoryCommand request, 
        CancellationToken cancellationToken)
    {
        // Fetch the category
        var category = await context.Set<Category>()
            .FirstOrDefaultAsync(
                c => c.CategoryId == request.CategoryId, 
                cancellationToken);

        if (category == null)
        {
            return false;
        }

        // Strict validation: Check if category is used by any items
        var hasItems = await context.Set<Item>()
            .AnyAsync(
                i => i.CategoryId == request.CategoryId, 
                cancellationToken);

        if (hasItems)
        {
            throw new InvalidOperationException(
                $"Cannot delete category '{category.CategoryName}' because it is used by one or more items");
        }

        // Remove and persist
        context.Set<Category>().Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
