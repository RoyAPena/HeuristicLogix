namespace HeuristicLogix.Shared.Modules.Finance;

/// <summary>
/// Finance module API contract.
/// Provides synchronous operations for credit checking and client management.
/// </summary>
public interface IFinanceModuleAPI : IModuleAPI
{
    /// <summary>
    /// Checks if a client is eligible for credit for a given order value.
    /// This is a synchronous call used during Conduce creation for immediate validation.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="orderValue">The total value of the order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Credit check result indicating approval status.</returns>
    Task<CreditCheckResult> CheckClientCreditAsync(
        Guid clientId,
        decimal orderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current credit limit for a client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Credit limit details or null if not found.</returns>
    Task<ClientCreditLimit?> GetClientCreditLimitAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets client information.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Client information or null if not found.</returns>
    Task<ClientInfo?> GetClientInfoAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a credit check operation.
/// </summary>
public class CreditCheckResult
{
    /// <summary>
    /// Whether the credit check was approved.
    /// </summary>
    public required bool IsApproved { get; init; }

    /// <summary>
    /// Available credit amount.
    /// </summary>
    public required decimal AvailableCredit { get; init; }

    /// <summary>
    /// Current credit utilization percentage (0-100).
    /// </summary>
    public required decimal UtilizationPercentage { get; init; }

    /// <summary>
    /// Reason for approval or rejection.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Warning message if approaching credit limit.
    /// </summary>
    public string? WarningMessage { get; init; }

    /// <summary>
    /// Creates an approved credit check result.
    /// </summary>
    public static CreditCheckResult Approved(decimal availableCredit, decimal utilizationPercentage, string? warning = null)
    {
        return new CreditCheckResult
        {
            IsApproved = true,
            AvailableCredit = availableCredit,
            UtilizationPercentage = utilizationPercentage,
            Reason = "Credit approved",
            WarningMessage = warning
        };
    }

    /// <summary>
    /// Creates a rejected credit check result.
    /// </summary>
    public static CreditCheckResult Rejected(string reason, decimal utilizationPercentage = 100)
    {
        return new CreditCheckResult
        {
            IsApproved = false,
            AvailableCredit = 0,
            UtilizationPercentage = utilizationPercentage,
            Reason = reason
        };
    }
}

/// <summary>
/// Client credit limit information.
/// </summary>
public class ClientCreditLimit
{
    /// <summary>
    /// Client identifier.
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Total credit limit.
    /// </summary>
    public required decimal CreditLimit { get; init; }

    /// <summary>
    /// Currently used credit.
    /// </summary>
    public required decimal UsedCredit { get; init; }

    /// <summary>
    /// Available credit (CreditLimit - UsedCredit).
    /// </summary>
    public decimal AvailableCredit => CreditLimit - UsedCredit;

    /// <summary>
    /// Credit utilization percentage.
    /// </summary>
    public decimal UtilizationPercentage => CreditLimit > 0 ? (UsedCredit / CreditLimit) * 100 : 0;

    /// <summary>
    /// Payment terms in days.
    /// </summary>
    public int PaymentTermsDays { get; init; }

    /// <summary>
    /// Whether the client is currently blocked.
    /// </summary>
    public bool IsBlocked { get; init; }

    /// <summary>
    /// Last credit review date.
    /// </summary>
    public DateTimeOffset? LastReviewDate { get; init; }
}

/// <summary>
/// Client information.
/// </summary>
public class ClientInfo
{
    /// <summary>
    /// Client identifier.
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Client name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Client tax ID or business registration number.
    /// </summary>
    public string? TaxId { get; init; }

    /// <summary>
    /// Whether the client is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Client type (e.g., "Retail", "Wholesale", "Government").
    /// </summary>
    public string? ClientType { get; init; }

    /// <summary>
    /// Primary contact phone.
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Primary contact email.
    /// </summary>
    public string? Email { get; init; }
}
