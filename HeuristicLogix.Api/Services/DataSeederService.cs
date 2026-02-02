using HeuristicLogix.Api.Persistence;
using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Api.Services;

/// <summary>
/// Seeds initial test data into the database.
/// Handles hybrid ID architecture (int for Inventory, Guid for Core/Purchasing).
/// Respects FK constraints by inserting in correct order.
/// </summary>
public class DataSeederService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeederService> _logger;

    // Store seeded IDs for reference
    private Guid _itbis18TaxId;
    private Guid _itbis16TaxId;
    private Guid _exentoTaxId;
    
    private int _unitId;
    private int _bag50kgId;
    private int _cubicMeterId;
    private int _kilogramId;
    private int _meterId;
    
    private int _constructionCategoryId;
    private int _toolsCategoryId;
    private int _cementCategoryId;
    
    private int _lancoBrandId;
    private int _truperBrandId;
    
    private int _cementItemId;
    private int _rebarItemId;
    
    private Guid _proveconSupplierId;

    public DataSeederService(AppDbContext context, ILogger<DataSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all test data in the correct order.
    /// Checks for existing data to avoid duplicates.
    /// </summary>
    public async Task<DataSeederResult> SeedAsync()
    {
        var result = new DataSeederResult();
        
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if data already exists
            var hasExistingData = await CheckExistingDataAsync();
            if (hasExistingData)
            {
                _logger.LogInformation("Database already contains seed data. Skipping seeding.");
                result.Success = true;
                result.Message = "Database already seeded";
                result.AlreadySeeded = true;
                return result;
            }

            // Use execution strategy to handle transactions with retry logic
            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                // Begin transaction for all-or-nothing seeding
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // ============================================================
                    // STEP 1: CORE SCHEMA (Guid IDs)
                    // ============================================================
                    _logger.LogInformation("Seeding Core schema...");
                    await SeedTaxConfigurationsAsync();
                    await _context.SaveChangesAsync();
                    result.TaxConfigurationsSeeded = 3;

                    await SeedUnitsOfMeasureAsync();
                    await _context.SaveChangesAsync();
                    result.UnitsOfMeasureSeeded = 5;

                    // ============================================================
                    // STEP 2: INVENTORY SCHEMA - MASTER DATA (int IDs)
                    // ============================================================
                    _logger.LogInformation("Seeding Inventory schema (master data)...");
                    await SeedCategoriesAsync();
                    await _context.SaveChangesAsync();
                    result.CategoriesSeeded = 3;

                    await SeedBrandsAsync();
                    await _context.SaveChangesAsync();
                    result.BrandsSeeded = 2;

                    // ============================================================
                    // STEP 3: INVENTORY SCHEMA - ITEMS (int IDs with mixed FKs)
                    // ============================================================
                    _logger.LogInformation("Seeding Items...");
                    await SeedItemsAsync();
                    await _context.SaveChangesAsync();
                    result.ItemsSeeded = 2;

                    // ============================================================
                    // STEP 4: INVENTORY SCHEMA - UNIT CONVERSIONS (Guid PK, int FKs)
                    // ============================================================
                    _logger.LogInformation("Seeding Item Unit Conversions...");
                    await SeedItemUnitConversionsAsync();
                    await _context.SaveChangesAsync();
                    result.ItemUnitConversionsSeeded = 2;

                    // ============================================================
                    // STEP 5: PURCHASING SCHEMA - SUPPLIERS (Guid IDs)
                    // ============================================================
                    _logger.LogInformation("Seeding Purchasing schema...");
                    await SeedSuppliersAsync();
                    await _context.SaveChangesAsync();
                    result.SuppliersSeeded = 1;

                    // ============================================================
                    // STEP 6: PURCHASING SCHEMA - ITEM-SUPPLIER LINKS (Composite PK)
                    // ============================================================
                    _logger.LogInformation("Seeding Item-Supplier relationships...");
                    await SeedItemSuppliersAsync();
                    await _context.SaveChangesAsync();
                    result.ItemSuppliersSeeded = 2;

                    // Commit transaction
                    await transaction.CommitAsync();

                    result.Success = true;
                    result.Message = "Database seeded successfully";
                    
                    _logger.LogInformation("Database seeding completed successfully!");
                    _logger.LogInformation($"Seeded: {result.TotalRecordsSeeded} total records");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Seeding failed, transaction rolled back", ex);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database seeding");
            result.Success = false;
            result.Message = $"Seeding failed: {ex.Message}";
            result.ErrorDetails = ex.ToString();
        }

        return result;
    }

    /// <summary>
    /// Checks if the database already has seed data.
    /// </summary>
    private async Task<bool> CheckExistingDataAsync()
    {
        var hasCategories = await _context.Categories.AnyAsync();
        var hasBrands = await _context.Brands.AnyAsync();
        var hasItems = await _context.Items.AnyAsync();
        
        return hasCategories || hasBrands || hasItems;
    }

    // ============================================================
    // CORE SCHEMA SEEDING
    // ============================================================

    private async Task SeedTaxConfigurationsAsync()
    {
        _logger.LogInformation("Seeding TaxConfigurations (Guid IDs)...");

        _itbis18TaxId = Guid.NewGuid();
        _itbis16TaxId = Guid.NewGuid();
        _exentoTaxId = Guid.NewGuid();

        var taxes = new[]
        {
            new TaxConfiguration
            {
                TaxConfigurationId = _itbis18TaxId,
                TaxName = "ITBIS 18%",
                TaxPercentageRate = 18.00m,
                IsActive = true
            },
            new TaxConfiguration
            {
                TaxConfigurationId = _itbis16TaxId,
                TaxName = "ITBIS 16%",
                TaxPercentageRate = 16.00m,
                IsActive = true
            },
            new TaxConfiguration
            {
                TaxConfigurationId = _exentoTaxId,
                TaxName = "Exento",
                TaxPercentageRate = 0.00m,
                IsActive = true
            }
        };

        await _context.TaxConfigurations.AddRangeAsync(taxes);
        _logger.LogInformation("Added 3 TaxConfigurations");
    }

    private async Task SeedUnitsOfMeasureAsync()
    {
        _logger.LogInformation("Seeding UnitsOfMeasure (int IDs)...");

        var units = new[]
        {
            new UnitOfMeasure { UnitOfMeasureId = 0, UnitOfMeasureName = "Unidad", UnitOfMeasureSymbol = "un" },
            new UnitOfMeasure { UnitOfMeasureId = 0, UnitOfMeasureName = "Saco 50kg", UnitOfMeasureSymbol = "saco" },
            new UnitOfMeasure { UnitOfMeasureId = 0, UnitOfMeasureName = "Metro Cúbico", UnitOfMeasureSymbol = "m³" },
            new UnitOfMeasure { UnitOfMeasureId = 0, UnitOfMeasureName = "Kilogramo", UnitOfMeasureSymbol = "kg" },
            new UnitOfMeasure { UnitOfMeasureId = 0, UnitOfMeasureName = "Metro", UnitOfMeasureSymbol = "m" }
        };

        await _context.UnitsOfMeasure.AddRangeAsync(units);
        await _context.SaveChangesAsync();

        // Retrieve generated IDs (database generates them)
        _unitId = await _context.UnitsOfMeasure
            .Where(u => u.UnitOfMeasureSymbol == "un")
            .Select(u => u.UnitOfMeasureId)
            .FirstAsync();
            
        _bag50kgId = await _context.UnitsOfMeasure
            .Where(u => u.UnitOfMeasureSymbol == "saco")
            .Select(u => u.UnitOfMeasureId)
            .FirstAsync();
            
        _cubicMeterId = await _context.UnitsOfMeasure
            .Where(u => u.UnitOfMeasureSymbol == "m³")
            .Select(u => u.UnitOfMeasureId)
            .FirstAsync();
            
        _kilogramId = await _context.UnitsOfMeasure
            .Where(u => u.UnitOfMeasureSymbol == "kg")
            .Select(u => u.UnitOfMeasureId)
            .FirstAsync();
            
        _meterId = await _context.UnitsOfMeasure
            .Where(u => u.UnitOfMeasureSymbol == "m")
            .Select(u => u.UnitOfMeasureId)
            .FirstAsync();

        _logger.LogInformation($"Added 5 UnitsOfMeasure (IDs: {_unitId}, {_bag50kgId}, {_cubicMeterId}, {_kilogramId}, {_meterId})");
    }

    // ============================================================
    // INVENTORY SCHEMA SEEDING
    // ============================================================

    private async Task SeedCategoriesAsync()
    {
        _logger.LogInformation("Seeding Categories (int IDs)...");

        var categories = new[]
        {
            new Category { CategoryId = 0, CategoryName = "Materiales de Construcción" },
            new Category { CategoryId = 0, CategoryName = "Herramientas" },
            new Category { CategoryId = 0, CategoryName = "Productos de Cemento" }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        // Retrieve generated IDs
        _constructionCategoryId = await _context.Categories
            .Where(c => c.CategoryName == "Materiales de Construcción")
            .Select(c => c.CategoryId)
            .FirstAsync();
            
        _toolsCategoryId = await _context.Categories
            .Where(c => c.CategoryName == "Herramientas")
            .Select(c => c.CategoryId)
            .FirstAsync();
            
        _cementCategoryId = await _context.Categories
            .Where(c => c.CategoryName == "Productos de Cemento")
            .Select(c => c.CategoryId)
            .FirstAsync();

        _logger.LogInformation($"Added 3 Categories (IDs: {_constructionCategoryId}, {_toolsCategoryId}, {_cementCategoryId})");
    }

    private async Task SeedBrandsAsync()
    {
        _logger.LogInformation("Seeding Brands (int IDs)...");

        var brands = new[]
        {
            new Brand { BrandId = 0, BrandName = "Lanco" },
            new Brand { BrandId = 0, BrandName = "Truper" }
        };

        await _context.Brands.AddRangeAsync(brands);
        await _context.SaveChangesAsync();

        // Retrieve generated IDs
        _lancoBrandId = await _context.Brands
            .Where(b => b.BrandName == "Lanco")
            .Select(b => b.BrandId)
            .FirstAsync();
            
        _truperBrandId = await _context.Brands
            .Where(b => b.BrandName == "Truper")
            .Select(b => b.BrandId)
            .FirstAsync();

        _logger.LogInformation($"Added 2 Brands (IDs: {_lancoBrandId}, {_truperBrandId})");
    }

    private async Task SeedItemsAsync()
    {
        _logger.LogInformation("Seeding Items (int IDs with mixed FK types)...");

        var items = new[]
        {
            // Portland Cement - Uses Guid FK for TaxConfigurationId!
            new Item
            {
                ItemId = 0, // Auto-generated int
                StockKeepingUnitCode = "CEM-PORT-50KG",
                ItemDescription = "Cemento Portland Tipo I - Saco 50kg",
                BrandId = _lancoBrandId, // int FK (nullable)
                CategoryId = _cementCategoryId, // int FK
                TaxConfigurationId = _itbis18TaxId, // Guid FK ??
                BaseUnitOfMeasureId = _bag50kgId, // int FK
                DefaultSalesUnitOfMeasureId = _bag50kgId, // int FK (nullable)
                CostPricePerBaseUnit = 450.0000m, // DECIMAL(18,4)
                SellingPricePerBaseUnit = 650.0000m, // DECIMAL(18,4)
                MinimumRequiredStockQuantity = 100.00m, // DECIMAL(18,2)
                CurrentStockQuantity = 500.00m // DECIMAL(18,2)
            },
            
            // Steel Rebar
            new Item
            {
                ItemId = 0, // Auto-generated int
                StockKeepingUnitCode = "ACE-VAR-3/8",
                ItemDescription = "Varilla de Acero 3/8\" x 6m",
                BrandId = null, // No brand
                CategoryId = _constructionCategoryId, // int FK
                TaxConfigurationId = _itbis18TaxId, // Guid FK ??
                BaseUnitOfMeasureId = _unitId, // int FK
                DefaultSalesUnitOfMeasureId = _unitId, // int FK (nullable)
                CostPricePerBaseUnit = 120.5000m, // DECIMAL(18,4)
                SellingPricePerBaseUnit = 175.7500m, // DECIMAL(18,4)
                MinimumRequiredStockQuantity = 50.00m, // DECIMAL(18,2)
                CurrentStockQuantity = 200.00m // DECIMAL(18,2)
            }
        };

        await _context.Items.AddRangeAsync(items);
        await _context.SaveChangesAsync();

        // Retrieve generated IDs
        _cementItemId = await _context.Items
            .Where(i => i.StockKeepingUnitCode == "CEM-PORT-50KG")
            .Select(i => i.ItemId)
            .FirstAsync();
            
        _rebarItemId = await _context.Items
            .Where(i => i.StockKeepingUnitCode == "ACE-VAR-3/8")
            .Select(i => i.ItemId)
            .FirstAsync();

        _logger.LogInformation($"Added 2 Items (IDs: {_cementItemId}, {_rebarItemId})");
        _logger.LogInformation("Items have mixed FK types: int for inventory, Guid for TaxConfigurationId");
    }

    private async Task SeedItemUnitConversionsAsync()
    {
        _logger.LogInformation("Seeding ItemUnitConversions (Guid PK with int FKs)...");

        var conversions = new[]
        {
            // Cement: 1 Bag = 50 kg
            new ItemUnitConversion
            {
                ItemUnitConversionId = Guid.NewGuid(), // Guid PK
                ItemId = _cementItemId, // int FK
                FromUnitOfMeasureId = _bag50kgId, // int FK (FromUnit)
                ToUnitOfMeasureId = _kilogramId, // int FK (ToUnit)
                ConversionFactorQuantity = 50.0000m // DECIMAL(18,4)
            },
            
            // Rebar: 1 Unit = 6 m
            new ItemUnitConversion
            {
                ItemUnitConversionId = Guid.NewGuid(), // Guid PK
                ItemId = _rebarItemId, // int FK
                FromUnitOfMeasureId = _unitId, // int FK (FromUnit)
                ToUnitOfMeasureId = _meterId, // int FK (ToUnit)
                ConversionFactorQuantity = 6.0000m // DECIMAL(18,4)
            }
        };

        await _context.ItemUnitConversions.AddRangeAsync(conversions);
        _logger.LogInformation("Added 2 ItemUnitConversions (Guid PKs, int FKs)");
    }

    // ============================================================
    // PURCHASING SCHEMA SEEDING
    // ============================================================

    private async Task SeedSuppliersAsync()
    {
        _logger.LogInformation("Seeding Suppliers (Guid IDs)...");

        _proveconSupplierId = Guid.NewGuid();

        var supplier = new Supplier
        {
            SupplierId = _proveconSupplierId, // Guid PK
            NationalTaxIdentificationNumber = "101234567", // 9-digit RNC
            SupplierBusinessName = "Provecon Materiales de Construcción SRL",
            SupplierTradeName = "Provecon",
            DefaultCreditDaysDuration = 30, // 30 days credit
            IsActive = true
        };

        await _context.Suppliers.AddAsync(supplier);
        _logger.LogInformation($"Added 1 Supplier (ID: {_proveconSupplierId})");
    }

    private async Task SeedItemSuppliersAsync()
    {
        _logger.LogInformation("Seeding ItemSuppliers (Composite PK: int + Guid)...");

        var itemSuppliers = new[]
        {
            // Cement from Provecon
            new ItemSupplier
            {
                ItemId = _cementItemId, // int PK/FK
                SupplierId = _proveconSupplierId, // Guid PK/FK
                SupplierInternalPartNumber = "PROV-CEM-50KG", // Supplier's internal code
                LastPurchasePriceAmount = 425.0000m, // Last purchase price DECIMAL(18,4)
                LastPurchaseDateTime = DateTimeOffset.UtcNow.AddDays(-15), // Last purchase 15 days ago
                IsPreferredSupplierForItem = true // Preferred supplier
            },
            
            // Rebar from Provecon
            new ItemSupplier
            {
                ItemId = _rebarItemId, // int PK/FK
                SupplierId = _proveconSupplierId, // Guid PK/FK
                SupplierInternalPartNumber = "PROV-ACE-3/8", // Supplier's internal code
                LastPurchasePriceAmount = 115.0000m, // Last purchase price DECIMAL(18,4)
                LastPurchaseDateTime = DateTimeOffset.UtcNow.AddDays(-7), // Last purchase 7 days ago
                IsPreferredSupplierForItem = true // Preferred supplier
            }
        };

        await _context.ItemSuppliers.AddRangeAsync(itemSuppliers);
        _logger.LogInformation("Added 2 ItemSuppliers (Composite PKs with int + Guid)");
    }
}

/// <summary>
/// Result of the seeding operation.
/// </summary>
public class DataSeederResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool AlreadySeeded { get; set; }
    public string? ErrorDetails { get; set; }
    
    public int TaxConfigurationsSeeded { get; set; }
    public int UnitsOfMeasureSeeded { get; set; }
    public int CategoriesSeeded { get; set; }
    public int BrandsSeeded { get; set; }
    public int ItemsSeeded { get; set; }
    public int ItemUnitConversionsSeeded { get; set; }
    public int SuppliersSeeded { get; set; }
    public int ItemSuppliersSeeded { get; set; }
    
    public int TotalRecordsSeeded => 
        TaxConfigurationsSeeded + 
        UnitsOfMeasureSeeded + 
        CategoriesSeeded + 
        BrandsSeeded + 
        ItemsSeeded + 
        ItemUnitConversionsSeeded + 
        SuppliersSeeded + 
        ItemSuppliersSeeded;
}
