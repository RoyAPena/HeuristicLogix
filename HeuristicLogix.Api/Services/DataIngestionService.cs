//using System.Security.Cryptography;
//using System.Text;
//using HeuristicLogix.Shared.Events;
//using HeuristicLogix.Shared.Models;
//using Microsoft.EntityFrameworkCore;
//using MiniExcelLibs;

//namespace HeuristicLogix.Api.Services;

///// <summary>
///// Service for ingesting historic delivery data from Excel/CSV files.
///// Each record is published as an event to build the AI knowledge base.
///// </summary>
//public interface IDataIngestionService
//{
//    /// <summary>
//    /// Ingests historic deliveries from an uploaded file.
//    /// </summary>
//    /// <param name="fileStream">The file stream (Excel or CSV).</param>
//    /// <param name="fileName">Original file name.</param>
//    /// <param name="uploadedBy">User who uploaded the file.</param>
//    /// <param name="cancellationToken">Cancellation token.</param>
//    /// <returns>Ingestion result with statistics.</returns>
//    Task<DataIngestionResult> IngestHistoricDeliveriesAsync(
//        Stream fileStream,
//        string fileName,
//        string uploadedBy,
//        CancellationToken cancellationToken = default);
//}

///// <summary>
///// Implementation of the data ingestion service using MiniExcel for parsing.
///// </summary>
//public class DataIngestionService : IDataIngestionService
//{
//    private readonly ITransactionalOutboxService _outbox;
//    private readonly HeuristicLogixDbContext _dbContext;
//    private readonly ILogger<DataIngestionService> _logger;

//    public DataIngestionService(
//        ITransactionalOutboxService outbox,
//        HeuristicLogixDbContext dbContext,
//        ILogger<DataIngestionService> logger)
//    {
//        _outbox = outbox;
//        _dbContext = dbContext;
//        _logger = logger;
//    }

//    public async Task<DataIngestionResult> IngestHistoricDeliveriesAsync(
//        Stream fileStream,
//        string fileName,
//        string uploadedBy,
//        CancellationToken cancellationToken = default)
//    {
//        string batchId = Guid.NewGuid().ToString();
//        DataIngestionResult result = new DataIngestionResult
//        {
//            BatchId = batchId,
//            FileName = fileName
//        };

//        try
//        {
//            _logger.LogInformation("Starting ingestion of {FileName} (Batch: {BatchId})", fileName, batchId);

//            // Calculate file hash for duplicate detection
//            string fileHash = await CalculateFileHashAsync(fileStream, cancellationToken);
//            fileStream.Position = 0; // Reset stream after hashing

//            // Check if this file was already ingested
//            if (await IsFileAlreadyIngestedAsync(fileHash, cancellationToken))
//            {
//                _logger.LogWarning("File {FileName} already ingested (Hash: {FileHash})", fileName, fileHash);
//                result.Errors.Add(new DataIngestionError
//                {
//                    RowNumber = 0,
//                    Message = "This file has already been ingested. No records were processed.",
//                    Severity = "Warning"
//                });
//                result.CompletedAt = DateTimeOffset.UtcNow;
//                return result;
//            }

//            // Parse Excel/CSV file using MiniExcel
//            List<HistoricDeliveryRecord> records = ParseFile(fileStream, fileName, result);
//            result.TotalRecords = records.Count;

//            _logger.LogInformation("Parsed {Count} records from {FileName}", records.Count, fileName);

//            // Process each record in a transaction
//            using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = 
//                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

//            try
//            {
//                foreach (HistoricDeliveryRecord record in records)
//                {
//                    if (cancellationToken.IsCancellationRequested)
//                    {
//                        break;
//                    }

//                    try
//                    {
//                        // Check for duplicate record (hash-based)
//                        string recordHash = CalculateRecordHash(record);
//                        if (await IsRecordAlreadyIngestedAsync(recordHash, cancellationToken))
//                        {
//                            result.DuplicateRecords++;
//                            _logger.LogDebug("Skipping duplicate record at row {RowNumber}", record.RowNumber);
//                            continue;
//                        }

//                        // Geocode if coordinates missing
//                        double latitude = record.Latitude ?? 0;
//                        double longitude = record.Longitude ?? 0;

