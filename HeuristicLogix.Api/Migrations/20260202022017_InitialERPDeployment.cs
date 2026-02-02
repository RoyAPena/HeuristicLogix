using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeuristicLogix.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialERPDeployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.EnsureSchema(
                name: "Logistics");

            migrationBuilder.EnsureSchema(
                name: "Purchasing");

            migrationBuilder.EnsureSchema(
                name: "Core");

            migrationBuilder.CreateTable(
                name: "Brands",
                schema: "Inventory",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Inventory",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Conduces",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RawAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalWeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DominantMaterialType = table.Column<int>(type: "int", nullable: true),
                    AIPredictedServiceTime = table.Column<double>(type: "float", nullable: true),
                    ActualServiceTime = table.Column<double>(type: "float", nullable: true),
                    ExpertDecisionNote = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AssignedTruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conduces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRoutes",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StopSequence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stops = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstimatedDistanceKm = table.Column<double>(type: "float", nullable: false),
                    EstimatedTotalTimeMinutes = table.Column<double>(type: "float", nullable: false),
                    HeuristicEfficiencyScore = table.Column<double>(type: "float", nullable: false),
                    WasManuallyOverridden = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedByExpertId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TotalWeightKg = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpertHeuristicFeedbacks",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConduceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AISuggestedTruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpertAssignedTruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryReasonTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecondaryReasonTags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpertNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DecisionTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    AIConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    MaterialsSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpertId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExpertDisplayName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsedForTraining = table.Column<bool>(type: "bit", nullable: false),
                    TrainedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    WasDragDropAssignment = table.Column<bool>(type: "bit", nullable: false),
                    OriginalDropZone = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NewDropZone = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpertHeuristicFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialItems",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WeightPerUnit = table.Column<double>(type: "float", nullable: false),
                    RequiresSpecialHandling = table.Column<bool>(type: "bit", nullable: false),
                    SpecialHandlingNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HeuristicLoadingTime = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AggregateId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTaxonomies",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WeightFactor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StandardUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsVerifiedByExpert = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTaxonomies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "Purchasing",
                columns: table => new
                {
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalTaxIdentificationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierBusinessName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SupplierTradeName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DefaultCreditDaysDuration = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "TaxConfigurations",
                schema: "Core",
                columns: table => new
                {
                    TaxConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxPercentageRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxConfigurations", x => x.TaxConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TruckType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeuristicCapacityScore = table.Column<double>(type: "float", nullable: false),
                    ExpertAssignmentCount = table.Column<int>(type: "int", nullable: false),
                    CompatibilityRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpertLoadingHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastHeuristicUpdate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                schema: "Core",
                columns: table => new
                {
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitOfMeasureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitOfMeasureSymbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.UnitOfMeasureId);
                });

            migrationBuilder.CreateTable(
                name: "ConduceItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConduceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaterialType = table.Column<int>(type: "int", nullable: false),
                    ProductTaxonomyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAutoClassified = table.Column<bool>(type: "bit", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConduceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConduceItem_Conduces_ConduceId",
                        column: x => x.ConduceId,
                        principalSchema: "Logistics",
                        principalTable: "Conduces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConduceItem_ProductTaxonomies_ProductTaxonomyId",
                        column: x => x.ProductTaxonomyId,
                        principalSchema: "Logistics",
                        principalTable: "ProductTaxonomies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StagingPurchaseInvoices",
                schema: "Purchasing",
                columns: table => new
                {
                    StagingPurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiscalReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceIssueDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingPurchaseInvoices", x => x.StagingPurchaseInvoiceId);
                    table.ForeignKey(
                        name: "FK_StagingPurchaseInvoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "Purchasing",
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "Inventory",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockKeepingUnitCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TaxConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseUnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    DefaultSalesUnitOfMeasureId = table.Column<int>(type: "int", nullable: true),
                    CostPricePerBaseUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SellingPricePerBaseUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinimumRequiredStockQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentStockQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Brands_BrandId",
                        column: x => x.BrandId,
                        principalSchema: "Inventory",
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Inventory",
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_TaxConfigurations_TaxConfigurationId",
                        column: x => x.TaxConfigurationId,
                        principalSchema: "Core",
                        principalTable: "TaxConfigurations",
                        principalColumn: "TaxConfigurationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_UnitsOfMeasure_BaseUnitOfMeasureId",
                        column: x => x.BaseUnitOfMeasureId,
                        principalSchema: "Core",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitOfMeasureId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_UnitsOfMeasure_DefaultSalesUnitOfMeasureId",
                        column: x => x.DefaultSalesUnitOfMeasureId,
                        principalSchema: "Core",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitOfMeasureId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemSuppliers",
                schema: "Purchasing",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierInternalPartNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastPurchasePriceAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LastPurchaseDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsPreferredSupplierForItem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSuppliers", x => new { x.ItemId, x.SupplierId });
                    table.ForeignKey(
                        name: "FK_ItemSuppliers_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemSuppliers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "Purchasing",
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemUnitConversions",
                schema: "Inventory",
                columns: table => new
                {
                    ItemUnitConversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    FromUnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    ToUnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    ConversionFactorQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUnitConversions", x => x.ItemUnitConversionId);
                    table.ForeignKey(
                        name: "FK_ItemUnitConversions_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemUnitConversions_UnitsOfMeasure_FromUnitOfMeasureId",
                        column: x => x.FromUnitOfMeasureId,
                        principalSchema: "Core",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitOfMeasureId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemUnitConversions_UnitsOfMeasure_ToUnitOfMeasureId",
                        column: x => x.ToUnitOfMeasureId,
                        principalSchema: "Core",
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitOfMeasureId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StagingPurchaseInvoiceDetails",
                schema: "Purchasing",
                columns: table => new
                {
                    StagingPurchaseInvoiceDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StagingPurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingPurchaseInvoiceDetails", x => x.StagingPurchaseInvoiceDetailId);
                    table.ForeignKey(
                        name: "FK_StagingPurchaseInvoiceDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StagingPurchaseInvoiceDetails_StagingPurchaseInvoices_StagingPurchaseInvoiceId",
                        column: x => x.StagingPurchaseInvoiceId,
                        principalSchema: "Purchasing",
                        principalTable: "StagingPurchaseInvoices",
                        principalColumn: "StagingPurchaseInvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brands_BrandName",
                schema: "Inventory",
                table: "Brands",
                column: "BrandName");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryName",
                schema: "Inventory",
                table: "Categories",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_ConduceItem_ConduceId",
                table: "ConduceItem",
                column: "ConduceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConduceItem_ProductTaxonomyId",
                table: "ConduceItem",
                column: "ProductTaxonomyId");

            migrationBuilder.CreateIndex(
                name: "IX_Conduces_Status",
                schema: "Logistics",
                table: "Conduces",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_ScheduledDate",
                schema: "Logistics",
                table: "DeliveryRoutes",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_Status",
                schema: "Logistics",
                table: "DeliveryRoutes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_TruckId",
                schema: "Logistics",
                table: "DeliveryRoutes",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpertHeuristicFeedbacks_ConduceId",
                schema: "Logistics",
                table: "ExpertHeuristicFeedbacks",
                column: "ConduceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpertHeuristicFeedbacks_PrimaryReasonTag",
                schema: "Logistics",
                table: "ExpertHeuristicFeedbacks",
                column: "PrimaryReasonTag");

            migrationBuilder.CreateIndex(
                name: "IX_ExpertHeuristicFeedbacks_RecordedAt",
                schema: "Logistics",
                table: "ExpertHeuristicFeedbacks",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BaseUnitOfMeasureId",
                schema: "Inventory",
                table: "Items",
                column: "BaseUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BrandId",
                schema: "Inventory",
                table: "Items",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId",
                schema: "Inventory",
                table: "Items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DefaultSalesUnitOfMeasureId",
                schema: "Inventory",
                table: "Items",
                column: "DefaultSalesUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_StockKeepingUnitCode",
                schema: "Inventory",
                table: "Items",
                column: "StockKeepingUnitCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_TaxConfigurationId",
                schema: "Inventory",
                table: "Items",
                column: "TaxConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSuppliers_ItemId",
                schema: "Purchasing",
                table: "ItemSuppliers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSuppliers_SupplierId",
                schema: "Purchasing",
                table: "ItemSuppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitConversions_FromUnitOfMeasureId",
                schema: "Inventory",
                table: "ItemUnitConversions",
                column: "FromUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitConversions_ItemId",
                schema: "Inventory",
                table: "ItemUnitConversions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitConversions_ItemId_FromUnitOfMeasureId_ToUnitOfMeasureId",
                schema: "Inventory",
                table: "ItemUnitConversions",
                columns: new[] { "ItemId", "FromUnitOfMeasureId", "ToUnitOfMeasureId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitConversions_ToUnitOfMeasureId",
                schema: "Inventory",
                table: "ItemUnitConversions",
                column: "ToUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialItems_Category",
                schema: "Logistics",
                table: "MaterialItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialItems_Name",
                schema: "Logistics",
                table: "MaterialItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_CreatedAt",
                schema: "Logistics",
                table: "OutboxEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_Status",
                schema: "Logistics",
                table: "OutboxEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxonomies_Category",
                schema: "Logistics",
                table: "ProductTaxonomies",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxonomies_Description",
                schema: "Logistics",
                table: "ProductTaxonomies",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxonomies_IsVerifiedByExpert",
                schema: "Logistics",
                table: "ProductTaxonomies",
                column: "IsVerifiedByExpert");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxonomies_UsageCount",
                schema: "Logistics",
                table: "ProductTaxonomies",
                column: "UsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPurchaseInvoiceDetails_ItemId",
                schema: "Purchasing",
                table: "StagingPurchaseInvoiceDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPurchaseInvoiceDetails_StagingPurchaseInvoiceId",
                schema: "Purchasing",
                table: "StagingPurchaseInvoiceDetails",
                column: "StagingPurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPurchaseInvoices_FiscalReceiptNumber",
                schema: "Purchasing",
                table: "StagingPurchaseInvoices",
                column: "FiscalReceiptNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPurchaseInvoices_SupplierId",
                schema: "Purchasing",
                table: "StagingPurchaseInvoices",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_NationalTaxIdentificationNumber",
                schema: "Purchasing",
                table: "Suppliers",
                column: "NationalTaxIdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierBusinessName",
                schema: "Purchasing",
                table: "Suppliers",
                column: "SupplierBusinessName");

            migrationBuilder.CreateIndex(
                name: "IX_TaxConfigurations_TaxName",
                schema: "Core",
                table: "TaxConfigurations",
                column: "TaxName");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_IsActive",
                schema: "Logistics",
                table: "Trucks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_PlateNumber",
                schema: "Logistics",
                table: "Trucks",
                column: "PlateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_UnitOfMeasureSymbol",
                schema: "Core",
                table: "UnitsOfMeasure",
                column: "UnitOfMeasureSymbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConduceItem");

            migrationBuilder.DropTable(
                name: "DeliveryRoutes",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "ExpertHeuristicFeedbacks",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "ItemSuppliers",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "ItemUnitConversions",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MaterialItems",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "OutboxEvents",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "StagingPurchaseInvoiceDetails",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Trucks",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "Conduces",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "ProductTaxonomies",
                schema: "Logistics");

            migrationBuilder.DropTable(
                name: "Items",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StagingPurchaseInvoices",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Brands",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "TaxConfigurations",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "Purchasing");
        }
    }
}
