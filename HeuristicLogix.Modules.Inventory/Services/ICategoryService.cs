using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Service interface for Category entity operations.
/// Follows hybrid ID architecture (int IDs for inventory master data).
/// </summary>
public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int categoryId);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int categoryId);
    Task<bool> ExistsAsync(int categoryId);
    Task<bool> ExistsByNameAsync(string categoryName, int? excludeCategoryId = null);
}
