namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Catalog of items offered by each supplier with their pricing.
/// Part of the Purchasing schema.
/// Composite primary key: ItemId (int) + SupplierId (Guid).
/// Hybrid ID: int for inventory reference, Guid for purchasing reference.
/// Updated upon Purchase Invoice approval with latest price and date.
/// </summary>
public class ItemSupplier(
    int itemId,
    Guid supplierId,
    string? supplierInternalPartNumber,
    decimal? lastPurchasePriceAmount,
    DateTimeOffset? lastPurchaseDateTime,
    bool isPreferredSupplierForItem)
{
    /// <summary>
    /// Foreign key to Item (part of composite PK).
    /// The item being supplied.
    /// Uses int to reference Inventory.Item.
    /// </summary>
    public required int ItemId { get; init; } = itemId;

    /// <summary>
    /// Foreign key to Supplier (part of composite PK).
    /// The supplier offering this item.
    /// Uses Guid to reference Purchasing.Supplier.
    /// </summary>
    public required Guid SupplierId { get; init; } = supplierId;

    /// <summary>
    /// Supplier's internal part number or SKU for this item.
    /// </summary>
    public string? SupplierInternalPartNumber { get; init; } = supplierInternalPartNumber;

    /// <summary>
    /// Last purchase price amount per BASE unit from this supplier.
    /// Updated upon Purchase Invoice approval.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public decimal? LastPurchasePriceAmount { get; set; } = lastPurchasePriceAmount;

    /// <summary>
    /// Date and time of the last purchase from this supplier.
    /// Updated upon Purchase Invoice approval.
    /// </summary>
    public DateTimeOffset? LastPurchaseDateTime { get; set; } = lastPurchaseDateTime;

    /// <summary>
    /// Indicates if this is the preferred supplier for the item.
    /// </summary>
    public required bool IsPreferredSupplierForItem { get; init; } = isPreferredSupplierForItem;

    // ============================================================
    // NAVIGATION PROPERTIES (EF Core Relationships)
    // ============================================================

    /// <summary>
    /// Navigation property to Item.
    /// </summary>
    public virtual Item? Item { get; init; }

    /// <summary>
    /// Navigation property to Supplier.
    /// </summary>
    public virtual Supplier? Supplier { get; init; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public ItemSupplier() : this(0, Guid.Empty, null, null, null, false)
    {
    }
}


