# ERP Database Setup Guide

## Prerequisites

- SQL Server 2019 or later (or Azure SQL)
- .NET 10 SDK installed
- Entity Framework Core Tools

## Step 1: Install EF Core Tools (if not already installed)

```powershell
dotnet tool install --global dotnet-ef
# Or update if already installed
dotnet tool update --global dotnet-ef
```

## Step 2: Configure Connection String

Edit `HeuristicLogix.Api\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HeuristicLogixERP;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**For Azure SQL:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=HeuristicLogixERP;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

## Step 3: Register AppDbContext in Program.cs

Ensure your `HeuristicLogix.Api\Program.cs` includes:

```csharp
using HeuristicLogix.Api.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ... rest of your configuration
```

## Step 4: Generate Initial Migration

```powershell
# Navigate to solution root directory
cd C:\Repository\HeuristicLogix

# Generate migration
dotnet ef migrations add ERP_PersistenceLayer_Initial `
  --project HeuristicLogix.Api `
  --context AppDbContext `
  --output-dir Persistence/Migrations
```

## Step 5: Review Generated Migration

The migration file will be created in `HeuristicLogix.Api\Persistence\Migrations\`.

Verify that it includes:
- ? All 4 schemas: Core, Inventory, Purchasing, Logistics
- ? All 12 ERP tables + existing logistics tables
- ? Unique indexes on SKU and NationalTaxIdentificationNumber
- ? String enum conversions
- ? Correct decimal precisions (18,4 and 18,2)

## Step 6: Apply Migration to Database

```powershell
# Apply migration (creates database if not exists)
dotnet ef database update `
  --project HeuristicLogix.Api `
  --context AppDbContext
```

## Step 7: Verify Database Creation

Connect to SQL Server and verify:

```sql
-- Check schemas
SELECT name FROM sys.schemas WHERE name IN ('Core', 'Inventory', 'Purchasing', 'Logistics');

-- Check tables
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA IN ('Core', 'Inventory', 'Purchasing')
ORDER BY TABLE_SCHEMA, TABLE_NAME;

-- Verify Core schema
SELECT * FROM Core.TaxConfigurations;       -- Should exist (empty)
SELECT * FROM Core.UnitsOfMeasure;          -- Should exist (empty)

-- Verify Inventory schema
SELECT * FROM Inventory.Categories;         -- Should exist (empty)
SELECT * FROM Inventory.Brands;             -- Should exist (empty)
SELECT * FROM Inventory.Items;              -- Should exist (empty)
SELECT * FROM Inventory.ItemUnitConversions; -- Should exist (empty)

-- Verify Purchasing schema
SELECT * FROM Purchasing.Suppliers;                     -- Should exist (empty)
SELECT * FROM Purchasing.ItemSuppliers;                 -- Should exist (empty)
SELECT * FROM Purchasing.PurchaseInvoices;              -- Should exist (empty)
SELECT * FROM Purchasing.PurchaseInvoiceDetails;        -- Should exist (empty)
SELECT * FROM Purchasing.StagingPurchaseInvoices;       -- Should exist (empty)
SELECT * FROM Purchasing.StagingPurchaseInvoiceDetails; -- Should exist (empty)
```

## Step 8: Seed Initial Data (Optional)

Create a seed data script or use Entity Framework seeding:

### Option A: SQL Script Seeding

Create `HeuristicLogix.Api\Persistence\Seed\InitialData.sql`:

```sql
-- ============================================================
-- SEED DATA: Core Schema
-- ============================================================

-- Tax Configurations
INSERT INTO Core.TaxConfigurations (TaxConfigurationId, TaxName, TaxPercentageRate, IsActive, CreatedAt)
VALUES 
  (NEWID(), 'ITBIS General 18%', 18.00, 1, GETUTCDATE()),
  (NEWID(), 'ITBIS Reducido 16%', 16.00, 1, GETUTCDATE()),
  (NEWID(), 'Exento 0%', 0.00, 1, GETUTCDATE());

