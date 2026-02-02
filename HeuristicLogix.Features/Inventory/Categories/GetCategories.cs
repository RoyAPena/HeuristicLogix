using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Inventory.Categories;

/// <summary>
/// Query to retrieve all categories from the Inventory schema.
/// Returns ordered list by CategoryName.
/// </summary>
public sealed record GetCategoriesQuery : IRequest<IEnumerable<Category>>;

/// <summary>
/// Handler for retrieving all categories.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class GetCategoriesHandler(DbContext context) 
    : IRequestHandler<GetCategoriesQuery, IEnumerable<Category>>
{
    public async Task<IEnumerable<Category>> Handle(
        GetCategoriesQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<Category>()
            .OrderBy(c => c.CategoryName)
            .ToListAsync(cancellationToken);
    }
}
