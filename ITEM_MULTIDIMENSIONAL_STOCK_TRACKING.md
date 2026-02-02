# Item Entity Multi-Dimensional Stock Tracking - COMPLETE ?

**Status**: READY FOR MIGRATION  
**Date**: January 2025  
**Entity**: `HeuristicLogix.Shared.Models.Item`  
**Feature**: Multi-dimensional stock tracking (Current, Reserved, Staging, Available)

---

## ?? Business Requirements

### Problem Statement
The original `Item` entity only tracked `CurrentStockQuantity`, which didn't distinguish between:
- **Available stock** (can be sold)
- **Reserved stock** (already committed to customers)
- **Staging stock** (arrived but not yet verified)

This caused:
- ? Overselling (selling reserved stock)
- ? Inventory discrepancies (staging vs official)
- ? Poor warehouse management (no location tracking)

### Solution
Multi-dimensional stock tracking with **four distinct stock quantities**:

1. **CurrentStockQuantity**: Physically verified stock in warehouse
2. **ReservedStockQuantity**: Committed to customers (invoiced/conduce pending)
3. **StagingStockQuantity**: Arrived but not yet posted to inventory
4. **AvailableStockQuantity**: Calculated (Current - Reserved) - what can be sold

---

## ?? New Properties

### 1. ReservedStockQuantity
```csharp
/// <summary>
/// Reserved stock quantity in the BASE unit.
/// Stock already invoiced or with a pending "Conduce" (delivery note) that hasn't left the warehouse.
/// This stock is committed to customers but not yet physically delivered.
/// Precision: DECIMAL(18,4).
/// </summary>
public required decimal ReservedStockQuantity { get; set; } = reservedStockQuantity;
```

**Use Cases**:
- Sales invoice created ? Reserve stock
- Conduce (delivery note) printed ? Keep reservation
- Goods physically leave warehouse ? Decrease CurrentStock, clear reservation

**Example**:
```
Current: 100 units
Reserved: 30 units (2 pending deliveries)
Available: 70 units (only this can be sold)
```

### 2. StagingStockQuantity
```csharp
/// <summary>
/// Staging stock quantity in the BASE unit.
/// Stock that has physically arrived (via Purchasing Staging) but hasn't been verified/posted to official inventory.
/// This represents goods in receiving/inspection that are not yet available for sale.
/// Precision: DECIMAL(18,4).
/// </summary>
public required decimal StagingStockQuantity { get; set; } = stagingStockQuantity;
```

**Use Cases**:
- Goods arrive from supplier ? Add to Staging
- Quality inspection performed ? Verify quantities
- Approved ? Post to CurrentStock, clear Staging
- Rejected ? Return to supplier, clear Staging

**Example**:
```
Staging: 50 units (arrived yesterday, awaiting inspection)
Current: 100 units (official inventory)
```

### 3. LocationCode
```csharp
/// <summary>
/// Physical warehouse location code (e.g., "P1-T10-A" for Pasillo 1, Torre 10, Anaquel A).
/// Optional field for warehouse management.
/// </summary>
public string? LocationCode { get; set; }
```

**Format Examples**:
- `"P1-T10-A"` ? Pasillo 1, Torre 10, Anaquel A
- `"Z2-R05-S3"` ? Zona 2, Rack 05, Shelf 3
- `"OUTDOOR"` ? Outdoor storage area

### 4. ImageUrl
```csharp
/// <summary>
/// URL reference for the product image.
/// Can point to internal storage, CDN, or external resource.
/// Optional field for UI display purposes.
/// </summary>
public string? ImageUrl { get; set; }
```

**URL Examples**:
- `"/images/products/cement-50kg.jpg"` (internal)
- `"https://cdn.heuristiclogix.com/products/123.jpg"` (CDN)
- `"https://supplier.com/catalog/item-photo.png"` (external)

---

## ?? Calculated Property: AvailableStockQuantity

