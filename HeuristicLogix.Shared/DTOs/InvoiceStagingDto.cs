using System.Collections.Generic;

namespace HeuristicLogix.Shared.DTOs;

public sealed class InvoiceStagingDto
{
    public required string InvoiceNumber { get; set; }
    public required string ClientName { get; set; }
    public required string Address { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public required decimal TotalWeightKg { get; set; }
    //public required MaterialType DominantMaterialType { get; set; }
    //public required GeocodingStatus GeocodingStatus { get; set; }
    public required bool RequiresManualReview { get; set; }
    public List<string> MaterialTags { get; set; } = new();
}
