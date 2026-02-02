using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeuristicLogix.Modules.Inventory.Persistence;

/// <summary>
/// Entity configuration for UnitOfMeasure.
/// Defines schema, table name, primary key, and constraints.
/// </summary>
public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        // Table configuration
        builder.ToTable("UnitsOfMeasure", "Core");

        // Primary Key (int with IDENTITY)
        builder.HasKey(u => u.UnitOfMeasureId);
        builder.Property(u => u.UnitOfMeasureId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        // Properties
        builder.Property(u => u.UnitOfMeasureName)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder.Property(u => u.UnitOfMeasureSymbol)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(true);

        // Unique Constraints
        builder.HasIndex(u => u.UnitOfMeasureSymbol)
            .IsUnique()
            .HasName("IX_UnitsOfMeasure_UnitOfMeasureSymbol");

        // Index for name searches
        builder.HasIndex(u => u.UnitOfMeasureName)
            .HasName("IX_UnitsOfMeasure_UnitOfMeasureName");
    }
}

