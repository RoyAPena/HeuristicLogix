namespace HeuristicLogix.Shared.Services;

/// <summary>
/// Generic Elasticsearch service interface for full-text search.
/// Designed for high-performance search across entities (Items, Suppliers, etc.).
/// </summary>
/// <typeparam name="T">The entity type to search</typeparam>
public interface IElasticSearchService<T> where T : class
{
    /// <summary>
    /// Performs a full-text search across the entity.
    /// </summary>
    /// <param name="query">Search query string</param>
    /// <param name="skip">Number of results to skip (pagination)</param>
    /// <param name="take">Number of results to take (pagination)</param>
    /// <returns>Search results with total count</returns>
    Task<ElasticSearchResult<T>> SearchAsync(string query, int skip = 0, int take = 50);

    /// <summary>
    /// Performs an advanced search with filters.
    /// </summary>
    /// <param name="request">Advanced search request with filters</param>
    /// <returns>Search results with total count</returns>
    Task<ElasticSearchResult<T>> SearchAsync(ElasticSearchRequest request);

    /// <summary>
    /// Indexes a single entity in Elasticsearch.
    /// </summary>
    /// <param name="entity">Entity to index</param>
    Task IndexAsync(T entity);

    /// <summary>
    /// Indexes multiple entities in bulk.
    /// </summary>
    /// <param name="entities">Entities to index</param>
    Task BulkIndexAsync(IEnumerable<T> entities);

    /// <summary>
    /// Removes an entity from the index.
    /// </summary>
    /// <param name="id">Entity ID to remove</param>
    Task DeleteAsync(string id);

    /// <summary>
    /// Re-indexes all entities (full rebuild).
    /// </summary>
    Task ReindexAllAsync();
}

/// <summary>
/// Elasticsearch search result wrapper.
/// </summary>
public class ElasticSearchResult<T> where T : class
{
    public List<T> Results { get; init; } = new();
    public long TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
    public bool HasMore => Skip + Take < TotalCount;
}

/// <summary>
/// Advanced search request with filters.
/// </summary>
public class ElasticSearchRequest
{
    public string Query { get; init; } = string.Empty;
    public Dictionary<string, object> Filters { get; init; } = new();
    public int Skip { get; init; }
    public int Take { get; init; } = 50;
    public List<string> SortBy { get; init; } = new();
    public bool SortDescending { get; init; }
}
