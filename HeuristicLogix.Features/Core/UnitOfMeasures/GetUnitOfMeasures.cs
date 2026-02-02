using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Query to retrieve all units of measure from the Core schema.
/// Returns ordered list by UnitOfMeasureName.
/// </summary>
public sealed record GetUnitOfMeasuresQuery : IRequest<IEnumerable<UnitOfMeasure>>;

/// <summary>
/// Handler for retrieving all units of measure.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class GetUnitOfMeasuresHandler(DbContext context) 
    : IRequestHandler<GetUnitOfMeasuresQuery, IEnumerable<UnitOfMeasure>>
{
    public async Task<IEnumerable<UnitOfMeasure>> Handle(
        GetUnitOfMeasuresQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<UnitOfMeasure>()
            .OrderBy(u => u.UnitOfMeasureName)
            .ToListAsync(cancellationToken);
    }
}
