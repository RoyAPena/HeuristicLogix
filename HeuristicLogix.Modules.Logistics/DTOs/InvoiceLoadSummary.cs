using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Modules.Logistics.DTOs;

/// <summary>
/// Summary of an invoice load for pre-save review.
/// Provides capacity analysis and truck compatibility information.
/// </summary>
public class InvoiceLoadSummary
{
    /// <summary>
    /// Invoice/Conduce number.
    /// </summary>
    public required string InvoiceNumber { get; init; }

    /// <summary>
    /// Client name.
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// Delivery address.
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Total weight of all items (kg).
    /// </summary>
    public decimal TotalWeightKg { get; init; }

    /// <summary>
    /// Number of line items.
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Breakdown of weight by material type.
    /// </summary>
    public required Dictionary<MaterialType, decimal> WeightByType { get; init; }

    /// <summary>
    /// Dominant material type for truck selection.
    /// </summary>
    public MaterialType DominantMaterialType { get; init; }

    /// <summary>
    /// Whether any items require special handling.
    /// </summary>
    public bool RequiresSpecialHandling { get; init; }

    /// <summary>
    /// List of materials requiring special handling.
    /// </summary>
    public List<string> SpecialHandlingItems { get; init; } = new List<string>();

    /// <summary>
    /// Compatible truck types for this load.
    /// </summary>
    public List<TruckType> CompatibleTruckTypes { get; init; } = new List<TruckType>();

    /// <summary>
    /// Human-readable summary message.
    /// Example: "Invoice #123: 500kg Heavy, 100kg Long (Rebar). Compatible with All Trucks."
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Warnings about the load (e.g., "Heavy weight - check truck capacity").
    /// </summary>
    public List<string> Warnings { get; init; } = new List<string>();

    /// <summary>
    /// Whether this load can be safely mixed (no hazardous or bulk-only items).
    /// </summary>
    public bool AllowsMixedLoad { get; init; }

    /// <summary>
    /// Geocoding result for the address.
    /// </summary>
    public GeocodingResult? GeocodingResult { get; init; }

    /// <summary>
    /// Whether the invoice is valid for persistence.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Validation errors if any.
    /// </summary>
    public List<string> ValidationErrors { get; init; } = new List<string>();

    /// <summary>
    /// Whether this invoice requires manual review (geocoding ambiguous or failed).
    /// </summary>
    public bool RequiresManualReview { get; init; }

    /// <summary>
    /// Geocoding status (Success, Ambiguous, Failed).
    /// </summary>
    public GeocodingStatus GeocodingStatus { get; init; }
}

/// <summary>
/// Detailed breakdown of an invoice item.
/// </summary>
public class InvoiceItemDetail
{
    /// <summary>
    /// Line number.
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// Material name.
    /// </summary>
    public required string MaterialName { get; init; }

    /// <summary>
    /// Quantity.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Unit of measure.
    /// </summary>
    public required string Unit { get; init; }

    /// <summary>
    /// Weight (kg).
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Material type classification.
    /// </summary>
    public MaterialType MaterialType { get; init; }

    /// <summary>
    /// Material characteristics.
    /// </summary>
    public required MaterialCharacteristics Characteristics { get; init; }

    /// <summary>
    /// Whether this item was matched to a product taxonomy.
    /// </summary>
    public bool HasTaxonomyMatch { get; init; }

    /// <summary>
    /// Matched taxonomy ID if found.
    /// </summary>
    public Guid? TaxonomyId { get; init; }
}
