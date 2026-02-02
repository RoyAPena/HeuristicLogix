# HeuristicLogix ERP - Local Database Deployment Guide

## ?? Mission: Deploy to LAPTOP-7MG6K7RV

**Target Configuration:**
- Server: `LAPTOP-7MG6K7RV`
- Database: `HeuristicLogix`
- Authentication: Windows Authentication
- Isolation Level: Read Committed Snapshot (RCSI)

---

## ? Quick Start (Automated)

### Option 1: PowerShell Automation Script

```powershell
# Run from repository root
.\deploy-database-local.ps1
```

This script will:
1. ? Verify connection string in appsettings.json
2. ? Check EF Core tools installation
3. ? Test SQL Server connection
4. ? Generate EF Core migration
5. ? Apply migration to database
6. ? Enable RCSI for concurrency
7. ? Verify schema structure and ID types
8. ? Provide deployment summary

---

## ?? Manual Steps (Step-by-Step)

### Step 1: Connection String Setup ?

**File:** `HeuristicLogix.Api\appsettings.json`

**Already Configured:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-7MG6K7RV;Database=HeuristicLogix;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

### Step 2: Generate Migration

```powershell
# Navigate to API project
cd HeuristicLogix.Api

# Generate initial migration
dotnet ef migrations add InitialERPDeployment --verbose

# Expected output:
# Build started...
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

**Migration Location:** `HeuristicLogix.Api\Migrations\`

### Step 3: Apply Migration

```powershell
# Still in HeuristicLogix.Api directory
dotnet ef database update --verbose

# Expected output:
# Applying migration '20250101000000_InitialERPDeployment'.
# Done.
```

### Step 4: Enable RCSI (SQL Server)

**Option A: SQL Server Management Studio**
1. Open SSMS
2. Connect to `LAPTOP-7MG6K7RV`
3. Open New Query
4. Run:
```sql
ALTER DATABASE [HeuristicLogix] SET READ_COMMITTED_SNAPSHOT ON;
```

**Option B: sqlcmd**
```powershell
sqlcmd -S LAPTOP-7MG6K7RV -d master -E -Q "ALTER DATABASE [HeuristicLogix] SET READ_COMMITTED_SNAPSHOT ON;"
```

**Option C: PowerShell**
```powershell
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = "Server=LAPTOP-7MG6K7RV;Database=master;Trusted_Connection=True;"
$connection.Open()
$cmd = $connection.CreateCommand()
$cmd.CommandText = "ALTER DATABASE [HeuristicLogix] SET READ_COMMITTED_SNAPSHOT ON;"
$cmd.ExecuteNonQuery()
$connection.Close()
Write-Host "? RCSI Enabled"
```

### Step 5: Verify Schema

Run the verification script:

```powershell
# Option 1: From SSMS
# Open: verify-database-schema.sql
# Execute against: HeuristicLogix database

# Option 2: From sqlcmd
sqlcmd -S LAPTOP-7MG6K7RV -d HeuristicLogix -E -i verify-database-schema.sql
```

---

## ?? Expected Schema Structure

### Schemas Created
- **Core** - System-wide configurations (TaxConfiguration, UnitOfMeasure)
- **Inventory** - Master inventory data (Category, Brand, Item, ItemUnitConversion)
- **Purchasing** - Supplier and invoice staging (Supplier, ItemSupplier, Staging tables)
- **Logistics** - Existing logistics entities (Conduce, Truck, DeliveryRoute, etc.)

### Tables by Schema

#### Core Schema (2 tables)
| Table | Primary Key | Type |
|-------|-------------|------|
| TaxConfigurations | TaxConfigurationId | Guid |
| UnitsOfMeasure | UnitOfMeasureId | **int** |

#### Inventory Schema (4 tables)
| Table | Primary Key | Type |
|-------|-------------|------|
| Categories | CategoryId | **int** |
| Brands | BrandId | **int** |
| Items | ItemId | **int** |
| ItemUnitConversions | ItemUnitConversionId | Guid |

#### Purchasing Schema (4 tables)
| Table | Primary Key | Type |
|-------|-------------|------|
| Suppliers | SupplierId | Guid |
| ItemSuppliers | (ItemId, SupplierId) | Composite |
| StagingPurchaseInvoices | StagingPurchaseInvoiceId | Guid |
| StagingPurchaseInvoiceDetails | StagingPurchaseInvoiceDetailId | Guid |

### ID Type Verification ?? CRITICAL

**Inventory entities use `int` IDs:**
```sql
-- Should all be 'int'
SELECT TABLE_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Inventory' 
  AND COLUMN_NAME LIKE '%Id' 
  AND ORDINAL_POSITION = 1;

