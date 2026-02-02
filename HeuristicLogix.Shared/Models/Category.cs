namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Product/Item category for classification.
/// Part of the Inventory schema.
/// Uses int ID for legacy compatibility.
/// </summary>
public class Category(
    int categoryId,
    string categoryName)
{
    /// <summary>
    /// Primary key: CategoryId per Architecture standards.
    /// Uses int for legacy inventory system compatibility.
    /// </summary>
    public int CategoryId { get; init; } = categoryId;

    /// <summary>
    /// Full descriptive category name (e.g., "Construction Materials", "Cement Products").
    /// </summary>
    public required string CategoryName { get; init; } = categoryName;

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public Category() : this(0, string.Empty)
    {
    }
}