-- Units of Measure
INSERT INTO Core.UnitsOfMeasure (UnitOfMeasureId, UnitCode, UnitName, IsActive, CreatedAt)
VALUES 
  (NEWID(), 'un', 'Unidad', 1, GETUTCDATE()),
  (NEWID(), 'kg', 'Kilogramo', 1, GETUTCDATE()),
  (NEWID(), 'm', 'Metro', 1, GETUTCDATE()),
  (NEWID(), 'm2', 'Metro Cuadrado', 1, GETUTCDATE()),
  (NEWID(), 'm3', 'Metro Cúbico', 1, GETUTCDATE()),
  (NEWID(), 'L', 'Litro', 1, GETUTCDATE()),
  (NEWID(), 'lb', 'Libra', 1, GETUTCDATE()),
  (NEWID(), 'gal', 'Galón', 1, GETUTCDATE()),
  (NEWID(), 'saco', 'Saco', 1, GETUTCDATE()),
  (NEWID(), 'caja', 'Caja', 1, GETUTCDATE()),
  (NEWID(), 'palet', 'Palet', 1, GETUTCDATE()),
  (NEWID(), 'pala', 'Pala', 1, GETUTCDATE());

-- ============================================================
-- SEED DATA: Inventory Schema
-- ============================================================

-- Categories
INSERT INTO Inventory.Categories (CategoryId, CategoryName, CategoryDescription, IsActive, CreatedAt)
VALUES 
  (NEWID(), 'Materiales de Construcción', 'Materiales generales para construcción', 1, GETUTCDATE()),
  (NEWID(), 'Cemento y Agregados', 'Cemento, arena, grava', 1, GETUTCDATE()),
  (NEWID(), 'Acero y Metales', 'Varillas, perfiles, tuberías metálicas', 1, GETUTCDATE()),
  (NEWID(), 'Madera y Derivados', 'Madera aserrada, MDF, plywood', 1, GETUTCDATE()),
  (NEWID(), 'Pinturas y Acabados', 'Pinturas, barnices, selladores', 1, GETUTCDATE());

-- Brands
INSERT INTO Inventory.Brands (BrandId, BrandName, CountryOfOrigin, IsActive, CreatedAt)
VALUES 
  (NEWID(), 'Cemex', 'México', 1, GETUTCDATE()),
  (NEWID(), 'LaFarge Holcim', 'Suiza', 1, GETUTCDATE()),
  (NEWID(), 'Ferretería Americana', 'República Dominicana', 1, GETUTCDATE()),
  (NEWID(), 'Aceros Nacionales', 'República Dominicana', 1, GETUTCDATE());

-- ============================================================
-- VERIFICATION QUERIES
-- ============================================================

SELECT 'Tax Configurations', COUNT(*) FROM Core.TaxConfigurations
UNION ALL
SELECT 'Units of Measure', COUNT(*) FROM Core.UnitsOfMeasure
UNION ALL
SELECT 'Categories', COUNT(*) FROM Inventory.Categories
UNION ALL
SELECT 'Brands', COUNT(*) FROM Inventory.Brands;
```

Execute the seed script:
```powershell
# Using sqlcmd
sqlcmd -S localhost -d HeuristicLogixERP -i "HeuristicLogix.Api\Persistence\Seed\InitialData.sql"

# Or using SQL Server Management Studio (SSMS)
# Open and execute the script manually
```

### Option B: C# Seeding in DbContext

Add to `AppDbContext.cs` in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ... existing configuration ...
    
    // Seed data
    SeedCoreData(modelBuilder);
    SeedInventoryData(modelBuilder);
}

private void SeedCoreData(ModelBuilder modelBuilder)
{
    var taxId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    var taxId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    var taxId3 = Guid.Parse("00000000-0000-0000-0000-000000000003");

    modelBuilder.Entity<TaxConfiguration>().HasData(
        new TaxConfiguration(taxId1, "ITBIS General 18%", 18.00m, true),
        new TaxConfiguration(taxId2, "ITBIS Reducido 16%", 16.00m, true),
        new TaxConfiguration(taxId3, "Exento 0%", 0.00m, true)
    );

    // Units of Measure
    var unitIds = new Dictionary<string, Guid>
    {
        ["un"] = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        ["kg"] = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        ["m"] = Guid.Parse("10000000-0000-0000-0000-000000000003"),
        // ... add more
    };

    modelBuilder.Entity<UnitOfMeasure>().HasData(
        new UnitOfMeasure(unitIds["un"], "un", "Unidad"),
        new UnitOfMeasure(unitIds["kg"], "kg", "Kilogramo"),
        new UnitOfMeasure(unitIds["m"], "m", "Metro")
        // ... add more
    );
}

private void SeedInventoryData(ModelBuilder modelBuilder)
{
    // Categories and Brands seeding
    // ... similar pattern
}
```