### Formula
```csharp
/// <summary>
/// Available stock quantity for sales commitment.
/// Calculated as: CurrentStockQuantity - ReservedStockQuantity.
/// This represents the actual stock that the sales department can commit to customers.
/// READ-ONLY: Computed at runtime.
/// </summary>
public decimal AvailableStockQuantity => CurrentStockQuantity - ReservedStockQuantity;
```

### Why This Matters

**Before** ?:
```
Sales sees: 100 units "in stock"
Reality: 70 already committed to other customers
Result: Overselling, unhappy customers
```

**After** ?:
```
Current: 100 units
Reserved: 30 units
Available: 70 units ? This is what sales can commit
```

### Use Cases

**Sales Order Creation**:
```csharp
if (item.AvailableStockQuantity >= orderQuantity)
{
    // Can fulfill order
    item.ReservedStockQuantity += orderQuantity;
}
else
{
    // Insufficient stock
    throw new InsufficientStockException($"Only {item.AvailableStockQuantity} available");
}
```

**Inventory Dashboard**:
```razor
<MudText>Current: @item.CurrentStockQuantity</MudText>
<MudText>Reserved: @item.ReservedStockQuantity</MudText>
<MudText Color="Color.Success">Available: @item.AvailableStockQuantity</MudText>
<MudText Color="Color.Warning">Staging: @item.StagingStockQuantity</MudText>
```

---

## ?? Stock Movement Workflows

### Workflow 1: Purchase Goods (Staging ? Current)

**Step 1: Goods Arrive**
```csharp
item.StagingStockQuantity += 100;  // 100 units arrived
// Current: 50, Staging: 100, Reserved: 10, Available: 40
```

**Step 2: Quality Inspection Approved**
```csharp
item.CurrentStockQuantity += 100;  // Post to inventory
item.StagingStockQuantity -= 100;  // Clear staging
// Current: 150, Staging: 0, Reserved: 10, Available: 140
```

### Workflow 2: Sales Order (Current ? Reserved ? Delivered)

**Step 1: Create Sales Invoice**
```csharp
if (item.AvailableStockQuantity >= 30)
{
    item.ReservedStockQuantity += 30;  // Reserve for customer
}
// Current: 150, Reserved: 40, Available: 110
```

**Step 2: Generate Conduce (Delivery Note)**
```csharp
// Stock still reserved, conduce printed
// Current: 150, Reserved: 40, Available: 110
```

**Step 3: Goods Leave Warehouse**
```csharp
item.CurrentStockQuantity -= 30;   // Physical stock decreased
item.ReservedStockQuantity -= 30;  // Clear reservation
// Current: 120, Reserved: 10, Available: 110
```

### Workflow 3: Rejected Goods (Staging ? Return)

**Step 1: Goods Arrive**
```csharp
item.StagingStockQuantity += 50;
// Staging: 50
```

**Step 2: Quality Inspection Failed**
```csharp
item.StagingStockQuantity -= 50;  // Remove from staging
// Generate return note to supplier
// Staging: 0
```

---

## ??? Database Migration Required

### SQL Migration Script

```sql
-- Add new columns to Item table
ALTER TABLE Inventory.Item
ADD 
    ReservedStockQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
    StagingStockQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
    LocationCode NVARCHAR(50) NULL,
    ImageUrl NVARCHAR(500) NULL;

-- Add indexes for performance
CREATE INDEX IX_Item_LocationCode ON Inventory.Item(LocationCode) WHERE LocationCode IS NOT NULL;
CREATE INDEX IX_Item_AvailableStock ON Inventory.Item(CurrentStockQuantity, ReservedStockQuantity);

-- Add check constraint to prevent negative available stock
ALTER TABLE Inventory.Item
ADD CONSTRAINT CK_Item_AvailableStock CHECK (CurrentStockQuantity >= ReservedStockQuantity);
```

### EF Core Migration (Recommended)

