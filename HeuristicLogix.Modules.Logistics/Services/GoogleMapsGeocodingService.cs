using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Logistics.Services;

public class GoogleMapsGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleMapsGeocodingService> _logger;
    private readonly string _apiKey;
    private readonly Dictionary<string, GeocodingResult> _cache = new();

    public GoogleMapsGeocodingService(HttpClient httpClient, ILogger<GoogleMapsGeocodingService> logger, string apiKey)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = apiKey;
    }

    public async Task<GeocodingResult> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            return new GeocodingResult { Success = false, Status = GeocodingStatus.Failed, ErrorMessage = "Address is empty" };

        if (_cache.TryGetValue(address, out var cached))
            return cached;

        try
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string error = $"Google Maps API error: {response.StatusCode}";
                _logger.LogError(error);
                return new GeocodingResult { Success = false, Status = GeocodingStatus.Failed, ErrorMessage = error };
            }
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var status = root.GetProperty("status").GetString();
            if (status != "OK")
            {
                string error = $"Google Maps returned status: {status}";
                _logger.LogWarning(error);
                return new GeocodingResult { Success = false, Status = GeocodingStatus.Failed, ErrorMessage = error };
            }
            var result = root.GetProperty("results")[0];
            var location = result.GetProperty("geometry").GetProperty("location");
            double lat = location.GetProperty("lat").GetDouble();
            double lng = location.GetProperty("lng").GetDouble();
            string formatted = result.GetProperty("formatted_address").GetString() ?? address;
            string locationType = result.GetProperty("geometry").GetProperty("location_type").GetString() ?? "UNKNOWN";
            double confidence = locationType switch
            {
                "ROOFTOP" => 1.0,
                "RANGE_INTERPOLATED" => 0.8,
                "GEOMETRIC_CENTER" => 0.6,
                "APPROXIMATE" => 0.5,
                _ => 0.4
            };
            GeocodingStatus geoStatus = confidence >= 0.95 ? GeocodingStatus.Success : (confidence >= 0.7 ? GeocodingStatus.Ambiguous : GeocodingStatus.Failed);
            var resultObj = new GeocodingResult
            {
                Success = geoStatus == GeocodingStatus.Success,
                Latitude = lat,
                Longitude = lng,
                FormattedAddress = formatted,
                LocationType = locationType,
                Confidence = confidence,
                Status = geoStatus,
                ErrorMessage = geoStatus == GeocodingStatus.Failed ? $"Low confidence ({locationType})" : null
            };
            _cache[address] = resultObj;
            return resultObj;
        }
        catch (Exception ex)
        {
            string error = ex is TaskCanceledException ? "Geocoding request timed out" : $"Geocoding error: {ex.Message}";
            _logger.LogError(ex, error);
            return new GeocodingResult { Success = false, Status = GeocodingStatus.Failed, ErrorMessage = error };
        }
    }
}
