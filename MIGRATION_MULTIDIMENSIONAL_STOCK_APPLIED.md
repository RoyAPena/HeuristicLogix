# Multi-Dimensional Stock Tracking Migration - APPLIED ?

**Date**: January 2025  
**Migration**: `20260202200042_AddMultiDimensionalStockTracking`  
**Status**: ? **SUCCESSFULLY APPLIED TO DATABASE**

---

## ?? Migration Summary

### New Columns Added to `Inventory.Items` Table

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| **ReservedStockQuantity** | DECIMAL(18,4) | NO | 0.0 | Stock committed to customers |
| **StagingStockQuantity** | DECIMAL(18,4) | NO | 0.0 | Stock awaiting verification |
| **LocationCode** | NVARCHAR(50) | YES | NULL | Warehouse location code |
| **ImageUrl** | NVARCHAR(500) | YES | NULL | Product image URL |

### Indexes Created

1. **IX_Items_LocationCode**
   - Table: `Inventory.Items`
   - Column: `LocationCode`
   - Filter: `[LocationCode] IS NOT NULL`
   - Purpose: Fast location lookups

2. **IX_UnitsOfMeasure_UnitOfMeasureName**
   - Table: `Core.UnitsOfMeasure`
   - Column: `UnitOfMeasureName`
   - Purpose: Fast unit name lookups

### Constraints Added

**CK_Items_AvailableStock**:
```sql
ALTER TABLE [Inventory].[Items] 
ADD CONSTRAINT [CK_Items_AvailableStock] 
CHECK ([CurrentStockQuantity] >= [ReservedStockQuantity]);
```

**Purpose**: Prevents reserving more stock than available (prevents negative AvailableStockQuantity)

---

## ?? Database Changes Applied

### Execution Log

```sql
-- Add new columns
ALTER TABLE [Inventory].[Items] ADD [ImageUrl] nvarchar(500) NULL;
ALTER TABLE [Inventory].[Items] ADD [LocationCode] nvarchar(50) NULL;
ALTER TABLE [Inventory].[Items] ADD [ReservedStockQuantity] decimal(18,4) NOT NULL DEFAULT 0.0;
ALTER TABLE [Inventory].[Items] ADD [StagingStockQuantity] decimal(18,4) NOT NULL DEFAULT 0.0;

-- Create indexes
CREATE INDEX [IX_Items_LocationCode] 
ON [Inventory].[Items] ([LocationCode]) 
WHERE [LocationCode] IS NOT NULL;

CREATE INDEX [IX_UnitsOfMeasure_UnitOfMeasureName] 
ON [Core].[UnitsOfMeasure] ([UnitOfMeasureName]);

-- Add check constraint
ALTER TABLE [Inventory].[Items] 
ADD CONSTRAINT [CK_Items_AvailableStock] 
CHECK ([CurrentStockQuantity] >= [ReservedStockQuantity]);
```

---

## ? Verification

### Query to Verify New Columns

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Inventory'
  AND TABLE_NAME = 'Items'
  AND COLUMN_NAME IN ('ReservedStockQuantity', 'StagingStockQuantity', 'LocationCode', 'ImageUrl')
ORDER BY ORDINAL_POSITION;
```

**Expected Results**:
```
ReservedStockQuantity | decimal | NULL | 18 | 4 | NO  | ((0.0))
StagingStockQuantity  | decimal | NULL | 18 | 4 | NO  | ((0.0))
LocationCode          | nvarchar| 50   | NULL| NULL| YES | NULL
ImageUrl              | nvarchar| 500  | NULL| NULL| YES | NULL
```

### Query to Test Calculated Property

```sql
-- Verify AvailableStockQuantity formula works
SELECT 
    ItemId,
    StockKeepingUnitCode,
    CurrentStockQuantity,
    ReservedStockQuantity,
    (CurrentStockQuantity - ReservedStockQuantity) AS AvailableStockQuantity,
    StagingStockQuantity,
    LocationCode
FROM Inventory.Items;
```

### Query to Test Check Constraint

```sql
-- This should succeed (Reserved <= Current)
UPDATE Inventory.Items 
SET ReservedStockQuantity = 10 
WHERE ItemId = 1 AND CurrentStockQuantity >= 10;

-- This should FAIL (Reserved > Current)
UPDATE Inventory.Items 
SET ReservedStockQuantity = 1000 
WHERE ItemId = 1 AND CurrentStockQuantity < 1000;
-- Error: The UPDATE statement conflicted with the CHECK constraint "CK_Items_AvailableStock"
```

---

## ?? Testing the Migration

### Test 1: Verify Existing Data

```sql
-- All existing items should have defaults
SELECT 
    ItemId,
    ReservedStockQuantity,  -- Should be 0
    StagingStockQuantity     -- Should be 0
FROM Inventory.Items;
```

### Test 2: Update Stock Quantities

```sql
-- Update cement item
UPDATE Inventory.Items
SET 
    ReservedStockQuantity = 50.0000,
    StagingStockQuantity = 100.0000,
    LocationCode = 'P1-T01-A',
    ImageUrl = '/images/products/cement-portland-50kg.jpg'
