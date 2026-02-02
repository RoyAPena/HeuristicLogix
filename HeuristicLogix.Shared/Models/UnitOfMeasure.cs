namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Unit of Measure master catalog.
/// Part of the Core schema.
/// Uses int ID for legacy compatibility.
/// </summary>
public class UnitOfMeasure(
    int unitOfMeasureId,
    string unitOfMeasureName,
    string unitOfMeasureSymbol)
{
    /// <summary>
    /// Primary key: UnitOfMeasureId per Architecture standards.
    /// Uses int for legacy inventory system compatibility.
    /// </summary>
    public int UnitOfMeasureId { get; init; } = unitOfMeasureId;

    /// <summary>
    /// Full descriptive name (e.g., "Kilogram", "Meter", "Unit").
    /// </summary>
    public required string UnitOfMeasureName { get; init; } = unitOfMeasureName;

    /// <summary>
    /// Short symbol for the unit (e.g., "kg", "m", "un").
    /// </summary>
    public required string UnitOfMeasureSymbol { get; init; } = unitOfMeasureSymbol;

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public UnitOfMeasure() : this(0, string.Empty, string.Empty)
    {
    }
}
