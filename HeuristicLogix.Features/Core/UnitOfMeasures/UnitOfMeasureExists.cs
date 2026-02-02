using MediatR;
using Microsoft.EntityFrameworkCore;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Query to check if a unit of measure exists by its identifier.
/// </summary>
/// <param name="UnitOfMeasureId">The unit of measure identifier to check</param>
public sealed record UnitOfMeasureExistsQuery(int UnitOfMeasureId) : IRequest<bool>;

/// <summary>
/// Handler for checking unit of measure existence by ID.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class UnitOfMeasureExistsHandler(DbContext context) 
    : IRequestHandler<UnitOfMeasureExistsQuery, bool>
{
    public async Task<bool> Handle(
        UnitOfMeasureExistsQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<UnitOfMeasure>()
            .AnyAsync(
                u => u.UnitOfMeasureId == request.UnitOfMeasureId, 
                cancellationToken);
    }
}