-- Expected:
-- Categories: int
-- Brands: int
-- Items: int
-- UnitsOfMeasure: int
```

**Core/Purchasing entities use `uniqueidentifier` (Guid):**
```sql
-- Should be 'uniqueidentifier'
SELECT TABLE_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA IN ('Core', 'Purchasing')
  AND COLUMN_NAME LIKE '%Id' 
  AND ORDINAL_POSITION = 1
  AND TABLE_NAME IN ('TaxConfigurations', 'Suppliers', 'StagingPurchaseInvoices');

-- Expected:
-- TaxConfigurations: uniqueidentifier
-- Suppliers: uniqueidentifier
-- StagingPurchaseInvoices: uniqueidentifier
```

### Foreign Key Verification

**Item Table has MIXED FK types:**
```sql
SELECT 
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name = 'Items'
  AND c.name LIKE '%Id'
ORDER BY c.column_id;

-- Expected:
-- ItemId: int (PK)
-- BrandId: int (FK to Brands)
-- CategoryId: int (FK to Categories)
-- TaxConfigurationId: uniqueidentifier (FK to TaxConfigurations) ??
-- BaseUnitOfMeasureId: int (FK to UnitsOfMeasure)
-- DefaultSalesUnitOfMeasureId: int (FK to UnitsOfMeasure, nullable)
```

### Unique Constraints Verification

```sql
-- Should have UNIQUE indexes
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.is_unique = 1 
  AND i.is_primary_key = 0
  AND t.name IN ('Items', 'Suppliers', 'UnitsOfMeasure');

-- Expected:
-- Items: StockKeepingUnitCode (UNIQUE)
-- Suppliers: NationalTaxIdentificationNumber (UNIQUE)
-- UnitsOfMeasure: UnitOfMeasureSymbol (UNIQUE)
```

### Decimal Precision Verification

```sql
-- Prices/Costs should be DECIMAL(18,4)
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    c.precision,
    c.scale
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE TYPE_NAME(c.user_type_id) = 'decimal'
  AND (c.name LIKE '%Price%' OR c.name LIKE '%Cost%' OR c.name LIKE '%Amount%');

-- Expected: precision=18, scale=4

-- Quantities/Stock should be DECIMAL(18,2)
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    c.precision,
    c.scale
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE TYPE_NAME(c.user_type_id) = 'decimal'
  AND (c.name LIKE '%Quantity%' OR c.name LIKE '%Stock%');