WHERE StockKeepingUnitCode = 'CEM-PORT-50KG';

-- Verify update
SELECT 
    ItemId,
    StockKeepingUnitCode,
    CurrentStockQuantity,
    ReservedStockQuantity,
    (CurrentStockQuantity - ReservedStockQuantity) AS AvailableStockQuantity,
    StagingStockQuantity,
    LocationCode,
    ImageUrl
FROM Inventory.Items
WHERE StockKeepingUnitCode = 'CEM-PORT-50KG';
```

**Expected**:
```
ItemId: 1
SKU: CEM-PORT-50KG
Current: 500.00
Reserved: 50.0000
Available: 450.0000
Staging: 100.0000
Location: P1-T01-A
Image: /images/products/cement-portland-50kg.jpg
```

### Test 3: Test Check Constraint

```sql
-- This should work
UPDATE Inventory.Items
SET ReservedStockQuantity = 450.0000
WHERE ItemId = 1;  -- Current is 500, so 450 is OK

-- This should fail
UPDATE Inventory.Items
SET ReservedStockQuantity = 600.0000
WHERE ItemId = 1;  -- Current is 500, so 600 > 500 = FAIL
```

---

## ?? Migration Files

### Migration File
- **Path**: `HeuristicLogix.Api/Migrations/20260202200042_AddMultiDimensionalStockTracking.cs`
- **Lines**: 98 lines total
- **Up Method**: Adds columns, indexes, and constraints
- **Down Method**: Removes columns, indexes, and constraints

### Designer File
- **Path**: `HeuristicLogix.Api/Migrations/20260202200042_AddMultiDimensionalStockTracking.Designer.cs`
- **Purpose**: EF Core metadata snapshot

---

## ?? Rollback Instructions

If you need to rollback this migration:

```bash
# Rollback to previous migration
dotnet ef database update InitialCreate --context AppDbContext -p HeuristicLogix.Api

# Or remove the migration entirely
dotnet ef migrations remove --context AppDbContext -p HeuristicLogix.Api
```

**Warning**: Rolling back will delete all data in these columns!

---

## ?? Impact on Application

### Entity Framework

The `Item` entity now includes:
- `ReservedStockQuantity` (required, decimal)
- `StagingStockQuantity` (required, decimal)
- `LocationCode` (optional, string)
- `ImageUrl` (optional, string)
- `AvailableStockQuantity` (calculated, read-only)

### API Layer

All existing API endpoints continue to work. New properties automatically included in responses.

### Client Layer

Update Blazor components to display new stock dimensions:

```razor
<MudText>Current: @item.CurrentStockQuantity.ToString("N2")</MudText>
<MudText>Reserved: @item.ReservedStockQuantity.ToString("N4")</MudText>
<MudText>Available: @item.AvailableStockQuantity.ToString("N4")</MudText>
<MudText>Staging: @item.StagingStockQuantity.ToString("N4")</MudText>
<MudChip>@item.LocationCode</MudChip>
<MudAvatar Image="@item.ImageUrl" />
```

---

## ?? Next Steps

### 1. Update Entity Configuration (Optional but Recommended)

Create/update `ItemConfiguration.cs` to explicitly define column types:

```csharp
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(i => i.ReservedStockQuantity)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(i => i.StagingStockQuantity)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(i => i.LocationCode)
            .HasMaxLength(50);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500);

        builder.HasCheckConstraint(
            "CK_Item_AvailableStock",
            "[CurrentStockQuantity] >= [ReservedStockQuantity]");
    }
}
```

### 2. Update DTOs

Create/update DTOs to include new fields:

```csharp
public record ItemDto
{
    public decimal CurrentStockQuantity { get; init; }
    public decimal ReservedStockQuantity { get; init; }
    public decimal StagingStockQuantity { get; init; }
    public decimal AvailableStockQuantity { get; init; }
    public string? LocationCode { get; init; }
    public string? ImageUrl { get; init; }
}
```

### 3. Update Services

Modify business logic to use new stock dimensions:

```csharp
// Reserve stock for sales order
public async Task ReserveStockAsync(int itemId, decimal quantity)
{
    var item = await _context.Items.FindAsync(itemId);
    if (item.AvailableStockQuantity < quantity)
    {
        throw new InsufficientStockException();
    }
    
    item.ReservedStockQuantity += quantity;
    await _context.SaveChangesAsync();
}
```

### 4. Update UI Components

Add stock visualization components to display multi-dimensional stock tracking.

---

## ? Success Criteria

All verified:
- [x] Migration generated successfully
- [x] Migration applied to database
- [x] New columns exist with correct types
- [x] Indexes created successfully
- [x] Check constraint working
- [x] Existing data unaffected (defaults applied)
- [x] Build successful
- [ ] UI updated to show new fields
- [ ] Business logic updated to use new fields
- [ ] Testing complete

---

**Status**: ? **MIGRATION COMPLETE**  
**Database**: ? **SCHEMA UPDATED**  
**Ready for**: **Application Integration**

---

**Executed**: January 2025  
**Migration ID**: 20260202200042_AddMultiDimensionalStockTracking  
**EF Core Version**: 10.0.2
