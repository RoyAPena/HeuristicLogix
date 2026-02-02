using FluentValidation;
using HeuristicLogix.Modules.Inventory.Services;
using HeuristicLogix.Shared.DTOs;
using HeuristicLogix.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeuristicLogix.Api.Controllers;

/// <summary>
/// API controller for Category maintenance operations.
/// Follows REST principles and hybrid ID architecture (int IDs).
/// </summary>
[ApiController]
[Route("api/inventory/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CategoryUpsertDto> _validator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        IValidator<CategoryUpsertDto> validator,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CategoryUpsertDto dto)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        try
        {
            // Map DTO to Entity
            var category = new Category
            {
                CategoryName = dto.CategoryName
            };

            var created = await _categoryService.CreateAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create category: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpsertDto dto)
    {
        if (id != dto.CategoryId)
        {
            return BadRequest(new { message = "ID mismatch between URL and body" });
        }

        if (!await _categoryService.ExistsAsync(id))
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        // Validate
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        try
        {
            // Map DTO to Entity
            var category = new Category
            {
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName
            };

            var updated = await _categoryService.UpdateAsync(category);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update category: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _categoryService.DeleteAsync(id);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete category: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}

