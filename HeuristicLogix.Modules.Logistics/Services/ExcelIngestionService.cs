using System.Text;
using HeuristicLogix.Shared.Domain;
using HeuristicLogix.Shared.Events;
using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;

namespace HeuristicLogix.Modules.Logistics.Services;

/// <summary>
/// Excel ingestion service for operational Conduce creation.
/// Follows SPEC_DATA_INGESTION.md with ProductTaxonomy integration.
/// Uses explicit typing (no var) and robust error handling.
/// </summary>
public interface IExcelIngestionService
{
    /// <summary>
    /// Processes an Excel file and creates Conduces with taxonomy-based weight calculation.
    /// </summary>
    /// <param name="fileStream">Excel file stream (.xlsx or .csv).</param>
    /// <param name="fileName">Original file name.</param>
    /// <param name="uploadedBy">User who uploaded the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processing report with statistics and errors.</returns>
    Task<ProcessingReport> ProcessExcelAsync(
        Stream fileStream,
        string fileName,
        string uploadedBy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of Excel ingestion service.
/// Creates operational Conduces from Excel data with taxonomy integration.
/// </summary>
public class ExcelIngestionService : IExcelIngestionService
{
    private readonly ITransactionalOutboxService _outbox;
    private readonly HeuristicLogixDbContext _dbContext;
    private readonly ILogger<ExcelIngestionService> _logger;

    public ExcelIngestionService(
        ITransactionalOutboxService outbox,
        HeuristicLogixDbContext dbContext,
        ILogger<ExcelIngestionService> logger)
    {
        _outbox = outbox;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ProcessingReport> ProcessExcelAsync(
        Stream fileStream,
        string fileName,
        string uploadedBy,
        CancellationToken cancellationToken = default)
    {
        ProcessingReport report = new ProcessingReport
        {
            FileName = fileName
        };

        try
        {
            _logger.LogInformation("Starting Excel ingestion for file: {FileName}", fileName);

            // Parse Excel using MiniExcel streaming
            List<ExcelRow> rows = ParseExcelFile(fileStream, report);
            report.TotalRows = rows.Count;

            _logger.LogInformation("Parsed {Count} rows from {FileName}", rows.Count, fileName);

            // Process rows in transaction
            using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (ExcelRow row in rows)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Processing cancelled by user");
                        break;
                    }

                    await ProcessRowAsync(row, uploadedBy, report, cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Excel ingestion completed. Success: {Success}, Errors: {Errors}, Empty: {Empty}",
                    report.SuccessfulRows, report.ErrorRows, report.EmptyRows);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Transaction rollback during Excel ingestion");
                throw;
            }

            report.CompletedAt = DateTimeOffset.UtcNow;
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during Excel ingestion");
            report.Errors.Add(new RowProcessingError
            {
                RowNumber = 0,
                Message = $"Fatal error: {ex.Message}"
            });
            report.CompletedAt = DateTimeOffset.UtcNow;
            return report;
        }
    }

    private List<ExcelRow> ParseExcelFile(Stream fileStream, ProcessingReport report)
    {
        List<ExcelRow> rows = new List<ExcelRow>();

        try
        {
            // MiniExcel dynamic streaming for performance
            IEnumerable<dynamic> excelRows = MiniExcel.Query(fileStream, useHeaderRow: true);

            int rowNumber = 1; // Row 1 is header, start at 2
            foreach (dynamic excelRow in excelRows)
            {
                rowNumber++;

                try
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)excelRow;

                    // Skip empty rows
                    if (IsEmptyRow(dict))
                    {
                        report.EmptyRows++;
                        _logger.LogDebug("Skipping empty row {RowNumber}", rowNumber);
                        continue;
                    }

                    ExcelRow parsedRow = MapExcelRow(dict, rowNumber);
                    rows.Add(parsedRow);
                }
                catch (Exception ex)
                {
                    report.ErrorRows++;
                    report.Errors.Add(new RowProcessingError
                    {
                        RowNumber = rowNumber,
                        Message = $"Parse error: {ex.Message}",
                        RawData = excelRow?.ToString()
                    });
                    _logger.LogWarning(ex, "Error parsing row {RowNumber}", rowNumber);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading Excel file");
            report.Errors.Add(new RowProcessingError
            {
                RowNumber = 0,
                Message = $"File read error: {ex.Message}"
            });
        }

        return rows;
    }

    private bool IsEmptyRow(IDictionary<string, object> dict)
    {
        // Row is empty if all values are null or whitespace
        foreach (KeyValuePair<string, object> kvp in dict)
        {
            if (kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()))
            {
                return false;
            }
        }
        return true;
    }

