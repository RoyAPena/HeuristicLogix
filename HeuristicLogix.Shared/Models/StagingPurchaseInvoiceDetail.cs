namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Staging table for Purchase Invoice detail lines.
/// Part of the Purchasing schema.
/// Uses Guid for its own ID but int for ItemId FK.
/// </summary>
public class StagingPurchaseInvoiceDetail(
    Guid stagingPurchaseInvoiceDetailId,
    Guid stagingPurchaseInvoiceId,
    int itemId,
    decimal receivedQuantity,
    decimal unitPriceAmount)
{
    /// <summary>
    /// Primary key: StagingPurchaseInvoiceDetailId per Architecture standards.
    /// Uses Guid for transactional entities.
    /// </summary>
    public Guid StagingPurchaseInvoiceDetailId { get; init; } = stagingPurchaseInvoiceDetailId;

    /// <summary>
    /// Foreign key to StagingPurchaseInvoice.
    /// Uses Guid to reference Purchasing.StagingPurchaseInvoice.
    /// </summary>
    public required Guid StagingPurchaseInvoiceId { get; init; } = stagingPurchaseInvoiceId;

    /// <summary>
    /// Foreign key to Item (required).
    /// Uses int to reference Inventory.Item.
    /// </summary>
    public required int ItemId { get; init; } = itemId;

    /// <summary>
    /// Quantity received.
    /// Precision: DECIMAL(18,2).
    /// </summary>
    public required decimal ReceivedQuantity { get; init; } = receivedQuantity;

    /// <summary>
    /// Unit price amount.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal UnitPriceAmount { get; init; } = unitPriceAmount;

    // ============================================================
    // NAVIGATION PROPERTIES (EF Core Relationships)
    // ============================================================

    /// <summary>
    /// Navigation property to StagingPurchaseInvoice (required).
    /// </summary>
    public virtual StagingPurchaseInvoice? StagingPurchaseInvoice { get; init; }

    /// <summary>
    /// Navigation property to Item (required).
    /// </summary>
    public virtual Item? Item { get; init; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public StagingPurchaseInvoiceDetail() : this(Guid.NewGuid(), Guid.Empty, 0, 0, 0)
    {
    }
}


