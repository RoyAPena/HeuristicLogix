using FluentValidation;
using HeuristicLogix.Modules.Inventory.Services;
using HeuristicLogix.Modules.Inventory.Validators;
using HeuristicLogix.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HeuristicLogix.Modules.Inventory;

/// <summary>
/// Extension methods for registering Inventory Module services.
/// This enables modular monolith architecture with clean separation.
/// </summary>
public static class InventoryModuleExtensions
{
    /// <summary>
    /// Registers Inventory Module services for the API/Backend.
    /// Use AddInventoryModuleClient() for Blazor WebAssembly client services.
    /// </summary>
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        // Register backend services (API layer only) - now using MediatR
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

        // Register validators (used by API for server-side validation)
        services.AddValidatorsFromAssemblyContaining<CategoryValidator>();
        services.AddScoped<IValidator<CategoryUpsertDto>, CategoryUpsertDtoValidator>();
        services.AddScoped<IValidator<UnitOfMeasureUpsertDto>, UnitOfMeasureUpsertDtoValidator>();

        return services;
    }

    /// <summary>
    /// Registers Inventory Module services for the Blazor WebAssembly Client.
    /// This includes HttpClient-based maintenance services.
    /// </summary>
    public static IServiceCollection AddInventoryModuleClient(this IServiceCollection services, string baseApiUrl)
    {
        // Register frontend maintenance services (Client layer - requires HttpClient)
        services.AddScoped<ICategoryMaintenanceService, CategoryMaintenanceService>();
        services.AddScoped<IUnitOfMeasureMaintenanceService, UnitOfMeasureMaintenanceService>();

        // Register client-side validators (optional, for client-side validation)
        services.AddScoped<IValidator<CategoryUpsertDto>, CategoryUpsertDtoValidator>();
        services.AddScoped<IValidator<UnitOfMeasureUpsertDto>, UnitOfMeasureUpsertDtoValidator>();

        return services;
    }
}


