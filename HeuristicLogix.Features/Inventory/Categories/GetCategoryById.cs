using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Query to retrieve a single category by its identifier.
/// Returns null if not found.
/// </summary>
/// <param name="CategoryId">The category identifier (int-based for Inventory schema)</param>
public sealed record GetCategoryByIdQuery(int CategoryId) : IRequest<Category?>;

/// <summary>
/// Handler for retrieving a category by ID.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class GetCategoryByIdHandler(DbContext context) 
    : IRequestHandler<GetCategoryByIdQuery, Category?>
{
    public async Task<Category?> Handle(
        GetCategoryByIdQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<Category>()
            .FirstOrDefaultAsync(
                c => c.CategoryId == request.CategoryId, 
                cancellationToken);
    }
}