//                        if (latitude == 0 || longitude == 0)
//                        {
//                            _logger.LogWarning("Row {RowNumber}: Missing coordinates for {Address}. Using 0,0.", 
//                                record.RowNumber, record.DeliveryAddress);
//                            // TODO: Integrate Google Geocoding API
//                        }

//                        // TAXONOMY LOOKUP: Check if product exists in taxonomy
//                        string sanitizedDescription = SanitizeString(record.RawDescription);
//                        ProductTaxonomy? taxonomy = await LookupOrCreateTaxonomyAsync(
//                            sanitizedDescription,
//                            record.RawUnit,
//                            uploadedBy,
//                            cancellationToken
//                        );

//                        // WEIGHT CALCULATION: Calculate weight from taxonomy if available
//                        decimal? calculatedWeight = null;
//                        bool isWeightCalculated = false;
//                        decimal? finalWeight = record.TotalWeightKg;

//                        if (taxonomy != null && record.Quantity.HasValue && taxonomy.WeightFactor > 0)
//                        {
//                            // Calculate weight: Quantity * WeightFactor
//                            calculatedWeight = record.Quantity.Value * taxonomy.WeightFactor;
//                            finalWeight = calculatedWeight;
//                            isWeightCalculated = true;
                            
//                            _logger.LogInformation(
//                                "Row {RowNumber}: Calculated weight {Weight}kg ({Quantity} x {Factor}) using taxonomy {TaxonomyId}",
//                                record.RowNumber, calculatedWeight, record.Quantity.Value, taxonomy.WeightFactor, taxonomy.Id);
//                        }
//                        else if (!record.TotalWeightKg.HasValue)
//                        {
//                            _logger.LogWarning(
//                                "Row {RowNumber}: No weight provided and no taxonomy found for '{Description}'. AI will estimate.",
//                                record.RowNumber, sanitizedDescription);
//                        }

//                        // Create domain event with taxonomy metadata
//                        HistoricDeliveryIngestedEvent domainEvent = new HistoricDeliveryIngestedEvent
//                        {
//                            DeliveryDate = record.DeliveryDate,
//                            ClientName = record.ClientName,
//                            RawDescription = sanitizedDescription,
//                            Quantity = record.Quantity,
//                            RawUnit = record.RawUnit,
//                            CalculatedWeight = calculatedWeight,
//                            TotalWeightKg = finalWeight,
//                            IsWeightCalculated = isWeightCalculated,
//                            TaxonomyId = taxonomy?.Id,
//                            IsTaxonomyVerified = taxonomy?.IsVerifiedByExpert ?? false,
//                            DeliveryAddress = record.DeliveryAddress,
//                            Latitude = latitude,
//                            Longitude = longitude,
//                            TruckLicensePlate = record.TruckLicensePlate,
//                            ServiceTimeMinutes = record.ServiceTimeMinutes,
//                            ExpertNotes = record.ExpertNotes,
//                            OverrideReason = record.OverrideReason,
//                            IsHistoric = true,
//                            IngestionBatchId = batchId,
//                            InitiatedBy = uploadedBy
//                        };

//                        // Wrap in CloudEvent with RawDescription as Subject for AI categorization
//                        CloudEvent<HistoricDeliveryIngestedEvent> cloudEvent = CloudEventFactory.Create(
//                            sourceModule: "Logistics",
//                            eventType: "HistoricDeliveryIngested",
//                            data: domainEvent,
//                            subject: sanitizedDescription,  // AI categorizes by product
//                            initiatedBy: uploadedBy,
//                            aiTier: 2  // Tier 2: Gemini 2.5 Flash for pattern recognition
//                        );

//                        // Add extension attributes for batch mode and taxonomy
//                        cloudEvent.Extensions["is_historic"] = "true";
//                        cloudEvent.Extensions["ingestion_batch_id"] = batchId;
//                        cloudEvent.Extensions["row_number"] = record.RowNumber.ToString();
//                        cloudEvent.Extensions["product_description"] = sanitizedDescription;
//                        cloudEvent.Extensions["has_taxonomy"] = (taxonomy != null).ToString().ToLower();
//                        cloudEvent.Extensions["is_taxonomy_verified"] = (taxonomy?.IsVerifiedByExpert ?? false).ToString().ToLower();
//                        cloudEvent.Extensions["weight_calculated"] = isWeightCalculated.ToString().ToLower();