```bash
# Generate migration
dotnet ef migrations add AddMultiDimensionalStockTracking -p HeuristicLogix.Api

# Review migration file
# Edit if needed to set default values for existing data

# Apply migration
dotnet ef database update -p HeuristicLogix.Api
```

**Migration File Preview**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<decimal>(
        name: "ReservedStockQuantity",
        schema: "Inventory",
        table: "Item",
        type: "decimal(18,4)",
        nullable: false,
        defaultValue: 0m);

    migrationBuilder.AddColumn<decimal>(
        name: "StagingStockQuantity",
        schema: "Inventory",
        table: "Item",
        type: "decimal(18,4)",
        nullable: false,
        defaultValue: 0m);

    migrationBuilder.AddColumn<string>(
        name: "LocationCode",
        schema: "Inventory",
        table: "Item",
        type: "nvarchar(50)",
        maxLength: 50,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "ImageUrl",
        schema: "Inventory",
        table: "Item",
        type: "nvarchar(500)",
        maxLength: 500,
        nullable: true);

    migrationBuilder.CreateIndex(
        name: "IX_Item_LocationCode",
        schema: "Inventory",
        table: "Item",
        column: "LocationCode",
        filter: "[LocationCode] IS NOT NULL");

    migrationBuilder.AddCheckConstraint(
        name: "CK_Item_AvailableStock",
        schema: "Inventory",
        table: "Item",
        sql: "[CurrentStockQuantity] >= [ReservedStockQuantity]");
}
```

---

## ?? Data Precision Standards

| Property | Precision | Reason |
|----------|-----------|---------|
| **CostPricePerBaseUnit** | DECIMAL(18,4) | Weighted Average Cost requires high precision |
| **SellingPricePerBaseUnit** | DECIMAL(18,4) | Pricing requires high precision |
| **CurrentStockQuantity** | DECIMAL(18,2) | Physical inventory - 2 decimals sufficient |
| **ReservedStockQuantity** | DECIMAL(18,4) | Match cost precision for valuation |
| **StagingStockQuantity** | DECIMAL(18,4) | Match cost precision for valuation |
| **MinimumRequiredStockQuantity** | DECIMAL(18,2) | Reorder alerts - 2 decimals sufficient |

**Why DECIMAL(18,4)?**
- Supports fractional units (0.5 kg, 0.25 meters)
- Maintains precision for financial calculations
- Prevents rounding errors in WAC calculations

---

## ?? Testing Scenarios

### Test 1: Calculate Available Stock
```csharp
var item = new Item
{
    CurrentStockQuantity = 100m,
    ReservedStockQuantity = 30m
};

Assert.Equal(70m, item.AvailableStockQuantity);
```

### Test 2: Prevent Overselling
```csharp
var item = new Item { CurrentStockQuantity = 100m, ReservedStockQuantity = 90m };

// Only 10 units available
if (item.AvailableStockQuantity >= 15m)
{
    // This should NOT execute
    Assert.Fail("Should not allow overselling");
}
```

### Test 3: Staging to Current Workflow
```csharp
var item = new Item 
{ 
    CurrentStockQuantity = 50m,
    StagingStockQuantity = 0m 
};

// Step 1: Goods arrive
item.StagingStockQuantity = 100m;
Assert.Equal(100m, item.StagingStockQuantity);

