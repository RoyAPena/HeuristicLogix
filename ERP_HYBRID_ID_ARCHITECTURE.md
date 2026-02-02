# ERP Hybrid ID Architecture - Implementation Complete

## Mission: Hybrid ID Architecture
**Role:** Senior .NET 10 Developer  
**Date:** January 2025  
**Status:** ? IMPLEMENTED

## Business Rationale

The hybrid ID architecture splits ID types to balance legacy compatibility with modern distributed system needs:

- **int IDs**: Inventory master data (legacy compatibility, efficient joins, smaller indexes)
- **Guid IDs**: Transactional/Core data (distributed systems, no collisions, microservice-ready)

## ID Type Matrix

### Inventory Schema (int IDs)
| Entity | Primary Key | Type | Reason |
|--------|-------------|------|--------|
| `Category` | `CategoryId` | `int` | Legacy inventory master data |
| `Brand` | `BrandId` | `int` | Legacy inventory master data |
| `UnitOfMeasure` | `UnitOfMeasureId` | `int` | Legacy inventory master data |
| `Item` | `ItemId` | `int` | **Core entity** - legacy compatibility |

### Core Schema (Mixed)
| Entity | Primary Key | Type | Reason |
|--------|-------------|------|--------|
| `TaxConfiguration` | `TaxConfigurationId` | `Guid` | Transactional/configuration data |
| `UnitOfMeasure` | `UnitOfMeasureId` | `int` | Inventory master (in Core schema) |

### Purchasing Schema (Guid IDs)
| Entity | Primary Key | Type | Reason |
|--------|-------------|------|--------|
| `Supplier` | `SupplierId` | `Guid` | Transactional entity |
| `StagingPurchaseInvoice` | `StagingPurchaseInvoiceId` | `Guid` | Transactional staging |
| `StagingPurchaseInvoiceDetail` | `StagingPurchaseInvoiceDetailId` | `Guid` | Transactional staging |

### Bridge/Junction Entities (Hybrid)
| Entity | Primary Key | Type | Foreign Keys | Reason |
|--------|-------------|------|--------------|--------|
| `ItemUnitConversion` | `ItemUnitConversionId` | `Guid` | `ItemId` (int) + UOM IDs (int) | Bridge: Guid PK, int FKs |
| `ItemSupplier` | Composite | `int` + `Guid` | `ItemId` (int), `SupplierId` (Guid) | Junction: mixed types |

## Foreign Key Reference Table

### Item Entity (Central Hub)
```csharp
public class Item
{
    public int ItemId { get; init; }                      // PK: int
    public int? BrandId { get; init; }                    // FK: int ? Brand
    public int CategoryId { get; init; }                  // FK: int ? Category
    public Guid TaxConfigurationId { get; init; }         // FK: Guid ? TaxConfiguration
    public int BaseUnitOfMeasureId { get; init; }         // FK: int ? UnitOfMeasure
    public int? DefaultSalesUnitOfMeasureId { get; init; } // FK: int ? UnitOfMeasure
}
```

**Key Point:** Item uses **int** for inventory references but **Guid** for `TaxConfigurationId` (core entity).

### ItemUnitConversion (Bridge Entity)
```csharp
public class ItemUnitConversion
{
    public Guid ItemUnitConversionId { get; init; }    // PK: Guid (transactional)
    public int ItemId { get; init; }                   // FK: int ? Item
    public int FromUnitOfMeasureId { get; init; }      // FK: int ? UnitOfMeasure
    public int ToUnitOfMeasureId { get; init; }        // FK: int ? UnitOfMeasure
}
```

**Key Point:** Uses **Guid** for its own PK (transactional nature) but **int** for all FKs (inventory references).

### ItemSupplier (Composite PK with Mixed Types)
```csharp
public class ItemSupplier
{
    // Composite PK: int + Guid
    public int ItemId { get; init; }       // PK/FK: int ? Item
    public Guid SupplierId { get; init; }  // PK/FK: Guid ? Supplier
}
```

**Key Point:** First composite PK in the system with **mixed types** (int + Guid).

### StagingPurchaseInvoiceDetail (Transactional with Inventory FK)
```csharp
public class StagingPurchaseInvoiceDetail
{
    public Guid StagingPurchaseInvoiceDetailId { get; init; } // PK: Guid
    public Guid StagingPurchaseInvoiceId { get; init; }       // FK: Guid ? StagingPurchaseInvoice
    public int ItemId { get; init; }                          // FK: int ? Item
}
```

