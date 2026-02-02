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
        return await _http.GetFromJsonAsync<IEnumerable<Category>>(BaseEndpoint) 
            ?? Array.Empty<Category>();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Category>($"{BaseEndpoint}/{id}");
    }

    public async Task<Category> CreateAsync(CategoryUpsertDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseEndpoint, dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Category>())!;
    }

    public async Task<Category> UpdateAsync(int id, CategoryUpsertDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseEndpoint}/{id}", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Category>())!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseEndpoint}/{id}");
        return response.IsSuccessStatusCode;
    }
}


