using HeuristicLogix.Api.Persistence;
using HeuristicLogix.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Api.Controllers;

/// <summary>
/// Development-only controller for database seeding operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly DataSeederService _seederService;
    private readonly ILogger<SeedController> _logger;
    private readonly IWebHostEnvironment _environment;

    public SeedController(
        DataSeederService seederService,
        ILogger<SeedController> logger,
        IWebHostEnvironment environment)
    {
        _seederService = seederService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Seeds the database with test data.
    /// Only available in Development environment.
    /// </summary>
    /// <remarks>
    /// POST /api/seed
    /// 
    /// Seeds:
    /// - 3 Tax Configurations (Guid IDs)
    /// - 5 Units of Measure (int IDs)
    /// - 3 Categories (int IDs)
    /// - 2 Brands (int IDs)
    /// - 2 Items (int IDs with mixed FK types)
    /// - 2 Item Unit Conversions (Guid PKs, int FKs)
    /// - 1 Supplier (Guid ID)
    /// - 2 Item-Supplier links (Composite PKs)
    /// 
    /// Verifies:
    /// - Hybrid ID architecture (int for Inventory, Guid for Core/Purchasing)
    /// - Foreign key constraints
    /// - Decimal precision (18,4 for prices, 18,2 for quantities)
    /// - Composite primary keys
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(DataSeederResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SeedDatabase()
    {
        // Only allow in Development
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("Seeding attempted in non-Development environment");
            return StatusCode(403, new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "Database seeding is only available in Development environment",
                Status = 403
            });
        }

        _logger.LogInformation("Database seeding requested via API");

        var result = await _seederService.SeedAsync();

        if (result.Success)
        {
            if (result.AlreadySeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    alreadySeeded = true
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                totalRecords = result.TotalRecordsSeeded,
                details = new
                {
                    taxConfigurations = result.TaxConfigurationsSeeded,
                    unitsOfMeasure = result.UnitsOfMeasureSeeded,
                    categories = result.CategoriesSeeded,
                    brands = result.BrandsSeeded,
                    items = result.ItemsSeeded,
                    itemUnitConversions = result.ItemUnitConversionsSeeded,
                    suppliers = result.SuppliersSeeded,
                    itemSuppliers = result.ItemSuppliersSeeded
                },
                verification = new
                {
                    hybridIDs = "? int for Inventory, Guid for Core/Purchasing",
                    foreignKeys = "? All FK constraints satisfied",
                    decimalPrecision = "? 18,4 for prices, 18,2 for quantities",
                    compositePKs = "? ItemSupplier uses (int + Guid)"
                }
            });
        }

        _logger.LogError("Seeding failed: {Message}", result.Message);
        return BadRequest(new ProblemDetails
        {
            Title = "Seeding Failed",
            Detail = result.Message,
            Status = 400
        });
    }

    /// <summary>
    /// Gets the current seeding status.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedingStatus([FromServices] AppDbContext context)
    {
        var taxCount = await context.TaxConfigurations.CountAsync();
        var categoryCount = await context.Categories.CountAsync();
        var brandCount = await context.Brands.CountAsync();
        var itemCount = await context.Items.CountAsync();
        var supplierCount = await context.Suppliers.CountAsync();
        
        var isSeeded = taxCount > 0 && categoryCount > 0 && itemCount > 0;

        return Ok(new
        {
            isSeeded,
            recordCounts = new
            {
                taxConfigurations = taxCount,
                unitsOfMeasure = await context.UnitsOfMeasure.CountAsync(),
                categories = categoryCount,
                brands = brandCount,
                items = itemCount,
                itemUnitConversions = await context.ItemUnitConversions.CountAsync(),
                suppliers = supplierCount,
                itemSuppliers = await context.ItemSuppliers.CountAsync()
            }
        });
    }
}