**Key Point:** Guid PK (staging/transactional) but **int** FK for `ItemId` (inventory reference).

## Database Schema Visualization

```
???????????????????????????????????????????????????????????????????
?                    HYBRID ID ARCHITECTURE                       ?
???????????????????????????????????????????????????????????????????

[INVENTORY SCHEMA - int IDs]
????????????????????
?   Category       ? ? int ID
? CategoryId (int) ?
????????????????????
        ?
        ? int FK
????????????????????      ????????????????????
?   Brand          ?      ?  UnitOfMeasure   ? ? int ID
? BrandId (int)    ?      ? UnitOfMeasureId  ?
????????????????????      ????????????????????
        ?                          ? ? ?
        ? int FK                   ? ? ? int FKs
        ?                          ? ? ?
???????????????????????????????????? ? ?
?   Item (CORE ENTITY)               ? ?
? ItemId (int) ??????????????????????? ?
? BrandId (int, nullable)           ?? ?
? CategoryId (int)                  ?? ?
? TaxConfigurationId (Guid) ???????????????
? BaseUnitOfMeasureId (int) ?????????? ?  ?
? DefaultSalesUnitOfMeasureId (int) ?? ?  ?
????????????????????????????????????????  ?
        ? int FK                           ?
        ?                                  ? Guid FK
        ?                                  ?
????????????????????????????????  ????????????????????
? ItemUnitConversion (BRIDGE)  ?  ? TaxConfiguration ? ? Guid ID
? ItemUnitConversionId (Guid)  ?  ? TaxConfigId(Guid)?
? ItemId (int) ?????????????????  ????????????????????
? FromUnitOfMeasureId (int)    ?    [CORE SCHEMA]
? ToUnitOfMeasureId (int)      ?
????????????????????????????????

[PURCHASING SCHEMA - Guid IDs]
????????????????????????????????
?   Supplier                   ? ? Guid ID
? SupplierId (Guid)            ?
????????????????????????????????
        ? Guid FK
        ?
????????????????????????????????     ???????????????????
? ItemSupplier (JUNCTION)      ??????? Item            ?
? Composite PK:                ?     ? ItemId (int)    ?
?   ItemId (int) ?????????????????????                 ?
?   SupplierId (Guid)          ?       int FK          ?
????????????????????????????????                       ?
        ?                                               ?
        ? Guid FK                                       ?
????????????????????????????????                       ?
? StagingPurchaseInvoice       ?                       ?
? StagingPurchaseInvoiceId     ?                       ?
?   (Guid)                     ?                       ?
? SupplierId (Guid)            ?                       ?
????????????????????????????????                       ?
        ? Guid FK                                       ?
        ?                                               ?
????????????????????????????????                       ?
? StagingPurchaseInvoiceDetail?                       ?
? StagingPurchaseInvoiceDetailId                      ?
?   (Guid)                     ?                       ?
? StagingPurchaseInvoiceId     ?                       ?
?   (Guid)                     ?                       ?
? ItemId (int) ?????????????????????????????????????????
????????????????????????????????     int FK
```

## Migration Strategy

### From All-Guid Schema (Previous Implementation)
```sql
-- Example migration pseudocode

-- 1. Alter Inventory.Categories
ALTER TABLE [Inventory].[Categories]
    ALTER COLUMN [CategoryId] int NOT NULL;

-- 2. Alter Inventory.Brands
ALTER TABLE [Inventory].[Brands]
    ALTER COLUMN [BrandId] int NOT NULL;

-- 3. Alter Core.UnitsOfMeasure
ALTER TABLE [Core].[UnitsOfMeasure]
    ALTER COLUMN [UnitOfMeasureId] int NOT NULL;

-- 4. Alter Inventory.Items (cascade FK changes)
ALTER TABLE [Inventory].[Items]
    ALTER COLUMN [ItemId] int NOT NULL,
    ALTER COLUMN [CategoryId] int NOT NULL,
    ALTER COLUMN [BrandId] int NULL,
    ALTER COLUMN [BaseUnitOfMeasureId] int NOT NULL,
    ALTER COLUMN [DefaultSalesUnitOfMeasureId] int NULL;
    -- TaxConfigurationId remains Guid

-- 5. Alter Inventory.ItemUnitConversions
-- PK remains Guid, but FKs become int
ALTER TABLE [Inventory].[ItemUnitConversions]
    ALTER COLUMN [ItemId] int NOT NULL,
    ALTER COLUMN [FromUnitOfMeasureId] int NOT NULL,
    ALTER COLUMN [ToUnitOfMeasureId] int NOT NULL;

-- 6. Alter Purchasing.ItemSuppliers
-- Composite PK with mixed types
ALTER TABLE [Purchasing].[ItemSuppliers]
    ALTER COLUMN [ItemId] int NOT NULL;
    -- SupplierId remains Guid

-- 7. Alter Purchasing.StagingPurchaseInvoiceDetails
ALTER TABLE [Purchasing].[StagingPurchaseInvoiceDetails]
    ALTER COLUMN [ItemId] int NOT NULL;
```

