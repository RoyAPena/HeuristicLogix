namespace HeuristicLogix.Shared.Modules.Inventory;

/// <summary>
/// Inventory module API contract.
/// Provides operations for stock management and material reservation.
/// </summary>
public interface IInventoryModuleAPI : IModuleAPI
{
    /// <summary>
    /// Gets product information.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product information or null if not found.</returns>
    Task<ProductInfo?> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current stock level for a product at a warehouse.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="warehouseId">The warehouse identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stock information or null if not found.</returns>
    Task<StockInfo?> GetStockAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if sufficient stock is available for materials.
    /// </summary>
    /// <param name="materials">List of materials to check.</param>
    /// <param name="warehouseId">Optional warehouse identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stock availability result.</returns>
    Task<StockAvailabilityResult> CheckStockAvailabilityAsync(
        List<MaterialCheck> materials,
        Guid? warehouseId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Product information.
/// </summary>
public class ProductInfo
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public required Guid ProductId { get; init; }

    /// <summary>
    /// Product name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Product SKU or code.
    /// </summary>
    public required string Sku { get; init; }

    /// <summary>
    /// Product category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Unit of measure (e.g., "kg", "units", "meters").
    /// </summary>
    public required string UnitOfMeasure { get; init; }

    /// <summary>
    /// Weight per unit in kilograms.
    /// </summary>
    public decimal WeightPerUnit { get; init; }

    /// <summary>
    /// Whether the product is active.
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Stock information for a product at a warehouse.
/// </summary>
public class StockInfo
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public required Guid ProductId { get; init; }

    /// <summary>
    /// Warehouse identifier.
    /// </summary>
    public required Guid WarehouseId { get; init; }

    /// <summary>
    /// Current quantity on hand.
    /// </summary>
    public required decimal QuantityOnHand { get; init; }

    /// <summary>
    /// Quantity reserved for orders.
    /// </summary>
    public required decimal QuantityReserved { get; init; }

    /// <summary>
    /// Available quantity (OnHand - Reserved).
    /// </summary>
    public decimal AvailableQuantity => QuantityOnHand - QuantityReserved;

    /// <summary>
    /// Minimum stock level (reorder point).
    /// </summary>
    public decimal? MinimumLevel { get; init; }

    /// <summary>
    /// Whether stock is below minimum level.
    /// </summary>
    public bool IsBelowMinimum => MinimumLevel.HasValue && AvailableQuantity < MinimumLevel.Value;

    /// <summary>
    /// Last stock movement date.
    /// </summary>
    public DateTimeOffset? LastMovementDate { get; init; }
}

/// <summary>
/// Material to check for stock availability.
/// </summary>
public class MaterialCheck
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public required Guid ProductId { get; init; }

    /// <summary>
    /// Required quantity.
    /// </summary>
    public required decimal Quantity { get; init; }

    /// <summary>
    /// Product name (for reporting).
    /// </summary>
    public string? ProductName { get; init; }
}

/// <summary>
/// Result of stock availability check.
/// </summary>
public class StockAvailabilityResult
{
    /// <summary>
    /// Whether all materials are available.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// Materials that are not available or have insufficient stock.
    /// </summary>
    public List<StockShortage> Shortages { get; init; } = new List<StockShortage>();

    /// <summary>
    /// Warning message if stock is available but below safety levels.
    /// </summary>
    public string? WarningMessage { get; init; }

    /// <summary>
    /// Creates a result indicating all materials are available.
    /// </summary>
    public static StockAvailabilityResult Available(string? warning = null)
    {
        return new StockAvailabilityResult
        {
            IsAvailable = true,
            WarningMessage = warning
        };
    }

    /// <summary>
    /// Creates a result indicating stock shortages.
    /// </summary>
    public static StockAvailabilityResult Unavailable(List<StockShortage> shortages)
    {
        return new StockAvailabilityResult
        {
            IsAvailable = false,
            Shortages = shortages
        };
    }
}

/// <summary>
/// Information about a stock shortage.
/// </summary>
public class StockShortage
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public required Guid ProductId { get; init; }

    /// <summary>
    /// Product name.
    /// </summary>
    public required string ProductName { get; init; }

    /// <summary>
    /// Required quantity.
    /// </summary>
    public required decimal RequiredQuantity { get; init; }

    /// <summary>
    /// Available quantity.
    /// </summary>
    public required decimal AvailableQuantity { get; init; }

    /// <summary>
    /// Shortage quantity (Required - Available).
    /// </summary>
    public decimal ShortageQuantity => RequiredQuantity - AvailableQuantity;

    /// <summary>
    /// Expected restock date if known.
    /// </summary>
    public DateTimeOffset? ExpectedRestockDate { get; init; }
}
