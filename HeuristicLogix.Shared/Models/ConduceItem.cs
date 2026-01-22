using HeuristicLogix.Shared.Domain;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Represents a line item within a Conduce (delivery order).
/// Each item represents a specific material/product being delivered.
/// </summary>
public class ConduceItem : Entity
{
    /// <summary>
    /// Parent Conduce identifier.
    /// </summary>
    public required Guid ConduceId { get; init; }

    /// <summary>
    /// Reference to parent Conduce.
    /// </summary>
    public Conduce? Conduce { get; set; }

    /// <summary>
    /// Material/Product name (raw description from source).
    /// Example: "Varilla de acero 1/2", "Cemento Portland", "Arena lavada"
    /// </summary>
    public required string MaterialName { get; set; }

    /// <summary>
    /// Quantity of material.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of measure (BOLSA, M3, TON, PIEZA, METRO, UNIDAD).
    /// </summary>
    public required string Unit { get; set; }

    /// <summary>
    /// Weight in kilograms (calculated or provided).
    /// </summary>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Material type classification for logistics planning.
    /// Used to determine truck type and loading strategy.
    /// </summary>
    public MaterialType MaterialType { get; set; } = MaterialType.General;

    /// <summary>
    /// Product taxonomy reference (if matched).
    /// </summary>
    public Guid? ProductTaxonomyId { get; set; }

    /// <summary>
    /// Reference to product taxonomy.
    /// </summary>
    public ProductTaxonomy? ProductTaxonomy { get; set; }

    /// <summary>
    /// Whether this item was automatically classified.
    /// False if manually verified by expert.
    /// </summary>
    public bool IsAutoClassified { get; set; } = true;

    /// <summary>
    /// Line number in original source (Excel row or form order).
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Additional notes about this item.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this item was added.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Material type classification for logistics planning.
/// Determines truck type requirements and loading strategy.
/// </summary>
public enum MaterialType
{
    /// <summary>
    /// General materials (default).
    /// No special handling required.
    /// </summary>
    General = 0,

    /// <summary>
    /// Long materials requiring special truck bed.
    /// Examples: Varillas, tubos, vigas, madera larga
    /// Requires: Flatbed truck with extended bed
    /// </summary>
    Long = 1,

    /// <summary>
    /// Heavy materials requiring reinforced truck.
    /// Examples: Cemento, acero estructural, bloques
    /// Requires: Heavy-duty truck with weight capacity
    /// </summary>
    Heavy = 2,

    /// <summary>
    /// Bulk materials requiring dump truck.
    /// Examples: Arena, piedra, grava, agregados
    /// Requires: Dump truck with tilting bed
    /// </summary>
    Bulk = 3,

    /// <summary>
    /// Fragile materials requiring careful handling.
    /// Examples: Vidrio, cerámica, pintura
    /// Requires: Specialized securing and padding
    /// </summary>
    Fragile = 4,

    /// <summary>
    /// Hazardous materials requiring certified transport.
    /// Examples: Chemicals, flammable materials
    /// Requires: Certified driver and special permits
    /// </summary>
    Hazardous = 5
}
