using HeuristicLogix.Features.Core.UnitOfMeasures;
using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Bridge service for UnitOfMeasure operations.
/// Delegates all operations to MediatR handlers (Vertical Slice Architecture).
/// Maintains IUnitOfMeasureService contract to avoid breaking UI components.
/// </summary>
public class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfMeasureService> _logger;

    public UnitOfMeasureService(IMediator mediator, ILogger<UnitOfMeasureService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all units of measure");
        return await _mediator.Send(new GetUnitOfMeasuresQuery());
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(int unitOfMeasureId)
    {
        _logger.LogInformation("Retrieving unit of measure with ID: {UnitOfMeasureId}", unitOfMeasureId);
        return await _mediator.Send(new GetUnitOfMeasureByIdQuery(unitOfMeasureId));
    }

    public async Task<UnitOfMeasure> CreateAsync(UnitOfMeasure unitOfMeasure)
    {
        _logger.LogInformation("Creating new unit of measure: {UnitName} ({Symbol})", 
            unitOfMeasure.UnitOfMeasureName, unitOfMeasure.UnitOfMeasureSymbol);
        var created = await _mediator.Send(new CreateUnitOfMeasureCommand(unitOfMeasure));
        _logger.LogInformation("Unit of measure created successfully with ID: {UnitOfMeasureId}", 
            created.UnitOfMeasureId);
        return created;
    }

    public async Task<UnitOfMeasure> UpdateAsync(UnitOfMeasure unitOfMeasure)
    {
        _logger.LogInformation("Updating unit of measure: {UnitOfMeasureId}", unitOfMeasure.UnitOfMeasureId);
        var updated = await _mediator.Send(new UpdateUnitOfMeasureCommand(unitOfMeasure));
        _logger.LogInformation("Unit of measure updated successfully: {UnitOfMeasureId}", 
            updated.UnitOfMeasureId);
        return updated;
    }

    public async Task<bool> DeleteAsync(int unitOfMeasureId)
    {
        _logger.LogInformation("Deleting unit of measure: {UnitOfMeasureId}", unitOfMeasureId);
        var deleted = await _mediator.Send(new DeleteUnitOfMeasureCommand(unitOfMeasureId));
        
        if (!deleted)
        {
            _logger.LogWarning("Unit of measure not found: {UnitOfMeasureId}", unitOfMeasureId);
        }
        else
        {
            _logger.LogInformation("Unit of measure deleted successfully: {UnitOfMeasureId}", unitOfMeasureId);
        }
        
        return deleted;
    }

    public async Task<bool> ExistsAsync(int unitOfMeasureId)
    {
        return await _mediator.Send(new UnitOfMeasureExistsQuery(unitOfMeasureId));
    }

    public async Task<bool> ExistsBySymbolAsync(string symbol, int? excludeUnitId = null)
    {
        return await _mediator.Send(new UnitOfMeasureExistsBySymbolQuery(symbol, excludeUnitId));
    }
}

