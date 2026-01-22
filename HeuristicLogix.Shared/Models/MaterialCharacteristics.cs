namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Characteristics and handling requirements for a material type.
/// Used for logistics planning and truck selection.
/// </summary>
public class MaterialCharacteristics
{
    /// <summary>
    /// Primary material type classification.
    /// </summary>
    public required MaterialType Type { get; init; }

    /// <summary>
    /// Whether this material requires special handling during loading/transport.
    /// Examples: Long materials (rebar/varilla), fragile items, hazardous materials.
    /// Note: All trucks can carry long materials, but requires special attention.
    /// </summary>
    public bool RequiresSpecialHandling { get; init; }

    /// <summary>
    /// Specific handling notes for logistics team.
    /// </summary>
    public string? HandlingNotes { get; init; }

    /// <summary>
    /// Preferred truck types for this material (ordered by preference).
    /// Empty list means any truck is suitable.
    /// </summary>
    public List<TruckType> PreferredTruckTypes { get; init; } = new List<TruckType>();

    /// <summary>
    /// Whether this material can be mixed with other types in same truck.
    /// </summary>
    public bool AllowsMixedLoad { get; init; } = true;

    /// <summary>
    /// Priority level for loading (higher = load first).
    /// Used for optimizing load order.
    /// </summary>
    public int LoadingPriority { get; init; }

    /// <summary>
    /// Creates default characteristics for General materials.
    /// </summary>
    public static MaterialCharacteristics General => new MaterialCharacteristics
    {
        Type = MaterialType.General,
        RequiresSpecialHandling = false,
        AllowsMixedLoad = true,
        LoadingPriority = 0
    };

    /// <summary>
    /// Creates characteristics for Long materials (Rebar/Varilla).
    /// </summary>
    public static MaterialCharacteristics Long => new MaterialCharacteristics
    {
        Type = MaterialType.Long,
        RequiresSpecialHandling = true,
        HandlingNotes = "Long materials require secure tie-down. All trucks can transport.",
        AllowsMixedLoad = true,
        LoadingPriority = 3,
        PreferredTruckTypes = new List<TruckType> { TruckType.Flatbed, TruckType.Dump }
    };

    /// <summary>
    /// Creates characteristics for Heavy materials.
    /// </summary>
    public static MaterialCharacteristics Heavy => new MaterialCharacteristics
    {
        Type = MaterialType.Heavy,
        RequiresSpecialHandling = false,
        HandlingNotes = "Heavy weight - check truck capacity limits",
        AllowsMixedLoad = true,
        LoadingPriority = 1,
        PreferredTruckTypes = new List<TruckType> { TruckType.Dump }
    };

    /// <summary>
    /// Creates characteristics for Bulk materials.
    /// </summary>
    public static MaterialCharacteristics Bulk => new MaterialCharacteristics
    {
        Type = MaterialType.Bulk,
        RequiresSpecialHandling = false,
        HandlingNotes = "Bulk materials - dump truck preferred for easy unloading",
        AllowsMixedLoad = false, // Bulk materials typically need dedicated truck
        LoadingPriority = 2,
        PreferredTruckTypes = new List<TruckType> { TruckType.Dump }
    };

    /// <summary>
    /// Creates characteristics for Fragile materials.
    /// </summary>
    public static MaterialCharacteristics Fragile => new MaterialCharacteristics
    {
        Type = MaterialType.Fragile,
        RequiresSpecialHandling = true,
        HandlingNotes = "Fragile - requires padding and careful securing",
        AllowsMixedLoad = true,
        LoadingPriority = 5, // Load last to avoid damage
        PreferredTruckTypes = new List<TruckType> { TruckType.Flatbed }
    };

    /// <summary>
    /// Creates characteristics for Hazardous materials.
    /// </summary>
    public static MaterialCharacteristics Hazardous => new MaterialCharacteristics
    {
        Type = MaterialType.Hazardous,
        RequiresSpecialHandling = true,
        HandlingNotes = "Hazardous materials - requires certified driver and special permits",
        AllowsMixedLoad = false, // Never mix hazardous with other materials
        LoadingPriority = 10, // Highest priority - handle separately
        PreferredTruckTypes = new List<TruckType> { TruckType.Flatbed }
    };
}
