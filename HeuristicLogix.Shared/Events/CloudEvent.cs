using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Events;

/// <summary>
/// CloudEvents specification wrapper for Kafka events.
/// Provides standard metadata for event-driven communication across modules and to AI services.
/// 
/// CloudEvents spec: https://cloudevents.io/
/// </summary>
/// <typeparam name="TData">The type of event data payload.</typeparam>
public class CloudEvent<TData> where TData : class
{
    /// <summary>
    /// Unique identifier for the event.
    /// CloudEvents spec: 'id' (required)
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Module that produced the event (e.g., "Logistics", "Finance", "Inventory").
    /// CloudEvents spec: 'source' (required)
    /// </summary>
    [JsonPropertyName("source")]
    public required string SourceModule { get; init; }

    /// <summary>
    /// Type of event (e.g., "ConduceCreated", "CreditCheckRequested").
    /// CloudEvents spec: 'type' (required)
    /// </summary>
    [JsonPropertyName("type")]
    public required string EventType { get; init; }

    /// <summary>
    /// The actual event data payload.
    /// CloudEvents spec: 'data' (optional)
    /// </summary>
    [JsonPropertyName("data")]
    public required TData Data { get; init; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// CloudEvents spec: 'time' (optional)
    /// </summary>
    [JsonPropertyName("time")]
    public required DateTimeOffset EventTimestamp { get; init; }

    /// <summary>
    /// Correlation ID for tracking related events across the system.
    /// Used for distributed tracing and causality tracking.
    /// </summary>
    [JsonPropertyName("correlationid")]
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Version of the CloudEvents specification being used.
    /// CloudEvents spec: 'specversion' (required)
    /// </summary>
    [JsonPropertyName("specversion")]
    public string SpecVersion { get; init; } = "1.0";

    /// <summary>
    /// Subject of the event (e.g., specific aggregate ID).
    /// CloudEvents spec: 'subject' (optional)
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; init; }

    /// <summary>
    /// Content type of the data payload.
    /// CloudEvents spec: 'datacontenttype' (optional)
    /// </summary>
    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; init; } = "application/json";

    /// <summary>
    /// Schema URI for the data payload structure.
    /// CloudEvents spec: 'dataschema' (optional)
    /// </summary>
    [JsonPropertyName("dataschema")]
    public string? DataSchema { get; init; }

    /// <summary>
    /// Extension attributes for custom metadata.
    /// Used for module-specific or AI-specific metadata.
    /// </summary>
    [JsonPropertyName("extensions")]
    public Dictionary<string, string> Extensions { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// User or system that initiated the event.
    /// Extension attribute.
    /// </summary>
    [JsonIgnore]
    public string? InitiatedBy
    {
        get => Extensions.TryGetValue("initiatedby", out string? value) ? value : null;
        init => Extensions["initiatedby"] = value ?? string.Empty;
    }

    /// <summary>
    /// Causation ID linking this event to the command or event that caused it.
    /// Extension attribute for event sourcing.
    /// </summary>
    [JsonIgnore]
    public string? CausationId
    {
        get => Extensions.TryGetValue("causationid", out string? value) ? value : null;
        init => Extensions["causationid"] = value ?? string.Empty;
    }

    /// <summary>
    /// AI tier for intelligence processing (1=Real-time, 2=Gemini, 3=GPT-5.2).
    /// Extension attribute for SPEC_INTELLIGENCE_HYBRID.md
    /// </summary>
    [JsonIgnore]
    public int? AITier
    {
        get => Extensions.TryGetValue("aitier", out string? value) && int.TryParse(value, out int tier) ? tier : null;
        init => Extensions["aitier"] = value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Priority level for event processing (1=High, 2=Normal, 3=Low).
    /// Extension attribute.
    /// </summary>
    [JsonIgnore]
    public int Priority
    {
        get => Extensions.TryGetValue("priority", out string? value) && int.TryParse(value, out int priority) ? priority : 2;
        init => Extensions["priority"] = value.ToString();
    }
}

/// <summary>
/// Factory for creating CloudEvents with consistent defaults.
/// </summary>
public static class CloudEventFactory
{
    /// <summary>
    /// Creates a new CloudEvent with standard metadata.
    /// </summary>
    /// <typeparam name="TData">The type of event data.</typeparam>
    /// <param name="sourceModule">Module that produces the event.</param>
    /// <param name="eventType">Type of event.</param>
    /// <param name="data">Event data payload.</param>
    /// <param name="correlationId">Optional correlation ID (generated if null).</param>
    /// <param name="subject">Optional subject (e.g., aggregate ID).</param>
    /// <param name="initiatedBy">Optional user/system that initiated the event.</param>
    /// <param name="causationId">Optional causation ID.</param>
    /// <param name="aiTier">Optional AI tier (1, 2, or 3).</param>
    /// <returns>Configured CloudEvent.</returns>
    public static CloudEvent<TData> Create<TData>(
        string sourceModule,
        string eventType,
        TData data,
        string? correlationId = null,
        string? subject = null,
        string? initiatedBy = null,
        string? causationId = null,
        int? aiTier = null) where TData : class
    {
        return new CloudEvent<TData>
        {
            Id = Guid.NewGuid(),
            SourceModule = sourceModule,
            EventType = eventType,
            Data = data,
            EventTimestamp = DateTimeOffset.UtcNow,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            Subject = subject,
            InitiatedBy = initiatedBy,
            CausationId = causationId,
            AITier = aiTier
        };
    }

    /// <summary>
    /// Creates a CloudEvent from a domain event.
    /// </summary>
    /// <typeparam name="TData">The type of domain event.</typeparam>
    /// <param name="sourceModule">Module that produces the event.</param>
    /// <param name="domainEvent">The domain event to wrap.</param>
    /// <param name="aiTier">Optional AI tier.</param>
    /// <returns>CloudEvent wrapping the domain event.</returns>
    public static CloudEvent<TData> FromDomainEvent<TData>(
        string sourceModule,
        TData domainEvent,
        int? aiTier = null) where TData : class
    {
        // Extract type name without namespace
        string eventType = typeof(TData).Name;

        return new CloudEvent<TData>
        {
            Id = Guid.NewGuid(),
            SourceModule = sourceModule,
            EventType = eventType,
            Data = domainEvent,
            EventTimestamp = DateTimeOffset.UtcNow,
            CorrelationId = Guid.NewGuid().ToString(),
            AITier = aiTier
        };
    }
}
