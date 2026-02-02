using System.Net.Http.Json;

namespace HeuristicLogix.Shared.Services;

/// <summary>
/// Generic HTTP-based maintenance service for CRUD operations.
/// Eliminates the need for specific maintenance service implementations.
/// Uses HttpClient to communicate with API on port 7086.
/// </summary>
/// <typeparam name="TEntity">The entity type (e.g., Category, UnitOfMeasure)</typeparam>
/// <typeparam name="TDto">The DTO type for create/update operations</typeparam>
/// <typeparam name="TId">The ID type (int or Guid)</typeparam>
public class BaseHttpMaintenanceService<TEntity, TDto, TId> : IBaseMaintenanceService<TEntity, TDto, TId>
    where TEntity : class
    where TDto : class
    where TId : struct
{
    private readonly HttpClient _http;
    private readonly string _baseEndpoint;
    private readonly string _entityName;

    /// <summary>
    /// Creates a new HTTP-based maintenance service.
    /// </summary>
    /// <param name="http">HttpClient configured with API base URL (https://localhost:7086)</param>
    /// <param name="entityPath">API path segment (e.g., "categories", "unitsofmeasure")</param>
    /// <param name="entityName">Human-readable entity name for logging (optional)</param>
    public BaseHttpMaintenanceService(HttpClient http, string entityPath, string? entityName = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _baseEndpoint = $"api/inventory/{entityPath}";
        _entityName = entityName ?? typeof(TEntity).Name;
    }

    /// <summary>
    /// Gets all entities from the API.
    /// </summary>
    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var url = _baseEndpoint;
        Console.WriteLine($"[{_entityName}] GET {url}");
        
        try
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<TEntity>>(url);
            Console.WriteLine($"[{_entityName}] GET Success: {result?.Count() ?? 0} items");
            return result ?? Array.Empty<TEntity>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_entityName}] GET Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets a single entity by ID.
    /// </summary>
    public async Task<TEntity?> GetByIdAsync(TId id)
    {
        var url = $"{_baseEndpoint}/{id}";
        Console.WriteLine($"[{_entityName}] GET {url}");
        
        try
        {
            var result = await _http.GetFromJsonAsync<TEntity>(url);
            Console.WriteLine($"[{_entityName}] GET Success: Found entity");
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine($"[{_entityName}] GET 404: Entity not found");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_entityName}] GET Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    public async Task<TEntity> CreateAsync(TDto dto)
    {
        var url = _baseEndpoint;
        Console.WriteLine($"[{_entityName}] POST {url}");
        Console.WriteLine($"[{_entityName}] DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");
        
        try
        {
            var response = await _http.PostAsJsonAsync(url, dto);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[{_entityName}] POST Failed: {response.StatusCode}");
                Console.WriteLine($"[{_entityName}] Response: {responseBody}");
            }
            
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TEntity>();
            Console.WriteLine($"[{_entityName}] POST Success: Entity created");
            return result!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_entityName}] POST Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public async Task<TEntity> UpdateAsync(TId id, TDto dto)
    {
        var url = $"{_baseEndpoint}/{id}";
        Console.WriteLine($"[{_entityName}] PUT {url}");
        Console.WriteLine($"[{_entityName}] ID param: {id}");
        Console.WriteLine($"[{_entityName}] DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");
        
        try
        {
            var response = await _http.PutAsJsonAsync(url, dto);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[{_entityName}] PUT Failed: {response.StatusCode}");
                Console.WriteLine($"[{_entityName}] Response: {responseBody}");
            }
            
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TEntity>();
            Console.WriteLine($"[{_entityName}] PUT Success: Entity updated");
            return result!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_entityName}] PUT Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    public async Task<bool> DeleteAsync(TId id)
    {
        var url = $"{_baseEndpoint}/{id}";
        Console.WriteLine($"[{_entityName}] DELETE {url}");
        
        try
        {
            var response = await _http.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[{_entityName}] DELETE Success");
                return true;
            }
            else
            {
                Console.WriteLine($"[{_entityName}] DELETE Failed: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[{_entityName}] Response: {responseBody}");
                
                // Parse and throw meaningful error with response content
                throw new HttpRequestException(
                    $"DELETE failed with status {response.StatusCode}: {responseBody}",
                    null,
                    response.StatusCode);
            }
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with all details
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_entityName}] DELETE Error: {ex.Message}");
            throw;
        }
    }
}
