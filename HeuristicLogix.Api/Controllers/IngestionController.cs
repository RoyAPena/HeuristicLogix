using HeuristicLogix.Api.Services;
using HeuristicLogix.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeuristicLogix.Api.Controllers;

/// <summary>
/// API controller for data ingestion operations.
/// Handles upload and processing of historic delivery data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly IDataIngestionService _ingestionService;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(
        IDataIngestionService ingestionService,
        ILogger<IngestionController> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    /// <summary>
    /// Uploads and processes a file containing historic delivery data.
    /// Supported formats: Excel (.xlsx), CSV (.csv)
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ingestion result with statistics.</returns>
    [HttpPost("historic-deliveries")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DataIngestionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadHistoricDeliveries(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload failed: No file provided");
            return BadRequest(new { error = "No file uploaded" });
        }

        // Validate file extension
        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (fileExtension != ".xlsx" && fileExtension != ".csv")
        {
            _logger.LogWarning("Upload failed: Invalid file type {Extension}", fileExtension);
            return BadRequest(new { error = "Invalid file type. Only .xlsx and .csv are supported." });
        }

        // Validate file size (max 10MB)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            _logger.LogWarning("Upload failed: File too large ({Size} bytes)", file.Length);
            return BadRequest(new { error = $"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB" });
        }

        try
        {
            _logger.LogInformation("Starting ingestion of file: {FileName} ({Size} bytes)", 
                file.FileName, file.Length);

            // Get current user (or default to "System")
            string uploadedBy = User.Identity?.Name ?? "System";

            // Process file
            using Stream stream = file.OpenReadStream();
            DataIngestionResult result = await _ingestionService.IngestHistoricDeliveriesAsync(
                stream,
                file.FileName,
                uploadedBy,
                cancellationToken
            );

            _logger.LogInformation(
                "Ingestion completed: {BatchId}, Processed: {Processed}, Skipped: {Skipped}, Errors: {Errors}",
                result.BatchId, result.ProcessedRecords, result.SkippedRecords, result.Errors.Count);

            // Return appropriate status based on result
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else if (result.ProcessedRecords > 0)
            {
                // Partial success
                return Ok(result);
            }
            else
            {
                // Complete failure
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during file ingestion");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error during ingestion", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status of an ingestion batch.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch status information.</returns>
    [HttpGet("batch/{batchId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchStatus(
        string batchId,
        CancellationToken cancellationToken)
    {
        // TODO: Implement batch status query
        await Task.CompletedTask;
        return NotFound(new { error = "Batch status query not yet implemented" });
    }

    /// <summary>
    /// Downloads a CSV template for historic delivery data.
    /// Includes ProductDescription as mandatory field.
    /// </summary>
    /// <returns>CSV template file.</returns>
    [HttpGet("template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DownloadTemplate()
    {
        string templateContent = @"DeliveryDate,ClientName,ProductDescription,DeliveryAddress,Latitude,Longitude,TruckLicensePlate,TotalWeightKg,ServiceTimeMinutes,ExpertNotes,OverrideReason
2024-01-15,CONSTRUCTORA ABC,AGGREGATE,Av Principal 123,10.1234,-67.5678,XYZ-789,1250.5,45,Cliente frecuente,
2024-01-15,FERRETERIA XYZ,CEMENT,Calle 2 con 3,10.2345,-67.6789,ABC-456,850.0,30,Primera entrega,
2024-01-16,MATERIALES SA,STEEL,Centro Comercial,10.3456,-67.7890,XYZ-789,1500.0,60,Entrega en almacen,Mejor capacidad de carga
2024-01-16,DISTRIBUIDORA NORTE,REBAR,Zona Industrial,10.4567,-67.8901,DEF-123,980.0,35,Cliente de contado,";

        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(templateContent);
        return File(fileBytes, "text/csv", "historic_deliveries_template.csv");
    }
}
