using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Models;

public class Conduce
{
    public required Guid Id { get; init; }
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

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
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