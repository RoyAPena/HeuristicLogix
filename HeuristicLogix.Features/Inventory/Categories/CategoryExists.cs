using MediatR;
using Microsoft.EntityFrameworkCore;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Query to check if a category exists by its identifier.
/// </summary>
/// <param name="CategoryId">The category identifier to check</param>
public sealed record CategoryExistsQuery(int CategoryId) : IRequest<bool>;

/// <summary>
/// Handler for checking category existence by ID.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class CategoryExistsHandler(DbContext context) 
    : IRequestHandler<CategoryExistsQuery, bool>
{
    public async Task<bool> Handle(
        CategoryExistsQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<Category>()
            .AnyAsync(
                c => c.CategoryId == request.CategoryId, 
                cancellationToken);
    }
}
