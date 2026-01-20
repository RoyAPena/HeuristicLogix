using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Represents a material item in a delivery order.
/// Used for heuristic capacity calculations and compatibility rule enforcement.
/// </summary>
public class MaterialItem
{
    /// <summary>
    /// Unique identifier for the material item.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name of the material (e.g., "Rebar", "CementBags", "Glass").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Category for grouping materials and applying compatibility rules.
    /// </summary>
    public required MaterialCategory Category { get; set; }

    /// <summary>
    /// Quantity of this material in the order.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit of measurement (e.g., "units", "bags", "meters").
    /// </summary>
    public required string UnitOfMeasure { get; set; }

    /// <summary>
    /// Weight per unit in kilograms. Used for heuristic capacity inference.
    /// </summary>
    public double WeightPerUnit { get; set; }

    /// <summary>
    /// Indicates if this material requires special handling (crane, forklift, etc.).
    /// </summary>
    public bool RequiresSpecialHandling { get; set; }

    /// <summary>
    /// Special handling notes for the delivery team.
    /// </summary>
    public string? SpecialHandlingNotes { get; set; }

    /// <summary>
    /// Historical average loading time for this material type (in minutes).
    /// Learned from expert feedback.
    /// </summary>
    public double? HeuristicLoadingTime { get; set; }

    /// <summary>
    /// Computed total weight for this item (Quantity * WeightPerUnit).
    /// </summary>
    public double TotalWeight => Quantity * WeightPerUnit;

    /// <summary>
    /// Timestamp when this item was added to the order.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Material categories for compatibility rule enforcement.
/// Serialized as string for ML readability and API consistency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MaterialCategory
{
    /// <summary>
    /// Heavy construction materials (steel, rebar, beams).
    /// </summary>
    HeavyConstruction,

    /// <summary>
    /// Fragile materials (glass, tiles, ceramics).
    /// </summary>
    Fragile,

    /// <summary>
    /// Bulk materials (sand, gravel, cement).
    /// </summary>
    Bulk,

    /// <summary>
    /// Long materials requiring flatbed (lumber, pipes, rebar lengths).
    /// </summary>
    LongFormat,

    /// <summary>
    /// Palletized goods (cement bags, blocks).
    /// </summary>
    Palletized,

    /// <summary>
    /// Miscellaneous hardware and tools.
    /// </summary>
    Hardware
}
