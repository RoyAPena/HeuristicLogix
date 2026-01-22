using HeuristicLogix.Shared.Domain;

namespace HeuristicLogix.Modules.Logistics.Services;

/// <summary>
/// Domain event publisher interface for future Kafka integration.
/// Placeholder for event-driven architecture.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a domain event to the event bus (Kafka).
    /// </summary>
    /// <param name="event">Domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if published successfully.</returns>
    Task<bool> PublishAsync(BaseEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple domain events in batch.
    /// </summary>
    /// <param name="events">Collection of domain events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of events published successfully.</returns>
    Task<int> PublishBatchAsync(IEnumerable<BaseEvent> events, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stub implementation of domain event publisher.
/// TODO: Replace with actual Kafka integration using TransactionalOutbox pattern.
/// </summary>
public class StubDomainEventPublisher : IDomainEventPublisher
{
    public Task<bool> PublishAsync(BaseEvent @event, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Kafka publishing via TransactionalOutbox
        // For now, events are just logged (they're already in the Outbox via AggregateRoot)
        return Task.FromResult(true);
    }

    public Task<int> PublishBatchAsync(IEnumerable<BaseEvent> events, CancellationToken cancellationToken = default)
    {
        // TODO: Implement batch Kafka publishing
        return Task.FromResult(events.Count());
    }
}
