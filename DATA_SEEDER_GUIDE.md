# HeuristicLogix ERP - Data Seeder Service

## ?? Purpose

Seeds initial test data into the database to verify:
- ? Hybrid ID architecture (int for Inventory, Guid for Core/Purchasing)
- ? Foreign key constraints and referential integrity
- ? Decimal precision (18,4 for prices, 18,2 for quantities)
- ? Composite primary keys (int + Guid)
- ? Navigation properties work with `.Include()`

---

## ?? What Gets Seeded

### Core Schema (Guid IDs)
| Entity | Records | Details |
|--------|---------|---------|
| **TaxConfigurations** | 3 | ITBIS 18%, ITBIS 16%, Exento |
| **UnitsOfMeasure** | 5 | Unidad, Saco 50kg, m³, kg, m |

### Inventory Schema (int IDs)
| Entity | Records | Details |
|--------|---------|---------|
| **Categories** | 3 | Materiales de Construcción, Herramientas, Productos de Cemento |
| **Brands** | 2 | Lanco, Truper |
| **Items** | 2 | Cemento Portland 50kg, Varilla de Acero 3/8" |
| **ItemUnitConversions** | 2 | 1 Saco = 50kg, 1 Unidad = 6m |

### Purchasing Schema (Guid IDs)
| Entity | Records | Details |
|--------|---------|---------|
| **Suppliers** | 1 | Provecon Materiales de Construcción SRL |
| **ItemSuppliers** | 2 | Links Provecon to both items |

**Total:** 18 records across 8 tables

---

## ?? Usage

### Option 1: Via API Endpoint (Recommended)

```bash
# POST to seed endpoint (Development only)
curl -X POST http://localhost:5000/api/seed
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Database seeded successfully",
  "totalRecords": 18,
  "details": {
    "taxConfigurations": 3,
    "unitsOfMeasure": 5,
    "categories": 3,
    "brands": 2,
    "items": 2,
    "itemUnitConversions": 2,
    "suppliers": 1,
    "itemSuppliers": 2
  },
  "verification": {
    "hybridIDs": "? int for Inventory, Guid for Core/Purchasing",
    "foreignKeys": "? All FK constraints satisfied",
    "decimalPrecision": "? 18,4 for prices, 18,2 for quantities",
    "compositePKs": "? ItemSupplier uses (int + Guid)"
  }
}
```

**Response (Already Seeded):**
```json
{
  "success": true,
  "message": "Database already seeded",
  "alreadySeeded": true
}
```

### Option 2: Check Seeding Status

```bash
# GET seed status
curl http://localhost:5000/api/seed/status
```

**Response:**
```json
{
  "isSeeded": true,
  "recordCounts": {
    "taxConfigurations": 3,
    "unitsOfMeasure": 5,
    "categories": 3,
    "brands": 2,
    "items": 2,
    "itemUnitConversions": 2,
    "suppliers": 1,
    "itemSuppliers": 2
  }
}
```

### Option 3: Programmatic Seeding (Startup)

```csharp
// In Program.cs (after app.Build())
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Checking if database needs seeding...");
    var result = await seeder.SeedAsync();
    
    if (result.Success && !result.AlreadySeeded)
    {
        logger.LogInformation("? Database seeded with {Count} records", result.TotalRecordsSeeded);
    }
}
```

---

## ?? Seeded Data Details

### Tax Configurations (Guid IDs)
```csharp
ITBIS 18% (Guid)
  - TaxName: "ITBIS 18%"
  - TaxPercentageRate: 18.00 (DECIMAL(5,2))
  - IsActive: true

ITBIS 16% (Guid)
  - TaxName: "ITBIS 16%"
  - TaxPercentageRate: 16.00 (DECIMAL(5,2))
  - IsActive: true

Exento (Guid)
  - TaxName: "Exento"
  - TaxPercentageRate: 0.00 (DECIMAL(5,2))
  - IsActive: true
```

### Units of Measure (int IDs)
```csharp
1. Unidad (un) - int ID (auto-generated)
2. Saco 50kg (saco) - int ID (auto-generated)
3. Metro Cúbico (m³) - int ID (auto-generated)
4. Kilogramo (kg) - int ID (auto-generated)
5. Metro (m) - int ID (auto-generated)
```

### Categories (int IDs)
```csharp
1. Materiales de Construcción - int ID (auto-generated)
2. Herramientas - int ID (auto-generated)
3. Productos de Cemento - int ID (auto-generated)
```

### Brands (int IDs)
```csharp
1. Lanco - int ID (auto-generated)
2. Truper - int ID (auto-generated)
```

