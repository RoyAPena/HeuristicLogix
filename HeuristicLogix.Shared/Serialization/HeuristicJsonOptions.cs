using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeuristicLogix.Shared.Serialization;

/// <summary>
/// Centralized JSON serialization configuration for HeuristicLogix.
/// Enforces string-based enum serialization for ML readability and API consistency.
/// </summary>
public static class HeuristicJsonOptions
{
    /// <summary>
    /// Default JSON serializer options configured for HeuristicLogix.
    /// All enums are serialized as strings (not integers) for ML training readability.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = CreateDefaultOptions();

    /// <summary>
    /// Web-optimized JSON serializer options (camelCase property naming).
    /// </summary>
    public static JsonSerializerOptions Web { get; } = CreateWebOptions();

    /// <summary>
    /// Creates the default JSON serializer options.
    /// </summary>
    private static JsonSerializerOptions CreateDefaultOptions()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // PascalCase for .NET conventions
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // CRITICAL: All enums must be strings for ML readability
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        return options;
    }

    /// <summary>
    /// Creates web-optimized JSON serializer options with camelCase naming.
    /// </summary>
    private static JsonSerializerOptions CreateWebOptions()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // CRITICAL: All enums must be strings for ML readability
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        return options;
    }

    /// <summary>
    /// Serializes an object to JSON using HeuristicLogix conventions.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>JSON string with enums as string names.</returns>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Default);
    }

    /// <summary>
    /// Deserializes JSON to an object using HeuristicLogix conventions.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <returns>Deserialized object.</returns>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Default);
    }
}
