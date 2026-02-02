using HeuristicLogix.Api.Persistence;
using HeuristicLogix.Api.Services;
using HeuristicLogix.Features.Inventory.Categories;
using HeuristicLogix.Modules.Inventory;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Database - AppDbContext (NEW ERP Architecture)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Register AppDbContext as DbContext for modules
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

// ============================================================
// MediatR REGISTRATION (Vertical Slice Architecture)
// ============================================================
// Scan the assembly containing the handlers (HeuristicLogix.Features)
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<GetCategoriesHandler>();
});

// Add services
builder.Services.AddScoped<DataSeederService>();

// ============================================================
// MODULE REGISTRATION (Modular Monolith Architecture)
// ============================================================
builder.Services.AddInventoryModule();

// Add controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
