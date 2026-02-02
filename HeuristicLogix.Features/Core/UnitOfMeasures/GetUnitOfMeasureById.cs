using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Query to retrieve a single unit of measure by its identifier.
/// Returns null if not found.
/// </summary>
/// <param name="UnitOfMeasureId">The unit of measure identifier (int-based for Core schema)</param>
public sealed record GetUnitOfMeasureByIdQuery(int UnitOfMeasureId) : IRequest<UnitOfMeasure?>;

/// <summary>
/// Handler for retrieving a unit of measure by ID.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class GetUnitOfMeasureByIdHandler(DbContext context) 
    : IRequestHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasure?>
{
    public async Task<UnitOfMeasure?> Handle(
        GetUnitOfMeasureByIdQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<UnitOfMeasure>()
            .FirstOrDefaultAsync(
                u => u.UnitOfMeasureId == request.UnitOfMeasureId, 
                cancellationToken);
    }
}
