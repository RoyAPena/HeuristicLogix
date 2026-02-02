using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Implementation of Category service.
/// Handles CRUD operations for inventory categories (int IDs).
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly DbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(DbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all categories");
        return await _context.Set<Category>()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        _logger.LogInformation("Retrieving category with ID: {CategoryId}", categoryId);
        return await _context.Set<Category>()
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _logger.LogInformation("Creating new category: {CategoryName}", category.CategoryName);
        
        // Check for duplicate name
        if (await ExistsByNameAsync(category.CategoryName))
        {
            throw new InvalidOperationException($"Category with name '{category.CategoryName}' already exists");
        }

        _context.Set<Category>().Add(category);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.CategoryId);
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _logger.LogInformation("Updating category: {CategoryId}", category.CategoryId);
        
        // Check for duplicate name (excluding current category)
        if (await ExistsByNameAsync(category.CategoryName, category.CategoryId))
        {
            throw new InvalidOperationException($"Another category with name '{category.CategoryName}' already exists");
        }

        _context.Set<Category>().Update(category);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Category updated successfully: {CategoryId}", category.CategoryId);
        return category;
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        _logger.LogInformation("Deleting category: {CategoryId}", categoryId);
        
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", categoryId);
            return false;
        }

        // Check if category is used by any items
        var hasItems = await _context.Set<Item>().AnyAsync(i => i.CategoryId == categoryId);
        if (hasItems)
        {
            throw new InvalidOperationException($"Cannot delete category '{category.CategoryName}' because it is used by one or more items");
        }

        _context.Set<Category>().Remove(category);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Category deleted successfully: {CategoryId}", categoryId);
        return true;
    }

    public async Task<bool> ExistsAsync(int categoryId)
    {
        return await _context.Set<Category>().AnyAsync(c => c.CategoryId == categoryId);
    }

    public async Task<bool> ExistsByNameAsync(string categoryName, int? excludeCategoryId = null)
    {
        var query = _context.Set<Category>().Where(c => c.CategoryName == categoryName);
        
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId != excludeCategoryId.Value);
        }
        
        return await query.AnyAsync();
    }
}

