using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Shared.Modules.Logistics;

/// <summary>
/// Logistics module API contract.
/// Provides operations for dispatch planning, route optimization, and delivery management.
/// </summary>
public interface ILogisticsModuleAPI : IModuleAPI
{
    /// <summary>
    /// Gets a conduce by ID.
    /// </summary>
    /// <param name="conduceId">The conduce identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Conduce or null if not found.</returns>
    Task<Conduce?> GetConduceAsync(
        Guid conduceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending conduces awaiting assignment.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending conduces.</returns>
    Task<List<Conduce>> GetPendingConducesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a truck by ID.
    /// </summary>
    /// <param name="truckId">The truck identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Truck or null if not found.</returns>
    Task<Truck?> GetTruckAsync(
        Guid truckId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active trucks in the fleet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active trucks.</returns>
    Task<List<Truck>> GetActiveTrucksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a delivery route by ID.
    /// </summary>
    /// <param name="routeId">The route identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Delivery route or null if not found.</returns>
    Task<DeliveryRoute?> GetRouteAsync(
        Guid routeId,
        CancellationToken cancellationToken = default);
}