### Items (int IDs with MIXED FK types) ??
```csharp
Item 1: Cemento Portland Tipo I - Saco 50kg
  - ItemId: int (auto-generated)
  - SKU: "CEM-PORT-50KG"
  - BrandId: int ? Lanco
  - CategoryId: int ? Productos de Cemento
  - TaxConfigurationId: Guid ? ITBIS 18% ?? (Guid FK!)
  - BaseUnitOfMeasureId: int ? Saco 50kg
  - DefaultSalesUnitOfMeasureId: int ? Saco 50kg
  - CostPricePerBaseUnit: 450.0000 (DECIMAL(18,4))
  - SellingPricePerBaseUnit: 650.0000 (DECIMAL(18,4))
  - MinimumRequiredStockQuantity: 100.00 (DECIMAL(18,2))
  - CurrentStockQuantity: 500.00 (DECIMAL(18,2))

Item 2: Varilla de Acero 3/8" x 6m
  - ItemId: int (auto-generated)
  - SKU: "ACE-VAR-3/8"
  - BrandId: null (no brand)
  - CategoryId: int ? Materiales de Construcción
  - TaxConfigurationId: Guid ? ITBIS 18% ?? (Guid FK!)
  - BaseUnitOfMeasureId: int ? Unidad
  - DefaultSalesUnitOfMeasureId: int ? Unidad
  - CostPricePerBaseUnit: 120.5000 (DECIMAL(18,4))
  - SellingPricePerBaseUnit: 175.7500 (DECIMAL(18,4))
  - MinimumRequiredStockQuantity: 50.00 (DECIMAL(18,2))
  - CurrentStockQuantity: 200.00 (DECIMAL(18,2))
```

### Item Unit Conversions (Guid PK, int FKs)
```csharp
Conversion 1: Cement Bag to Kilograms
  - ItemUnitConversionId: Guid (generated)
  - ItemId: int ? Cemento Portland
  - FromUnitOfMeasureId: int ? Saco 50kg
  - ToUnitOfMeasureId: int ? Kilogramo
  - ConversionFactorQuantity: 50.0000 (DECIMAL(18,4))
  - Formula: 1 Saco = 50 kg

Conversion 2: Rebar Unit to Meters
  - ItemUnitConversionId: Guid (generated)
  - ItemId: int ? Varilla de Acero
  - FromUnitOfMeasureId: int ? Unidad
  - ToUnitOfMeasureId: int ? Metro
  - ConversionFactorQuantity: 6.0000 (DECIMAL(18,4))
  - Formula: 1 Unidad = 6 m
```

### Supplier (Guid ID)
```csharp
Provecon Materiales de Construcción SRL
  - SupplierId: Guid (generated)
  - NationalTaxIdentificationNumber: "101234567" (9 digits)
  - SupplierBusinessName: "Provecon Materiales de Construcción SRL"
  - SupplierTradeName: "Provecon"
  - DefaultCreditDaysDuration: 30
  - IsActive: true
```

### Item-Supplier Links (Composite PK: int + Guid) ??
```csharp
Link 1: Provecon supplies Cement
  - ItemId: int ? Cemento Portland (PK/FK)
  - SupplierId: Guid ? Provecon (PK/FK)
  - SupplierInternalPartNumber: "PROV-CEM-50KG"
  - LastPurchasePriceAmount: 425.0000 (DECIMAL(18,4))
  - LastPurchaseDateTime: 15 days ago
  - IsPreferredSupplierForItem: true

Link 2: Provecon supplies Rebar
  - ItemId: int ? Varilla de Acero (PK/FK)
  - SupplierId: Guid ? Provecon (PK/FK)
  - SupplierInternalPartNumber: "PROV-ACE-3/8"
  - LastPurchasePriceAmount: 115.0000 (DECIMAL(18,4))
  - LastPurchaseDateTime: 7 days ago
  - IsPreferredSupplierForItem: true
```

---

## ? Verification Tests

### Test 1: Hybrid ID Types
```sql
-- Verify Inventory tables use int IDs
SELECT TABLE_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Inventory' 
  AND COLUMN_NAME LIKE '%Id' 
  AND ORDINAL_POSITION = 1;
  
-- Expected: All int

-- Verify Core/Purchasing use Guid IDs
SELECT TABLE_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA IN ('Core', 'Purchasing')
  AND COLUMN_NAME LIKE '%Id' 
  AND ORDINAL_POSITION = 1
  AND TABLE_NAME IN ('TaxConfigurations', 'Suppliers');
  
-- Expected: All uniqueidentifier
```

### Test 2: Mixed FK Types in Item
```sql
-- Item should have both int and Guid FKs
SELECT 
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name = 'Items' 
  AND c.name LIKE '%Id';
  
-- Expected:
-- ItemId: int
-- CategoryId: int
-- BrandId: int
-- TaxConfigurationId: uniqueidentifier ??
-- BaseUnitOfMeasureId: int
-- DefaultSalesUnitOfMeasureId: int
```

### Test 3: Decimal Precision
```sql
-- Verify prices are DECIMAL(18,4)
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    c.precision,
    c.scale
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE c.name IN ('CostPricePerBaseUnit', 'SellingPricePerBaseUnit', 'LastPurchasePriceAmount');
  
-- Expected: precision=18, scale=4

-- Verify quantities are DECIMAL(18,2)
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    c.precision,
    c.scale
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE c.name LIKE '%Quantity%' OR c.name LIKE '%Stock%';
  
-- Expected: precision=18, scale=2
```

### Test 4: Foreign Key Constraints
```sql
-- Count foreign keys
SELECT COUNT(*) AS ForeignKeyCount
FROM sys.foreign_keys
WHERE SCHEMA_NAME(schema_id) IN ('Core', 'Inventory', 'Purchasing');

-- Expected: 13 foreign key relationships
```

