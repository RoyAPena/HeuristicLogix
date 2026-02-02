namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Defines conversion factors between different units for the same item.
/// Part of the Inventory schema.
/// Bridge entity: Uses Guid for its own PK but int for ItemId FK.
/// Formula: ToUnitQuantity = FromUnitQuantity * ConversionFactorQuantity
/// </summary>
public class ItemUnitConversion(
    Guid itemUnitConversionId,
    int itemId,
    int fromUnitOfMeasureId,
    int toUnitOfMeasureId,
    decimal conversionFactorQuantity)
{
    /// <summary>
    /// Primary key: ItemUnitConversionId per Architecture standards.
    /// Uses Guid as this is a transactional/bridge entity.
    /// </summary>
    public Guid ItemUnitConversionId { get; init; } = itemUnitConversionId;

    /// <summary>
    /// Foreign key to Item.
    /// The item this conversion applies to.
    /// Uses int to reference Inventory.Item.
    /// </summary>
    public required int ItemId { get; init; } = itemId;

    /// <summary>
    /// Foreign key to UnitOfMeasure.
    /// The source unit being converted FROM.
    /// Uses int to reference Core.UnitOfMeasure.
    /// </summary>
    public required int FromUnitOfMeasureId { get; init; } = fromUnitOfMeasureId;

    /// <summary>
    /// Foreign key to UnitOfMeasure.
    /// The target unit being converted TO.
    /// Uses int to reference Core.UnitOfMeasure.
    /// </summary>
    public required int ToUnitOfMeasureId { get; init; } = toUnitOfMeasureId;

    /// <summary>
    /// Conversion factor: How many ToUnits equal one FromUnit.
    /// Formula: ToQuantity = FromQuantity * ConversionFactorQuantity
    /// Example: If 1 "Metro" = 40 "Palas", this value is 40.00
    /// Precision: DECIMAL(18,4).
    /// </summary>
    public required decimal ConversionFactorQuantity { get; init; } = conversionFactorQuantity;

    // ============================================================
    // NAVIGATION PROPERTIES (EF Core Relationships)
    // ============================================================

    /// <summary>
    /// Navigation property to Item.
    /// </summary>
    public virtual Item? Item { get; init; }

    /// <summary>
    /// Navigation property to From UnitOfMeasure.
    /// </summary>
    public virtual UnitOfMeasure? FromUnitOfMeasure { get; init; }

    /// <summary>
    /// Navigation property to To UnitOfMeasure.
    /// </summary>
    public virtual UnitOfMeasure? ToUnitOfMeasure { get; init; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    public ItemUnitConversion() : this(Guid.NewGuid(), 0, 0, 0, 0)
    {
    }
}


