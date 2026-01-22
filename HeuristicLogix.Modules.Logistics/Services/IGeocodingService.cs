namespace HeuristicLogix.Modules.Logistics.Services;

public enum GeocodingStatus
{
    Success,
    Ambiguous,
    Failed
}

public class GeocodingResult
{
    public bool Success { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? FormattedAddress { get; set; }
    public string? LocationType { get; set; } // e.g. ROOFTOP, RANGE_INTERPOLATED
    public double Confidence { get; set; } // 1.0 = precise, <0.7 = ambiguous
    public string? ErrorMessage { get; set; }
    public GeocodingStatus Status { get; set; }
}

public interface IGeocodingService
{
    Task<GeocodingResult> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default);
}
