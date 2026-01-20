using System.Text.Json.Serialization;
using HeuristicLogix.Shared.Domain;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Conduce (Delivery Order) aggregate root.
/// Represents a delivery order in the logistics workflow.
/// </summary>
public class Conduce : AggregateRoot
{
    public required string ClientName { get; set; }
    public required string RawAddress { get; set; }

    // Obligatorios para Google Maps
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public ConduceStatus Status { get; set; } = ConduceStatus.Pending;

    // --- CAPTURA DE DATOS PARA IA ---
    // Lo que la IA estimó vs lo que el experto (tu padre) decidió
    public double? AIPredictedServiceTime { get; set; }
    public double? ActualServiceTime { get; set; }
    public string? ExpertDecisionNote { get; set; }

    // Navigation properties
    public Guid? AssignedTruckId { get; set; }
    public Guid? RouteId { get; set; }

    /// <summary>
    /// Creates a new Conduce and raises ConduceCreated event.
    /// </summary>
    public static Conduce Create(
        string clientName,
        string rawAddress,
        double latitude,
        double longitude,
        string? createdBy = null)
    {
        Conduce conduce = new Conduce
        {
            Id = Guid.NewGuid(),
            ClientName = clientName,
            RawAddress = rawAddress,
            Latitude = latitude,
            Longitude = longitude,
            Status = ConduceStatus.Pending,
            CreatedBy = createdBy
        };

        conduce.RaiseDomainEvent(new ConduceCreatedEvent
        {
            ConduceId = conduce.Id,
            ClientName = clientName,
            Address = rawAddress,
            Latitude = latitude,
            Longitude = longitude
        });

        return conduce;
    }

    /// <summary>
    /// Assigns a truck to this conduce.
    /// </summary>
    public void AssignTruck(Guid truckId, string? assignedBy = null)
    {
        AssignedTruckId = truckId;
        Status = ConduceStatus.Scheduled;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = assignedBy;

        RaiseDomainEvent(new TruckAssignedEvent
        {
            ConduceId = Id,
            TruckId = truckId,
            AssignedBy = assignedBy
        });
    }

    /// <summary>
    /// Finalizes the conduce after delivery completion.
    /// </summary>
    public void Finalize(double actualServiceTime, string? note = null)
    {
        ActualServiceTime = actualServiceTime;
        ExpertDecisionNote = note;
        Status = ConduceStatus.Completed;
        LastModifiedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ConduceFinalizedEvent
        {
            ConduceId = Id,
            ActualServiceTime = actualServiceTime,
            Note = note
        });
    }
}

/// <summary>
/// Domain event raised when a new Conduce is created.
/// </summary>
public class ConduceCreatedEvent : BaseEvent
{
    public required Guid ConduceId { get; init; }
    public required string ClientName { get; init; }
    public required string Address { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}

/// <summary>
/// Domain event raised when a Truck is assigned to a Conduce.
/// </summary>
public class TruckAssignedEvent : BaseEvent
{
    public required Guid ConduceId { get; init; }
    public required Guid TruckId { get; init; }
    public string? AssignedBy { get; init; }
}

/// <summary>
/// Domain event raised when a Conduce is finalized after delivery.
/// </summary>
public class ConduceFinalizedEvent : BaseEvent
{
    public required Guid ConduceId { get; init; }
    public required double ActualServiceTime { get; init; }
    public string? Note { get; init; }
}

/// <summary>
/// Status of a conduce (delivery order) in the logistics workflow.
/// Serialized as string for ML readability and API consistency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConduceStatus
{
    /// <summary>
    /// Order is pending assignment to a truck.
    /// </summary>
    Pending,

    /// <summary>
    /// Order has been scheduled for delivery.
    /// </summary>
    Scheduled,

    /// <summary>
    /// Order is currently out for delivery.
    /// </summary>
    OutForDelivery,

    /// <summary>
    /// Order has been successfully delivered.
    /// </summary>
    Completed,

    /// <summary>
    /// Order was canceled.
    /// </summary>
    Canceled
}