using HeuristicLogix.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Logistics.Services;

/// <summary>
/// Service for automatically classifying materials based on name/description.
/// Uses keyword matching to determine MaterialType for logistics planning.
/// </summary>
public interface IMaterialClassificationService
{
    /// <summary>
    /// Classifies a material by name and returns the MaterialType.
    /// </summary>
    /// <param name="materialName">Material name or description.</param>
    /// <returns>MaterialType classification.</returns>
    MaterialType ClassifyMaterial(string materialName);

    /// <summary>
    /// Classifies a material and returns detailed characteristics.
    /// Includes handling requirements and truck preferences.
    /// </summary>
    /// <param name="materialName">Material name or description.</param>
    /// <returns>Material characteristics with handling requirements.</returns>
    MaterialCharacteristics GetMaterialCharacteristics(string materialName);

    /// <summary>
    /// Determines the dominant material type from a collection of items.
    /// Used to select appropriate truck type for the entire delivery.
    /// </summary>
    /// <param name="items">Collection of conduce items.</param>
    /// <returns>Dominant MaterialType.</returns>
    MaterialType GetDominantMaterialType(IEnumerable<ConduceItem> items);

    /// <summary>
    /// Checks if any items in the collection require special handling.
    /// </summary>
    /// <param name="items">Collection of conduce items.</param>
    /// <returns>True if special handling is required.</returns>
    bool RequiresSpecialHandling(IEnumerable<ConduceItem> items);
}

/// <summary>
/// Implementation of material classification service.
/// Uses explicit keyword matching with priority rules.
/// </summary>
public class MaterialClassificationService : IMaterialClassificationService
{
    private readonly ILogger<MaterialClassificationService> _logger;

    // Material type keywords (uppercase for case-insensitive matching)
    private static readonly Dictionary<MaterialType, List<string>> MaterialKeywords = new Dictionary<MaterialType, List<string>>
    {
        [MaterialType.Long] = new List<string>
        {
            // Spanish keywords
            "VARILLA", "CABILLA", "VIGA", "TUBO", "TUBERIA", "PERFIL", "RIEL",
            "MADERA", "TABLÓN", "VIGUETA", "POSTE", "ANGULAR", "CANAL",
            // English keywords
            "REBAR", "BEAM", "PIPE", "TUBE", "LUMBER", "PLANK", "POST", "RAIL"
        },

        [MaterialType.Heavy] = new List<string>
        {
            // Spanish keywords
            "CEMENTO", "ACERO", "BLOQUE", "BLOCK", "LADRILLO", "LOSA",
            "CONCRETO", "HORMIGÓN", "ADOQUÍN", "MÁRMOL", "GRANITO",
            // English keywords
            "CEMENT", "STEEL", "CONCRETE", "SLAB", "BRICK", "MARBLE", "GRANITE"
        },

        [MaterialType.Bulk] = new List<string>
        {
            // Spanish keywords
            "ARENA", "PIEDRA", "GRAVA", "GRAVILLA", "AGREGADO", "TIERRA",
            "ESCOMBRO", "CASCAJO", "RIPIO",
            // English keywords
            "SAND", "GRAVEL", "AGGREGATE", "STONE", "DIRT", "SOIL"
        },

        [MaterialType.Fragile] = new List<string>
        {
            // Spanish keywords
            "VIDRIO", "CRISTAL", "CERÁMICA", "PORCELANATO", "ESPEJO",
            "VITRINA", "VENTANA", "PISO CERÁMICO",
            // English keywords
            "GLASS", "CERAMIC", "MIRROR", "TILE", "WINDOW"
        },

        [MaterialType.Hazardous] = new List<string>
        {
            // Spanish keywords
            "QUÍMICO", "SOLVENTE", "THINNER", "ÁCIDO", "INFLAMABLE",
            "CORROSIVO", "TÓXICO", "PELIGROSO",
            // English keywords
            "CHEMICAL", "SOLVENT", "ACID", "FLAMMABLE", "TOXIC", "HAZARDOUS"
        }
    };

