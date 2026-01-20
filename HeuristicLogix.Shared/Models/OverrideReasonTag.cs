using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Tags categorizing the reason an expert overrode an AI-suggested assignment.
/// Used for training the heuristic model to improve future predictions.
/// Serialized as string names for ML readability and log analysis.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OverrideReasonTag
{
    /// <summary>
    /// No specific reason provided.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// AI suggested wrong truck type for the materials.
    /// </summary>
    WrongTruckType = 1,

    /// <summary>
    /// Truck capacity was overestimated by the AI.
    /// </summary>
    CapacityOverestimated = 2,

    /// <summary>
    /// Truck capacity was underestimated by the AI.
    /// </summary>
    CapacityUnderestimated = 3,

    /// <summary>
    /// Material compatibility rules were violated.
    /// </summary>
    MaterialIncompatibility = 4,

    /// <summary>
    /// Route optimization was suboptimal.
    /// </summary>
    BetterRouteAvailable = 5,

    /// <summary>
    /// Customer-specific preference or requirement.
    /// </summary>
    CustomerPreference = 6,

    /// <summary>
    /// Driver availability or scheduling conflict.
    /// </summary>
    DriverAvailability = 7,

    /// <summary>
    /// Truck maintenance or mechanical issue.
    /// </summary>
    TruckMaintenance = 8,

    /// <summary>
    /// Geographic or road access constraint.
    /// </summary>
    AccessConstraint = 9,

    /// <summary>
    /// Time window constraint for delivery.
    /// </summary>
    TimeWindowConstraint = 10,

    /// <summary>
    /// Special handling requirement not recognized by AI.
    /// </summary>
    SpecialHandlingRequired = 11,

    /// <summary>
    /// Weather-related delivery adjustment.
    /// </summary>
    WeatherRelated = 12,

    /// <summary>
    /// Expert intuition based on experience (catch-all for learning).
    /// </summary>
    ExpertIntuition = 99
}
