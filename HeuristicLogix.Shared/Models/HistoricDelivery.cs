using HeuristicLogix.Shared.Domain;

namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Represents a unit-aware historic delivery record from data ingestion.
/// Used for building the initial AI knowledge base with product taxonomy support.
/// </summary>
public class HistoricDeliveryRecord
{
    /// <summary>
    /// When the delivery occurred.
    /// </summary>
    public required DateTime DeliveryDate { get; init; }

    /// <summary>
    /// Client name (sanitized: trimmed and uppercase).
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// Raw product description as entered in Excel/CSV.
    /// Will be used for taxonomy lookup or AI classification.
    /// Example: "Cemento Portland", "Agregado Arena", "Cabilla 12mm"
    /// </summary>
    public required string RawDescription { get; init; }

    /// <summary>
    /// Quantity of the product delivered.
    /// Parsed from Excel using decimal.TryParse.
    /// </summary>
    public decimal? Quantity { get; init; }

    /// <summary>
    /// Raw unit of measure as entered in Excel/CSV.
    /// Examples: "Bolsa", "M3", "Ton", "Piezas", "Metros"
    /// If not provided, will be null and full description passed to AI.
    /// </summary>
    public string? RawUnit { get; init; }

    /// <summary>
    /// Full delivery address.
    /// </summary>
    public required string DeliveryAddress { get; init; }

    /// <summary>
    /// GPS latitude (auto-geocoded if not provided).
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// GPS longitude (auto-geocoded if not provided).
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// Truck license plate that handled the delivery (sanitized: trimmed and uppercase).
    /// </summary>
    public required string TruckLicensePlate { get; init; }

    /// <summary>
    /// Total weight of cargo in kilograms (if provided directly).
    /// May be calculated from Quantity * WeightFactor if taxonomy exists.
    /// </summary>
    public decimal? TotalWeightKg { get; init; }

    /// <summary>
    /// Actual time spent on delivery in minutes.
    /// </summary>
    public required decimal ServiceTimeMinutes { get; init; }

    /// <summary>
    /// Expert observations about the delivery.
    /// </summary>
    public string? ExpertNotes { get; init; }

    /// <summary>
    /// Reason why AI suggestion was overridden (if applicable).
    /// </summary>
    public string? OverrideReason { get; init; }

    /// <summary>
    /// Row number in the source file (for error reporting).
    /// </summary>
    public int RowNumber { get; init; }
}

/// <summary>
/// Domain event raised when a unit-aware historic delivery is ingested.
/// Includes taxonomy-based weight calculation if available.
/// </summary>
public class HistoricDeliveryIngestedEvent : BaseEvent
{
    /// <summary>
    /// When the delivery occurred (historic).
    /// </summary>
    public required DateTime DeliveryDate { get; init; }

    /// <summary>
    /// Client name (sanitized).
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// Raw product description for AI classification.
    /// </summary>
    public required string RawDescription { get; init; }

    /// <summary>
    /// Quantity delivered (if parsed successfully).
    /// </summary>
    public decimal? Quantity { get; init; }

    /// <summary>
    /// Raw unit of measure (if provided).
    /// </summary>
    public string? RawUnit { get; init; }

    /// <summary>
    /// Calculated weight based on taxonomy (Quantity * WeightFactor).
    /// Null if taxonomy doesn't exist - AI will estimate.
    /// </summary>
    public decimal? CalculatedWeight { get; init; }

    /// <summary>
    /// Total weight in kilograms (either calculated or provided directly).
    /// </summary>
    public decimal? TotalWeightKg { get; init; }

    /// <summary>
    /// Whether weight was calculated from taxonomy (true) or provided directly (false).
    /// </summary>
    public bool IsWeightCalculated { get; init; }

    /// <summary>
    /// Product taxonomy ID if found (for linking).
    /// </summary>
    public Guid? TaxonomyId { get; init; }

    /// <summary>
    /// Whether the product taxonomy is verified by expert.
    /// </summary>
    public bool IsTaxonomyVerified { get; init; }

    /// <summary>
    /// Full delivery address.
    /// </summary>
    public required string DeliveryAddress { get; init; }

    /// <summary>
    /// GPS latitude.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// GPS longitude.
    /// </summary>
    public required double Longitude { get; init; }

    /// <summary>
    /// Truck license plate (sanitized).
    /// </summary>
    public required string TruckLicensePlate { get; init; }

    /// <summary>
    /// Service time in minutes.
    /// </summary>
    public required decimal ServiceTimeMinutes { get; init; }

    /// <summary>
    /// Expert observations.
    /// </summary>
    public string? ExpertNotes { get; init; }

    /// <summary>
    /// Override reason.
    /// </summary>
    public string? OverrideReason { get; init; }

    /// <summary>
    /// Marks this as a historic event (not real-time).
    /// </summary>
    public bool IsHistoric { get; init; } = true;

    /// <summary>
    /// Batch identifier for grouping related ingestion records.
    /// </summary>
    public required string IngestionBatchId { get; init; }
}

/// <summary>
/// Result of a data ingestion operation.
/// </summary>
public class DataIngestionResult
{
    /// <summary>
    /// Unique identifier for this ingestion batch.
    /// </summary>
    public required string BatchId { get; init; }

    /// <summary>
    /// Original file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Total number of records in the file.
    /// </summary>
    public int TotalRecords { get; init; }

    /// <summary>
    /// Number of records successfully processed.
    /// </summary>
    public int ProcessedRecords { get; init; }

    /// <summary>
    /// Number of records skipped due to errors.
    /// </summary>
    public int SkippedRecords { get; init; }

    /// <summary>
    /// Number of duplicate records detected.
    /// </summary>
    public int DuplicateRecords { get; init; }

    /// <summary>
    /// List of validation errors encountered.
    /// </summary>
    public List<DataIngestionError> Errors { get; init; } = new List<DataIngestionError>();

    /// <summary>
    /// When the ingestion started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the ingestion completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Total processing time.
    /// </summary>
    public TimeSpan? ProcessingTime => CompletedAt - StartedAt;

    /// <summary>
    /// Whether the ingestion was successful.
    /// </summary>
    public bool IsSuccess => SkippedRecords == 0 && Errors.Count == 0;
}

/// <summary>
/// Represents an error encountered during data ingestion.
/// </summary>
public class DataIngestionError
{
    /// <summary>
    /// Row number where the error occurred.
    /// </summary>
    public int RowNumber { get; init; }

    /// <summary>
    /// Field name that caused the error.
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Severity level (Error, Warning).
    /// </summary>
    public string Severity { get; init; } = "Error";
}
