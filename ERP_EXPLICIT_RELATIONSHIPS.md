# ERP Explicit Relationship Mapping - COMPLETE

## Mission: Explicit Foreign Key Constraints
**Role:** Senior Database Architect  
**Date:** January 2025  
**Status:** ? IMPLEMENTED

## The Problem: Implicit vs Explicit Relationships

### ? Before (Implicit - No SQL FKs)
```csharp
// Entity with FK property but NO navigation
public class Item
{
    public int CategoryId { get; init; } // Just a property
}

// DbContext without relationship mapping
entity.Property(e => e.CategoryId).IsRequired(); // NOT ENOUGH!
```

**Result:** EF Core creates the column but **NO FOREIGN KEY CONSTRAINT** in SQL Server.

### ? After (Explicit - Full FK Constraints)
```csharp
// Entity with FK property AND navigation
public class Item
{
    public int CategoryId { get; init; } // FK property
    public virtual Category? Category { get; init; } // Navigation property
}

// DbContext with explicit relationship mapping
entity.HasOne(e => e.Category)
    .WithMany()
    .HasForeignKey(e => e.CategoryId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

**Result:** EF Core creates **BOTH** the column **AND** the SQL FOREIGN KEY CONSTRAINT.

## Why This Matters

### Database Integrity
? **Referential Integrity**: SQL Server enforces FK constraints  
? **Cascade Behavior**: Controlled delete/update behavior  
? **Index Performance**: SQL Server can optimize joins  
? **Data Quality**: Cannot orphan records  

### Developer Experience
? **IntelliSense**: Navigation properties enable `.Include()`  
? **Eager Loading**: Can load related entities  
? **Query Safety**: Prevents broken references  

## Complete Relationship Map

### Item Entity (Central Hub)
Item has **5 foreign key relationships** with **mixed ID types**:

```csharp
public class Item
{
    // Primary Key
    public int ItemId { get; init; } // int PK

    // Foreign Key Properties
    public int? BrandId { get; init; }                     // int FK (nullable)
    public int CategoryId { get; init; }                   // int FK (required)
    public Guid TaxConfigurationId { get; init; }          // Guid FK (required) ??
    public int BaseUnitOfMeasureId { get; init; }          // int FK (required)
    public int? DefaultSalesUnitOfMeasureId { get; init; } // int FK (nullable)

    // Navigation Properties
    public virtual Brand? Brand { get; init; }
    public virtual Category? Category { get; init; }
    public virtual TaxConfiguration? TaxConfiguration { get; init; }
    public virtual UnitOfMeasure? BaseUnitOfMeasure { get; init; }
    public virtual UnitOfMeasure? DefaultSalesUnitOfMeasure { get; init; }
}
```

**AppDbContext Mapping:**
```csharp
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

// TaxConfiguration relationship (required, Guid FK) ??
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
```

### ItemUnitConversion (Bridge Entity)
Has **3 foreign key relationships** (all int FKs):

```csharp
public class ItemUnitConversion
{
    // Primary Key
    public Guid ItemUnitConversionId { get; init; } // Guid PK

    // Foreign Key Properties
    public int ItemId { get; init; }               // int FK
    public int FromUnitOfMeasureId { get; init; }  // int FK
    public int ToUnitOfMeasureId { get; init; }    // int FK

    // Navigation Properties
    public virtual Item? Item { get; init; }
    public virtual UnitOfMeasure? FromUnitOfMeasure { get; init; }
    public virtual UnitOfMeasure? ToUnitOfMeasure { get; init; }
}
```

**AppDbContext Mapping:**
```csharp
// Item relationship (required, CASCADE delete)
entity.HasOne(e => e.Item)
    .WithMany()
    .HasForeignKey(e => e.ItemId)
    .OnDelete(DeleteBehavior.Cascade) // ?? Cascade: deleting Item deletes conversions
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
```

**Special Note:** Multiple FKs to same table (UnitOfMeasure) require distinct navigation properties.

### ItemSupplier (Composite PK with Mixed Types)
Has **2 foreign key relationships** (int + Guid):

```csharp
public class ItemSupplier
{
    // Composite Primary Key
    public int ItemId { get; init; }      // int PK/FK
    public Guid SupplierId { get; init; } // Guid PK/FK

