using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Api.Persistence;

/// <summary>
/// Entity Framework Core DbContext for HeuristicLogix ERP.
/// Configured with strict Architecture.md standards:
/// - HYBRID ID ARCHITECTURE:
///   * Inventory Schema: int IDs (Category, Brand, UnitOfMeasure, Item)
///   * Core/Purchasing: Guid IDs (TaxConfiguration, Supplier, Staging tables)
///   * Bridge Entities: Guid PK with int FKs where needed (ItemUnitConversion)
/// - Primary Keys: TableName + Id convention
/// - Enums: String conversion (NEVER integers)
/// - Precision: DECIMAL(18,4) for amounts, DECIMAL(18,2) for quantities
/// - Unique Constraints: StockKeepingUnitCode and NationalTaxIdentificationNumber
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Core Schema
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    // Inventory Schema
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemUnitConversion> ItemUnitConversions => Set<ItemUnitConversion>();

    // Purchasing Schema
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ItemSupplier> ItemSuppliers => Set<ItemSupplier>();
    public DbSet<StagingPurchaseInvoice> StagingPurchaseInvoices => Set<StagingPurchaseInvoice>();
    public DbSet<StagingPurchaseInvoiceDetail> StagingPurchaseInvoiceDetails => Set<StagingPurchaseInvoiceDetail>();

    // Logistics Schema (existing)
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    public DbSet<Conduce> Conduces => Set<Conduce>();
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<ExpertHeuristicFeedback> ExpertFeedbacks => Set<ExpertHeuristicFeedback>();
    public DbSet<DeliveryRoute> DeliveryRoutes => Set<DeliveryRoute>();
    public DbSet<MaterialItem> MaterialItems => Set<MaterialItem>();
    public DbSet<ProductTaxonomy> ProductTaxonomies => Set<ProductTaxonomy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // IGNORE ABSTRACT BASE CLASSES (Domain Events)
        // ============================================================
        modelBuilder.Ignore<HeuristicLogix.Shared.Domain.BaseEvent>();
        modelBuilder.Ignore<HeuristicLogix.Shared.Domain.Entity>();
        modelBuilder.Ignore<HeuristicLogix.Shared.Domain.AggregateRoot>();

        // ============================================================
        // AUTOMATED SCHEMA MAPPING (Modular Monolith Architecture)
        // ============================================================
        // Automatically discover and apply all IEntityTypeConfiguration<T>
        // from the Inventory Module assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HeuristicLogix.Modules.Inventory.InventoryModuleExtensions).Assembly);

        // ============================================================
        // CORE SCHEMA CONFIGURATION
        // ============================================================

        // TaxConfiguration - Uses Guid ID (transactional/core entity)
        modelBuilder.Entity<TaxConfiguration>(entity =>
        {
            entity.ToTable("TaxConfigurations", "Core");
            entity.HasKey(e => e.TaxConfigurationId);

            entity.Property(e => e.TaxName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TaxPercentageRate)
                .IsRequired()
                .HasPrecision(5, 2);

            entity.Property(e => e.IsActive).IsRequired();

            entity.HasIndex(e => e.TaxName);
        });

        // NOTE: UnitOfMeasure and Category configurations are now
        // automatically loaded from HeuristicLogix.Modules.Inventory
        // via ApplyConfigurationsFromAssembly

        // ============================================================
        // INVENTORY SCHEMA CONFIGURATION
        // All Inventory entities use int IDs for legacy compatibility
        // ============================================================

        // Brand - Uses int ID
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands", "Inventory");
            entity.HasKey(e => e.BrandId);

            entity.Property(e => e.BrandName)
                .IsRequired()
                .HasMaxLength(300);

            entity.HasIndex(e => e.BrandName);
        });

        // Item - Uses int ID (references: int for inventory FKs, Guid for TaxConfigurationId)
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items", "Inventory");
            entity.HasKey(e => e.ItemId);

            entity.Property(e => e.StockKeepingUnitCode)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ItemDescription)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.BrandId); // Nullable int FK

            entity.Property(e => e.CategoryId).IsRequired(); // int FK

            entity.Property(e => e.TaxConfigurationId).IsRequired(); // Guid FK

            entity.Property(e => e.BaseUnitOfMeasureId).IsRequired(); // int FK

            entity.Property(e => e.DefaultSalesUnitOfMeasureId); // Nullable int FK

            entity.Property(e => e.CostPricePerBaseUnit)
                .IsRequired()
                .HasPrecision(18, 4);

            entity.Property(e => e.SellingPricePerBaseUnit)
                .IsRequired()
                .HasPrecision(18, 4);

            entity.Property(e => e.MinimumRequiredStockQuantity)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.CurrentStockQuantity)
                .IsRequired()
                .HasPrecision(18, 2);

            // UNIQUE constraint on SKU
            entity.HasIndex(e => e.StockKeepingUnitCode).IsUnique();

            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.TaxConfigurationId);
            entity.HasIndex(e => e.BaseUnitOfMeasureId);

            // ============================================================
            // EXPLICIT FOREIGN KEY RELATIONSHIPS
            // ============================================================

            // Brand relationship (nullable)
            entity.HasOne(e => e.Brand)
                .WithMany()
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Category relationship (required)
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // TaxConfiguration relationship (required, Guid FK)
            entity.HasOne(e => e.TaxConfiguration)
                .WithMany()
                .HasForeignKey(e => e.TaxConfigurationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // BaseUnitOfMeasure relationship (required)
            entity.HasOne(e => e.BaseUnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.BaseUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // DefaultSalesUnitOfMeasure relationship (nullable)
            entity.HasOne(e => e.DefaultSalesUnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.DefaultSalesUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        // ItemUnitConversion - Bridge entity: Guid PK, int FKs
        modelBuilder.Entity<ItemUnitConversion>(entity =>
        {
            entity.ToTable("ItemUnitConversions", "Inventory");
            entity.HasKey(e => e.ItemUnitConversionId); // Guid PK

            entity.Property(e => e.ItemId).IsRequired(); // int FK

            entity.Property(e => e.FromUnitOfMeasureId).IsRequired(); // int FK

            entity.Property(e => e.ToUnitOfMeasureId).IsRequired(); // int FK

            entity.Property(e => e.ConversionFactorQuantity)
                .IsRequired()
                .HasPrecision(18, 4);

            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => new { e.ItemId, e.FromUnitOfMeasureId, e.ToUnitOfMeasureId });

            // ============================================================
            // EXPLICIT FOREIGN KEY RELATIONSHIPS
            // ============================================================

            // Item relationship (required)
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // FromUnitOfMeasure relationship (required)
            entity.HasOne(e => e.FromUnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.FromUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // ToUnitOfMeasure relationship (required)
            entity.HasOne(e => e.ToUnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.ToUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        // ============================================================
        // PURCHASING SCHEMA CONFIGURATION
        // All Purchasing entities use Guid IDs (transactional)
        // ============================================================

        // Supplier - Uses Guid ID
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers", "Purchasing");
            entity.HasKey(e => e.SupplierId);

            entity.Property(e => e.NationalTaxIdentificationNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.SupplierBusinessName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.SupplierTradeName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.DefaultCreditDaysDuration);

            entity.Property(e => e.IsActive).IsRequired();

            // UNIQUE constraint on NationalTaxIdentificationNumber
            entity.HasIndex(e => e.NationalTaxIdentificationNumber).IsUnique();

            entity.HasIndex(e => e.SupplierBusinessName);
        });

        // ItemSupplier - Composite PK (int ItemId, Guid SupplierId)
        modelBuilder.Entity<ItemSupplier>(entity =>
        {
            entity.ToTable("ItemSuppliers", "Purchasing");
            
            // Composite primary key: int + Guid
            entity.HasKey(e => new { e.ItemId, e.SupplierId });

            entity.Property(e => e.ItemId).IsRequired(); // int FK

            entity.Property(e => e.SupplierId).IsRequired(); // Guid FK

            entity.Property(e => e.SupplierInternalPartNumber).HasMaxLength(200);

            entity.Property(e => e.LastPurchasePriceAmount)
                .HasPrecision(18, 4);

            entity.Property(e => e.LastPurchaseDateTime);

            entity.Property(e => e.IsPreferredSupplierForItem).IsRequired();

            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.SupplierId);

            // ============================================================
            // EXPLICIT FOREIGN KEY RELATIONSHIPS
            // ============================================================

            // Item relationship (required, int FK)
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Supplier relationship (required, Guid FK)
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        // StagingPurchaseInvoice - Uses Guid ID
        modelBuilder.Entity<StagingPurchaseInvoice>(entity =>
        {
            entity.ToTable("StagingPurchaseInvoices", "Purchasing");
            entity.HasKey(e => e.StagingPurchaseInvoiceId); // Guid PK

            entity.Property(e => e.SupplierId).IsRequired(); // Guid FK

            entity.Property(e => e.FiscalReceiptNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.InvoiceIssueDateTime).IsRequired();

            entity.Property(e => e.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 4);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.FiscalReceiptNumber);

            // ============================================================
            // EXPLICIT FOREIGN KEY RELATIONSHIPS
            // ============================================================

            // Supplier relationship (required, Guid FK)
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Navigation: One-to-Many with StagingPurchaseInvoiceDetail
            entity.HasMany(e => e.Details)
                .WithOne(d => d.StagingPurchaseInvoice)
                .HasForeignKey(d => d.StagingPurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        // StagingPurchaseInvoiceDetail - Uses Guid ID with int ItemId FK
        modelBuilder.Entity<StagingPurchaseInvoiceDetail>(entity =>
        {
            entity.ToTable("StagingPurchaseInvoiceDetails", "Purchasing");
            entity.HasKey(e => e.StagingPurchaseInvoiceDetailId); // Guid PK

            entity.Property(e => e.StagingPurchaseInvoiceId).IsRequired(); // Guid FK

            entity.Property(e => e.ItemId).IsRequired(); // int FK

            entity.Property(e => e.ReceivedQuantity)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.UnitPriceAmount)
                .IsRequired()
                .HasPrecision(18, 4);

            entity.HasIndex(e => e.StagingPurchaseInvoiceId);
            entity.HasIndex(e => e.ItemId);

            // ============================================================
            // EXPLICIT FOREIGN KEY RELATIONSHIPS
            // ============================================================

            // StagingPurchaseInvoice relationship (required, Guid FK)
            // Note: This is handled by the inverse navigation in StagingPurchaseInvoice

            // Item relationship (required, int FK)
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        // ============================================================
        // LOGISTICS SCHEMA CONFIGURATION (Existing entities)
        // ============================================================

        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.ToTable("OutboxEvents", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AggregateId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
            entity.Property(e => e.CorrelationId).HasMaxLength(450);
            entity.Property(e => e.MetadataJson);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<Conduce>(entity =>
        {
            entity.ToTable("Conduces", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RawAddress).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.ExpertDecisionNote).HasMaxLength(4000);

            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Truck>(entity =>
        {
            entity.ToTable("Trucks", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlateNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TruckType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.CompatibilityRules);
            entity.Property(e => e.ExpertLoadingHistory);

            entity.HasIndex(e => e.PlateNumber).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<ExpertHeuristicFeedback>(entity =>
        {
            entity.ToTable("ExpertHeuristicFeedbacks", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrimaryReasonTag)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(100);
            entity.Property(e => e.ExpertId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ExpertDisplayName).HasMaxLength(500);
            entity.Property(e => e.ExpertNotes).HasMaxLength(4000);
            entity.Property(e => e.MaterialsSnapshot);
            entity.Property(e => e.SessionId).HasMaxLength(450);
            entity.Property(e => e.OriginalDropZone).HasMaxLength(450);
            entity.Property(e => e.NewDropZone).HasMaxLength(450);

            entity.Property(e => e.SecondaryReasonTags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<OverrideReasonTag>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<OverrideReasonTag>());

            entity.HasIndex(e => e.ConduceId);
            entity.HasIndex(e => e.PrimaryReasonTag);
            entity.HasIndex(e => e.RecordedAt);
        });

        modelBuilder.Entity<DeliveryRoute>(entity =>
        {
            entity.ToTable("DeliveryRoutes", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.LastModifiedByExpertId).HasMaxLength(450);

            entity.Property(e => e.StopSequence)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>());

            entity.Property(e => e.Stops)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<RouteStop>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<RouteStop>());

            entity.HasIndex(e => e.TruckId);
            entity.HasIndex(e => e.ScheduledDate);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<MaterialItem>(entity =>
        {
            entity.ToTable("MaterialItems", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SpecialHandlingNotes).HasMaxLength(2000);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
        });

        modelBuilder.Entity<ProductTaxonomy>(entity =>
        {
            entity.ToTable("ProductTaxonomies", "Logistics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WeightFactor).HasPrecision(18, 4);
            entity.Property(e => e.StandardUnit).HasMaxLength(50);
            entity.Property(e => e.IsVerifiedByExpert).IsRequired();
            entity.Property(e => e.UsageCount).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.VerifiedBy).HasMaxLength(450);

            entity.HasIndex(e => e.Description).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsVerifiedByExpert);
            entity.HasIndex(e => e.UsageCount);
        });
    }
}

