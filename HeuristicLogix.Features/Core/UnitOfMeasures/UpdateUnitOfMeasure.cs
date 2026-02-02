using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Command to update an existing unit of measure in the Core schema.
/// Enforces business rules: unit of measure symbol must be unique (excluding current).
/// </summary>
/// <param name="UnitOfMeasure">The unit of measure entity with updated values</param>
public sealed record UpdateUnitOfMeasureCommand(UnitOfMeasure UnitOfMeasure) : IRequest<UnitOfMeasure>;

/// <summary>
/// Handler for updating an existing unit of measure.
/// Validates duplicate symbols (excluding current unit) before persisting.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class UpdateUnitOfMeasureHandler(DbContext context) 
    : IRequestHandler<UpdateUnitOfMeasureCommand, UnitOfMeasure>
{
    public async Task<UnitOfMeasure> Handle(
        UpdateUnitOfMeasureCommand request, 
        CancellationToken cancellationToken)
    {
        // Strict validation: Check for duplicate symbol (excluding current unit)
        var duplicateExists = await context.Set<UnitOfMeasure>()
            .AnyAsync(
                u => u.UnitOfMeasureSymbol == request.UnitOfMeasure.UnitOfMeasureSymbol 
                     && u.UnitOfMeasureId != request.UnitOfMeasure.UnitOfMeasureId, 
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Another unit of measure with symbol '{request.UnitOfMeasure.UnitOfMeasureSymbol}' already exists");
        }

        // Update and persist
        context.Set<UnitOfMeasure>().Update(request.UnitOfMeasure);
        await context.SaveChangesAsync(cancellationToken);

        return request.UnitOfMeasure;
    }
}
