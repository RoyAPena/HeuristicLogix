namespace HeuristicLogix.Shared.Domain;

/// <summary>
/// Base class for all domain events in HeuristicLogix.
/// Domain events represent something that happened in the domain that domain experts care about.
/// </summary>
public abstract class BaseEvent
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional correlation ID for tracking related events across the system.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Optional causation ID linking this event to the command or event that caused it.
    /// </summary>
    public string? CausationId { get; init; }

    /// <summary>
    /// User or system that initiated the event.
    /// </summary>
    public string? InitiatedBy { get; init; }
}
