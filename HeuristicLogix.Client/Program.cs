using HeuristicLogix.Client;
using HeuristicLogix.Shared.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http.Json;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with HeuristicLogix JSON options (string-based enum serialization)
builder.Services.AddScoped(sp =>
{
    HttpClient httpClient = new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    return httpClient;
});

// Register global JSON serializer options for consistent enum handling
builder.Services.AddSingleton(HeuristicJsonOptions.Web);

// Registro de MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();