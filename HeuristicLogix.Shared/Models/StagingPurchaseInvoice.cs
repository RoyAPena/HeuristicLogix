namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Staging table for Purchase Invoice ingestion.
/// Part of the Purchasing schema.
/// Mass ingestion pattern to prevent DB locking.
/// </summary>
public class StagingPurchaseInvoice(
    Guid stagingPurchaseInvoiceId,
    Guid supplierId,
    string fiscalReceiptNumber,
    DateTimeOffset invoiceIssueDateTime,
    decimal totalAmount)
{
    /// <summary>
    /// Primary key: StagingPurchaseInvoiceId per Architecture standards.
    /// </summary>
    public Guid StagingPurchaseInvoiceId { get; init; } = stagingPurchaseInvoiceId;

    /// <summary>
    /// Foreign key to Supplier (required).
    /// </summary>
    public required Guid SupplierId { get; init; } = supplierId;

    /// <summary>
    /// Fiscal Receipt Number (NCF) from the original document.
    /// Format: B + Type (2 digits) + Sequence (8 digits).
    /// </summary>
    public required string FiscalReceiptNumber { get; init; } = fiscalReceiptNumber;

    /// <summary>
    /// Date and time when the supplier issued the invoice.
    /// </summary>
    public required DateTimeOffset InvoiceIssueDateTime { get; init; } = invoiceIssueDateTime;

    /// <summary>
    /// Total invoice amount.
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal TotalAmount { get; init; } = totalAmount;

    // ============================================================
    // NAVIGATION PROPERTIES (EF Core Relationships)
    // ============================================================

    /// <summary>
    /// Navigation property to Supplier (required).
    /// </summary>
    public virtual Supplier? Supplier { get; init; }

    /// <summary>
    /// Navigation property: Staging detail lines.
    /// </summary>
    public ICollection<StagingPurchaseInvoiceDetail> Details { get; init; } = new List<StagingPurchaseInvoiceDetail>();

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public StagingPurchaseInvoice() : this(Guid.NewGuid(), Guid.Empty, string.Empty, DateTimeOffset.UtcNow, 0)
    {
    }
}

