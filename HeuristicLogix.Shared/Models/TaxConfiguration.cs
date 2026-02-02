namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Tax configuration for fiscal compliance.
/// Part of the Core schema.
/// </summary>
public class TaxConfiguration(
    Guid taxConfigurationId,
    string taxName,
    decimal taxPercentageRate,
    bool isActive)
{
    /// <summary>
    /// Primary key: TaxConfigurationId per Architecture standards.
    /// </summary>
    public Guid TaxConfigurationId { get; init; } = taxConfigurationId;

    /// <summary>
    /// Tax name (e.g., "ITBIS General 18%").
    /// </summary>
    public required string TaxName { get; init; } = taxName;

    /// <summary>
    /// Tax rate as percentage (e.g., 18.00 for 18%).
    /// Precision: DECIMAL(5,2).
    /// </summary>
    public required decimal TaxPercentageRate { get; init; } = taxPercentageRate;

    /// <summary>
    /// Whether this tax configuration is currently active.
    /// </summary>
    public required bool IsActive { get; init; } = isActive;

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public TaxConfiguration() : this(Guid.NewGuid(), string.Empty, 0, true)
    {
    }
}
