using MediatR;
using Microsoft.EntityFrameworkCore;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Query to check if a category exists by name.
/// Supports optional exclusion of a specific category (for update scenarios).
/// </summary>
/// <param name="CategoryName">The category name to check</param>
/// <param name="ExcludeCategoryId">Optional category ID to exclude from the check</param>
public sealed record CategoryExistsByNameQuery(
    string CategoryName, 
    int? ExcludeCategoryId = null) : IRequest<bool>;

/// <summary>
/// Handler for checking category existence by name.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class CategoryExistsByNameHandler(DbContext context) 
    : IRequestHandler<CategoryExistsByNameQuery, bool>
{
    public async Task<bool> Handle(
        CategoryExistsByNameQuery request, 
        CancellationToken cancellationToken)
    {
        var query = context.Set<Category>()
            .Where(c => c.CategoryName == request.CategoryName);

        if (request.ExcludeCategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId != request.ExcludeCategoryId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
