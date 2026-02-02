using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Service interface for UnitOfMeasure entity operations.
/// Follows hybrid ID architecture (int IDs for inventory master data).
/// </summary>
public interface IUnitOfMeasureService
{
    Task<IEnumerable<UnitOfMeasure>> GetAllAsync();
    Task<UnitOfMeasure?> GetByIdAsync(int unitOfMeasureId);
    Task<UnitOfMeasure> CreateAsync(UnitOfMeasure unitOfMeasure);
    Task<UnitOfMeasure> UpdateAsync(UnitOfMeasure unitOfMeasure);
    Task<bool> DeleteAsync(int unitOfMeasureId);
    Task<bool> ExistsAsync(int unitOfMeasureId);
    Task<bool> ExistsBySymbolAsync(string symbol, int? excludeUnitId = null);
}
