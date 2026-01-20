using System.Text.Json.Serialization;
using HeuristicLogix.Shared.Domain;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// DeliveryRoute aggregate root.
/// Represents a planned delivery route for a truck with optimized stop sequence.
/// </summary>
public class DeliveryRoute : AggregateRoot
{
    /// <summary>
    /// The truck assigned to this route.
    /// </summary>
    public required Guid TruckId { get; set; }

    /// <summary>
    /// Date when this route is scheduled for execution.
    /// </summary>
    public required DateOnly ScheduledDate { get; set; }

    /// <summary>
    /// Ordered list of conduce IDs representing the stop sequence.
    /// </summary>
    public List<Guid> StopSequence { get; set; } = new List<Guid>();

    /// <summary>
    /// Detailed stops with associated materials and timing.
    /// </summary>
    public List<RouteStop> Stops { get; set; } = new List<RouteStop>();

    /// <summary>
    /// Current status of the route.
    /// </summary>
    public RouteStatus Status { get; set; } = RouteStatus.Draft;

    /// <summary>
    /// Total estimated distance in kilometers.
    /// </summary>
    public double EstimatedDistanceKm { get; set; }

    /// <summary>
    /// Total estimated time including travel and service time (in minutes).
    /// </summary>
    public double EstimatedTotalTimeMinutes { get; set; }

    /// <summary>
    /// Heuristic efficiency score (0-100) based on route optimization.
    /// Higher scores indicate better route efficiency.
    /// </summary>
    public double HeuristicEfficiencyScore { get; set; }

    /// <summary>
    /// Indicates if this route was manually overridden by an expert.
    /// </summary>
    public bool WasManuallyOverridden { get; set; }

    /// <summary>
    /// ID of the expert who last modified this route.
    /// </summary>
    public string? LastModifiedByExpertId { get; set; }

    /// <summary>
    /// Total number of stops in the route.
    /// </summary>
    public int TotalStops => Stops.Count;

    /// <summary>
    /// Total weight of all materials in the route (in kg).
    /// </summary>
    public double TotalWeightKg { get; set; }

    /// <summary>
    /// Optimizes the route sequence using AI suggestions.
    /// </summary>
    public void Optimize(List<Guid> newStopSequence, double newEfficiencyScore, string? optimizedBy = null)
    {
        StopSequence = newStopSequence;
        HeuristicEfficiencyScore = newEfficiencyScore;
        Status = RouteStatus.Scheduled;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = optimizedBy;

        RaiseDomainEvent(new RouteOptimizedEvent
        {
            RouteId = Id,
            TruckId = TruckId,
            NewStopSequence = newStopSequence,
            EfficiencyScore = newEfficiencyScore
        });
    }

    /// <summary>
    /// Starts route execution.
    /// </summary>
    public void Start()
    {
        if (Status != RouteStatus.Scheduled)
        {
            throw new InvalidOperationException($"Cannot start route with status {Status}");
        }

        Status = RouteStatus.InProgress;
        LastModifiedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new RouteStartedEvent
        {
            RouteId = Id,
            TruckId = TruckId,
            ScheduledDate = ScheduledDate
        });
    }

    /// <summary>
    /// Completes the route.
    /// </summary>
    public void Complete()
    {
        Status = RouteStatus.Completed;
        LastModifiedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new RouteCompletedEvent
        {
            RouteId = Id,
            TruckId = TruckId,
            TotalStops = TotalStops,
            ActualDistanceKm = EstimatedDistanceKm // TODO: Update with actual
        });
    }
}

/// <summary>
/// Domain event raised when a route is optimized.
/// </summary>
public class RouteOptimizedEvent : BaseEvent
{
    public required Guid RouteId { get; init; }
    public required Guid TruckId { get; init; }
    public required List<Guid> NewStopSequence { get; init; }
    public required double EfficiencyScore { get; init; }
}

/// <summary>
/// Domain event raised when a route starts execution.
/// </summary>
public class RouteStartedEvent : BaseEvent
{
    public required Guid RouteId { get; init; }
    public required Guid TruckId { get; init; }
    public required DateOnly ScheduledDate { get; init; }
}

/// <summary>
/// Domain event raised when a route is completed.
/// </summary>
public class RouteCompletedEvent : BaseEvent
{
    public required Guid RouteId { get; init; }
    public required Guid TruckId { get; init; }
    public required int TotalStops { get; init; }
    public required double ActualDistanceKm { get; init; }
}

/// <summary>
/// Represents a single stop in a delivery route.
/// </summary>
public class RouteStop
{
    /// <summary>
    /// Unique identifier for this stop.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Reference to the conduce (delivery order) for this stop.
    /// </summary>
    public required Guid ConduceId { get; set; }

    /// <summary>
    /// Position in the route sequence (1-based).
    /// </summary>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// Materials to be delivered at this stop.
    /// </summary>
    public List<MaterialItem> Materials { get; set; } = new List<MaterialItem>();

    /// <summary>
    /// Estimated arrival time at this stop.
    /// </summary>
    public TimeOnly? EstimatedArrivalTime { get; set; }

    /// <summary>
    /// Actual arrival time (populated during delivery execution).
    /// </summary>
    public TimeOnly? ActualArrivalTime { get; set; }

    /// <summary>
    /// Estimated service time at this stop (in minutes).
    /// </summary>
    public double EstimatedServiceTimeMinutes { get; set; }

    /// <summary>
    /// Actual service time (populated after delivery completion).
    /// </summary>
    public double? ActualServiceTimeMinutes { get; set; }

    /// <summary>
    /// Distance from previous stop in kilometers.
    /// </summary>
    public double DistanceFromPreviousKm { get; set; }

    /// <summary>
    /// Notes from the delivery driver about this stop.
    /// </summary>
    public string? DeliveryNotes { get; set; }

    /// <summary>
    /// Status of this individual stop.
    /// </summary>
    public StopStatus Status { get; set; } = StopStatus.Pending;
}

/// <summary>
/// Status of a delivery route.
/// Serialized as string for ML readability and API consistency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RouteStatus
{
    /// <summary>
    /// Route is being planned.
    /// </summary>
    Draft,

    /// <summary>
    /// Route has been finalized and scheduled.
    /// </summary>
    Scheduled,

    /// <summary>
    /// Route is currently being executed.
    /// </summary>
    InProgress,

    /// <summary>
    /// Route has been completed.
    /// </summary>
    Completed,

    /// <summary>
    /// Route was cancelled.
    /// </summary>
    Cancelled
}

/// <summary>
/// Status of an individual route stop.
/// Serialized as string for ML readability and API consistency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StopStatus
{
    /// <summary>
    /// Stop is pending delivery.
    /// </summary>
    Pending,

    /// <summary>
    /// Driver is en route to this stop.
    /// </summary>
    EnRoute,

    /// <summary>
    /// Driver has arrived at the stop.
    /// </summary>
    Arrived,

    /// <summary>
    /// Delivery at this stop is complete.
    /// </summary>
    Completed,

    /// <summary>
    /// Stop was skipped (customer not available, etc.).
    /// </summary>
    Skipped
}