-- Expected: precision=18, scale=2
```

---

## ?? Testing the Deployment

### Test 1: SQL Server Connection
```powershell
# Test connection from PowerShell
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=LAPTOP-7MG6K7RV;Database=HeuristicLogix;Trusted_Connection=True;"
try {
    $conn.Open()
    Write-Host "? Connection successful!" -ForegroundColor Green
    $conn.Close()
} catch {
    Write-Host "? Connection failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Test 2: EF Core Connection
```csharp
// In HeuristicLogix.Api or test project
using var context = new AppDbContext(options);
var canConnect = await context.Database.CanConnectAsync();
Console.WriteLine($"Database connection: {(canConnect ? "? Success" : "? Failed")}");
```

### Test 3: Query Test
```sql
-- Verify all schemas exist
SELECT name FROM sys.schemas 
WHERE name IN ('Core', 'Inventory', 'Purchasing', 'Logistics');

-- Should return 4 rows
```

---

## ?? Troubleshooting

### Issue 1: "Cannot connect to server"
**Problem:** SQL Server not running or connection string incorrect

**Solution:**
1. Verify SQL Server is running:
   ```powershell
   Get-Service | Where-Object {$_.Name -like "*SQL*"}
   ```
2. Check server name:
   ```sql
   SELECT @@SERVERNAME;
   ```
3. Enable Windows Authentication in SQL Server

### Issue 2: "EF Core tools not found"
**Problem:** `dotnet ef` command not recognized

**Solution:**
```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef
```

### Issue 3: "Build failed" during migration
**Problem:** Code compilation errors

**Solution:**
```powershell
# Clean and rebuild
dotnet clean
dotnet build

# Check for errors
dotnet build --no-incremental
```

### Issue 4: "Database already exists"
**Problem:** Previous deployment attempt

**Solution:**
```sql
-- Drop and recreate (?? DESTRUCTIVE)
USE master;
GO
DROP DATABASE IF EXISTS [HeuristicLogix];
GO

-- Then run migration again
```

### Issue 5: ID type mismatch
**Problem:** Generated IDs are wrong type (all Guid or all int)

**Solution:**
1. Delete Migrations folder
2. Verify entity definitions in `HeuristicLogix.Shared\Models`
3. Verify AppDbContext configuration
4. Regenerate migration

---

## ?? Success Criteria Checklist

- [ ] ? Connection string configured in appsettings.json
- [ ] ? EF Core migration generated successfully
- [ ] ? Database created on LAPTOP-7MG6K7RV
- [ ] ? All 4 schemas exist (Core, Inventory, Purchasing, Logistics)
- [ ] ? Inventory tables use **int** IDs
- [ ] ? Core/Purchasing tables use **Guid** IDs
- [ ] ? Item table has mixed FK types (int + Guid for TaxConfigurationId)
- [ ] ? Unique constraints on SKU, RNC, UOM Symbol
- [ ] ? Foreign key constraints created
- [ ] ? Decimal precision correct (18,4 for prices, 18,2 for quantities)
- [ ] ? RCSI enabled for concurrency
- [ ] ? Navigation properties accessible via Include()

---

## ?? Additional Resources

### Files Created
1. `deploy-database-local.ps1` - Automated deployment script
2. `verify-database-schema.sql` - SQL verification script
3. `DATABASE_DEPLOYMENT_GUIDE.md` - This file

### Documentation References
- `ERP_HYBRID_ID_ARCHITECTURE.md` - ID type architecture
- `ERP_EXPLICIT_RELATIONSHIPS.md` - FK relationships
- `ERP_PERSISTENCE_ERD.md` - Entity relationship diagram
- `ERP_DATABASE_SETUP_GUIDE.md` - Detailed setup guide

### Migration Files Location
```
HeuristicLogix.Api\
??? Migrations\
    ??? 20250101000000_InitialERPDeployment.cs
    ??? 20250101000000_InitialERPDeployment.Designer.cs
    ??? AppDbContextModelSnapshot.cs
```

---

## ?? Next Steps After Deployment

1. **Seed Initial Data**
   - Tax configurations (ITBIS 18%, etc.)
   - Units of measure (kg, m, un, etc.)
   - Base categories

2. **Test API Integration**
   - Verify API can connect to database
   - Test CRUD operations on each entity

3. **Performance Tuning**
   - Review query execution plans
   - Add additional indexes if needed

4. **Backup Strategy**
   - Set up automated backups
   - Document recovery procedures

---

## ? Deployment Complete!

Your HeuristicLogix ERP database is now ready for development and testing!

**Connection String:**
```
Server=LAPTOP-7MG6K7RV;Database=HeuristicLogix;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;
```

**Hybrid ID Architecture:**
- Inventory: int IDs (legacy compatibility)
- Core/Purchasing: Guid IDs (distributed systems)
- RCSI: Enabled (multi-user concurrency)

**Start developing with confidence!** ??
