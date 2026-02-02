namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Brand/Manufacturer catalog for items.
/// Part of the Inventory schema.
/// Uses int ID for legacy compatibility.
/// </summary>
public class Brand(
    int brandId,
    string brandName)
{
    /// <summary>
    /// Primary key: BrandId per Architecture standards.
    /// Uses int for legacy inventory system compatibility.
    /// </summary>
    public int BrandId { get; init; } = brandId;

    /// <summary>
    /// Full brand/manufacturer name (e.g., "Cemex", "LaFarge Holcim").
    /// </summary>
    public required string BrandName { get; init; } = brandName;

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public Brand() : this(0, string.Empty)
    {
    }
}
