namespace HeuristicLogix.Shared.DTOs;

/// <summary>
/// DTO for UnitOfMeasure create/update operations.
/// Used for both insert and update to maintain a single contract.
/// </summary>
public record UnitOfMeasureUpsertDto
{
    public int UnitOfMeasureId { get; init; }
    public string UnitOfMeasureName { get; init; } = string.Empty;
    public string UnitOfMeasureSymbol { get; init; } = string.Empty;
}
