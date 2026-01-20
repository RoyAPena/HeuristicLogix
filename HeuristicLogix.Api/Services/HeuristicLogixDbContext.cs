using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HeuristicLogix.Api.Services;

/// <summary>
/// Entity Framework Core DbContext for HeuristicLogix.
/// Configured with string-based enum serialization per ARCHITECTURE.md standards.
/// </summary>
public class HeuristicLogixDbContext : DbContext
{
    public HeuristicLogixDbContext(DbContextOptions<HeuristicLogixDbContext> options)
        : base(options)
    {
    }

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

        // Configure OutboxEvent
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AggregateId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>() // CRITICAL: Store enum as string
                .HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
            entity.Property(e => e.CorrelationId).HasMaxLength(450);
            entity.Property(e => e.MetadataJson);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });

        // Configure Conduce
        modelBuilder.Entity<Conduce>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RawAddress).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>() // CRITICAL: Store enum as string
                .HasMaxLength(50);
            entity.Property(e => e.ExpertDecisionNote).HasMaxLength(4000);

            entity.HasIndex(e => e.Status);
        });

        // Configure Truck
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlateNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TruckType)
                .IsRequired()
                .HasConversion<string>() // CRITICAL: Store enum as string
                .HasMaxLength(50);
            entity.Property(e => e.CompatibilityRules);
            entity.Property(e => e.ExpertLoadingHistory);

            entity.HasIndex(e => e.PlateNumber).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Configure ExpertHeuristicFeedback
        modelBuilder.Entity<ExpertHeuristicFeedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrimaryReasonTag)
                .IsRequired()
                .HasConversion<string>() // CRITICAL: Store enum as string
                .HasMaxLength(100);
            entity.Property(e => e.ExpertId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ExpertDisplayName).HasMaxLength(500);
            entity.Property(e => e.ExpertNotes).HasMaxLength(4000);
            entity.Property(e => e.MaterialsSnapshot);
            entity.Property(e => e.SessionId).HasMaxLength(450);
            entity.Property(e => e.OriginalDropZone).HasMaxLength(450);
            entity.Property(e => e.NewDropZone).HasMaxLength(450);

            // Store SecondaryReasonTags as JSON string
            entity.Property(e => e.SecondaryReasonTags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<OverrideReasonTag>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<OverrideReasonTag>());

            entity.HasIndex(e => e.ConduceId);
            entity.HasIndex(e => e.PrimaryReasonTag);
            entity.HasIndex(e => e.RecordedAt);
        });

        // Configure DeliveryRoute
        modelBuilder.Entity<DeliveryRoute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>() // CRITICAL: Store enum as string
                .HasMaxLength(50);
            entity.Property(e => e.LastModifiedByExpertId).HasMaxLength(450);

            // Store StopSequence and Stops as JSON
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

        // Configure MaterialItem
        modelBuilder.Entity<MaterialItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category)
                .IsRequired()
                .HasConversion<string>() // CRITICAL: Store enum as string
                .HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SpecialHandlingNotes).HasMaxLength(2000);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
        });

        // Configure ProductTaxonomy
        modelBuilder.Entity<ProductTaxonomy>(entity =>
        {
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