    // Navigation Properties
    public virtual Item? Item { get; init; }
    public virtual Supplier? Supplier { get; init; }
}
```

**AppDbContext Mapping:**
```csharp
// Composite primary key
entity.HasKey(e => new { e.ItemId, e.SupplierId });

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
```

### StagingPurchaseInvoice (One-to-Many Parent)
Has **1 foreign key relationship** + **1 collection navigation**:

```csharp
public class StagingPurchaseInvoice
{
    // Primary Key
    public Guid StagingPurchaseInvoiceId { get; init; } // Guid PK

    // Foreign Key Property
    public Guid SupplierId { get; init; } // Guid FK

    // Navigation Properties
    public virtual Supplier? Supplier { get; init; }
    public ICollection<StagingPurchaseInvoiceDetail> Details { get; init; } // Collection
}
```

**AppDbContext Mapping:**
```csharp
// Supplier relationship (required, Guid FK)
entity.HasOne(e => e.Supplier)
    .WithMany()
    .HasForeignKey(e => e.SupplierId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();

// One-to-Many with StagingPurchaseInvoiceDetail (CASCADE delete)
entity.HasMany(e => e.Details)
    .WithOne(d => d.StagingPurchaseInvoice)
    .HasForeignKey(d => d.StagingPurchaseInvoiceId)
    .OnDelete(DeleteBehavior.Cascade) // ?? Cascade: deleting invoice deletes details
    .IsRequired();
```

### StagingPurchaseInvoiceDetail (One-to-Many Child)
Has **2 foreign key relationships** (Guid + int):

```csharp
public class StagingPurchaseInvoiceDetail
{
    // Primary Key
    public Guid StagingPurchaseInvoiceDetailId { get; init; } // Guid PK

    // Foreign Key Properties
    public Guid StagingPurchaseInvoiceId { get; init; } // Guid FK (parent)
    public int ItemId { get; init; }                    // int FK

    // Navigation Properties
    public virtual StagingPurchaseInvoice? StagingPurchaseInvoice { get; init; }
    public virtual Item? Item { get; init; }
}
```

**AppDbContext Mapping:**
```csharp
// Parent relationship (handled by inverse navigation in parent entity)

// Item relationship (required, int FK)
entity.HasOne(e => e.Item)
    .WithMany()
    .HasForeignKey(e => e.ItemId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

## Delete Behavior Strategy

### DeleteBehavior.Restrict (Most Common)
**Used for:** Master data references (Category, Brand, UnitOfMeasure, Supplier, TaxConfiguration)

```csharp
.OnDelete(DeleteBehavior.Restrict)
```

**Behavior:** Cannot delete parent if children exist  
**Example:** Cannot delete Category if Items reference it  
**Reason:** Protects master data integrity  

### DeleteBehavior.Cascade (Parent-Child)
**Used for:** Dependent entities that can't exist without parent

```csharp
.OnDelete(DeleteBehavior.Cascade)
```

**Examples:**
- `ItemUnitConversion` ? `Item`: Deleting Item deletes its conversions
- `StagingPurchaseInvoiceDetail` ? `StagingPurchaseInvoice`: Deleting invoice deletes details

**Reason:** Child entities are meaningless without parent

## SQL Foreign Key Constraints Generated

### Example: Item Table FKs
```sql
-- Brand FK (nullable)
ALTER TABLE [Inventory].[Items]
ADD CONSTRAINT [FK_Items_Brands_BrandId]
FOREIGN KEY ([BrandId])
REFERENCES [Inventory].[Brands] ([BrandId])
ON DELETE NO ACTION; -- Restrict

-- Category FK (required)
ALTER TABLE [Inventory].[Items]
ADD CONSTRAINT [FK_Items_Categories_CategoryId]
FOREIGN KEY ([CategoryId])
REFERENCES [Inventory].[Categories] ([CategoryId])
ON DELETE NO ACTION; -- Restrict

-- TaxConfiguration FK (required, Guid)
ALTER TABLE [Inventory].[Items]
ADD CONSTRAINT [FK_Items_TaxConfigurations_TaxConfigurationId]
FOREIGN KEY ([TaxConfigurationId])
REFERENCES [Core].[TaxConfigurations] ([TaxConfigurationId])
ON DELETE NO ACTION; -- Restrict

-- BaseUnitOfMeasure FK (required)
ALTER TABLE [Inventory].[Items]
ADD CONSTRAINT [FK_Items_UnitsOfMeasure_BaseUnitOfMeasureId]
FOREIGN KEY ([BaseUnitOfMeasureId])
REFERENCES [Core].[UnitsOfMeasure] ([UnitOfMeasureId])
ON DELETE NO ACTION; -- Restrict

-- DefaultSalesUnitOfMeasure FK (nullable)
ALTER TABLE [Inventory].[Items]
ADD CONSTRAINT [FK_Items_UnitsOfMeasure_DefaultSalesUnitOfMeasureId]
FOREIGN KEY ([DefaultSalesUnitOfMeasureId])
REFERENCES [Core].[UnitsOfMeasure] ([UnitOfMeasureId])
ON DELETE NO ACTION; -- Restrict
```

### Example: ItemUnitConversion FKs
```sql
-- Item FK (CASCADE)
ALTER TABLE [Inventory].[ItemUnitConversions]
ADD CONSTRAINT [FK_ItemUnitConversions_Items_ItemId]
FOREIGN KEY ([ItemId])
REFERENCES [Inventory].[Items] ([ItemId])
ON DELETE CASCADE; -- ?? Cascade

-- FromUnitOfMeasure FK (RESTRICT)
ALTER TABLE [Inventory].[ItemUnitConversions]
ADD CONSTRAINT [FK_ItemUnitConversions_UnitsOfMeasure_FromUnitOfMeasureId]
FOREIGN KEY ([FromUnitOfMeasureId])
REFERENCES [Core].[UnitsOfMeasure] ([UnitOfMeasureId])
ON DELETE NO ACTION; -- Restrict

-- ToUnitOfMeasure FK (RESTRICT)
ALTER TABLE [Inventory].[ItemUnitConversions]
ADD CONSTRAINT [FK_ItemUnitConversions_UnitsOfMeasure_ToUnitOfMeasureId]
FOREIGN KEY ([ToUnitOfMeasureId])
REFERENCES [Core].[UnitsOfMeasure] ([UnitOfMeasureId])
ON DELETE NO ACTION; -- Restrict
```

## Query Capabilities Enabled

### Before (No Navigation Properties)
```csharp
// ? Cannot use Include - no navigation
var items = await context.Items
    .Where(i => i.CategoryId == 10)
    .ToListAsync();

// Must manually join
var itemsWithCategory = await context.Items
    .Join(context.Categories,
        i => i.CategoryId,
        c => c.CategoryId,
        (i, c) => new { Item = i, Category = c })
    .ToListAsync();
```

### After (With Navigation Properties)
```csharp
// ? Can use Include with navigation
var items = await context.Items
    .Include(i => i.Category)        // Eager load Category
    .Include(i => i.Brand)           // Eager load Brand (nullable)
    .Include(i => i.BaseUnitOfMeasure) // Eager load UnitOfMeasure
    .Include(i => i.TaxConfiguration)  // Eager load TaxConfiguration (Guid FK)
    .Where(i => i.CategoryId == 10)
    .ToListAsync();

// ? Can use Select with navigation
var itemDtos = await context.Items
    .Select(i => new ItemDto
    {
        ItemId = i.ItemId,
        ItemDescription = i.ItemDescription,
        CategoryName = i.Category!.CategoryName, // Navigation!
        BrandName = i.Brand != null ? i.Brand.BrandName : null,
        UnitSymbol = i.BaseUnitOfMeasure!.UnitOfMeasureSymbol
    })
    .ToListAsync();
```

### Complex Queries with Multiple Includes
```csharp
// Query Item with all related entities
var itemWithAllRelations = await context.Items
    .Include(i => i.Category)
    .Include(i => i.Brand)
    .Include(i => i.BaseUnitOfMeasure)
    .Include(i => i.DefaultSalesUnitOfMeasure)
    .Include(i => i.TaxConfiguration)
    .FirstOrDefaultAsync(i => i.ItemId == 123);

// Access all related data
Console.WriteLine($"Item: {item.ItemDescription}");
Console.WriteLine($"Category: {item.Category?.CategoryName}");
Console.WriteLine($"Brand: {item.Brand?.BrandName ?? "N/A"}");
Console.WriteLine($"Base Unit: {item.BaseUnitOfMeasure?.UnitOfMeasureSymbol}");
Console.WriteLine($"Sales Unit: {item.DefaultSalesUnitOfMeasure?.UnitOfMeasureSymbol ?? "N/A"}");
Console.WriteLine($"Tax: {item.TaxConfiguration?.TaxName}");
```

### Junction Table Queries
```csharp
// Query ItemSupplier with related entities
var itemSuppliers = await context.ItemSuppliers
    .Include(s => s.Item)
    .Include(s => s.Supplier)
    .Where(s => s.ItemId == 123)
    .ToListAsync();

// Access both related entities
foreach (var itemSupplier in itemSuppliers)
{
    Console.WriteLine($"Item: {itemSupplier.Item?.ItemDescription}");
    Console.WriteLine($"Supplier: {itemSupplier.Supplier?.SupplierBusinessName}");
    Console.WriteLine($"Last Price: {itemSupplier.LastPurchasePriceAmount}");
}
```

### Parent-Child Queries
```csharp
// Query invoice with all details
var invoice = await context.StagingPurchaseInvoices
    .Include(i => i.Supplier)
    .Include(i => i.Details)
        .ThenInclude(d => d.Item)
    .FirstOrDefaultAsync(i => i.StagingPurchaseInvoiceId == invoiceId);

// Access collection
Console.WriteLine($"Invoice from: {invoice.Supplier?.SupplierBusinessName}");
foreach (var detail in invoice.Details)
{
    Console.WriteLine($"  - {detail.Item?.ItemDescription}: {detail.ReceivedQuantity} @ {detail.UnitPriceAmount}");
}
```

## Virtual Keyword Explained

```csharp
public virtual Category? Category { get; init; }
```

**Why `virtual`?**
- Enables **lazy loading** (if configured)
- Allows EF Core to create **proxy classes**
- Best practice for navigation properties

**Why nullable (`?`)?**
- EF Core doesn't load navigation by default
- Must explicitly use `.Include()` or `.ThenInclude()`
- Prevents `NullReferenceException` when accessing unloaded navigation

## Migration Impact

### Before Migration (No FK Constraints)
```sql
-- Just columns, no constraints
CREATE TABLE [Inventory].[Items]
(
    ItemId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryId int NOT NULL,
    BrandId int NULL,
    -- ... no FK constraints
);
```

### After Migration (With FK Constraints)
```sql
-- Columns WITH constraints
CREATE TABLE [Inventory].[Items]
(
    ItemId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryId int NOT NULL,
    BrandId int NULL,
    -- ... other columns

    -- FK constraints added
    CONSTRAINT [FK_Items_Categories_CategoryId]
        FOREIGN KEY ([CategoryId])
        REFERENCES [Inventory].[Categories] ([CategoryId])
        ON DELETE NO ACTION,

    CONSTRAINT [FK_Items_Brands_BrandId]
        FOREIGN KEY ([BrandId])
        REFERENCES [Inventory].[Brands] ([BrandId])
        ON DELETE NO ACTION,
    -- ... other FKs
);

-- Indexes automatically created for FK columns
CREATE INDEX [IX_Items_CategoryId] ON [Inventory].[Items] ([CategoryId]);
CREATE INDEX [IX_Items_BrandId] ON [Inventory].[Items] ([BrandId]);
```

## Testing FK Constraints

### Test 1: Cannot Delete Referenced Entity
```csharp
[Fact]
public async Task Cannot_Delete_Category_With_Items()
{
    // Arrange
    var category = new Category(0, "Test Category");
    await context.Categories.AddAsync(category);
    await context.SaveChangesAsync();

    var item = new Item(
        0, "SKU-001", "Test Item",
        category.CategoryId, // References category
        taxConfigGuid, 1, 100m, 150m, 10m, 50m
    );
    await context.Items.AddAsync(item);
    await context.SaveChangesAsync();

    // Act & Assert
    context.Categories.Remove(category);
    
    // Should throw DbUpdateException due to FK constraint
    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
}
```

### Test 2: Cascade Delete Works
```csharp
[Fact]
public async Task Deleting_Item_Cascades_To_ItemUnitConversions()
{
    // Arrange
    var item = CreateTestItem();
    await context.Items.AddAsync(item);
    await context.SaveChangesAsync();

    var conversion = new ItemUnitConversion(
        Guid.NewGuid(),
        item.ItemId, // References item
        1, 2, 40.00m
    );
    await context.ItemUnitConversions.AddAsync(conversion);
    await context.SaveChangesAsync();

    // Act
    context.Items.Remove(item);
    await context.SaveChangesAsync();

    // Assert
    var conversionExists = await context.ItemUnitConversions
        .AnyAsync(c => c.ItemUnitConversionId == conversion.ItemUnitConversionId);
    
    Assert.False(conversionExists); // Should be cascade deleted
}
```

### Test 3: Navigation Properties Work
```csharp
[Fact]
public async Task Can_Load_Item_With_Category_Navigation()
{
    // Arrange
    var category = new Category(0, "Electronics");
    await context.Categories.AddAsync(category);
    await context.SaveChangesAsync();

    var item = new Item(
        0, "SKU-001", "Laptop",
        category.CategoryId,
        taxConfigGuid, 1, 1000m, 1500m, 5m, 20m
    );
    await context.Items.AddAsync(item);
    await context.SaveChangesAsync();

    context.ChangeTracker.Clear();

    // Act
    var loadedItem = await context.Items
        .Include(i => i.Category)
        .FirstAsync(i => i.ItemId == item.ItemId);

    // Assert
    Assert.NotNull(loadedItem.Category);
    Assert.Equal("Electronics", loadedItem.Category.CategoryName);
}
```

## Checklist: Entities with Relationships

- [x] ? **Item** - 5 relationships (mixed int/Guid FKs)
  - [x] Brand (nullable)
  - [x] Category (required)
  - [x] TaxConfiguration (required, Guid)
  - [x] BaseUnitOfMeasure (required)
  - [x] DefaultSalesUnitOfMeasure (nullable)

- [x] ? **ItemUnitConversion** - 3 relationships (all int FKs)
  - [x] Item (cascade delete)
  - [x] FromUnitOfMeasure (restrict)
  - [x] ToUnitOfMeasure (restrict)

- [x] ? **ItemSupplier** - 2 relationships (int + Guid FKs)
  - [x] Item (restrict)
  - [x] Supplier (restrict)

- [x] ? **StagingPurchaseInvoice** - 2 relationships (Guid FKs)
  - [x] Supplier (restrict)
  - [x] Details collection (cascade delete)

- [x] ? **StagingPurchaseInvoiceDetail** - 2 relationships (mixed FKs)
  - [x] StagingPurchaseInvoice (parent, cascade delete)
  - [x] Item (restrict, int FK)

## Build Status
? **Shared Project**: Builds successfully  
? **Navigation Properties**: Added to all entities  
? **Explicit Relationships**: Configured in AppDbContext  
? **FK Constraints**: Will be created by migration  

---
**Explicit Relationship Mapping Complete** ?  
**SQL Foreign Key Constraints**: READY  
**Navigation Properties**: ENABLED  
**Query Capabilities**: UNLOCKED