Then regenerate migration:
```powershell
dotnet ef migrations add ERP_SeedData --project HeuristicLogix.Api --context AppDbContext
dotnet ef database update --project HeuristicLogix.Api --context AppDbContext
```

## Step 9: Enable Read Committed Snapshot Isolation (RCSI)

For optimal concurrency per Architecture.md:

```sql
USE master;
GO

ALTER DATABASE HeuristicLogixERP
SET READ_COMMITTED_SNAPSHOT ON;
GO

-- Verify
SELECT name, is_read_committed_snapshot_on
FROM sys.databases
WHERE name = 'HeuristicLogixERP';
```

## Step 10: Create Database Backup

```sql
BACKUP DATABASE HeuristicLogixERP
TO DISK = 'C:\Backups\HeuristicLogixERP_Initial.bak'
WITH FORMAT, INIT, NAME = 'HeuristicLogixERP Initial Setup';
```

## Troubleshooting

### Issue: "A network-related or instance-specific error occurred"
**Solution**: Verify SQL Server is running and connection string is correct.

```powershell
# Check SQL Server service
Get-Service MSSQLSERVER
# Or for named instance
Get-Service MSSQL$INSTANCENAME
```

### Issue: "Cannot create database because schema does not exist"
**Solution**: EF Core will create schemas automatically. Ensure you have permissions.

```sql
-- Grant schema creation permission
GRANT CREATE SCHEMA TO [YourUsername];
```

### Issue: Migration fails with "Enum conversion error"
**Solution**: Ensure all enum properties use `.HasConversion<string>()` in Fluent API.

### Issue: Decimal precision warnings
**Solution**: Explicitly set precision for all decimal columns:
```csharp
.HasPrecision(18, 4)  // For amounts
.HasPrecision(18, 2)  // For quantities
```

## Rollback Migration (if needed)

```powershell
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --project HeuristicLogix.Api --context AppDbContext

# Remove last migration
dotnet ef migrations remove --project HeuristicLogix.Api --context AppDbContext
```

## Generate SQL Script (for review or manual execution)

```powershell
# Generate SQL script without executing
dotnet ef migrations script `
  --project HeuristicLogix.Api `
  --context AppDbContext `
  --output migration.sql `
  --idempotent
```

The `--idempotent` flag ensures the script can be run multiple times safely.

## Health Check Queries

After setup, run these to verify everything:

```sql
-- Check all schemas exist
SELECT name AS SchemaName 
FROM sys.schemas 
WHERE name IN ('Core', 'Inventory', 'Purchasing', 'Logistics')
ORDER BY name;

-- Check all ERP tables
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    (SELECT COUNT(*) FROM sys.columns c WHERE c.object_id = t.object_id) AS ColumnCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('Core', 'Inventory', 'Purchasing')
ORDER BY s.name, t.name;

-- Verify unique indexes
SELECT 
    i.name AS IndexName,
    OBJECT_NAME(i.object_id) AS TableName,
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.is_unique = 1 AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY SchemaName, TableName, IndexName;

-- Check decimal precision
SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType,
    c.precision,
    c.scale
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE TYPE_NAME(c.user_type_id) = 'decimal'
  AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY SchemaName, TableName, ColumnName;

-- Verify enum columns are strings (varchar/nvarchar)
SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType,
    c.max_length
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE c.name LIKE '%Status%' OR c.name LIKE '%Type%'
  AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY SchemaName, TableName, ColumnName;
```

## Next Steps After Database Setup

1. **Verify AppDbContext registration** in dependency injection
2. **Create Repository pattern** (if desired) or use DbContext directly
3. **Implement Domain Services**:
   - PurchaseInvoiceStagingService
   - InventoryService
   - SupplierService
4. **Build API Controllers** for each entity
5. **Develop Blazor UI** components
6. **Set up integration tests** with in-memory database

---
**Setup Guide Version**: 1.0  
**Last Updated**: January 2025  
**For**: HeuristicLogix ERP - .NET 10 / C# 14
