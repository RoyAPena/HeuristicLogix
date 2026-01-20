using HeuristicLogix.Shared.Models;
using HeuristicLogix.Shared.Serialization;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Api.Services;

/// <summary>
/// Service for managing the Transactional Outbox Pattern.
/// Ensures reliable event publishing to Kafka with at-least-once delivery semantics.
/// </summary>
public interface ITransactionalOutboxService
{
    /// <summary>
    /// Adds an event to the outbox within the current database transaction.
    /// </summary>
    Task<OutboxEvent> AddEventAsync<TPayload>(
        string eventType,
        string topic,
        string aggregateId,
        TPayload payload,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending outbox events ready for publishing.
    /// </summary>
    Task<List<OutboxEvent>> GetPendingEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as published after successful Kafka delivery.
    /// </summary>
    Task MarkAsPublishedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as failed after retry attempts.
    /// </summary>
    Task MarkAsFailedAsync(
        Guid eventId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the attempt count for an event.
    /// </summary>
    Task IncrementAttemptAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of the Transactional Outbox Service with instant notification.
/// </summary>
public class TransactionalOutboxService : ITransactionalOutboxService
{
    private readonly HeuristicLogixDbContext _dbContext;
    private readonly IOutboxEventNotifier _notifier;

    public TransactionalOutboxService(
        HeuristicLogixDbContext dbContext,
        IOutboxEventNotifier notifier)
    {
        _dbContext = dbContext;
        _notifier = notifier;
    }

    public async Task<OutboxEvent> AddEventAsync<TPayload>(
        string eventType,
        string topic,
        string aggregateId,
        TPayload payload,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        // Serialize payload using HeuristicJsonOptions (ensures string-based enums)
        string payloadJson = HeuristicJsonOptions.Serialize(payload);

        OutboxEvent outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Topic = topic,
            AggregateId = aggregateId,
            PayloadJson = payloadJson,
            CorrelationId = correlationId,
            Status = OutboxEventStatus.Pending,
            AttemptCount = 0
        };

        _dbContext.OutboxEvents.Add(outboxEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // INSTANT NOTIFICATION: Signal background publisher that a new event is ready
        // This eliminates the polling delay - publisher will process immediately
        await _notifier.NotifyEventAddedAsync(cancellationToken);

        return outboxEvent;
    }

    public async Task<List<OutboxEvent>> GetPendingEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsPublishedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        OutboxEvent? outboxEvent = await _dbContext.OutboxEvents
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (outboxEvent != null)
        {
            outboxEvent.Status = OutboxEventStatus.Published;
            outboxEvent.PublishedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(
        Guid eventId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        OutboxEvent? outboxEvent = await _dbContext.OutboxEvents
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (outboxEvent != null)
        {
            outboxEvent.Status = OutboxEventStatus.Failed;
            outboxEvent.ErrorMessage = errorMessage;
            outboxEvent.LastAttemptAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task IncrementAttemptAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        OutboxEvent? outboxEvent = await _dbContext.OutboxEvents
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (outboxEvent != null)
        {
            outboxEvent.AttemptCount++;
            outboxEvent.LastAttemptAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
