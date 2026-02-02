//using HeuristicLogix.Modules.Logistics.Services;
//using HeuristicLogix.Shared.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;

//namespace HeuristicLogix.Api.Controllers;

///// <summary>
///// API controller for operational Excel ingestion.
///// Handles bulk Conduce creation from Excel files.
///// </summary>
//[ApiController]
//[Route("api/logistics/[controller]")]
//public class ExcelController : ControllerBase
//{
//    private readonly IExcelIngestionService _ingestionService;
//    private readonly ILogger<ExcelController> _logger;

//    public ExcelController(
//        IExcelIngestionService ingestionService,
//        ILogger<ExcelController> logger)
//    {
//        _ingestionService = ingestionService;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Uploads and processes an Excel file for operational Conduce creation.
//    /// Supported formats: Excel (.xlsx), CSV (.csv)
//    /// </summary>
//    /// <param name="file">The Excel file to upload.</param>
//    /// <param name="cancellationToken">Cancellation token.</param>
//    /// <returns>Processing report with statistics and errors.</returns>
//    [HttpPost("upload")]
//    [Consumes("multipart/form-data")]
//    [ProducesResponseType(typeof(ProcessingReport), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> UploadExcel(
//        IFormFile file,
//        CancellationToken cancellationToken)
//    {
//        // Validate file
//        if (file == null || file.Length == 0)
//        {
//            _logger.LogWarning("Upload failed: No file provided");
//            return BadRequest(new { error = "No se subió ningún archivo" });
//        }

//        // Validate file extension
//        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
//        if (fileExtension != ".xlsx" && fileExtension != ".csv")
//        {
//            _logger.LogWarning("Upload failed: Invalid file type {Extension}", fileExtension);
//            return BadRequest(new { error = "Tipo de archivo inválido. Solo .xlsx y .csv son soportados." });
//        }

//        // Validate file size (max 20MB)
//        const long maxFileSize = 20 * 1024 * 1024; // 20MB
//        if (file.Length > maxFileSize)
//        {
//            _logger.LogWarning("Upload failed: File too large ({Size} bytes)", file.Length);
//            return BadRequest(new { error = $"El archivo excede el tamaño máximo de {maxFileSize / 1024 / 1024}MB" });
//        }

//        try
//        {
//            _logger.LogInformation("Starting Excel ingestion: {FileName} ({Size} bytes)",
//                file.FileName, file.Length);

//            // Get current user (or default to "System")
//            string uploadedBy = User.Identity?.Name ?? "System";

//            // Process Excel file
//            using Stream stream = file.OpenReadStream();
//            ProcessingReport report = await _ingestionService.ProcessExcelAsync(
//                stream,
//                file.FileName,
//                uploadedBy,
//                cancellationToken);

//            _logger.LogInformation(
//                "Excel ingestion completed: {ReportId}, Success: {Success}/{Total}, Errors: {Errors}, Empty: {Empty}",
//                report.ReportId, report.SuccessfulRows, report.TotalRows, report.ErrorRows, report.EmptyRows);

//            // Return appropriate status
//            if (report.IsSuccess)
//            {
//                return Ok(report);
//            }
//            else if (report.SuccessfulRows > 0)
//            {
//                // Partial success
//                return StatusCode(StatusCodes.Status207MultiStatus, report);
//            }
//            else
//            {
//                // Complete failure
//                return StatusCode(StatusCodes.Status500InternalServerError, report);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Fatal error during Excel ingestion");
//            return StatusCode(StatusCodes.Status500InternalServerError,
//                new { error = "Error interno durante el procesamiento", message = ex.Message });
//        }
//    }

//    /// <summary>
//    /// Downloads the operational Excel template with Spanish column names.
//    /// </summary>
//    /// <returns>CSV template file.</returns>
//    [HttpGet("template")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    public IActionResult DownloadTemplate()
//    {
//        string templateContent = @"ClienteNombre,ProductoDescripcion,Cantidad,UnidadMedida,Direccion,Latitud,Longitud,CamionPlaca
//CONSTRUCTORA ABC,CEMENTO PORTLAND,50,BOLSA,Av Principal 123 Caracas,10.4806,-66.9036,XYZ-789
//FERRETERIA XYZ,AGREGADO ARENA,5,M3,Calle 2 con 3 Maracay,10.2469,-67.5958,ABC-456
//MATERIALES SA,ACERO ESTRUCTURAL,1.5,TON,Centro Comercial Valencia,10.1617,-68.0032,XYZ-789
//DISTRIBUIDORA NORTE,CABILLA 12MM,200,PIEZA,Zona Industrial,10.0647,-69.3570,DEF-123";

//        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(templateContent);
//        return File(fileBytes, "text/csv", "plantilla_excel_conduces.csv");
//    }

//    /// <summary>
//    /// Gets the processing report for a specific ingestion.
//    /// </summary>
//    /// <param name="reportId">The report identifier.</param>
//    /// <returns>Processing report details.</returns>
//    [HttpGet("report/{reportId}")]
//    [ProducesResponseType(typeof(ProcessingReport), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> GetReport(Guid reportId)
//    {
//        // TODO: Store reports in database for retrieval
//        await Task.CompletedTask;
//        return NotFound(new { error = "Consulta de reportes no implementada aún" });
//    }
//}
