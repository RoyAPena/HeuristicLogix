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
        var url = BaseEndpoint;
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] GET {url}");
        return await _http.GetFromJsonAsync<IEnumerable<UnitOfMeasure>>(url) 
            ?? Array.Empty<UnitOfMeasure>();
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(int id)
    {
        var url = $"{BaseEndpoint}/{id}";
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] GET {url}");
        return await _http.GetFromJsonAsync<UnitOfMeasure>(url);
    }

    public async Task<UnitOfMeasure> CreateAsync(UnitOfMeasureUpsertDto dto)
    {
        var url = BaseEndpoint;
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] POST {url}");
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] DTO: UnitOfMeasureId={dto.UnitOfMeasureId}, Name={dto.UnitOfMeasureName}, Symbol={dto.UnitOfMeasureSymbol}");
        var response = await _http.PostAsJsonAsync(url, dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UnitOfMeasure>())!;
    }

    public async Task<UnitOfMeasure> UpdateAsync(int id, UnitOfMeasureUpsertDto dto)
    {
        var url = $"{BaseEndpoint}/{id}";
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] PUT {url}");
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] ID param: {id}");
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] DTO: UnitOfMeasureId={dto.UnitOfMeasureId}, Name={dto.UnitOfMeasureName}, Symbol={dto.UnitOfMeasureSymbol}");
        var response = await _http.PutAsJsonAsync(url, dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UnitOfMeasure>())!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var url = $"{BaseEndpoint}/{id}";
        Console.WriteLine($"[UnitOfMeasureMaintenanceService] DELETE {url}");
        var response = await _http.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }
}



