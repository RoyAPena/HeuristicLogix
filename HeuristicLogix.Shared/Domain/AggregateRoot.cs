namespace HeuristicLogix.Shared.Domain;

/// <summary>
/// Base class for aggregate roots in HeuristicLogix.
/// Aggregates are the unit of consistency and transaction boundaries.
/// They collect domain events that represent state changes.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<BaseEvent> _domainEvents = new List<BaseEvent>();

    /// <summary>
    /// Read-only collection of domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyList<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event to be published after the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events. Should be called after events are published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Timestamp when the aggregate was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when the aggregate was last modified.
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; set; }

    /// <summary>
    /// User or system that created this aggregate.
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// User or system that last modified this aggregate.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}
