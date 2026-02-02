using FluentValidation;
using HeuristicLogix.Modules.Inventory.Services;
using HeuristicLogix.Modules.Inventory.Validators;
using HeuristicLogix.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Inventory;

/// <summary>
/// Extension methods for registering Inventory Module services.
/// This enables modular monolith architecture with clean separation.
/// </summary>
public static class InventoryModuleExtensions
{
    /// <summary>
    /// Registers all Inventory Module services, validators, and maintenance services.
    /// </summary>
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        // Register backend services (API layer)
        services.AddScoped<ICategoryService>(sp => 
            new CategoryService(
                sp.GetRequiredService<DbContext>(), 
                sp.GetRequiredService<ILogger<CategoryService>>()));
                
        services.AddScoped<IUnitOfMeasureService>(sp => 
            new UnitOfMeasureService(
                sp.GetRequiredService<DbContext>(), 
                sp.GetRequiredService<ILogger<UnitOfMeasureService>>()));

        // Register frontend maintenance services (Client layer)
        services.AddScoped<ICategoryMaintenanceService, CategoryMaintenanceService>();
        services.AddScoped<IUnitOfMeasureMaintenanceService, UnitOfMeasureMaintenanceService>();

        // Register validators (used by both API and Client)
        services.AddValidatorsFromAssemblyContaining<CategoryValidator>();
        services.AddScoped<IValidator<CategoryUpsertDto>, CategoryUpsertDtoValidator>();
        services.AddScoped<IValidator<UnitOfMeasureUpsertDto>, UnitOfMeasureUpsertDtoValidator>();

        return services;
    }
}


