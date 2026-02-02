using System.Net.Http.Json;
using HeuristicLogix.Shared.DTOs;
using HeuristicLogix.Shared.Models;
using HeuristicLogix.Shared.Services;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Implementation of Category maintenance service with int ID support.
/// Handles HTTP communication with the API for Category operations.
/// </summary>
public class CategoryMaintenanceService(HttpClient http) : ICategoryMaintenanceService
{
    private const string BaseEndpoint = "api/inventory/categories";
    private readonly HttpClient _http = http;

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        var url = BaseEndpoint;
        Console.WriteLine($"[CategoryMaintenanceService] GET {url}");
        return await _http.GetFromJsonAsync<IEnumerable<Category>>(url) 
            ?? Array.Empty<Category>();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var url = $"{BaseEndpoint}/{id}";
        Console.WriteLine($"[CategoryMaintenanceService] GET {url}");
        return await _http.GetFromJsonAsync<Category>(url);
    }

    public async Task<Category> CreateAsync(CategoryUpsertDto dto)
    {
        var url = BaseEndpoint;
        Console.WriteLine($"[CategoryMaintenanceService] POST {url}");
        Console.WriteLine($"[CategoryMaintenanceService] DTO: CategoryId={dto.CategoryId}, CategoryName={dto.CategoryName}");
        var response = await _http.PostAsJsonAsync(url, dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Category>())!;
    }

    public async Task<Category> UpdateAsync(int id, CategoryUpsertDto dto)
    {
        var url = $"{BaseEndpoint}/{id}";
        Console.WriteLine($"[CategoryMaintenanceService] PUT {url}");
        Console.WriteLine($"[CategoryMaintenanceService] ID param: {id}");
        Console.WriteLine($"[CategoryMaintenanceService] DTO: CategoryId={dto.CategoryId}, CategoryName={dto.CategoryName}");
        var response = await _http.PutAsJsonAsync(url, dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Category>())!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var url = $"{BaseEndpoint}/{id}";
        Console.WriteLine($"[CategoryMaintenanceService] DELETE {url}");
        var response = await _http.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }
}