    private ExcelRow MapExcelRow(IDictionary<string, object> dict, int rowNumber)
    {
        // Explicit typing - no var keyword
        string clienteNombre = CleanString(GetValue(dict, "ClienteNombre", "Cliente", "ClientName")?.ToString()
            ?? throw new ArgumentException("ClienteNombre es requerido"));

        string productoDescripcion = CleanString(GetValue(dict, "ProductoDescripcion", "Producto", "RawDescription")?.ToString()
            ?? throw new ArgumentException("ProductoDescripcion es requerido"));

        string direccion = GetValue(dict, "Direccion", "DeliveryAddress", "Address")?.ToString()
            ?? throw new ArgumentException("Direccion es requerida");

        string placa = CleanString(GetValue(dict, "CamionPlaca", "Placa", "TruckLicensePlate")?.ToString()
            ?? throw new ArgumentException("CamionPlaca es requerida"));

        // Parse cantidad with decimal.TryParse (explicit)
        decimal cantidad = 0m;
        object? cantidadValue = GetValue(dict, "Cantidad", "Quantity");
        if (cantidadValue != null)
        {
            string cantidadString = cantidadValue.ToString() ?? string.Empty;
            if (!decimal.TryParse(cantidadString, out cantidad))
            {
                throw new ArgumentException($"Cantidad inválida: {cantidadString}");
            }
        }

        // Parse unidad (optional)
        string? unidadMedida = CleanString(GetValue(dict, "UnidadMedida", "Unidad", "RawUnit")?.ToString());

        // Parse coordinates (optional)
        double? latitud = ParseNullableDouble(GetValue(dict, "Latitud", "Latitude"));
        double? longitud = ParseNullableDouble(GetValue(dict, "Longitud", "Longitude"));

        return new ExcelRow
        {
            RowNumber = rowNumber,
            ClienteNombre = clienteNombre,
            ProductoDescripcion = productoDescripcion,
            Cantidad = cantidad,
            UnidadMedida = unidadMedida,
            Direccion = direccion,
            Latitud = latitud,
            Longitud = longitud,
            Placa = placa
        };
    }

    private async Task ProcessRowAsync(
        ExcelRow row,
        string uploadedBy,
        ProcessingReport report,
        CancellationToken cancellationToken)
    {
        try
        {
            // STEP 1: TAXONOMY LOOKUP
            ProductTaxonomy? taxonomy = await GetOrCreateTaxonomyAsync(
                row.ProductoDescripcion,
                row.UnidadMedida,
                uploadedBy,
                cancellationToken);

            if (taxonomy != null && !taxonomy.IsVerifiedByExpert)
            {
                report.NewTaxonomiesCreated++;
                report.Warnings.Add(new RowProcessingWarning
                {
                    RowNumber = row.RowNumber,
                    Message = $"Taxonomía '{row.ProductoDescripcion}' creada - pendiente verificación",
                    Severity = WarningSeverity.Medium
                });
            }

            // STEP 2: WEIGHT CALCULATION
            decimal? pesoCalculado = null;
            if (taxonomy != null && taxonomy.WeightFactor > 0 && row.Cantidad > 0)
            {
                pesoCalculado = row.Cantidad * taxonomy.WeightFactor;
                _logger.LogDebug(
                    "Row {RowNumber}: Peso calculado {Peso}kg ({Cantidad} x {Factor})",
                    row.RowNumber, pesoCalculado, row.Cantidad, taxonomy.WeightFactor);
            }
            else if (row.Cantidad > 0)
            {
                report.Warnings.Add(new RowProcessingWarning
                {
                    RowNumber = row.RowNumber,
                    Message = $"No se pudo calcular peso - taxonomía sin WeightFactor",
                    Severity = WarningSeverity.High
                });
            }

            // STEP 3: CREATE CONDUCE
            Conduce conduce = Conduce.Create(
                clientName: row.ClienteNombre,
                rawAddress: row.Direccion,
                latitude: row.Latitud ?? 0,
                longitude: row.Longitud ?? 0,
                createdBy: uploadedBy);

            _dbContext.Conduces.Add(conduce);

            // STEP 4: CREATE CONDUCECREATEDEDVENT
            ConduceCreatedEvent domainEvent = new ConduceCreatedEvent
            {
                ConduceId = conduce.Id,
                ClientName = row.ClienteNombre,
                Address = row.Direccion,
                Latitude = row.Latitud ?? 0,
                Longitude = row.Longitud ?? 0,
                InitiatedBy = uploadedBy
            };

            // STEP 5: WRAP IN CLOUDEVENT
            CloudEvent<ConduceCreatedEvent> cloudEvent = CloudEventFactory.Create(
                sourceModule: "Logistics",
                eventType: "ConduceCreated",
                data: domainEvent,
                subject: row.ProductoDescripcion,
                initiatedBy: uploadedBy,
                aiTier: 1  // Tier 1: Real-time operational
            );

            // STEP 6: ADD METADATA FOR PYTHON AI
            cloudEvent.Extensions["is_historic"] = "true";  // Mark as ingested data
            cloudEvent.Extensions["producto_descripcion"] = row.ProductoDescripcion;
            cloudEvent.Extensions["cantidad"] = row.Cantidad.ToString();
            cloudEvent.Extensions["unidad_medida"] = row.UnidadMedida ?? string.Empty;
            cloudEvent.Extensions["peso_calculado"] = pesoCalculado?.ToString() ?? string.Empty;
            cloudEvent.Extensions["has_taxonomy"] = (taxonomy != null).ToString().ToLower();
            cloudEvent.Extensions["taxonomy_verified"] = (taxonomy?.IsVerifiedByExpert ?? false).ToString().ToLower();
            cloudEvent.Extensions["row_number"] = row.RowNumber.ToString();

            // STEP 7: PUBLISH TO OUTBOX (TRANSACTIONAL)
            await _outbox.AddEventAsync(
                eventType: cloudEvent.EventType,
                topic: "expert.decisions.v1",
                aggregateId: conduce.Id.ToString(),
                payload: cloudEvent,
                correlationId: report.ReportId.ToString(),
                cancellationToken: cancellationToken);

            report.SuccessfulRows++;
        }
        catch (Exception ex)
        {
            report.ErrorRows++;
            report.Errors.Add(new RowProcessingError
            {
                RowNumber = row.RowNumber,
                Message = $"Error procesando fila: {ex.Message}",
                RawData = $"{row.ClienteNombre}|{row.ProductoDescripcion}|{row.Cantidad}"
            });
            _logger.LogError(ex, "Error processing row {RowNumber}", row.RowNumber);
        }
    }

