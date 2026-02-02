using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Implementation of UnitOfMeasure service.
/// Handles CRUD operations for units of measure (int IDs).
/// </summary>
public class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly DbContext _context;
    private readonly ILogger<UnitOfMeasureService> _logger;

    public UnitOfMeasureService(DbContext context, ILogger<UnitOfMeasureService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all units of measure");
        return await _context.Set<UnitOfMeasure>()
            .OrderBy(u => u.UnitOfMeasureName)
            .ToListAsync();
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(int unitOfMeasureId)
    {
        _logger.LogInformation("Retrieving unit of measure with ID: {UnitOfMeasureId}", unitOfMeasureId);
        return await _context.Set<UnitOfMeasure>()
            .FirstOrDefaultAsync(u => u.UnitOfMeasureId == unitOfMeasureId);
    }

    public async Task<UnitOfMeasure> CreateAsync(UnitOfMeasure unitOfMeasure)
    {
        _logger.LogInformation("Creating new unit of measure: {UnitName} ({Symbol})", 
            unitOfMeasure.UnitOfMeasureName, unitOfMeasure.UnitOfMeasureSymbol);
        
        // Check for duplicate symbol
        if (await ExistsBySymbolAsync(unitOfMeasure.UnitOfMeasureSymbol))
        {
            throw new InvalidOperationException(
                $"Unit of measure with symbol '{unitOfMeasure.UnitOfMeasureSymbol}' already exists");
        }

        _context.Set<UnitOfMeasure>().Add(unitOfMeasure);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Unit of measure created successfully with ID: {UnitOfMeasureId}", 
            unitOfMeasure.UnitOfMeasureId);
        return unitOfMeasure;
    }

    public async Task<UnitOfMeasure> UpdateAsync(UnitOfMeasure unitOfMeasure)
    {
        _logger.LogInformation("Updating unit of measure: {UnitOfMeasureId}", unitOfMeasure.UnitOfMeasureId);
        
        // Check for duplicate symbol (excluding current unit)
        if (await ExistsBySymbolAsync(unitOfMeasure.UnitOfMeasureSymbol, unitOfMeasure.UnitOfMeasureId))
        {
            throw new InvalidOperationException(
                $"Another unit of measure with symbol '{unitOfMeasure.UnitOfMeasureSymbol}' already exists");
        }

        _context.Set<UnitOfMeasure>().Update(unitOfMeasure);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Unit of measure updated successfully: {UnitOfMeasureId}", 
            unitOfMeasure.UnitOfMeasureId);
        return unitOfMeasure;
    }

    public async Task<bool> DeleteAsync(int unitOfMeasureId)
    {
        _logger.LogInformation("Deleting unit of measure: {UnitOfMeasureId}", unitOfMeasureId);
        
        var unit = await GetByIdAsync(unitOfMeasureId);
        if (unit == null)
        {
            _logger.LogWarning("Unit of measure not found: {UnitOfMeasureId}", unitOfMeasureId);
            return false;
        }

        // Check if unit is used by any items
        var hasItemsAsBase = await _context.Set<Item>().AnyAsync(i => i.BaseUnitOfMeasureId == unitOfMeasureId);
        var hasItemsAsSales = await _context.Set<Item>().AnyAsync(i => i.DefaultSalesUnitOfMeasureId == unitOfMeasureId);
        var hasConversions = await _context.Set<ItemUnitConversion>().AnyAsync(
            c => c.FromUnitOfMeasureId == unitOfMeasureId || c.ToUnitOfMeasureId == unitOfMeasureId);
        
        if (hasItemsAsBase || hasItemsAsSales || hasConversions)
        {
            throw new InvalidOperationException(
                $"Cannot delete unit of measure '{unit.UnitOfMeasureName}' because it is used by items or conversions");
        }

        _context.Set<UnitOfMeasure>().Remove(unit);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Unit of measure deleted successfully: {UnitOfMeasureId}", unitOfMeasureId);
        return true;
    }

    public async Task<bool> ExistsAsync(int unitOfMeasureId)
    {
        return await _context.Set<UnitOfMeasure>().AnyAsync(u => u.UnitOfMeasureId == unitOfMeasureId);
    }

    public async Task<bool> ExistsBySymbolAsync(string symbol, int? excludeUnitId = null)
    {
        var query = _context.Set<UnitOfMeasure>().Where(u => u.UnitOfMeasureSymbol == symbol);
        
        if (excludeUnitId.HasValue)
        {
            query = query.Where(u => u.UnitOfMeasureId != excludeUnitId.Value);
        }
        
        return await query.AnyAsync();
    }
}