## Code Examples

### Creating an Item with Mixed FK Types
```csharp
var newItem = new Item(
    itemId: 0,                                        // int (auto-increment in DB)
    stockKeepingUnitCode: "SKU-12345",
    itemDescription: "Portland Cement Type I 50kg Bag",
    categoryId: 10,                                   // int FK
    taxConfigurationId: Guid.Parse("..."),            // Guid FK
    baseUnitOfMeasureId: 5,                           // int FK
    costPricePerBaseUnit: 450.00m,
    sellingPricePerBaseUnit: 650.00m,
    minimumRequiredStockQuantity: 100,
    currentStockQuantity: 500
)
{
    BrandId = 3,                                      // int FK (nullable)
    DefaultSalesUnitOfMeasureId = 8                   // int FK (nullable)
};

await context.Items.AddAsync(newItem);
await context.SaveChangesAsync();
```

### Creating ItemSupplier with Composite PK (Mixed Types)
```csharp
var itemSupplier = new ItemSupplier(
    itemId: 123,                                      // int (inventory)
    supplierId: Guid.Parse("..."),                    // Guid (purchasing)
    supplierInternalPartNumber: "SUP-ABC-456",
    lastPurchasePriceAmount: 420.00m,
    lastPurchaseDateTime: DateTimeOffset.UtcNow,
    isPreferredSupplierForItem: true
);

await context.ItemSuppliers.AddAsync(itemSupplier);
await context.SaveChangesAsync();
```

### Querying with Mixed ID Types
```csharp
// Query Item with mixed FK joins
var items = await context.Items
    .Include(i => i.Category)        // int join
    .Include(i => i.Brand)           // int join (nullable)
    .Include(i => i.BaseUnitOfMeasure) // int join
    .Include(i => i.TaxConfiguration)  // Guid join
    .Where(i => i.CategoryId == 10)    // int comparison
    .ToListAsync();

// Query ItemSupplier with mixed composite key
var suppliers = await context.ItemSuppliers
    .Where(s => s.ItemId == 123 && s.SupplierId == supplierGuid)
    .ToListAsync();
```

## Performance Considerations

### Advantages of int IDs (Inventory)
? **Smaller Indexes**: 4 bytes vs 16 bytes per index entry  
? **Faster Joins**: Inventory entities join frequently  
? **Sequential**: Better for B-tree indexes (less fragmentation)  
? **Legacy Compatibility**: Existing systems can integrate easily  

### Advantages of Guid IDs (Transactional)
? **No Collisions**: Safe for distributed systems  
? **Microservice-Ready**: Can generate IDs client-side  
? **Merge-Safe**: No conflicts when merging data from multiple sources  
? **Security**: IDs not guessable  

### Hybrid Trade-offs
?? **Mixed Indexes**: Composite keys with int+Guid are larger  
?? **Query Complexity**: Developers must track which entities use which ID type  
? **Best of Both**: Performance for inventory, flexibility for transactions  

## Entity Framework Core Configuration

### Identity Generation

**int IDs (Inventory):**
```csharp
// EF Core auto-configures int PKs as IDENTITY(1,1)
entity.HasKey(e => e.ItemId);
// SQL: ItemId int IDENTITY(1,1) NOT NULL
```

**Guid IDs (Transactional):**
```csharp
// Application generates Guids (not database)
entity.HasKey(e => e.SupplierId);
entity.Property(e => e.SupplierId).ValueGeneratedNever();
// SQL: SupplierId uniqueidentifier NOT NULL

// OR let SQL Server generate
entity.Property(e => e.SupplierId).HasDefaultValueSql("NEWSEQUENTIALID()");
```

