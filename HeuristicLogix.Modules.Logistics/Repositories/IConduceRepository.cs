using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Modules.Logistics.Repositories;

/// <summary>
/// Repository for Conduce (Invoice/Delivery Order) persistence.
/// Follows Repository pattern for clean separation of concerns.
/// </summary>
public interface IConduceRepository
{
    /// <summary>
    /// Creates a new conduce in the database.
    /// </summary>
    /// <param name="conduce">Conduce to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created conduce with assigned ID.</returns>
    Task<Conduce> CreateAsync(Conduce conduce, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a conduce by ID.
    /// </summary>
    /// <param name="id">Conduce ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Conduce if found.</returns>
    Task<Conduce?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all conduces with optional filtering.
    /// </summary>
    /// <param name="status">Filter by status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of conduces.</returns>
    Task<List<Conduce>> GetAllAsync(ConduceStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conduce.
    /// </summary>
    /// <param name="conduce">Conduce to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated conduce.</returns>
    Task<Conduce> UpdateAsync(Conduce conduce, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a conduce.
    /// </summary>
    /// <param name="id">Conduce ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a conduce with the given invoice number already exists.
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists.</returns>
    Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
}
