using ClosedXML.Excel;
using HeuristicLogix.Modules.Logistics.DTOs;
using HeuristicLogix.Modules.Logistics.Repositories;
using HeuristicLogix.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HeuristicLogix.Modules.Logistics.Services;

/// <summary>
/// Service for ingesting invoices from Excel files.
/// Handles material classification, weight calculation, and load analysis.
/// </summary>
public interface IInvoiceExcelService
{
    /// <summary>
    /// Reads an Excel file and generates invoice load summaries for review.
    /// Does NOT save to database - returns summaries for approval.
    /// </summary>
    /// <param name="fileStream">Excel file stream.</param>
    /// <param name="fileName">Original file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of invoice load summaries awaiting approval.</returns>
    Task<List<InvoiceLoadSummary>> AnalyzeExcelAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves and saves an invoice to the database.
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to approve.</param>
    /// <param name="approvedBy">Who approved it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created conduce.</returns>
    Task<Conduce> ApproveAndSaveInvoiceAsync(
        string invoiceNumber,
        string? approvedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves multiple approved invoices in batch.
    /// </summary>
    /// <param name="invoiceNumbers">Invoice numbers to save.</param>
    /// <param name="approvedBy">Who approved them.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of invoices saved.</returns>
    Task<int> ApproveAndSaveBatchAsync(
        List<string> invoiceNumbers,
        string? approvedBy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of invoice Excel service with ClosedXML.
/// </summary>
public class InvoiceExcelService : IInvoiceExcelService
{
    private readonly ILogger<InvoiceExcelService> _logger;
    private readonly IMaterialClassificationService _materialClassificationService;
    private readonly IGeocodingService _geocodingService;
    private readonly IConduceRepository _conduceRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IConduceStagingService _stagingService;

    // Excel column mapping
    private const string ColInvoiceNumber = "NumeroFactura";
    private const string ColClientName = "NombreCliente";
    private const string ColAddress = "Direccion";
    private const string ColDeliveryDate = "FechaEntrega";
    private const string ColMaterialName = "Material";
    private const string ColQuantity = "Cantidad";
    private const string ColUnit = "Unidad";
    private const string ColWeightKg = "PesoKg";

    public InvoiceExcelService(
        ILogger<InvoiceExcelService> logger,
        IMaterialClassificationService materialClassificationService,
        IGeocodingService geocodingService,
        IConduceRepository conduceRepository,
        IDomainEventPublisher eventPublisher,
        IConduceStagingService stagingService)
    {
        _logger = logger;
        _materialClassificationService = materialClassificationService;
        _geocodingService = geocodingService;
        _conduceRepository = conduceRepository;
        _eventPublisher = eventPublisher;
        _stagingService = stagingService;
    }

    public async Task<List<InvoiceLoadSummary>> AnalyzeExcelAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Excel analysis for file: {FileName}", fileName);

        List<InvoiceLoadSummary> summaries = new List<InvoiceLoadSummary>();

        try
        {
            using XLWorkbook workbook = new XLWorkbook(fileStream);
            IXLWorksheet worksheet = workbook.Worksheet(1);

            // Find header row
            IXLRow headerRow = worksheet.Row(1);
            Dictionary<string, int> columnMapping = MapColumns(headerRow);

            // Validate required columns exist
            List<string> missingColumns = ValidateColumns(columnMapping);
            if (missingColumns.Any())
            {
                string error = $"Missing required columns: {string.Join(", ", missingColumns)}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            // Group rows by invoice number
            Dictionary<string, List<IXLRow>> invoiceGroups = GroupRowsByInvoice(worksheet, columnMapping);

            _logger.LogInformation("Found {InvoiceCount} invoices in Excel file", invoiceGroups.Count);

            // Process each invoice
            foreach (KeyValuePair<string, List<IXLRow>> group in invoiceGroups)
            {
                InvoiceLoadSummary summary = await ProcessInvoiceGroupAsync(
                    group.Key,
                    group.Value,
                    columnMapping,
                    cancellationToken);

                summaries.Add(summary);

                // Also stage for later approval
                StagedConduce staged = ConvertToStagedConduce(summary, group.Value, columnMapping);
                _stagingService.StageConduce(staged);
            }

            _logger.LogInformation(
                "Excel analysis complete: {TotalInvoices} invoices, {ValidInvoices} valid",
                summaries.Count, summaries.Count(s => s.IsValid));

            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing Excel file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Conduce> ApproveAndSaveInvoiceAsync(
        string invoiceNumber,
        string? approvedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving and saving invoice: {InvoiceNumber}", invoiceNumber);

        // Get staged conduce
        List<StagedConduce> stagedConduces = _stagingService.GetStagedConduces();
        StagedConduce? staged = stagedConduces.FirstOrDefault(
            sc => sc.InvoiceNumber?.Equals(invoiceNumber, StringComparison.OrdinalIgnoreCase) == true);

        if (staged == null)
        {
            throw new InvalidOperationException($"No staged conduce found for invoice: {invoiceNumber}");
        }

        if (!staged.IsValid)
        {
            throw new InvalidOperationException($"Cannot save invalid invoice: {invoiceNumber}. Errors: {string.Join(", ", staged.Errors)}");
        }

        // Check if already exists
        bool exists = await _conduceRepository.ExistsByInvoiceNumberAsync(invoiceNumber, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Invoice already exists: {invoiceNumber}");
        }

        // Convert to domain entity
        Conduce conduce = ConvertToConduce(staged, approvedBy);

        // Save to database
        Conduce created = await _conduceRepository.CreateAsync(conduce, cancellationToken);

        // Remove from staging
        _stagingService.RemoveStagedConduce(staged.StagingId);

        // Publish domain events (via outbox)
        List<BaseEvent> domainEvents = conduce.GetDomainEvents().ToList();
        await _eventPublisher.PublishBatchAsync(domainEvents, cancellationToken);

        _logger.LogInformation(
            "Invoice saved: {InvoiceNumber} (ID: {ConduceId}, {ItemCount} items, {TotalWeight}kg)",
            invoiceNumber, created.Id, created.Items.Count, created.TotalWeightKg);

        return created;
    }

    public async Task<int> ApproveAndSaveBatchAsync(
        List<string> invoiceNumbers,
        string? approvedBy,
        CancellationToken cancellationToken = default)
    {
        int savedCount = 0;

        foreach (string invoiceNumber in invoiceNumbers)
        {
            try
            {
                await ApproveAndSaveInvoiceAsync(invoiceNumber, approvedBy, cancellationToken);
                savedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice {InvoiceNumber}", invoiceNumber);
                // Continue with other invoices
            }
        }

        _logger.LogInformation("Batch save complete: {SavedCount}/{TotalCount} invoices", savedCount, invoiceNumbers.Count);
        return savedCount;
    }

    // Private helper methods

    private Dictionary<string, int> MapColumns(IXLRow headerRow)
    {
        Dictionary<string, int> mapping = new Dictionary<string, int>();

        foreach (IXLCell cell in headerRow.CellsUsed())
        {
            string columnName = cell.GetString().Trim();
            mapping[columnName] = cell.Address.ColumnNumber;
        }

        return mapping;
    }

    private List<string> ValidateColumns(Dictionary<string, int> columnMapping)
    {
        List<string> requiredColumns = new List<string>
        {
            ColInvoiceNumber,
            ColClientName,
            ColAddress,
            ColMaterialName,
            ColQuantity,
            ColUnit
        };

        return requiredColumns.Where(col => !columnMapping.ContainsKey(col)).ToList();
    }

    private Dictionary<string, List<IXLRow>> GroupRowsByInvoice(
        IXLWorksheet worksheet,
        Dictionary<string, int> columnMapping)
    {
        Dictionary<string, List<IXLRow>> groups = new Dictionary<string, List<IXLRow>>();
        int invoiceNumberCol = columnMapping[ColInvoiceNumber];

        foreach (IXLRow row in worksheet.RowsUsed().Skip(1)) // Skip header
        {
            string invoiceNumber = row.Cell(invoiceNumberCol).GetString().Trim();
            
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                continue;
            }

            if (!groups.ContainsKey(invoiceNumber))
            {
                groups[invoiceNumber] = new List<IXLRow>();
            }

            groups[invoiceNumber].Add(row);
        }

        return groups;
    }

    private async Task<InvoiceLoadSummary> ProcessInvoiceGroupAsync(
        string invoiceNumber,
        List<IXLRow> rows,
        Dictionary<string, int> columnMapping,
        CancellationToken cancellationToken)
    {
        // Get header info from first row
        IXLRow firstRow = rows.First();
        string clientName = firstRow.Cell(columnMapping[ColClientName]).GetString().Trim();
        string address = firstRow.Cell(columnMapping[ColAddress]).GetString().Trim();

        // Parse delivery date if present
        DateTimeOffset? deliveryDate = null;
        if (columnMapping.ContainsKey(ColDeliveryDate))
        {
            try
            {
                DateTime dateValue = firstRow.Cell(columnMapping[ColDeliveryDate]).GetDateTime();
                deliveryDate = new DateTimeOffset(dateValue);
            }
            catch
            {
                // Invalid date - ignore
            }
        }

        // Geocode address
        GeocodingResult geocodingResult = await _geocodingService.GeocodeAddressAsync(address, cancellationToken);

        // Process items
        List<InvoiceItemDetail> itemDetails = new List<InvoiceItemDetail>();
        Dictionary<MaterialType, decimal> weightByType = new Dictionary<MaterialType, decimal>();
        List<string> specialHandlingItems = new List<string>();
        List<string> validationErrors = new List<string>();
        List<string> warnings = new List<string>();

        int lineNumber = 1;
        foreach (IXLRow row in rows)
        {
            InvoiceItemDetail? itemDetail = ProcessItemRow(
                row,
                columnMapping,
                lineNumber,
                validationErrors);

            if (itemDetail != null)
            {
                itemDetails.Add(itemDetail);

                // Accumulate weight by type
                decimal weight = itemDetail.WeightKg ?? 0m;
                MaterialType type = itemDetail.MaterialType;
                
                if (!weightByType.ContainsKey(type))
                {
                    weightByType[type] = 0m;
                }
                weightByType[type] += weight;

                // Track special handling items
                if (itemDetail.Characteristics.RequiresSpecialHandling)
                {
                    specialHandlingItems.Add($"{itemDetail.MaterialName} (Line {lineNumber})");
                }
            }

            lineNumber++;
        }

        // Calculate totals
        decimal totalWeight = weightByType.Values.Sum();

        // Determine dominant material type
        MaterialType dominantType = MaterialType.General;
        if (weightByType.Any())
        {
            dominantType = weightByType.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        // Determine compatible truck types
        List<TruckType> compatibleTruckTypes = DetermineCompatibleTruckTypes(itemDetails, totalWeight, warnings);

        // Check if load allows mixing
        bool allowsMixedLoad = itemDetails.All(i => i.Characteristics.AllowsMixedLoad);

        // Generate summary message
        string summary = GenerateSummaryMessage(
            invoiceNumber,
            clientName,
            weightByType,
            specialHandlingItems,
            compatibleTruckTypes);

        // Geocoding validation and manual review flag
        bool requiresManualReview = false;
        if (geocodingResult.Status == GeocodingStatus.Failed)
        {
            validationErrors.Add($"Geocoding failed: {geocodingResult.ErrorMessage}");
            requiresManualReview = true;
        }
        else if (geocodingResult.Status == GeocodingStatus.Ambiguous)
        {
            warnings.Add($"Address geocoding is ambiguous (type: {geocodingResult.LocationType}). Please review location.");
            requiresManualReview = true;
        }

        return new InvoiceLoadSummary
        {
            InvoiceNumber = invoiceNumber,
            ClientName = clientName,
            Address = address,
            TotalWeightKg = totalWeight,
            ItemCount = itemDetails.Count,
            WeightByType = weightByType,
            DominantMaterialType = dominantType,
            RequiresSpecialHandling = specialHandlingItems.Any(),
            SpecialHandlingItems = specialHandlingItems,
            CompatibleTruckTypes = compatibleTruckTypes,
            Summary = summary,
            Warnings = warnings,
            AllowsMixedLoad = allowsMixedLoad,
            GeocodingResult = geocodingResult,
            IsValid = validationErrors.Count == 0,
            ValidationErrors = validationErrors,
            RequiresManualReview = requiresManualReview,
            GeocodingStatus = geocodingResult.Status
        };
    }

    private InvoiceItemDetail? ProcessItemRow(
        IXLRow row,
        Dictionary<string, int> columnMapping,
        int lineNumber,
        List<string> validationErrors)
    {
        string materialName = row.Cell(columnMapping[ColMaterialName]).GetString().Trim();
        
        if (string.IsNullOrWhiteSpace(materialName))
        {
            validationErrors.Add($"Line {lineNumber}: Missing material name");
            return null;
        }

        // Parse quantity
        decimal quantity = 0m;
        try
        {
            quantity = decimal.Parse(row.Cell(columnMapping[ColQuantity]).GetString());
        }
        catch
        {
            validationErrors.Add($"Line {lineNumber}: Invalid quantity");
            return null;
        }

        string unit = row.Cell(columnMapping[ColUnit]).GetString().Trim().ToUpperInvariant();
        
        if (string.IsNullOrWhiteSpace(unit))
        {
            validationErrors.Add($"Line {lineNumber}: Missing unit");
            return null;
        }

        // Parse weight if provided
        decimal? weightKg = null;
        if (columnMapping.ContainsKey(ColWeightKg))
        {
            try
            {
                string weightStr = row.Cell(columnMapping[ColWeightKg]).GetString();
                if (!string.IsNullOrWhiteSpace(weightStr))
                {
                    weightKg = decimal.Parse(weightStr);
                }
            }
            catch
            {
                // Weight optional - ignore parse errors
            }
        }

        // Classify material
        MaterialCharacteristics characteristics = _materialClassificationService.GetMaterialCharacteristics(materialName);

        return new InvoiceItemDetail
        {
            LineNumber = lineNumber,
            MaterialName = materialName,
            Quantity = quantity,
            Unit = unit,
            WeightKg = weightKg,
            MaterialType = characteristics.Type,
            Characteristics = characteristics,
            HasTaxonomyMatch = false // TODO: Implement taxonomy matching
        };
    }

    private List<TruckType> DetermineCompatibleTruckTypes(
        List<InvoiceItemDetail> items,
        decimal totalWeight,
        List<string> warnings)
    {
        // In this hardware store, ALL trucks can carry ANY combination of materials
        // including long materials (rebar/varilla)
        List<TruckType> compatibleTypes = new List<TruckType>
        {
            TruckType.Flatbed,
            TruckType.Dump,
            TruckType.Crane
        };

        // Add warnings for heavy loads
        if (totalWeight > 5000m)
        {
            warnings.Add($"Heavy load ({totalWeight:F0}kg) - verify truck capacity before assignment");
        }

        // Check for hazardous materials (requires special truck)
        bool hasHazardous = items.Any(i => i.MaterialType == MaterialType.Hazardous);
        if (hasHazardous)
        {
            warnings.Add("Contains hazardous materials - requires certified driver and special permits");
        }

        // Check for bulk-only loads (dump truck preferred)
        bool isBulkOnly = items.All(i => i.MaterialType == MaterialType.Bulk);
        if (isBulkOnly)
        {
            warnings.Add("Bulk materials only - Dump truck strongly preferred for easy unloading");
        }

        return compatibleTypes;
    }

    private string GenerateSummaryMessage(
        string invoiceNumber,
        string clientName,
        Dictionary<MaterialType, decimal> weightByType,
        List<string> specialHandlingItems,
        List<TruckType> compatibleTruckTypes)
    {
        List<string> weightParts = new List<string>();
        
        foreach (KeyValuePair<MaterialType, decimal> kvp in weightByType.OrderByDescending(kvp => kvp.Value))
        {
            weightParts.Add($"{kvp.Value:F0}kg {kvp.Key}");
        }

        string weightSummary = string.Join(", ", weightParts);
        string truckSummary = compatibleTruckTypes.Count == 3
            ? "Compatible with All Trucks"
            : $"Compatible with: {string.Join(", ", compatibleTruckTypes)}";

        string specialHandling = specialHandlingItems.Any()
            ? $" ? Special handling required: {string.Join(", ", specialHandlingItems.Take(3))}"
            : "";

        return $"Invoice #{invoiceNumber} ({clientName}): {weightSummary}. {truckSummary}.{specialHandling}";
    }

    private StagedConduce ConvertToStagedConduce(
        InvoiceLoadSummary summary,
        List<IXLRow> rows,
        Dictionary<string, int> columnMapping)
    {
        StagedConduce staged = new StagedConduce
        {
            StagingId = Guid.NewGuid(),
            ClientName = summary.ClientName,
            Address = summary.Address,
            GeocodingResult = summary.GeocodingResult,
            InvoiceNumber = summary.InvoiceNumber,
            DataSource = "Excel",
            Errors = summary.ValidationErrors,
            Warnings = summary.Warnings
        };

        // Add items
        int lineNumber = 1;
        foreach (IXLRow row in rows)
        {
            string materialName = row.Cell(columnMapping[ColMaterialName]).GetString().Trim();
            
            if (string.IsNullOrWhiteSpace(materialName)) continue;

            decimal quantity = 0m;
            decimal.TryParse(row.Cell(columnMapping[ColQuantity]).GetString(), out quantity);
            
            string unit = row.Cell(columnMapping[ColUnit]).GetString().Trim();
            
            decimal? weightKg = null;
            if (columnMapping.ContainsKey(ColWeightKg))
            {
                decimal weight;
                if (decimal.TryParse(row.Cell(columnMapping[ColWeightKg]).GetString(), out weight))
                {
                    weightKg = weight;
                }
            }

            MaterialCharacteristics characteristics = _materialClassificationService.GetMaterialCharacteristics(materialName);

            staged.Items.Add(new StagedConduceItem
            {
                LineNumber = lineNumber,
                MaterialName = materialName,
                Quantity = quantity,
                Unit = unit,
                WeightKg = weightKg,
                MaterialType = characteristics.Type
            });

            lineNumber++;
        }

        return staged;
    }

    private Conduce ConvertToConduce(StagedConduce staged, string? approvedBy)
    {
        if (staged.GeocodingResult == null || !staged.GeocodingResult.Success)
        {
            throw new InvalidOperationException("Cannot convert staged conduce with failed geocoding");
        }

        Conduce conduce = Conduce.Create(
            clientName: staged.ClientName,
            rawAddress: staged.Address,
            latitude: staged.GeocodingResult.Latitude,
            longitude: staged.GeocodingResult.Longitude,
            createdBy: approvedBy);

        conduce.InvoiceNumber = staged.InvoiceNumber;
        conduce.DeliveryDate = staged.DeliveryDate;
        conduce.Status = ConduceStatus.Pending;

        // Convert items
        foreach (StagedConduceItem stagedItem in staged.Items)
        {
            ConduceItem item = new ConduceItem
            {
                ConduceId = conduce.Id,
                MaterialName = stagedItem.MaterialName,
                Quantity = stagedItem.Quantity,
                Unit = stagedItem.Unit,
                WeightKg = stagedItem.WeightKg,
                MaterialType = stagedItem.MaterialType,
                LineNumber = stagedItem.LineNumber,
                IsAutoClassified = true
            };

            conduce.Items.Add(item);
        }

        // Calculate totals
        conduce.TotalWeightKg = conduce.Items.Sum(i => i.WeightKg ?? 0m);
        conduce.DominantMaterialType = _materialClassificationService.GetDominantMaterialType(conduce.Items);

        return conduce;
    }
}
