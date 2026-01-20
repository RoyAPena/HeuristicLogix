namespace HeuristicLogix.Shared.Models;

/// <summary>
/// Report generated during Excel ingestion process.
/// Tracks processing results, errors, and warnings.
/// </summary>
public class ProcessingReport
{
    /// <summary>
    /// Unique identifier for this processing report.
    /// </summary>
    public Guid ReportId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Original file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// When processing started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When processing completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Total processing time.
    /// </summary>
    public TimeSpan? ProcessingDuration => CompletedAt - StartedAt;

    /// <summary>
    /// Total number of rows in the file (excluding header).
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of rows successfully processed.
    /// </summary>
    public int SuccessfulRows { get; set; }

    /// <summary>
    /// Number of rows skipped due to errors.
    /// </summary>
    public int ErrorRows { get; set; }

    /// <summary>
    /// Number of empty or null rows skipped.
    /// </summary>
    public int EmptyRows { get; set; }

    /// <summary>
    /// Number of new product taxonomies created (pending verification).
    /// </summary>
    public int NewTaxonomiesCreated { get; set; }

    /// <summary>
    /// Collection of errors encountered during processing.
    /// </summary>
    public List<RowProcessingError> Errors { get; init; } = new List<RowProcessingError>();

    /// <summary>
    /// Collection of warnings (non-fatal issues).
    /// </summary>
    public List<RowProcessingWarning> Warnings { get; init; } = new List<RowProcessingWarning>();

    /// <summary>
    /// Whether the processing was successful (no errors).
    /// </summary>
    public bool IsSuccess => ErrorRows == 0 && Errors.Count == 0;

    /// <summary>
    /// Success rate percentage.
    /// </summary>
    public decimal SuccessRate => TotalRows > 0 ? (decimal)SuccessfulRows / TotalRows * 100 : 0;
}

/// <summary>
/// Represents an error that occurred while processing a row.
/// </summary>
public class RowProcessingError
{
    /// <summary>
    /// Row number (1-based, excluding header).
    /// </summary>
    public int RowNumber { get; init; }

    /// <summary>
    /// Field name that caused the error (if applicable).
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Raw row data (if available).
    /// </summary>
    public string? RawData { get; init; }

    /// <summary>
    /// When the error occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents a non-fatal warning during row processing.
/// </summary>
public class RowProcessingWarning
{
    /// <summary>
    /// Row number (1-based, excluding header).
    /// </summary>
    public int RowNumber { get; init; }

    /// <summary>
    /// Warning message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Severity level.
    /// </summary>
    public WarningSeverity Severity { get; init; } = WarningSeverity.Low;
}

/// <summary>
/// Warning severity levels.
/// </summary>
public enum WarningSeverity
{
    Low,
    Medium,
    High
}
