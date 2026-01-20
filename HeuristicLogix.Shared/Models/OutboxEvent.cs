using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Represents an outbox event for the Transactional Outbox Pattern.
/// Ensures reliable event publishing to Kafka with at-least-once delivery semantics.
/// </summary>
public class OutboxEvent
{
    /// <summary>
    /// Unique identifier for the outbox event.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Type of the event (e.g., "expert.decision.created", "heuristic.telemetry.recorded").
    /// </summary>
    public required string EventType { get; set; }

    /// <summary>
    /// Kafka topic to publish this event to.
    /// Examples: "expert.decisions.v1", "heuristic.telemetry.v1"
    /// </summary>
    public required string Topic { get; set; }

    /// <summary>
    /// Aggregate ID that this event relates to (e.g., ConduceId, TruckId).
    /// Used as Kafka partition key for ordering guarantees.
    /// </summary>
    public required string AggregateId { get; set; }

    /// <summary>
    /// JSON payload of the event.
    /// All enums must be serialized as strings per ARCHITECTURE.md standards.
    /// </summary>
    public required string PayloadJson { get; set; }

    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when the event was successfully published to Kafka.
    /// Null if not yet published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Current processing status of the outbox event.
    /// </summary>
    public OutboxEventStatus Status { get; set; } = OutboxEventStatus.Pending;

    /// <summary>
    /// Number of times publishing this event has been attempted.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Error message if publishing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of the last processing attempt.
    /// </summary>
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>
    /// Optional correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional metadata for the event (JSON object).
    /// </summary>
    public string? MetadataJson { get; set; }
}

/// <summary>
/// Status of an outbox event in the publishing workflow.
/// Serialized as string for database and API consistency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OutboxEventStatus
{
    /// <summary>
    /// Event is pending publication to Kafka.
    /// </summary>
    Pending,

    /// <summary>
    /// Event is currently being published.
    /// </summary>
    Processing,

    /// <summary>
    /// Event was successfully published to Kafka.
    /// </summary>
    Published,

    /// <summary>
    /// Event publishing failed after retry attempts.
    /// Requires manual intervention.
    /// </summary>
    Failed,

    /// <summary>
    /// Event was archived after successful publication.
    /// </summary>
    Archived
}
