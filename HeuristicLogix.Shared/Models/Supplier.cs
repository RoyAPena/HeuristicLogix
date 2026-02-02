namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Supplier (Vendor) master record.
/// Part of the Purchasing schema.
/// </summary>
public class Supplier(
    Guid supplierId,
    string nationalTaxIdentificationNumber,
    string supplierBusinessName,
    string supplierTradeName,
    int? defaultCreditDaysDuration,
    bool isActive)
{
    /// <summary>
    /// Primary key: SupplierId per Architecture standards.
    /// </summary>
    public Guid SupplierId { get; init; } = supplierId;

    /// <summary>
    /// National Tax Identification Number (RNC in Dominican Republic).
    /// Must be 9 or 11 characters.
    /// Must be unique per business rules.
    /// </summary>
    public required string NationalTaxIdentificationNumber { get; init; } = nationalTaxIdentificationNumber;

    /// <summary>
    /// Legal business name of the supplier.
    /// </summary>
    public required string SupplierBusinessName { get; init; } = supplierBusinessName;

    /// <summary>
    /// Trade name / DBA (Doing Business As) of the supplier.
    /// </summary>
    public required string SupplierTradeName { get; init; } = supplierTradeName;

    /// <summary>
    /// Default credit terms in days (template value).
    /// Real due date = InvoiceDate + CreditDays.
    /// </summary>
    public int? DefaultCreditDaysDuration { get; init; } = defaultCreditDaysDuration;

    /// <summary>
    /// Whether this supplier is currently active for purchases.
    /// </summary>
    public required bool IsActive { get; init; } = isActive;

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public Supplier() : this(Guid.NewGuid(), string.Empty, string.Empty, string.Empty, null, true)
    {
    }
}