    // Priority order for material types (higher number = higher priority)
    private static readonly Dictionary<MaterialType, int> MaterialTypePriority = new Dictionary<MaterialType, int>
    {
        [MaterialType.Hazardous] = 5,  // Highest priority
        [MaterialType.Long] = 4,
        [MaterialType.Heavy] = 3,
        [MaterialType.Bulk] = 2,
        [MaterialType.Fragile] = 1,
        [MaterialType.General] = 0     // Default/lowest
    };

    public MaterialClassificationService(ILogger<MaterialClassificationService> logger)
    {
        _logger = logger;
    }

    public MaterialType ClassifyMaterial(string materialName)
    {
        if (string.IsNullOrWhiteSpace(materialName))
        {
            _logger.LogWarning("Empty material name provided for classification");
            return MaterialType.General;
        }

        string upperName = materialName.Trim().ToUpperInvariant();

        // Check each material type's keywords
        foreach (KeyValuePair<MaterialType, List<string>> typeKeywords in MaterialKeywords)
        {
            foreach (string keyword in typeKeywords.Value)
            {
                if (upperName.Contains(keyword))
                {
                    _logger.LogDebug(
                        "Material '{MaterialName}' classified as {MaterialType} (matched keyword: {Keyword})",
                        materialName, typeKeywords.Key, keyword);
                    
                    return typeKeywords.Key;
                }
            }
        }

        _logger.LogDebug("Material '{MaterialName}' classified as General (no keyword match)", materialName);
        return MaterialType.General;
    }

    public MaterialCharacteristics GetMaterialCharacteristics(string materialName)
    {
        MaterialType type = ClassifyMaterial(materialName);

        MaterialCharacteristics characteristics = type switch
        {
            MaterialType.Long => MaterialCharacteristics.Long,
            MaterialType.Heavy => MaterialCharacteristics.Heavy,
            MaterialType.Bulk => MaterialCharacteristics.Bulk,
            MaterialType.Fragile => MaterialCharacteristics.Fragile,
            MaterialType.Hazardous => MaterialCharacteristics.Hazardous,
            _ => MaterialCharacteristics.General
        };

        _logger.LogDebug(
            "Material '{MaterialName}' characteristics: Type={Type}, RequiresSpecialHandling={RequiresSpecialHandling}",
            materialName, type, characteristics.RequiresSpecialHandling);

        return characteristics;
    }

    public MaterialType GetDominantMaterialType(IEnumerable<ConduceItem> items)
    {
        if (items == null || !items.Any())
        {
            _logger.LogWarning("No items provided for dominant material type calculation");
            return MaterialType.General;
        }

        // Group items by material type and calculate total weight per type
        Dictionary<MaterialType, decimal> weightByType = items
            .GroupBy(item => item.MaterialType)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => item.WeightKg ?? 0m)
            );

        // If we have weight data, use heaviest type with priority consideration
        if (weightByType.Any(kvp => kvp.Value > 0))
        {
            // Calculate weighted score: weight * priority
            MaterialType dominantType = weightByType
                .OrderByDescending(kvp => 
                {
                    int priority = MaterialTypePriority.GetValueOrDefault(kvp.Key, 0);
                    decimal weight = kvp.Value;
                    return weight * priority;
                })
                .First()
                .Key;

            _logger.LogInformation(
                "Dominant material type determined by weight: {MaterialType} ({Count} item types analyzed)",
                dominantType, weightByType.Count);

            return dominantType;
        }

        // Fallback: Use highest priority type by item count
        MaterialType dominantByCount = items
            .GroupBy(item => item.MaterialType)
            .OrderByDescending(group => MaterialTypePriority.GetValueOrDefault(group.Key, 0))
            .ThenByDescending(group => group.Count())
            .First()
            .Key;

        _logger.LogInformation(
            "Dominant material type determined by priority/count: {MaterialType}",
            dominantByCount);

        return dominantByCount;
    }

    public bool RequiresSpecialHandling(IEnumerable<ConduceItem> items)
    {
        if (items == null || !items.Any())
        {
            return false;
        }

        bool requiresSpecialHandling = items.Any(item =>
        {
            MaterialCharacteristics characteristics = GetMaterialCharacteristics(item.MaterialName);
            return characteristics.RequiresSpecialHandling;
        });

        if (requiresSpecialHandling)
        {
            _logger.LogInformation("Load requires special handling (contains Long, Fragile, or Hazardous materials)");
        }

        return requiresSpecialHandling;
    }
}
