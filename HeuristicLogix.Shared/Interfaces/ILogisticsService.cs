namespace HeuristicLogix.Shared.Interfaces;

using HeuristicLogix.Shared.Models;

/// <summary>
/// Service interface for logistics intelligence operations.
/// Handles heuristic capacity calculations, route optimization, and expert feedback processing.
/// </summary>
public interface ILogisticsService
{
    #region Heuristic Capacity Calculations

    /// <summary>
    /// Calculates the heuristic capacity score for a truck based on historical expert assignments.
    /// </summary>
    /// <param name="truckId">The truck identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The calculated heuristic capacity score (0-100).</returns>
    Task<double> CalculateHeuristicCapacityScoreAsync(Guid truckId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates heuristic capacity scores for all active trucks.
    /// Should be called periodically or after significant expert feedback accumulation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping truck IDs to their updated scores.</returns>
    Task<Dictionary<Guid, double>> RecalculateAllHeuristicScoresAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the estimated remaining capacity for a truck given current assignments.
    /// </summary>
    /// <param name="truckId">The truck identifier.</param>
    /// <param name="scheduledDate">The date to check capacity for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Remaining capacity as a percentage (0-100).</returns>
    Task<double> GetRemainingCapacityAsync(Guid truckId, DateOnly scheduledDate, CancellationToken cancellationToken = default);

    #endregion

    #region Material Compatibility

    /// <summary>
    /// Validates if a set of materials can be loaded together on a specific truck.
    /// </summary>
    /// <param name="truckId">The truck identifier.</param>
    /// <param name="materials">List of materials to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with any compatibility violations.</returns>
    Task<MaterialCompatibilityResult> ValidateMaterialCompatibilityAsync(
        Guid truckId,
        List<MaterialItem> materials,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of materials that are incompatible with a given material on a specific truck.
    /// </summary>
    /// <param name="truckId">The truck identifier.</param>
    /// <param name="materialName">The material name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of incompatible material names.</returns>
    Task<List<string>> GetIncompatibleMaterialsAsync(
        Guid truckId,
        string materialName,
        CancellationToken cancellationToken = default);

    #endregion

    #region Route Planning

    /// <summary>
    /// Suggests the optimal truck for a conduce based on heuristic analysis.
    /// </summary>
    /// <param name="conduceId">The conduce identifier.</param>
    /// <param name="availableTruckIds">List of available truck IDs to consider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suggestion result with confidence score.</returns>
    Task<TruckSuggestionResult> SuggestOptimalTruckAsync(
        Guid conduceId,
        List<Guid> availableTruckIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimizes the stop sequence for a delivery route.
    /// </summary>
    /// <param name="routeId">The route identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Optimized route with updated stop sequence.</returns>
    Task<DeliveryRoute> OptimizeRouteSequenceAsync(Guid routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the heuristic efficiency score for a route.
    /// </summary>
    /// <param name="route">The route to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Efficiency score (0-100).</returns>
    Task<double> CalculateRouteEfficiencyScoreAsync(DeliveryRoute route, CancellationToken cancellationToken = default);

    #endregion

    #region Expert Feedback Processing

    /// <summary>
    /// Records expert feedback when an AI suggestion is overridden.
    /// </summary>
    /// <param name="feedback">The feedback to record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved feedback with any computed fields populated.</returns>
    Task<ExpertHeuristicFeedback> RecordExpertFeedbackAsync(
        ExpertHeuristicFeedback feedback,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unprocessed feedback records for model training.
    /// </summary>
    /// <param name="maxRecords">Maximum number of records to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of feedback records not yet used for training.</returns>
    Task<List<ExpertHeuristicFeedback>> GetUnprocessedFeedbackAsync(
        int maxRecords = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks feedback records as processed for training.
    /// </summary>
    /// <param name="feedbackIds">IDs of feedback records to mark.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkFeedbackAsProcessedAsync(List<Guid> feedbackIds, CancellationToken cancellationToken = default);

    #endregion

    #region Analytics

    /// <summary>
    /// Gets the override rate by reason tag for analytics.
    /// </summary>
    /// <param name="fromDate">Start date for the analysis period.</param>
    /// <param name="toDate">End date for the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping reason tags to their frequency.</returns>
    Task<Dictionary<OverrideReasonTag, int>> GetOverrideAnalyticsByReasonAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the AI suggestion accuracy rate over a time period.
    /// </summary>
    /// <param name="fromDate">Start date for the analysis period.</param>
    /// <param name="toDate">End date for the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Accuracy percentage (0-100).</returns>
    Task<double> GetAISuggestionAccuracyAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Result of material compatibility validation.
/// </summary>
public record MaterialCompatibilityResult(
    bool IsCompatible,
    List<MaterialCompatibilityViolation> Violations);

/// <summary>
/// Details of a material compatibility violation.
/// </summary>
public record MaterialCompatibilityViolation(
    string Material1,
    string Material2,
    string Reason);

/// <summary>
/// Result of truck suggestion analysis.
/// </summary>
public record TruckSuggestionResult(
    Guid SuggestedTruckId,
    double ConfidenceScore,
    string Reasoning,
    List<TruckAlternative> Alternatives);

/// <summary>
/// Alternative truck option with scoring.
/// </summary>
public record TruckAlternative(
    Guid TruckId,
    double Score,
    string Reason);