// Step 2: Post to inventory
item.CurrentStockQuantity += item.StagingStockQuantity;
item.StagingStockQuantity = 0m;
Assert.Equal(150m, item.CurrentStockQuantity);
Assert.Equal(0m, item.StagingStockQuantity);
```

### Test 4: Location Code Format
```csharp
var item = new Item { LocationCode = "P1-T10-A" };
Assert.Matches(@"^[A-Z0-9\-]+$", item.LocationCode);
```

---

## ?? UI Mockup: Item Stock Card

```razor
<MudCard>
    <MudCardHeader>
        <MudAvatar Image="@item.ImageUrl" Size="Size.Large" />
        <MudText Typo="Typo.h6">@item.ItemDescription</MudText>
        <MudChip Size="Size.Small">@item.StockKeepingUnitCode</MudChip>
    </MudCardHeader>
    <MudCardContent>
        <MudSimpleTable Dense="true">
            <tbody>
                <tr>
                    <td>Current Stock</td>
                    <td><strong>@item.CurrentStockQuantity.ToString("N2")</strong></td>
                </tr>
                <tr>
                    <td>Reserved</td>
                    <td><MudText Color="Color.Warning">@item.ReservedStockQuantity.ToString("N2")</MudText></td>
                </tr>
                <tr>
                    <td><strong>Available</strong></td>
                    <td><MudText Color="Color.Success" Typo="Typo.h6">@item.AvailableStockQuantity.ToString("N2")</MudText></td>
                </tr>
                <tr>
                    <td>Staging</td>
                    <td><MudText Color="Color.Info">@item.StagingStockQuantity.ToString("N2")</MudText></td>
                </tr>
                <tr>
                    <td>Location</td>
                    <td><MudChip Size="Size.Small" Variant="Variant.Outlined">@(item.LocationCode ?? "N/A")</MudChip></td>
                </tr>
            </tbody>
        </MudSimpleTable>
    </MudCardContent>
</MudCard>
```

---

## ?? Integration Points

### 1. Sales Module
**When creating sales order**:
```csharp
// Check available stock
if (item.AvailableStockQuantity < orderLineQuantity)
{
    throw new InsufficientStockException();
}

// Reserve stock
item.ReservedStockQuantity += orderLineQuantity;
```

### 2. Purchasing Module
**When goods arrive**:
```csharp
// Add to staging
item.StagingStockQuantity += purchaseOrderLineQuantity;

// After inspection approved
item.CurrentStockQuantity += item.StagingStockQuantity;
item.StagingStockQuantity = 0;

// Recalculate WAC
item.CostPricePerBaseUnit = CalculateWeightedAverageCost();
```

### 3. Warehouse Module
**When goods leave**:
```csharp
// Physical delivery
item.CurrentStockQuantity -= deliveryQuantity;
item.ReservedStockQuantity -= deliveryQuantity;
```

**Location assignment**:
```csharp
// Assign location after receiving
item.LocationCode = "P1-T10-A";
```

---

## ? Checklist

**Code Changes**:
- [x] Add `ReservedStockQuantity` property
- [x] Add `StagingStockQuantity` property
- [x] Add `LocationCode` property
- [x] Add `ImageUrl` property
- [x] Add `AvailableStockQuantity` calculated property
- [x] Update primary constructor parameters
- [x] Update parameterless constructor
- [x] Maintain XML documentation style
- [x] Maintain decimal precision standards

**Next Steps**:
- [ ] Generate EF Core migration
- [ ] Review migration SQL
- [ ] Apply migration to database
- [ ] Update ItemConfiguration (Fluent API)
- [ ] Update Item DTO classes
- [ ] Update Item services/repositories
- [ ] Update Item UI components
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Update API documentation

---

## ?? Deployment Steps

1. **Review Code**: Verify Item.cs changes
2. **Generate Migration**: `dotnet ef migrations add AddMultiDimensionalStockTracking`
3. **Review SQL**: Check migration file
4. **Backup Database**: Before applying migration
5. **Apply Migration**: `dotnet ef database update`
6. **Verify Schema**: Check columns added correctly
7. **Update Services**: Modify business logic to use new properties
8. **Update UI**: Display multi-dimensional stock info
9. **Test Workflows**: Verify staging, reservations work
10. **Deploy to Production**: After thorough testing

---

**Status**: ? **CODE REFACTORING COMPLETE**  
**Next**: **Generate and Apply EF Core Migration**  
**Priority**: **HIGH** (Foundation for Sales/Purchasing modules)

---

**Architect**: GitHub Copilot  
**Implementation Date**: January 2025  
**Feature**: Multi-dimensional Stock Tracking v1.0
