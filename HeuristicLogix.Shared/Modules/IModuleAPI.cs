namespace HeuristicLogix.Shared.Modules;

/// <summary>
/// Base interface for all module APIs in HeuristicLogix.
/// Provides a common contract for cross-module communication.
/// </summary>
public interface IModuleAPI
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Health check for the module.
    /// </summary>
    /// <returns>True if the module is healthy, false otherwise.</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
