using HeuristicLogix.Shared.Domain;
using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Product taxonomy for standardizing product descriptions and weight calculations.
/// Serves as the foundation for the product catalog in the Inventory module.
/// </summary>
public class ProductTaxonomy : Entity
{
    /// <summary>
    /// Standardized product description (sanitized, uppercase).
    /// Example: "AGGREGATE", "CEMENT PORTLAND", "STEEL REBAR 12MM"
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Product category for grouping similar products.
    /// Examples: "AGGREGATE", "CEMENT", "STEEL", "REBAR", "LUMBER"
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Weight factor for calculating total weight from quantity.
    /// Units: kg per unit (e.g., kg/bag, kg/m3, kg/piece)
    /// Example: Cement bag = 50 kg, Aggregate m3 = 1600 kg
    /// </summary>
    public decimal WeightFactor { get; set; }

    /// <summary>
    /// Standard unit of measure for this product.
    /// Examples: "BAG", "M3", "TON", "PIECE", "METER"
    /// </summary>
    public string? StandardUnit { get; set; }

    /// <summary>
    /// Whether this taxonomy entry has been verified by an expert.
    /// False = Pending verification (auto-created from historic data)
    /// True = Verified by expert or pre-configured
    /// </summary>
    public bool IsVerifiedByExpert { get; set; } = false;

    /// <summary>
    /// Number of times this product has been used in deliveries.
    /// Used to prioritize verification of frequently used products.
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Additional notes about this product (handling, storage, etc.).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this taxonomy entry was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When this taxonomy entry was last modified.
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; set; }

    /// <summary>
    /// Who verified this taxonomy entry (if verified).
    /// </summary>
    public string? VerifiedBy { get; set; }

    /// <summary>
    /// When this entry was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// Marks this entry as verified by an expert.
    /// </summary>
    public void MarkAsVerified(string verifiedBy)
    {
        IsVerifiedByExpert = true;
        VerifiedBy = verifiedBy;
        VerifiedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Increments the usage count when this product is used in a delivery.
    /// </summary>
    public void IncrementUsage()
    {
        UsageCount++;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Taxonomy verification status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaxonomyStatus
{
    /// <summary>
    /// Pending expert verification.
    /// </summary>
    PendingVerification,

    /// <summary>
    /// Verified by expert.
    /// </summary>
    Verified,

    /// <summary>
    /// Deprecated (no longer used).
    /// </summary>
    Deprecated
}
