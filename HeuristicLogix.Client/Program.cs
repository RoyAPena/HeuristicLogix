using HeuristicLogix.Client;
using HeuristicLogix.Modules.Inventory;
using HeuristicLogix.Shared.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http.Json;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Get API base address from configuration or use default
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7086";

// Configure HttpClient with HeuristicLogix JSON options (string-based enum serialization)
builder.Services.AddScoped(sp =>
{
    HttpClient httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
    return httpClient;
});

// Register global JSON serializer options for consistent enum handling
builder.Services.AddSingleton(HeuristicJsonOptions.Web);

// Register MudBlazor services
builder.Services.AddMudServices();

// ============================================================
// MODULE REGISTRATION (Client-side services for Blazor WASM)
// ============================================================
builder.Services.AddInventoryModuleClient(apiBaseUrl);

await builder.Build().RunAsync();
