using FluentValidation;
using HeuristicLogix.Modules.Inventory.Services;
using HeuristicLogix.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeuristicLogix.Api.Controllers;

/// <summary>
/// API controller for UnitOfMeasure maintenance operations.
/// Follows REST principles and hybrid ID architecture (int IDs).
/// </summary>
[ApiController]
[Route("api/inventory/[controller]")]
public class UnitsOfMeasureController : ControllerBase
{
    private readonly IUnitOfMeasureService _unitService;
    private readonly IValidator<UnitOfMeasure> _validator;
    private readonly ILogger<UnitsOfMeasureController> _logger;

    public UnitsOfMeasureController(
        IUnitOfMeasureService unitService,
        IValidator<UnitOfMeasure> validator,
        ILogger<UnitsOfMeasureController> logger)
    {
        _unitService = unitService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all units of measure.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UnitOfMeasure>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var units = await _unitService.GetAllAsync();
        return Ok(units);
    }

    /// <summary>
    /// Gets a unit of measure by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UnitOfMeasure), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        
        if (unit == null)
        {
            return NotFound(new { message = $"Unit of measure with ID {id} not found" });
        }

        return Ok(unit);
    }

    /// <summary>
    /// Creates a new unit of measure.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UnitOfMeasure), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UnitOfMeasure unit)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(unit);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        try
        {
            var created = await _unitService.CreateAsync(unit);
            return CreatedAtAction(nameof(GetById), new { id = created.UnitOfMeasureId }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create unit of measure: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing unit of measure.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UnitOfMeasure), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UnitOfMeasure unit)
    {
        if (id != unit.UnitOfMeasureId)
        {
            return BadRequest(new { message = "ID mismatch between URL and body" });
        }

        if (!await _unitService.ExistsAsync(id))
        {
            return NotFound(new { message = $"Unit of measure with ID {id} not found" });
        }

        // Validate
        var validationResult = await _validator.ValidateAsync(unit);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        try
        {
            var updated = await _unitService.UpdateAsync(unit);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update unit of measure: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a unit of measure.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _unitService.DeleteAsync(id);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Unit of measure with ID {id} not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete unit of measure: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}
