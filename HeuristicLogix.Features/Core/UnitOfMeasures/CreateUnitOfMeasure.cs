using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Command to create a new unit of measure in the Core schema.
/// Enforces business rules: unit of measure symbol must be unique.
/// </summary>
/// <param name="UnitOfMeasure">The unit of measure entity to create</param>
public sealed record CreateUnitOfMeasureCommand(UnitOfMeasure UnitOfMeasure) : IRequest<UnitOfMeasure>;

/// <summary>
/// Handler for creating a new unit of measure.
/// Validates duplicate symbols before persisting.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class CreateUnitOfMeasureHandler(DbContext context) 
    : IRequestHandler<CreateUnitOfMeasureCommand, UnitOfMeasure>
{
    public async Task<UnitOfMeasure> Handle(
        CreateUnitOfMeasureCommand request, 
        CancellationToken cancellationToken)
    {
        // Strict validation: Check for duplicate symbol
        var symbolExists = await context.Set<UnitOfMeasure>()
            .AnyAsync(
                u => u.UnitOfMeasureSymbol == request.UnitOfMeasure.UnitOfMeasureSymbol, 
                cancellationToken);

        if (symbolExists)
        {
            throw new InvalidOperationException(
                $"Unit of measure with symbol '{request.UnitOfMeasure.UnitOfMeasureSymbol}' already exists");
        }

        // Add and persist
        context.Set<UnitOfMeasure>().Add(request.UnitOfMeasure);
        await context.SaveChangesAsync(cancellationToken);

        return request.UnitOfMeasure;
    }
}
