namespace HeuristicLogix.Shared.Interfaces;

public interface IGeocodingService
{
    /// <summary>
    /// Converts a hardware store delivery address into geographic coordinates.
    /// </summary>
    Task<GeocodeResult> GetCoordinatesAsync(string address);
}

public record GeocodeResult(double Latitude, double Longitude, bool Success, string? ErrorMessage);