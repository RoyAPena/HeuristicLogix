using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Features.Core.UnitOfMeasures;

/// <summary>
/// Command to delete a unit of measure from the Core schema.
/// Enforces referential integrity: prevents deletion if unit is in use by items or conversions.
/// </summary>
/// <param name="UnitOfMeasureId">The identifier of the unit of measure to delete</param>
public sealed record DeleteUnitOfMeasureCommand(int UnitOfMeasureId) : IRequest<bool>;

/// <summary>
/// Handler for deleting a unit of measure.
/// Validates referential integrity before deletion.
/// Uses primary constructor (C# 12+) for dependency injection.
/// </summary>
public sealed class DeleteUnitOfMeasureHandler(DbContext context) 
    : IRequestHandler<DeleteUnitOfMeasureCommand, bool>
{
    public async Task<bool> Handle(
        DeleteUnitOfMeasureCommand request, 
        CancellationToken cancellationToken)
    {
        // Fetch the unit of measure
        var unit = await context.Set<UnitOfMeasure>()
            .FirstOrDefaultAsync(
                u => u.UnitOfMeasureId == request.UnitOfMeasureId, 
                cancellationToken);

        if (unit == null)
        {
            return false;
        }

        // Strict validation: Check if unit is used by any items
        var hasItemsAsBase = await context.Set<Item>()
            .AnyAsync(
                i => i.BaseUnitOfMeasureId == request.UnitOfMeasureId, 
                cancellationToken);
        
        var hasItemsAsSales = await context.Set<Item>()
            .AnyAsync(
                i => i.DefaultSalesUnitOfMeasureId == request.UnitOfMeasureId, 
                cancellationToken);
        
        var hasConversions = await context.Set<ItemUnitConversion>()
            .AnyAsync(
                c => c.FromUnitOfMeasureId == request.UnitOfMeasureId 
                     || c.ToUnitOfMeasureId == request.UnitOfMeasureId, 
                cancellationToken);

        if (hasItemsAsBase || hasItemsAsSales || hasConversions)
        {
            throw new InvalidOperationException(
                $"Cannot delete unit of measure '{unit.UnitOfMeasureName}' because it is used by items or conversions");
        }

        // Remove and persist
        context.Set<UnitOfMeasure>().Remove(unit);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
