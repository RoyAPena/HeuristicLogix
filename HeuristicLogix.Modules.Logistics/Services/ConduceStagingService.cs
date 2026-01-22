using HeuristicLogix.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Logistics.Services;

/// <summary>
/// Represents a staged conduce awaiting review and approval.
/// Contains parsed data with validation status before database persistence.
/// </summary>
public class StagedConduce
{
    /// <summary>
    /// Temporary staging identifier.
    /// </summary>
    public required Guid StagingId { get; init; }

    /// <summary>
    /// Client name.
    /// </summary>
    public required string ClientName { get; set; }

    /// <summary>
    /// Delivery address (raw).
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Geocoding result.
    /// </summary>
    public GeocodingResult? GeocodingResult { get; set; }

    /// <summary>
    /// Invoice/Conduce number.
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Scheduled delivery date.
    /// </summary>
    public DateTimeOffset? DeliveryDate { get; set; }

    /// <summary>
    /// Collection of line items.
    /// </summary>
    public List<StagedConduceItem> Items { get; set; } = new List<StagedConduceItem>();

    /// <summary>
    /// Validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Validation warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = new List<string>();

    /// <summary>
    /// Whether this staged conduce is valid for persistence.
    /// </summary>
    public bool IsValid => Errors.Count == 0 && Items.Any(i => i.IsValid);

    /// <summary>
    /// Source of data (Excel, Manual, API).
    /// </summary>
    public required string DataSource { get; init; }

    /// <summary>
    /// When staged.
    /// </summary>
    public DateTimeOffset StagedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Who staged this conduce.
    /// </summary>
    public string? StagedBy { get; set; }
}

/// <summary>
/// Represents a staged conduce item awaiting review.
/// </summary>
public class StagedConduceItem
{
    /// <summary>
    /// Line number in source.
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// Material name/description.
    /// </summary>
    public required string MaterialName { get; set; }

    /// <summary>
    /// Quantity.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of measure.
    /// </summary>
    public required string Unit { get; set; }

    /// <summary>
    /// Weight (kg), if provided or calculated.
    /// </summary>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Auto-classified material type.
    /// </summary>
    public MaterialType MaterialType { get; set; }

    /// <summary>
    /// Matched product taxonomy (if found).
    /// </summary>
    public ProductTaxonomy? MatchedTaxonomy { get; set; }

    /// <summary>
    /// Item-specific errors.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Item-specific warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = new List<string>();

    /// <summary>
    /// Whether this item is valid.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}

/// <summary>
/// Service for staging conduces before persistence.
/// Provides preview and validation functionality.
/// </summary>
public interface IConduceStagingService
{
    /// <summary>
    /// Stages a conduce for review.
    /// </summary>
    /// <param name="conduce">Staged conduce to add.</param>
    void StageConduce(StagedConduce conduce);

    /// <summary>
    /// Gets all staged conduces for review.
    /// </summary>
    /// <returns>Collection of staged conduces.</returns>
    List<StagedConduce> GetStagedConduces();

    /// <summary>
    /// Gets a specific staged conduce.
    /// </summary>
    /// <param name="stagingId">Staging identifier.</param>
    /// <returns>Staged conduce if found.</returns>
    StagedConduce? GetStagedConduce(Guid stagingId);

    /// <summary>
    /// Removes a staged conduce (after approval or rejection).
    /// </summary>
    /// <param name="stagingId">Staging identifier.</param>
    /// <returns>True if removed.</returns>
    bool RemoveStagedConduce(Guid stagingId);

    /// <summary>
    /// Clears all staged conduces.
    /// </summary>
    void ClearStaging();

    /// <summary>
    /// Gets summary statistics of staged conduces.
    /// </summary>
    /// <returns>Staging statistics.</returns>
    StagingStatistics GetStatistics();
}

/// <summary>
/// Statistics about staged conduces.
/// </summary>
public class StagingStatistics
{
    public int TotalStaged { get; init; }
    public int ValidConduces { get; init; }
    public int InvalidConduces { get; init; }
    public int TotalItems { get; init; }
    public int ItemsWithErrors { get; init; }
    public Dictionary<string, int> DataSourceCounts { get; init; } = new Dictionary<string, int>();
}

/// <summary>
/// In-memory implementation of conduce staging service.
/// Thread-safe for concurrent access.
/// </summary>
public class InMemoryConduceStagingService : IConduceStagingService
{
    private readonly Dictionary<Guid, StagedConduce> _stagedConduces = new Dictionary<Guid, StagedConduce>();
    private readonly object _lock = new object();
    private readonly ILogger<InMemoryConduceStagingService> _logger;

    public InMemoryConduceStagingService(ILogger<InMemoryConduceStagingService> logger)
    {
        _logger = logger;
    }

    public void StageConduce(StagedConduce conduce)
    {
        lock (_lock)
        {
            _stagedConduces[conduce.StagingId] = conduce;
            _logger.LogInformation(
                "Staged conduce {StagingId} for client {ClientName} ({ItemCount} items, {DataSource})",
                conduce.StagingId, conduce.ClientName, conduce.Items.Count, conduce.DataSource);
        }
    }

    public List<StagedConduce> GetStagedConduces()
    {
        lock (_lock)
        {
            return _stagedConduces.Values.ToList();
        }
    }

    public StagedConduce? GetStagedConduce(Guid stagingId)
    {
        lock (_lock)
        {
            _stagedConduces.TryGetValue(stagingId, out StagedConduce? conduce);
            return conduce;
        }
    }

    public bool RemoveStagedConduce(Guid stagingId)
    {
        lock (_lock)
        {
            bool removed = _stagedConduces.Remove(stagingId);
            if (removed)
            {
                _logger.LogInformation("Removed staged conduce {StagingId}", stagingId);
            }
            return removed;
        }
    }

    public void ClearStaging()
    {
        lock (_lock)
        {
            int count = _stagedConduces.Count;
            _stagedConduces.Clear();
            _logger.LogInformation("Cleared {Count} staged conduces", count);
        }
    }

    public StagingStatistics GetStatistics()
    {
        lock (_lock)
        {
            List<StagedConduce> conduces = _stagedConduces.Values.ToList();

            return new StagingStatistics
            {
                TotalStaged = conduces.Count,
                ValidConduces = conduces.Count(c => c.IsValid),
                InvalidConduces = conduces.Count(c => !c.IsValid),
                TotalItems = conduces.Sum(c => c.Items.Count),
                ItemsWithErrors = conduces.Sum(c => c.Items.Count(i => !i.IsValid)),
                DataSourceCounts = conduces
                    .GroupBy(c => c.DataSource)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
    }
}