//                        // Publish to outbox (transactional)
//                        await _outbox.AddEventAsync(
//                            eventType: cloudEvent.EventType,
//                            topic: "historic.deliveries.v1",
//                            aggregateId: batchId,
//                            payload: cloudEvent,
//                            correlationId: batchId,
//                            cancellationToken: cancellationToken
//                        );

//                        // Track ingested record
//                        await TrackIngestedRecordAsync(batchId, recordHash, record, cancellationToken);

//                        result.ProcessedRecords++;
//                    }
//                    catch (Exception ex)
//                    {
//                        result.SkippedRecords++;
//                        result.Errors.Add(new DataIngestionError
//                        {
//                            RowNumber = record.RowNumber,
//                            Message = $"Error processing record: {ex.Message}"
//                        });
//                        _logger.LogError(ex, "Error processing row {RowNumber}", record.RowNumber);
//                    }
//                }

//                // Track ingestion batch
//                await TrackIngestionBatchAsync(batchId, fileName, fileHash, uploadedBy, result, cancellationToken);

//                await _dbContext.SaveChangesAsync(cancellationToken);
//                await transaction.CommitAsync(cancellationToken);

//                _logger.LogInformation(
//                    "Ingestion completed. Processed: {Processed}, Skipped: {Skipped}, Duplicates: {Duplicates}",
//                    result.ProcessedRecords, result.SkippedRecords, result.DuplicateRecords);
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync(cancellationToken);
//                _logger.LogError(ex, "Transaction rollback during ingestion");
//                throw;
//            }

//            result.CompletedAt = DateTimeOffset.UtcNow;
//            return result;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Fatal error during ingestion of {FileName}", fileName);
//            result.Errors.Add(new DataIngestionError
//            {
//                RowNumber = 0,
//                Message = $"Fatal error: {ex.Message}"
//            });
//            result.CompletedAt = DateTimeOffset.UtcNow;
//            return result;
//        }
//    }

//    private List<HistoricDeliveryRecord> ParseFile(Stream fileStream, string fileName, DataIngestionResult result)
//    {
//        List<HistoricDeliveryRecord> records = new List<HistoricDeliveryRecord>();

//        try
//        {
//            // MiniExcel dynamic reading
//            IEnumerable<dynamic> rows = MiniExcel.Query(fileStream, useHeaderRow: true);

//            int rowNumber = 2; // Row 1 is header
//            foreach (dynamic row in rows)
//            {
//                try
//                {
//                    HistoricDeliveryRecord record = MapRowToRecord(row, rowNumber);
//                    records.Add(record);
//                }
//                catch (Exception ex)
//                {
//                    result.Errors.Add(new DataIngestionError
//                    {
//                        RowNumber = rowNumber,
//                        Message = $"Parse error: {ex.Message}"
//                    });
//                    _logger.LogWarning(ex, "Error parsing row {RowNumber}", rowNumber);
//                }

//                rowNumber++;
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error reading file {FileName}", fileName);
//            result.Errors.Add(new DataIngestionError
//            {
//                RowNumber = 0,
//                Message = $"File read error: {ex.Message}"
//            });
//        }

//        return records;
//    }

//    private HistoricDeliveryRecord MapRowToRecord(dynamic row, int rowNumber)
//    {
//        // MiniExcel returns dynamic objects - explicit typing for each field
//        IDictionary<string, object> dict = (IDictionary<string, object>)row;

//        // Parse required fields with sanitization
//        string clientName = SanitizeString(GetValue(dict, "ClientName", "Cliente")?.ToString() 
//            ?? throw new ArgumentException("ClientName is required"));
        
//        string rawDescription = GetValue(dict, "RawDescription", "ProductDescription", "ProductoDescripcion", "Producto", "Description")?.ToString()
//            ?? throw new ArgumentException("RawDescription (or ProductDescription) is required");
        
//        string deliveryAddress = GetValue(dict, "DeliveryAddress", "Direccion")?.ToString() 
//            ?? throw new ArgumentException("DeliveryAddress is required");
        
//        string truckLicensePlate = SanitizeString(GetValue(dict, "TruckLicensePlate", "TruckPlateNumber", "CamionPlaca", "Placa")?.ToString() 
//            ?? throw new ArgumentException("TruckLicensePlate is required"));

