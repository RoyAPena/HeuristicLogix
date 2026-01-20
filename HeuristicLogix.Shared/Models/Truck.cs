using System.Text.Json.Serialization;
using HeuristicLogix.Shared.Domain;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Truck aggregate root.
/// Represents a delivery truck with Heuristic Capacity.
/// Capacity is NOT defined by rigid 3D dimensions but is learned from expert assignment history.
/// </summary>
public class Truck : AggregateRoot
{
    public required string PlateNumber { get; set; }

    public required TruckType TruckType { get; set; }

    /// <summary>
    /// Heuristic capacity score derived from expert loading history.
    /// Represents the learned "effective capacity" based on what experts have historically loaded.
    /// </summary>
    public double HeuristicCapacityScore { get; set; }

    /// <summary>
    /// Number of successful expert assignments used to train the capacity inference.
    /// Higher values indicate more reliable heuristic predictions.
    /// </summary>
    public int ExpertAssignmentCount { get; set; }

    /// <summary>
    /// JSON-serialized compatibility rules defining material exclusions.
    /// Example: {"Rebar": ["CementBags"], "Glass": ["Steel", "Rebar"]}
    /// Key = material that cannot be loaded WITH the list of excluded materials.
    /// </summary>
    public string? CompatibilityRules { get; set; }

    /// <summary>
    /// Historical material combinations that experts have successfully loaded on this truck type.
    /// Used for ML training and capacity inference. JSON array of material group arrays.
    /// </summary>
    public string? ExpertLoadingHistory { get; set; }

    /// <summary>
    /// Timestamp of last capacity recalculation from expert history.
    /// </summary>
    public DateTimeOffset? LastHeuristicUpdate { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Updates the heuristic capacity score based on new expert assignments.
    /// </summary>
    public void UpdateHeuristicCapacity(double newScore)
    {
        HeuristicCapacityScore = newScore;
        ExpertAssignmentCount++;
        LastHeuristicUpdate = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new TruckCapacityUpdatedEvent
        {
            TruckId = Id,
            NewCapacityScore = newScore,
            AssignmentCount = ExpertAssignmentCount
        });
    }
}

/// <summary>
/// Domain event raised when truck capacity is updated.
/// </summary>
public class TruckCapacityUpdatedEvent : BaseEvent
{
    public required Guid TruckId { get; init; }
    public required double NewCapacityScore { get; init; }
    public required int AssignmentCount { get; init; }
}

/// <summary>
/// Type classification for delivery trucks.
/// Serialized as string for ML readability and API consistency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TruckType
{
    /// <summary>
    /// Flatbed truck for long materials and heavy construction items.
    /// </summary>
    Flatbed,

    /// <summary>
    /// Dump truck for bulk materials (sand, gravel, etc.).
    /// </summary>
    Dump,

    /// <summary>
    /// Crane truck for heavy lifting and special handling.
    /// </summary>
    Crane
}