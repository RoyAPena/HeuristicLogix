namespace HeuristicLogix.Shared.DTOs;

/// <summary>
/// DTO for Category create/update operations.
/// Used for both insert and update to maintain a single contract.
/// </summary>
public record CategoryUpsertDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