### Composite Key Configuration
```csharp
// Mixed-type composite PK
modelBuilder.Entity<ItemSupplier>(entity =>
{
    entity.HasKey(e => new { e.ItemId, e.SupplierId });
    // SQL: PRIMARY KEY (ItemId, SupplierId)
    // Where ItemId is int and SupplierId is uniqueidentifier
});
```

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public void Item_Should_Use_Int_Id()
{
    var item = new Item();
    Assert.IsType<int>(item.ItemId);
}

[Fact]
public void Supplier_Should_Use_Guid_Id()
{
    var supplier = new Supplier();
    Assert.IsType<Guid>(supplier.SupplierId);
}

[Fact]
public void ItemSupplier_Should_Have_Mixed_Composite_Key()
{
    var itemSupplier = new ItemSupplier(
        itemId: 1,
        supplierId: Guid.NewGuid(),
        null, null, null, false
    );
    
    Assert.IsType<int>(itemSupplier.ItemId);
    Assert.IsType<Guid>(itemSupplier.SupplierId);
}
```

### Integration Tests
```csharp
[Fact]
public async Task Can_Create_Item_With_Mixed_Foreign_Keys()
{
    // Arrange
    var category = new Category(0, "Test Category");
    var taxConfig = new TaxConfiguration(Guid.NewGuid(), "ITBIS 18%", 18.00m, true);
    var unit = new UnitOfMeasure(0, "Kilogram", "kg");
    
    await context.Categories.AddAsync(category);
    await context.TaxConfigurations.AddAsync(taxConfig);
    await context.UnitsOfMeasure.AddAsync(unit);
    await context.SaveChangesAsync();
    
    var item = new Item(
        0, "SKU-001", "Test Item",
        category.CategoryId,        // int FK
        taxConfig.TaxConfigurationId, // Guid FK
        unit.UnitOfMeasureId,       // int FK
        100m, 150m, 10m, 50m
    );
    
    // Act
    await context.Items.AddAsync(item);
    await context.SaveChangesAsync();
    
    // Assert
    Assert.True(item.ItemId > 0); // int identity assigned
}
```

## Documentation Standards

### XML Comments
All entities include XML documentation specifying ID types:

```csharp
/// <summary>
/// Primary key: ItemId per Architecture standards.
/// Uses int for legacy inventory system compatibility.
/// </summary>
public int ItemId { get; init; } = itemId;

/// <summary>
/// Foreign key to TaxConfiguration (required).
/// Uses Guid to reference Core.TaxConfiguration.
/// </summary>
public required Guid TaxConfigurationId { get; init; } = taxConfigurationId;
```

### AppDbContext Comments
Each entity configuration includes ID type documentation:

```csharp
// Item - Uses int ID (references: int for inventory FKs, Guid for TaxConfigurationId)
modelBuilder.Entity<Item>(entity =>
{
    entity.HasKey(e => e.ItemId);
    
    entity.Property(e => e.CategoryId).IsRequired(); // int FK
    entity.Property(e => e.TaxConfigurationId).IsRequired(); // Guid FK
    // ...
});
```

## Migration Checklist

- [x] ? Update `Category` to use `int` ID
- [x] ? Update `Brand` to use `int` ID
- [x] ? Update `UnitOfMeasure` to use `int` ID
- [x] ? Update `Item` to use `int` ID with mixed FK types
- [x] ? Update `ItemUnitConversion` to use `Guid` PK with `int` FKs
- [x] ? Update `ItemSupplier` composite PK to `(int, Guid)`
- [x] ? Update `StagingPurchaseInvoiceDetail` to use `int` for `ItemId` FK
- [x] ? Update `AppDbContext` Fluent API with ID type comments
- [x] ? Verify build success
- [x] ? Create comprehensive documentation

## Build Status
? **Build Successful** - All entities compile without errors  
? **Hybrid ID Architecture** - Fully implemented  
? **100% Documented** - All ID types clearly marked

---
**Implementation Complete** ?  
**Hybrid ID Architecture**: DEPLOYED  
**Legacy Compatibility**: MAINTAINED  
**Distributed Systems Ready**: ACHIEVED
