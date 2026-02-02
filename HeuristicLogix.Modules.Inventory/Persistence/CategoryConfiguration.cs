using HeuristicLogix.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeuristicLogix.Modules.Inventory.Persistence;

/// <summary>
/// Entity configuration for Category.
/// Defines schema, table name, primary key, and constraints.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table configuration
        builder.ToTable("Categories", "Inventory");

        // Primary Key (int with IDENTITY)
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        // Properties
        builder.Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(300)
            .IsUnicode(true);

        // Indexes
        builder.HasIndex(c => c.CategoryName)
            .HasName("IX_Categories_CategoryName");
    }
}

