using HeuristicLogix.Shared.DTOs;
using HeuristicLogix.Shared.Models;
using HeuristicLogix.Shared.Services;

namespace HeuristicLogix.Modules.Inventory.Services;

/// <summary>
/// Specific maintenance service for Category operations with int ID support.
/// Encapsulates API endpoint and provides strongly-typed operations.
/// </summary>
public interface ICategoryMaintenanceService : ISpecificMaintenanceService<Category, CategoryUpsertDto, int>
{
}


