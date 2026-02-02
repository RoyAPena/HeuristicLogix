# Data Seeder - Quick Reference

## ?? Quick Start

```bash
# 1. Start API
cd HeuristicLogix.Api
dotnet run

# 2. Seed (in new terminal)
curl -X POST http://localhost:5000/api/seed

# 3. Check status
curl http://localhost:5000/api/seed/status
```

## ?? What Gets Seeded

| Count | Entity | ID Type |
|-------|--------|---------|
| 3 | TaxConfigurations | Guid |
| 5 | UnitsOfMeasure | int |
| 3 | Categories | int |
| 2 | Brands | int |
| 2 | Items | int (mixed FKs!) |
| 2 | ItemUnitConversions | Guid PK, int FKs |
| 1 | Suppliers | Guid |
| 2 | ItemSuppliers | (int, Guid) PK |
| **18** | **Total** | **Hybrid** |

## ?? Critical Tests

### Item Has Mixed FK Types
```sql
SELECT c.name, TYPE_NAME(c.user_type_id)
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name = 'Items' AND c.name LIKE '%Id';

-- Expected:
-- ItemId: int
-- CategoryId: int
-- TaxConfigurationId: uniqueidentifier ??
-- BaseUnitOfMeasureId: int
```

### Decimal Precision
```sql
SELECT ItemDescription, CostPricePerBaseUnit, CurrentStockQuantity
FROM Inventory.Items;

-- Expected:
-- Cement: 450.0000 (18,4), 500.00 (18,2)
-- Rebar: 120.5000 (18,4), 200.00 (18,2)
```

### Navigation Properties
```csharp
var item = await context.Items
    .Include(i => i.Category)
    .Include(i => i.TaxConfiguration) // Guid FK
    .FirstAsync();
```

## ? Verification

```powershell
# Run verification script
sqlcmd -S LAPTOP-7MG6K7RV -d HeuristicLogix -E -i verify-database-schema.sql
```

## ?? Files

- `HeuristicLogix.Api\Services\DataSeederService.cs`
- `HeuristicLogix.Api\Controllers\SeedController.cs`
- `DATA_SEEDER_GUIDE.md`
- `test-data-seeder.ps1`

---
**Status:** ? READY  
**Endpoint:** `POST /api/seed`
