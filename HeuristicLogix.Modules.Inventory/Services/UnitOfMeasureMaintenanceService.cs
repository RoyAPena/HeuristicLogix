using System.Net.Http.Json;
using HeuristicLogix.Shared.DTOs;
using HeuristicLogix.Shared.Models;
using HeuristicLogix.Shared.Services;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Implementation of UnitOfMeasure maintenance service with int ID support.
/// Handles HTTP communication with the API for UnitOfMeasure operations.
/// </summary>
public class UnitOfMeasureMaintenanceService(HttpClient http) : IUnitOfMeasureMaintenanceService
{
    private const string BaseEndpoint = "api/inventory/unitsofmeasure";
    private readonly HttpClient _http = http;

    public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<IEnumerable<UnitOfMeasure>>(BaseEndpoint) 
            ?? Array.Empty<UnitOfMeasure>();
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<UnitOfMeasure>($"{BaseEndpoint}/{id}");
    }

    public async Task<UnitOfMeasure> CreateAsync(UnitOfMeasureUpsertDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseEndpoint, dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UnitOfMeasure>())!;
    }

    public async Task<UnitOfMeasure> UpdateAsync(int id, UnitOfMeasureUpsertDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseEndpoint}/{id}", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UnitOfMeasure>())!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseEndpoint}/{id}");
        return response.IsSuccessStatusCode;
    }
}


