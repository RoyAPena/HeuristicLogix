using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using HeuristicLogix.Shared.DTOs;
using HeuristicLogix.Client.ViewModels;

namespace HeuristicLogix.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LogisticsController : ControllerBase
{
    private readonly IInvoiceExcelService _excelService;
    private readonly IConfiguration _config;

    public LogisticsController(IInvoiceExcelService excelService, IConfiguration config)
    {
        _excelService = excelService;
        _config = config;
    }

    [HttpPost("ProcessStaging")]
    public async Task<IActionResult> ProcessStaging([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var invoices = await _excelService.AnalyzeExcelAsync(file.OpenReadStream(), file.FileName, cancellationToken);
        var googleKey = _config["GoogleMaps:FrontendKey"] ?? throw new InvalidOperationException("Google Maps key not configured.");

        var vm = new InvoiceValidationViewModel
        {
            Invoices = invoices.Select(s => new InvoiceStagingDto
            {
                InvoiceNumber = s.InvoiceNumber,
                ClientName = s.ClientName,
                Address = s.Address,
                Latitude = s.GeocodingResult?.Latitude ?? 0,
                Longitude = s.GeocodingResult?.Longitude ?? 0,
                TotalWeightKg = s.TotalWeightKg,
                DominantMaterialType = s.DominantMaterialType,
                GeocodingStatus = s.GeocodingStatus,
                RequiresManualReview = s.RequiresManualReview,
                MaterialTags = s.WeightByType.Keys.Select(t => t.ToString()).ToList()
            }).ToList(),
            GoogleMapsFrontendKey = googleKey
        };
        return new JsonResult(vm);
    }
}
