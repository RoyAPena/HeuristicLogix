# Blazor WebAssembly Configuration for Tactical Dashboard

## Program.cs Configuration

Add the following to `HeuristicLogix.Client/Program.cs`:

```csharp
using HeuristicLogix.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register MudBlazor services
builder.Services.AddMudServices();

// Register HTTP client with base address
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Register custom services (explicit typing - no var)
builder.Services.AddScoped<ITaxonomyService, TaxonomyService>();

await builder.Build().RunAsync();
```

## MudBlazor Theme Configuration

Add to `wwwroot/index.html` or `App.razor`:

```html
<MudThemeProvider Theme="_theme" />
<MudDialogProvider />
<MudSnackbarProvider />

@code {
    private MudTheme _theme = new MudTheme
    {
        Palette = new PaletteLight
        {
            Primary = "#1E88E5",
            Success = "#43A047",
            Warning = "#FB8C00",
            Error = "#E53935",
            Dark = "#424242",
            Background = "#F5F5F5"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#42A5F5",
            Success = "#66BB6A",
            Warning = "#FFA726",
            Error = "#EF5350",
            Dark = "#212121",
            Background = "#121212",
            Surface = "#1E1E1E",
            AppbarBackground = "#1E1E1E"
        }
    };
}
```

## API Endpoint Registration

Ensure these endpoints are registered in `HeuristicLogix.Api/Program.cs`:

```csharp
// Add controllers
builder.Services.AddControllers();

// Ensure TaxonomyController and ExcelController are discovered
// (automatically discovered if in Controllers folder)
```

## Required NuGet Packages

### HeuristicLogix.Client:
```bash
dotnet add package MudBlazor
```

### HeuristicLogix.Api:
```bash
# Already added in previous steps
# - MiniExcel
# - Microsoft.EntityFrameworkCore
```

## CORS Configuration

Add to `HeuristicLogix.Api/Program.cs` if using separate Blazor WASM:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasm", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// After app build:
app.UseCors("BlazorWasm");
```

## File Structure

```
HeuristicLogix.Client/
??? Pages/
?   ??? TaxonomyManager.razor      ? Grid for taxonomy management
?   ??? ExcelUploader.razor         ? File upload component
??? Services/
?   ??? TaxonomyService.cs          ? API communication service
??? Shared/
?   ??? TacticalDashboardNav.razor  ? Navigation menu section
??? wwwroot/
    ??? index.html                   ? MudBlazor theme

HeuristicLogix.Shared/
??? DTOs/
    ??? TaxonomyDto.cs               ? Shared DTOs

HeuristicLogix.Api/
??? Controllers/
    ??? TaxonomyController.cs        ? API endpoints
```
