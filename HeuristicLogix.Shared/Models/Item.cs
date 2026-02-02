namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Inventory Item (Product/SKU) master record.
/// Part of the Inventory schema.
/// Tracks stock with Weighted Average Cost (WAC).
/// Uses int ID for legacy compatibility.
/// Supports multi-dimensional stock tracking: Current, Reserved, Staging, and Available.
/// </summary>
public class Item(
    int itemId,
    string stockKeepingUnitCode,
    string itemDescription,
    int categoryId,
    Guid taxConfigurationId,
    int baseUnitOfMeasureId,
    decimal costPricePerBaseUnit,
    decimal sellingPricePerBaseUnit,
    decimal minimumRequiredStockQuantity,
    decimal currentStockQuantity,
    decimal reservedStockQuantity,
    decimal stagingStockQuantity)
{
    /// <summary>
    /// Primary key: ItemId per Architecture standards.
    /// Uses int for legacy inventory system compatibility.
    /// </summary>
    public int ItemId { get; init; } = itemId;

    /// <summary>
    /// Stock Keeping Unit Code (SKU) - unique product identifier.
    /// Must be unique per business rules.
    /// </summary>
    public required string StockKeepingUnitCode { get; init; } = stockKeepingUnitCode;

    /// <summary>
    /// Full item description (e.g., "Portland Cement Type I 50kg Bag").
    /// </summary>
    public required string ItemDescription { get; init; } = itemDescription;

    /// <summary>
    /// Foreign key to Brand (nullable).
    /// Uses int to reference Inventory.Brand.
    /// </summary>
    public int? BrandId { get; init; }

    /// <summary>
    /// Foreign key to Category (required).
    /// Uses int to reference Inventory.Category.
    /// </summary>
    public required int CategoryId { get; init; } = categoryId;

    /// <summary>
    /// Foreign key to TaxConfiguration (required).
    /// Uses Guid to reference Core.TaxConfiguration.
    /// </summary>
    public required Guid TaxConfigurationId { get; init; } = taxConfigurationId;

    /// <summary>
    /// Foreign key to UnitOfMeasure.
    /// This is the BASE unit - the smallest indivisible unit (e.g., "Pala", "Unidad").
    /// Uses int to reference Core.UnitOfMeasure.
    /// </summary>
    public required int BaseUnitOfMeasureId { get; init; } = baseUnitOfMeasureId;

    /// <summary>
    /// Foreign key to UnitOfMeasure for default sales unit (nullable).
    /// Uses int to reference Core.UnitOfMeasure.
    /// </summary>
    public int? DefaultSalesUnitOfMeasureId { get; init; }

    /// <summary>
    /// Weighted Average Cost per BASE unit.
    /// Recalculated upon Purchase Invoice approval.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal CostPricePerBaseUnit { get; set; } = costPricePerBaseUnit;

    /// <summary>
    /// Selling price per BASE unit.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal SellingPricePerBaseUnit { get; set; } = sellingPricePerBaseUnit;

    /// <summary>
    /// Minimum required stock quantity for reorder alerts.
    /// Precision: DECIMAL(18,2).
    /// </summary>
    public required decimal MinimumRequiredStockQuantity { get; init; } = minimumRequiredStockQuantity;

    /// <summary>
    /// Current stock quantity in the BASE unit.
    /// Represents physically verified stock in the warehouse.
    /// READ-ONLY for UI: Only updated via formal transactions.
    /// Precision: DECIMAL(18,2).
    /// </summary>
    public required decimal CurrentStockQuantity { get; set; } = currentStockQuantity;

    /// <summary>
    /// Reserved stock quantity in the BASE unit.
    /// Stock already invoiced or with a pending "Conduce" (delivery note) that hasn't left the warehouse.
    /// This stock is committed to customers but not yet physically delivered.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal ReservedStockQuantity { get; set; } = reservedStockQuantity;

    /// <summary>
    /// Staging stock quantity in the BASE unit.
    /// Stock that has physically arrived (via Purchasing Staging) but hasn't been verified/posted to official inventory.
    /// This represents goods in receiving/inspection that are not yet available for sale.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal StagingStockQuantity { get; set; } = stagingStockQuantity;

    /// <summary>
    /// Physical warehouse location code (e.g., "P1-T10-A" for Pasillo 1, Torre 10, Anaquel A).
    /// Optional field for warehouse management.
    /// </summary>
    public string? LocationCode { get; set; }

    /// <summary>
    /// URL reference for the product image.
    /// Can point to internal storage, CDN, or external resource.
    /// Optional field for UI display purposes.
    /// </summary>
    public string? ImageUrl { get; set; }

    // ============================================================
    // CALCULATED PROPERTIES (Business Logic)
    // ============================================================

    /// <summary>
    /// Available stock quantity for sales commitment.
    /// Calculated as: CurrentStockQuantity - ReservedStockQuantity.
    /// This represents the actual stock that the sales department can commit to customers.
    /// READ-ONLY: Computed at runtime.
    /// </summary>
    public decimal AvailableStockQuantity => CurrentStockQuantity - ReservedStockQuantity;

    // ============================================================
    // NAVIGATION PROPERTIES (EF Core Relationships)
    // ============================================================

    /// <summary>
    /// Navigation property to Brand (nullable).
    /// </summary>
    public virtual Brand? Brand { get; init; }

    /// <summary>
    /// Navigation property to Category (required).
    /// </summary>
    public virtual Category? Category { get; init; }

    /// <summary>
    /// Navigation property to TaxConfiguration (required).
    /// </summary>
    public virtual TaxConfiguration? TaxConfiguration { get; init; }

    /// <summary>
    /// Navigation property to Base UnitOfMeasure (required).
    /// </summary>
    public virtual UnitOfMeasure? BaseUnitOfMeasure { get; init; }

    /// <summary>
    /// Navigation property to Default Sales UnitOfMeasure (nullable).
    /// </summary>
    public virtual UnitOfMeasure? DefaultSalesUnitOfMeasure { get; init; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public Item() : this(0, string.Empty, string.Empty, 0, Guid.Empty, 0, 0, 0, 0, 0, 0, 0)
    {
    }
}


