using HeuristicLogix.Shared.DTOs;
using HeuristicLogix.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Api.Controllers;

/// <summary>
/// API controller for managing product taxonomies.
/// Supports Human-in-the-Loop (HIL) verification workflow.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TaxonomyController : ControllerBase
{
    private readonly HeuristicLogixDbContext _dbContext;
    private readonly ILogger<TaxonomyController> _logger;

    public TaxonomyController(
        HeuristicLogixDbContext dbContext,
        ILogger<TaxonomyController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets all product taxonomies with optional filtering.
    /// </summary>
    /// <param name="isVerified">Filter by verification status.</param>
    /// <param name="category">Filter by category.</param>
    /// <param name="searchTerm">Search in description.</param>
    /// <param name="sortBy">Sort by field (UsageCount, CreatedAt, Description).</param>
    /// <param name="descending">Sort direction.</param>
    /// <returns>List of taxonomies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaxonomyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxonomies(
        [FromQuery] bool? isVerified = null,
        [FromQuery] string? category = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string sortBy = "UsageCount",
        [FromQuery] bool descending = true)
    {
        try
        {
            IQueryable<ProductTaxonomy> query = _dbContext.ProductTaxonomies.AsQueryable();

            // Apply filters
            if (isVerified.HasValue)
            {
                query = query.Where(t => t.IsVerifiedByExpert == isVerified.Value);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category == category.ToUpper());
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string upperSearch = searchTerm.ToUpper();
                query = query.Where(t => t.Description.Contains(upperSearch));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "usagecount" => descending ? query.OrderByDescending(t => t.UsageCount) : query.OrderBy(t => t.UsageCount),
                "createdat" => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "description" => descending ? query.OrderByDescending(t => t.Description) : query.OrderBy(t => t.Description),
                _ => query.OrderByDescending(t => t.UsageCount)
            };

            List<ProductTaxonomy> taxonomies = await query.ToListAsync();

            List<TaxonomyDto> dtos = taxonomies.Select(t => new TaxonomyDto
            {
                Id = t.Id,
                RawDescription = t.Description, // Using Description as both for now
                Description = t.Description,
                Category = t.Category,
                WeightFactor = t.WeightFactor,
                StandardUnit = t.StandardUnit,
                IsVerifiedByExpert = t.IsVerifiedByExpert,
                UsageCount = t.UsageCount,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                VerifiedBy = t.VerifiedBy,
                VerifiedAt = t.VerifiedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} taxonomies", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving taxonomies");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Error al obtener taxonomías" });
        }
    }

    /// <summary>
    /// Gets a single taxonomy by ID.
    /// </summary>
    /// <param name="id">Taxonomy ID.</param>
    /// <returns>Taxonomy DTO.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaxonomyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxonomy(Guid id)
    {
        ProductTaxonomy? taxonomy = await _dbContext.ProductTaxonomies.FindAsync(id);

        if (taxonomy == null)
        {
            return NotFound(new { error = "Taxonomía no encontrada" });
        }

        TaxonomyDto dto = new TaxonomyDto
        {
            Id = taxonomy.Id,
            RawDescription = taxonomy.Description,
            Description = taxonomy.Description,
            Category = taxonomy.Category,
            WeightFactor = taxonomy.WeightFactor,
            StandardUnit = taxonomy.StandardUnit,
            IsVerifiedByExpert = taxonomy.IsVerifiedByExpert,
            UsageCount = taxonomy.UsageCount,
            Notes = taxonomy.Notes,
            CreatedAt = taxonomy.CreatedAt,
            VerifiedBy = taxonomy.VerifiedBy,
            VerifiedAt = taxonomy.VerifiedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Verifies a taxonomy entry (Human-in-the-Loop approval).
    /// </summary>
    /// <param name="request">Verification request.</param>
    /// <returns>Updated taxonomy.</returns>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyTaxonomyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyTaxonomy([FromBody] VerifyTaxonomyRequest request)
    {
        try
        {
            ProductTaxonomy? taxonomy = await _dbContext.ProductTaxonomies.FindAsync(request.TaxonomyId);

            if (taxonomy == null)
            {
                return NotFound(new VerifyTaxonomyResponse
                {
                    Success = false,
                    ErrorMessage = "Taxonomía no encontrada"
                });
            }

            if (taxonomy.IsVerifiedByExpert)
            {
                return BadRequest(new VerifyTaxonomyResponse
                {
                    Success = false,
                    ErrorMessage = "Esta taxonomía ya ha sido verificada"
                });
            }

            // Update taxonomy
            taxonomy.WeightFactor = request.WeightFactor;
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                taxonomy.Category = request.Category.ToUpper();
            }
            if (!string.IsNullOrWhiteSpace(request.StandardUnit))
            {
                taxonomy.StandardUnit = request.StandardUnit.ToUpper();
            }
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                taxonomy.Notes = request.Notes;
            }

            taxonomy.MarkAsVerified(request.VerifiedBy);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Taxonomy verified: {Description} by {VerifiedBy} (WeightFactor: {WeightFactor})",
                taxonomy.Description, request.VerifiedBy, request.WeightFactor);

            TaxonomyDto dto = new TaxonomyDto
            {
                Id = taxonomy.Id,
                RawDescription = taxonomy.Description,
                Description = taxonomy.Description,
                Category = taxonomy.Category,
                WeightFactor = taxonomy.WeightFactor,
                StandardUnit = taxonomy.StandardUnit,
                IsVerifiedByExpert = taxonomy.IsVerifiedByExpert,
                UsageCount = taxonomy.UsageCount,
                Notes = taxonomy.Notes,
                CreatedAt = taxonomy.CreatedAt,
                VerifiedBy = taxonomy.VerifiedBy,
                VerifiedAt = taxonomy.VerifiedAt
            };

            return Ok(new VerifyTaxonomyResponse
            {
                Success = true,
                Taxonomy = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying taxonomy {TaxonomyId}", request.TaxonomyId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new VerifyTaxonomyResponse
                {
                    Success = false,
                    ErrorMessage = $"Error al verificar taxonomía: {ex.Message}"
                });
        }
    }

    /// <summary>
    /// Gets counts of taxonomies by verification status.
    /// </summary>
    /// <returns>Statistics.</returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            int totalCount = await _dbContext.ProductTaxonomies.CountAsync();
            int verifiedCount = await _dbContext.ProductTaxonomies.CountAsync(t => t.IsVerifiedByExpert);
            int pendingCount = totalCount - verifiedCount;

            Dictionary<string, int> categoryStats = await _dbContext.ProductTaxonomies
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            object stats = new
            {
                total = totalCount,
                verified = verifiedCount,
                pending = pendingCount,
                categories = categoryStats
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting taxonomy stats");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error al obtener estadísticas" });
        }
    }
}
