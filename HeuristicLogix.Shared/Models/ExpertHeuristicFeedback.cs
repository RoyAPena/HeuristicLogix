using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Captures expert feedback when overriding AI-suggested assignments.
/// This data is critical for training and improving the heuristic model.
/// All enum properties are serialized as strings for ML readability.
/// </summary>
public class ExpertHeuristicFeedback
{
    /// <summary>
    /// Unique identifier for this feedback record.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The conduce (delivery order) that was reassigned.
    /// </summary>
    public required Guid ConduceId { get; set; }

    /// <summary>
    /// The truck that AI originally suggested.
    /// </summary>
    public Guid? AISuggestedTruckId { get; set; }

    /// <summary>
    /// The truck that the expert assigned instead.
    /// </summary>
    public required Guid ExpertAssignedTruckId { get; set; }

    /// <summary>
    /// Primary reason for the override.
    /// Serialized as string name (e.g., "WrongTruckType") for ML training.
    /// </summary>
    public required OverrideReasonTag PrimaryReasonTag { get; set; }

    /// <summary>
    /// String representation of the primary reason tag for ML readability.
    /// Computed property that returns the enum name as a string.
    /// </summary>
    [JsonIgnore]
    public string PrimaryReasonTagName => PrimaryReasonTag.ToString();

    /// <summary>
    /// Secondary reason tags (if multiple factors contributed).
    /// Each tag is serialized as string name for ML training.
    /// </summary>
    public List<OverrideReasonTag> SecondaryReasonTags { get; set; } = new List<OverrideReasonTag>();

    /// <summary>
    /// String representations of secondary reason tags for ML readability.
    /// Computed property that returns enum names as strings.
    /// </summary>
    [JsonIgnore]
    public List<string> SecondaryReasonTagNames => SecondaryReasonTags.ConvertAll(tag => tag.ToString());

    /// <summary>
    /// All reason tags combined (primary + secondary) as string names for ML feature extraction.
    /// </summary>
    [JsonPropertyName("allReasonTagsForML")]
    public List<string> AllReasonTagNamesForML
    {
        get
        {
            List<string> allTags = new List<string> { PrimaryReasonTag.ToString() };
            allTags.AddRange(SecondaryReasonTags.ConvertAll(tag => tag.ToString()));
            return allTags;
        }
    }

    /// <summary>
    /// Free-text explanation from the expert.
    /// Valuable for understanding nuances not captured by tags.
    /// </summary>
    public string? ExpertNotes { get; set; }

    /// <summary>
    /// Time taken by the expert to make the decision (in seconds).
    /// Quick decisions may indicate obvious AI errors; longer times suggest complex trade-offs.
    /// </summary>
    public double DecisionTimeSeconds { get; set; }

    /// <summary>
    /// AI confidence score for the original suggestion (0-100).
    /// Helps identify when low-confidence suggestions are overridden.
    /// </summary>
    public double? AIConfidenceScore { get; set; }

    /// <summary>
    /// Snapshot of materials in the conduce at decision time.
    /// JSON-serialized list of material summaries.
    /// </summary>
    public string? MaterialsSnapshot { get; set; }

    /// <summary>
    /// Identifier of the expert who made the decision.
    /// </summary>
    public required string ExpertId { get; set; }

    /// <summary>
    /// Display name of the expert for audit purposes.
    /// </summary>
    public string? ExpertDisplayName { get; set; }

    /// <summary>
    /// Timestamp when the feedback was recorded.
    /// </summary>
    public DateTimeOffset RecordedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Indicates if this feedback has been used for model training.
    /// </summary>
    public bool UsedForTraining { get; set; }

    /// <summary>
    /// Timestamp when this feedback was used for training.
    /// </summary>
    public DateTimeOffset? TrainedAt { get; set; }

    /// <summary>
    /// Session identifier for grouping related decisions.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Indicates if this was a drag-and-drop assignment in the UI.
    /// </summary>
    public bool WasDragDropAssignment { get; set; }

    /// <summary>
    /// Original drop zone identifier (for UI context).
    /// </summary>
    public string? OriginalDropZone { get; set; }

    /// <summary>
    /// New drop zone identifier (for UI context).
    /// </summary>
    public string? NewDropZone { get; set; }

    /// <summary>
    /// Creates a ML-friendly summary of this feedback record.
    /// All enum values are represented as strings.
    /// </summary>
    /// <returns>Dictionary suitable for ML feature extraction.</returns>
    public Dictionary<string, object> ToMLFeatureDictionary()
    {
        Dictionary<string, object> features = new Dictionary<string, object>
        {
            ["feedback_id"] = Id.ToString(),
            ["conduce_id"] = ConduceId.ToString(),
            ["expert_assigned_truck_id"] = ExpertAssignedTruckId.ToString(),
            ["primary_reason"] = PrimaryReasonTag.ToString(),
            ["secondary_reasons"] = SecondaryReasonTagNames,
            ["decision_time_seconds"] = DecisionTimeSeconds,
            ["was_drag_drop"] = WasDragDropAssignment,
            ["recorded_at_utc"] = RecordedAt.UtcDateTime.ToString("O")
        };

        if (AISuggestedTruckId.HasValue)
        {
            features["ai_suggested_truck_id"] = AISuggestedTruckId.Value.ToString();
        }

        if (AIConfidenceScore.HasValue)
        {
            features["ai_confidence_score"] = AIConfidenceScore.Value;
        }

        if (!string.IsNullOrWhiteSpace(ExpertNotes))
        {
            features["expert_notes"] = ExpertNotes;
        }

        return features;
    }
}
