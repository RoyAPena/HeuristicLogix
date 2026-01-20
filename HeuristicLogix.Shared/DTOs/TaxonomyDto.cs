namespace HeuristicLogix.Shared.DTOs;

/// <summary>
/// Data transfer object for ProductTaxonomy.
/// Used for API communication between Blazor WASM and backend.
/// </summary>
public class TaxonomyDto
{
    /// <summary>
    /// Taxonomy unique identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Raw product description as entered during ingestion.
    /// </summary>
    public required string RawDescription { get; init; }

    /// <summary>
    /// Normalized product name (cleaned and standardized).
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Product category (CEMENT, AGGREGATE, STEEL, REBAR, etc.).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Weight factor for calculating total weight (kg per unit).
    /// </summary>
    public decimal WeightFactor { get; init; }

    /// <summary>
    /// Standard unit of measure (BAG, M3, TON, PIECE, METER).
    /// </summary>
    public string? StandardUnit { get; init; }

    /// <summary>
    /// Whether this taxonomy has been verified by an expert.
    /// </summary>
    public bool IsVerifiedByExpert { get; init; }

    /// <summary>
    /// Number of times this product has been used.
    /// </summary>
    public int UsageCount { get; init; }

    /// <summary>
    /// Additional notes about the product.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// When this taxonomy was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Who verified this taxonomy (if verified).
    /// </summary>
    public string? VerifiedBy { get; init; }

    /// <summary>
    /// When this taxonomy was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; init; }
}

/// <summary>
/// Request to verify a taxonomy entry.
/// </summary>
public class VerifyTaxonomyRequest
{
    /// <summary>
    /// Taxonomy ID to verify.
    /// </summary>
    public required Guid TaxonomyId { get; init; }

    /// <summary>
    /// Updated weight factor (kg per unit).
    /// </summary>
    public required decimal WeightFactor { get; init; }

    /// <summary>
    /// Updated category (if needed).
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Updated standard unit (if needed).
    /// </summary>
    public string? StandardUnit { get; init; }

    /// <summary>
    /// Expert notes about the verification.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Who is verifying this taxonomy.
    /// </summary>
    public required string VerifiedBy { get; init; }
}

/// <summary>
/// Response after verifying a taxonomy.
/// </summary>
public class VerifyTaxonomyResponse
{
    /// <summary>
    /// Whether the verification was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Updated taxonomy DTO.
    /// </summary>
    public TaxonomyDto? Taxonomy { get; init; }

    /// <summary>
    /// Error message if verification failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Query parameters for filtering taxonomies.
/// </summary>
public class TaxonomyQueryParams
{
    /// <summary>
    /// Filter by verification status.
    /// </summary>
    public bool? IsVerified { get; init; }

    /// <summary>
    /// Filter by category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Search term for description.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Sort by field (UsageCount, CreatedAt, Description).
    /// </summary>
    public string? SortBy { get; init; } = "UsageCount";

    /// <summary>
    /// Sort direction (Ascending or Descending).
    /// </summary>
    public bool Descending { get; init; } = true;
}

/// <summary>
/// Summary response for Excel ingestion.
/// </summary>
public class IngestionSummaryDto
{
    /// <summary>
    /// Unique report identifier.
    /// </summary>
    public required Guid ReportId { get; init; }

    /// <summary>
    /// Original file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Total rows processed.
    /// </summary>
    public int TotalRows { get; init; }

    /// <summary>
    /// Successfully processed rows.
    /// </summary>
    public int SuccessfulRows { get; init; }

    /// <summary>
    /// Rows with errors.
    /// </summary>
    public int ErrorRows { get; init; }

    /// <summary>
    /// New product taxonomies created.
    /// </summary>
    public int NewProductsIdentified { get; init; }

    /// <summary>
    /// Processing duration.
    /// </summary>
    public TimeSpan? ProcessingDuration { get; init; }

    /// <summary>
    /// Success rate percentage.
    /// </summary>
    public decimal SuccessRate { get; init; }

    /// <summary>
    /// Whether the ingestion was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// List of errors (if any).
    /// </summary>
    public List<string> Errors { get; init; } = new List<string>();

    /// <summary>
    /// List of warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new List<string>();
}