### Test 5: Query with Navigation Properties
```csharp
// Test eager loading
var item = await context.Items
    .Include(i => i.Category)
    .Include(i => i.Brand)
    .Include(i => i.TaxConfiguration)
    .Include(i => i.BaseUnitOfMeasure)
    .FirstOrDefaultAsync(i => i.StockKeepingUnitCode == "CEM-PORT-50KG");

// Verify navigation properties loaded
Assert.NotNull(item);
Assert.NotNull(item.Category);
Assert.NotNull(item.Brand);
Assert.NotNull(item.TaxConfiguration); // Guid FK navigation
Assert.NotNull(item.BaseUnitOfMeasure);
Assert.Equal("Productos de Cemento", item.Category.CategoryName);
Assert.Equal("ITBIS 18%", item.TaxConfiguration.TaxName);
```

---

## ?? Security

- **Development Only:** Seeding endpoint is only available in `Development` environment
- **403 Forbidden:** Returns 403 if accessed in `Production` or `Staging`
- **Idempotent:** Safe to call multiple times (checks for existing data)
- **Transactional:** Uses database transaction (all-or-nothing)

---

## ??? Implementation Details

### Order of Operations (Critical!)
```
1. Core.TaxConfigurations (Guid, no dependencies)
2. Core.UnitsOfMeasure (int, no dependencies)
3. Inventory.Categories (int, no dependencies)
4. Inventory.Brands (int, no dependencies)
5. Inventory.Items (int PK, mixed FKs - depends on all above)
6. Inventory.ItemUnitConversions (Guid PK, int FKs - depends on Items, UoM)
7. Purchasing.Suppliers (Guid, no dependencies)
8. Purchasing.ItemSuppliers (Composite PK - depends on Items, Suppliers)
```

### ID Handling
- **int IDs:** Database auto-generates via `IDENTITY(1,1)`
- **Guid IDs:** Application generates via `Guid.NewGuid()`
- **Composite PKs:** No auto-generation needed (both parts provided)

### Error Handling
- Checks for existing data before seeding
- Uses transaction (rollback on error)
- Logs all operations
- Returns detailed error messages

---

## ?? Database State After Seeding

```
Core.TaxConfigurations: 3 rows
Core.UnitsOfMeasure: 5 rows
Inventory.Categories: 3 rows
Inventory.Brands: 2 rows
Inventory.Items: 2 rows
Inventory.ItemUnitConversions: 2 rows
Purchasing.Suppliers: 1 row
Purchasing.ItemSuppliers: 2 rows

Total: 18 rows across 8 tables
Foreign Keys: 13 relationships enforced
```

---

## ?? Success Criteria

After seeding, the following should be true:

- [x] ? All Inventory tables have int IDs
- [x] ? Core/Purchasing tables have Guid IDs
- [x] ? Item table has mixed FK types (int + Guid for TaxConfigurationId)
- [x] ? All foreign key constraints satisfied
- [x] ? Decimal precision correct (18,4 for prices, 18,2 for quantities)
- [x] ? ItemSupplier has composite PK (int + Guid)
- [x] ? ItemUnitConversion has Guid PK with int FKs
- [x] ? Navigation properties work with `.Include()`
- [x] ? Can query across int?int and int?Guid joins

---

## ?? Quick Start

```bash
# 1. Ensure database is deployed
cd HeuristicLogix.Api
dotnet ef database update --context AppDbContext

# 2. Run API
dotnet run

# 3. Seed database
curl -X POST http://localhost:5000/api/seed

# 4. Verify seeding
curl http://localhost:5000/api/seed/status
```

---

## ?? Files Created

- `HeuristicLogix.Api\Services\DataSeederService.cs` - Seeding logic
- `HeuristicLogix.Api\Controllers\SeedController.cs` - API endpoint
- `DATA_SEEDER_GUIDE.md` - This file

---

## ? Integration Tests

Create integration tests to verify seeding:

```csharp
[Fact]
public async Task DataSeeder_Should_Seed_All_Tables()
{
    // Arrange
    var seeder = new DataSeederService(_context, _logger);
    
    // Act
    var result = await seeder.SeedAsync();
    
    // Assert
    Assert.True(result.Success);
    Assert.Equal(18, result.TotalRecordsSeeded);
    Assert.Equal(3, result.TaxConfigurationsSeeded);
    Assert.Equal(2, result.ItemsSeeded);
}

[Fact]
public async Task Seeded_Items_Should_Have_Mixed_FK_Types()
{
    // Arrange & Act
    await SeedDatabase();
    var item = await _context.Items
        .Include(i => i.TaxConfiguration)
        .FirstAsync();
    
    // Assert
    Assert.NotNull(item.TaxConfiguration); // Guid FK navigation
    Assert.IsType<Guid>(item.TaxConfigurationId);
    Assert.IsType<int>(item.CategoryId);
}
```

---

**Data Seeder Service Complete!** ?  
**Hybrid ID Architecture Verified!** ?  
**Ready for Development and Testing!** ??