    private async Task<ProductTaxonomy?> GetOrCreateTaxonomyAsync(
        string productoDescripcion,
        string? unidadMedida,
        string createdBy,
        CancellationToken cancellationToken)
    {
        // Look up taxonomy by description + unit
        string lookupKey = $"{productoDescripcion}_{unidadMedida ?? "UNIT"}";
        
        ProductTaxonomy? existingTaxonomy = await _dbContext.ProductTaxonomies
            .FirstOrDefaultAsync(
                t => t.Description == productoDescripcion && t.StandardUnit == unidadMedida,
                cancellationToken);

        if (existingTaxonomy != null)
        {
            existingTaxonomy.IncrementUsage();
            return existingTaxonomy;
        }

        // Create new taxonomy (pending verification)
        ProductTaxonomy newTaxonomy = new ProductTaxonomy
        {
            Id = Guid.NewGuid(),
            Description = productoDescripcion,
            Category = InferCategory(productoDescripcion),
            WeightFactor = 0, // Will be set when verified
            StandardUnit = unidadMedida,
            IsVerifiedByExpert = false,
            UsageCount = 1,
            Notes = $"Auto-creada desde Excel por {createdBy}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ProductTaxonomies.Add(newTaxonomy);

        _logger.LogInformation(
            "Taxonomía creada: '{Description}' (Categoría: {Category}, Unidad: {Unit})",
            productoDescripcion, newTaxonomy.Category, unidadMedida);

        return newTaxonomy;
    }

    private string InferCategory(string productoDescripcion)
    {
        string upper = productoDescripcion.ToUpper();

        if (upper.Contains("CEMENTO") || upper.Contains("CEMENT"))
            return "CEMENT";
        if (upper.Contains("AGREGADO") || upper.Contains("ARENA") || upper.Contains("PIEDRA") ||
            upper.Contains("AGGREGATE") || upper.Contains("SAND") || upper.Contains("GRAVA"))
            return "AGGREGATE";
        if (upper.Contains("ACERO") || upper.Contains("STEEL"))
            return "STEEL";
        if (upper.Contains("CABILLA") || upper.Contains("REBAR") || upper.Contains("VARILLA"))
            return "REBAR";
        if (upper.Contains("BLOCK") || upper.Contains("BLOQUE"))
            return "BLOCKS";
        if (upper.Contains("TUBER") || upper.Contains("PIPE") || upper.Contains("TUBERIA"))
            return "PIPES";
        if (upper.Contains("MADERA") || upper.Contains("LUMBER"))
            return "LUMBER";

        return "OTHER";
    }

    /// <summary>
    /// Clean and normalize string: Trim and uppercase.
    /// Ensures "cemento" and "CEMENTO" are treated equally.
    /// </summary>
    private string CleanString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return input.Trim().ToUpper();
    }

    private object? GetValue(IDictionary<string, object> dict, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (dict.TryGetValue(key, out object? value) && value != null)
            {
                return value;
            }
        }
        return null;
    }

    private double? ParseNullableDouble(object? value)
    {
        if (value == null) return null;
        string valueString = value.ToString() ?? string.Empty;
        if (double.TryParse(valueString, out double result))
        {
            return result;
        }
        return null;
    }
}

/// <summary>
/// Represents a parsed row from the Excel file.
/// Uses explicit typing (no var).
/// </summary>
public class ExcelRow
{
    public int RowNumber { get; init; }
    public required string ClienteNombre { get; init; }
    public required string ProductoDescripcion { get; init; }
    public decimal Cantidad { get; init; }
    public string? UnidadMedida { get; init; }
    public required string Direccion { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public required string Placa { get; init; }
}