//        // Parse quantity (optional, use decimal.TryParse for explicit typing)
//        decimal? quantity = null;
//        object? quantityValue = GetValue(dict, "Quantity", "Cantidad");
//        if (quantityValue != null)
//        {
//            string quantityString = quantityValue.ToString() ?? string.Empty;
//            if (decimal.TryParse(quantityString, out decimal parsedQuantity))
//            {
//                quantity = parsedQuantity;
//            }
//        }

//        // Parse raw unit (optional)
//        string? rawUnit = GetValue(dict, "RawUnit", "Unit", "Unidad")?.ToString()?.Trim().ToUpper();

//        // Parse total weight (optional, may be calculated from quantity)
//        decimal? totalWeightKg = null;
//        object? weightValue = GetValue(dict, "TotalWeightKg", "PesoTotal", "Weight");
//        if (weightValue != null)
//        {
//            string weightString = weightValue.ToString() ?? string.Empty;
//            if (decimal.TryParse(weightString, out decimal parsedWeight))
//            {
//                totalWeightKg = parsedWeight;
//            }
//        }

//        return new HistoricDeliveryRecord
//        {
//            RowNumber = rowNumber,
//            DeliveryDate = DateTime.Parse(GetValue(dict, "DeliveryDate", "Fecha")?.ToString() ?? throw new ArgumentException("DeliveryDate is required")),
//            ClientName = clientName,
//            RawDescription = rawDescription,
//            Quantity = quantity,
//            RawUnit = rawUnit,
//            DeliveryAddress = deliveryAddress,
//            Latitude = ParseNullableDouble(GetValue(dict, "Latitude", "Latitud")),
//            Longitude = ParseNullableDouble(GetValue(dict, "Longitude", "Longitud")),
//            TruckLicensePlate = truckLicensePlate,
//            TotalWeightKg = totalWeightKg,
//            ServiceTimeMinutes = decimal.Parse(GetValue(dict, "ServiceTimeMinutes", "TiempoServicio")?.ToString() ?? throw new ArgumentException("ServiceTimeMinutes is required")),
//            ExpertNotes = GetValue(dict, "ExpertNotes", "NotasExperto")?.ToString()?.Trim(),
//            OverrideReason = GetValue(dict, "OverrideReason", "RazonOverride")?.ToString()?.Trim()
//        };
//    }

//    /// <summary>
//    /// Sanitizes string input: Trim and uppercase for consistency.
//    /// Ensures "arena" and "ARENA" are treated equally by the AI.
//    /// </summary>
//    private string SanitizeString(string input)
//    {
//        if (string.IsNullOrWhiteSpace(input))
//        {
//            return string.Empty;
//        }

//        return input.Trim().ToUpper();
//    }

//    private object? GetValue(IDictionary<string, object> dict, params string[] keys)
//    {
//        foreach (string key in keys)
//        {
//            if (dict.TryGetValue(key, out object? value) && value != null)
//            {
//                return value;
//            }
//        }
//        return null;
//    }

//    private double? ParseNullableDouble(object? value)
//    {
//        if (value == null) return null;
//        if (double.TryParse(value.ToString(), out double result))
//        {
//            return result;
//        }
//        return null;
//    }

//    private async Task<string> CalculateFileHashAsync(Stream fileStream, CancellationToken cancellationToken)
//    {
//        using SHA256 sha256 = SHA256.Create();
//        byte[] hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
//        return Convert.ToHexString(hashBytes);
//    }

//    private string CalculateRecordHash(HistoricDeliveryRecord record)
//    {
//        string recordData = $"{record.DeliveryDate:O}|{record.ClientName}|{record.RawDescription}|{record.TruckLicensePlate}|{record.Quantity}";
//        using SHA256 sha256 = SHA256.Create();
//        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(recordData));
//        return Convert.ToHexString(hashBytes);
//    }

//    /// <summary>
//    /// Looks up product in taxonomy or creates a "Pending Verification" entry.
//    /// This is the taxonomy hook for future product catalog integration.
//    /// </summary>
//    private async Task<ProductTaxonomy?> LookupOrCreateTaxonomyAsync(
//        string sanitizedDescription,
//        string? rawUnit,
//        string createdBy,
//        CancellationToken cancellationToken)
//    {
//        // Look up existing taxonomy by description
//        ProductTaxonomy? existingTaxonomy = await _dbContext.ProductTaxonomies
//            .FirstOrDefaultAsync(t => t.Description == sanitizedDescription, cancellationToken);

