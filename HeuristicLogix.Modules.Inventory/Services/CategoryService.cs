using HeuristicLogix.Features.Inventory.Categories;
using HeuristicLogix.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Bridge service for Category operations.
/// Delegates all operations to MediatR handlers (Vertical Slice Architecture).
/// Maintains ICategoryService contract to avoid breaking UI components.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IMediator mediator, ILogger<CategoryService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all categories");
        return await _mediator.Send(new GetCategoriesQuery());
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        _logger.LogInformation("Retrieving category with ID: {CategoryId}", categoryId);
        return await _mediator.Send(new GetCategoryByIdQuery(categoryId));
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _logger.LogInformation("Creating new category: {CategoryName}", category.CategoryName);
        var created = await _mediator.Send(new CreateCategoryCommand(category));
        _logger.LogInformation("Category created successfully with ID: {CategoryId}", created.CategoryId);
        return created;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _logger.LogInformation("Updating category: {CategoryId}", category.CategoryId);
        var updated = await _mediator.Send(new UpdateCategoryCommand(category));
        _logger.LogInformation("Category updated successfully: {CategoryId}", updated.CategoryId);
        return updated;
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        _logger.LogInformation("Deleting category: {CategoryId}", categoryId);
        var deleted = await _mediator.Send(new DeleteCategoryCommand(categoryId));
        
        if (!deleted)
        {
            _logger.LogWarning("Category not found: {CategoryId}", categoryId);
        }
        else
        {
            _logger.LogInformation("Category deleted successfully: {CategoryId}", categoryId);
        }
        
        return deleted;
    }

    public async Task<bool> ExistsAsync(int categoryId)
    {
        return await _mediator.Send(new CategoryExistsQuery(categoryId));
    }

    public async Task<bool> ExistsByNameAsync(string categoryName, int? excludeCategoryId = null)
    {
        return await _mediator.Send(new CategoryExistsByNameQuery(categoryName, excludeCategoryId));
    }
}

