namespace HeuristicLogix.Shared.Services;

/// <summary>
/// Generic service interface for maintenance CRUD operations with hybrid ID support.
/// Supports both int and Guid IDs seamlessly.
/// </summary>
public interface IBaseMaintenanceService<TEntity, TDto, TId> 
    where TEntity : class
    where TDto : class
    where TId : struct
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity> CreateAsync(TDto dto);
    Task<TEntity> UpdateAsync(TId id, TDto dto);
    Task<bool> DeleteAsync(TId id);
}

/// <summary>
/// Adapter to bridge specific maintenance services (with DTOs) to the generic IBaseMaintenanceService interface.
/// This allows MaintenanceBase to work with both generic and specific services.
/// </summary>
public class BaseMaintenanceServiceAdapter<TEntity, TDto, TId>(ISpecificMaintenanceService<TEntity, TDto, TId> specificService) 
    : IBaseMaintenanceService<TEntity, TDto, TId>
    where TEntity : class
    where TDto : class
    where TId : struct
{
    private readonly ISpecificMaintenanceService<TEntity, TDto, TId> _specificService = specificService;

    public Task<IEnumerable<TEntity>> GetAllAsync() => _specificService.GetAllAsync();

    public Task<TEntity?> GetByIdAsync(TId id) => _specificService.GetByIdAsync(id);

    public Task<TEntity> CreateAsync(TDto dto) => _specificService.CreateAsync(dto);

    public Task<TEntity> UpdateAsync(TId id, TDto dto) => _specificService.UpdateAsync(id, dto);

    public Task<bool> DeleteAsync(TId id) => _specificService.DeleteAsync(id);
}

/// <summary>
/// Common interface for specific maintenance services with hybrid ID support.
/// Allows the adapter to work with any strongly-typed service.
/// </summary>
public interface ISpecificMaintenanceService<TEntity, TDto, TId>
    where TEntity : class
    where TDto : class
    where TId : struct
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity> CreateAsync(TDto dto);
    Task<TEntity> UpdateAsync(TId id, TDto dto);
    Task<bool> DeleteAsync(TId id);
}