//        if (existingTaxonomy != null)
//        {
//            // Increment usage count
//            existingTaxonomy.IncrementUsage();
//            _logger.LogDebug("Found existing taxonomy for '{Description}' (ID: {Id}, Usage: {Usage})",
//                sanitizedDescription, existingTaxonomy.Id, existingTaxonomy.UsageCount);
//            return existingTaxonomy;
//        }

//        // No taxonomy found - create "Pending Verification" entry
//        ProductTaxonomy newTaxonomy = new ProductTaxonomy
//        {
//            Id = Guid.NewGuid(),
//            Description = sanitizedDescription,
//            Category = InferCategoryFromDescription(sanitizedDescription),
//            WeightFactor = 0, // Will be set when verified
//            StandardUnit = rawUnit,
//            IsVerifiedByExpert = false, // Pending verification
//            UsageCount = 1,
//            Notes = $"Auto-created from historic data ingestion by {createdBy}",
//            CreatedAt = DateTimeOffset.UtcNow
//        };

//        _dbContext.ProductTaxonomies.Add(newTaxonomy);

//        _logger.LogInformation(
//            "Created pending taxonomy entry for '{Description}' (Category: {Category}, ID: {Id})",
//            sanitizedDescription, newTaxonomy.Category, newTaxonomy.Id);

//        return newTaxonomy;
//    }

//    /// <summary>
//    /// Infers product category from description (basic heuristic).
//    /// Will be improved with AI categorization in future.
//    /// </summary>
//    private string InferCategoryFromDescription(string description)
//    {
//        // Basic keyword matching - can be improved with ML
//        string upperDescription = description.ToUpper();

//        if (upperDescription.Contains("CEMENTO") || upperDescription.Contains("CEMENT"))
//            return "CEMENT";
//        if (upperDescription.Contains("AGREGADO") || upperDescription.Contains("ARENA") || 
//            upperDescription.Contains("AGGREGATE") || upperDescription.Contains("SAND") ||
//            upperDescription.Contains("GRAVEL") || upperDescription.Contains("GRAVA"))
//            return "AGGREGATE";
//        if (upperDescription.Contains("ACERO") || upperDescription.Contains("STEEL"))
//            return "STEEL";
//        if (upperDescription.Contains("CABILLA") || upperDescription.Contains("REBAR") ||
//            upperDescription.Contains("VARILLA"))
//            return "REBAR";
//        if (upperDescription.Contains("BLOCK") || upperDescription.Contains("BLOQUE"))
//            return "BLOCKS";
//        if (upperDescription.Contains("TUBER") || upperDescription.Contains("PIPE") ||
//            upperDescription.Contains("TUBERIA"))
//            return "PIPES";
//        if (upperDescription.Contains("MADERA") || upperDescription.Contains("LUMBER"))
//            return "LUMBER";

//        return "OTHER"; // Unknown category - will be categorized by AI
//    }

//    private async Task<bool> IsFileAlreadyIngestedAsync(string fileHash, CancellationToken cancellationToken)
//    {
//        // TODO: Query IngestionBatches table
//        // For now, return false (implement with EF Core)
//        await Task.CompletedTask;
//        return false;
//    }

//    private async Task<bool> IsRecordAlreadyIngestedAsync(string recordHash, CancellationToken cancellationToken)
//    {
//        // TODO: Query IngestedRecords table
//        await Task.CompletedTask;
//        return false;
//    }

//    private async Task TrackIngestedRecordAsync(
//        string batchId,
//        string recordHash,
//        HistoricDeliveryRecord record,
//        CancellationToken cancellationToken)
//    {
//        // TODO: Insert into IngestedRecords table
//        await Task.CompletedTask;
//    }

//    private async Task TrackIngestionBatchAsync(
//        string batchId,
//        string fileName,
//        string fileHash,
//        string uploadedBy,
//        DataIngestionResult result,
//        CancellationToken cancellationToken)
//    {
//        // TODO: Insert into IngestionBatches table
//        await Task.CompletedTask;
//    }
//}
