using System.Net.Http.Json;
using HeuristicLogix.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Client.Services;

/// <summary>
/// Service for managing product taxonomies via API.
/// Uses explicit typing (no var).
/// </summary>
public interface ITaxonomyService
{
    Task<List<TaxonomyDto>> GetTaxonomiesAsync(
        bool? isVerified = null,
        string? category = null,
        string? searchTerm = null,
        string sortBy = "UsageCount",
        bool descending = true);

    Task<TaxonomyDto?> GetTaxonomyAsync(Guid id);

    Task<VerifyTaxonomyResponse> VerifyTaxonomyAsync(VerifyTaxonomyRequest request);

    Task<TaxonomyStatsDto> GetStatsAsync();
}

/// <summary>
/// Implementation of taxonomy service with HTTP client.
/// </summary>
public class TaxonomyService : ITaxonomyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TaxonomyService> _logger;

    public TaxonomyService(HttpClient httpClient, ILogger<TaxonomyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<TaxonomyDto>> GetTaxonomiesAsync(
        bool? isVerified = null,
        string? category = null,
        string? searchTerm = null,
        string sortBy = "UsageCount",
        bool descending = true)
    {
        try
        {
            string url = BuildQueryUrl(isVerified, category, searchTerm, sortBy, descending);
            
            List<TaxonomyDto>? taxonomies = await _httpClient.GetFromJsonAsync<List<TaxonomyDto>>(url);
            
            return taxonomies ?? new List<TaxonomyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving taxonomies");
            return new List<TaxonomyDto>();
        }
    }

    public async Task<TaxonomyDto?> GetTaxonomyAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TaxonomyDto>($"api/taxonomy/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving taxonomy {Id}", id);
            return null;
        }
    }

    public async Task<VerifyTaxonomyResponse> VerifyTaxonomyAsync(VerifyTaxonomyRequest request)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/taxonomy/verify", request);
            
            if (response.IsSuccessStatusCode)
            {
                VerifyTaxonomyResponse? result = await response.Content.ReadFromJsonAsync<VerifyTaxonomyResponse>();
                return result ?? new VerifyTaxonomyResponse { Success = false, ErrorMessage = "No response" };
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Verification failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return new VerifyTaxonomyResponse { Success = false, ErrorMessage = $"Error {response.StatusCode}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying taxonomy");
            return new VerifyTaxonomyResponse { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<TaxonomyStatsDto> GetStatsAsync()
    {
        try
        {
            TaxonomyStatsDto? stats = await _httpClient.GetFromJsonAsync<TaxonomyStatsDto>("api/taxonomy/stats");
            return stats ?? new TaxonomyStatsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving taxonomy stats");
            return new TaxonomyStatsDto();
        }
    }

    private string BuildQueryUrl(
        bool? isVerified,
        string? category,
        string? searchTerm,
        string sortBy,
        bool descending)
    {
        List<string> queryParams = new List<string>();

        if (isVerified.HasValue)
        {
            queryParams.Add($"isVerified={isVerified.Value.ToString().ToLower()}");
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            queryParams.Add($"category={Uri.EscapeDataString(category)}");
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
        }

        queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        queryParams.Add($"descending={descending.ToString().ToLower()}");

        string queryString = string.Join("&", queryParams);
        return $"api/taxonomy?{queryString}";
    }
}

/// <summary>
/// DTO for taxonomy statistics.
/// </summary>
public class TaxonomyStatsDto
{
    public int Total { get; init; }
    public int Verified { get; init; }
    public int Pending { get; init; }
    public Dictionary<string, int> Categories { get; init; } = new Dictionary<string, int>();
}
