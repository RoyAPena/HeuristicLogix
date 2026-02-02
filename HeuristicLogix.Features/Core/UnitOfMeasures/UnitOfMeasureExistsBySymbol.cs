using MediatR;
using Microsoft.EntityFrameworkCore;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Query to check if a unit of measure exists by symbol.
/// Supports optional exclusion of a specific unit (for update scenarios).
/// </summary>
/// <param name="Symbol">The unit of measure symbol to check</param>
/// <param name="ExcludeUnitId">Optional unit of measure ID to exclude from the check</param>
public sealed record UnitOfMeasureExistsBySymbolQuery(
    string Symbol, 
    int? ExcludeUnitId = null) : IRequest<bool>;

/// <summary>
/// Handler for checking unit of measure existence by symbol.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class UnitOfMeasureExistsBySymbolHandler(DbContext context) 
    : IRequestHandler<UnitOfMeasureExistsBySymbolQuery, bool>
{
    public async Task<bool> Handle(
        UnitOfMeasureExistsBySymbolQuery request, 
        CancellationToken cancellationToken)
    {
        var query = context.Set<UnitOfMeasure>()
            .Where(u => u.UnitOfMeasureSymbol == request.Symbol);

        if (request.ExcludeUnitId.HasValue)
        {
            query = query.Where(u => u.UnitOfMeasureId != request.ExcludeUnitId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
