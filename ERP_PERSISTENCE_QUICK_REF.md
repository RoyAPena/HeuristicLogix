# ERP Persistence Layer - Quick Reference

## Entity Relationships at a Glance

```
[Core Schema]
??? TaxConfiguration (Tax rates)
??? UnitOfMeasure (Measurement units: kg, m, un, etc.)

[Inventory Schema]
??? Category (Product categories, hierarchical)
??? Brand (Manufacturers)
??? Item (Main inventory entity)
?   ??? FK ? BaseUnitOfMeasureId (UnitOfMeasure)
?   ??? FK ? CategoryId (Category, optional)
?   ??? FK ? BrandId (Brand, optional)
??? ItemUnitConversion (Multi-unit support)
    ??? FK ? ItemId (Item)
    ??? FK ? TransactionalUnitOfMeasureId (UnitOfMeasure)

[Purchasing Schema]
??? Supplier (Vendors)
??? ItemSupplier (Supplier catalog)
?   ??? FK ? ItemId (Item)
?   ??? FK ? SupplierId (Supplier)
??? StagingPurchaseInvoice (Import staging - Phase A)
?   ??? Details ? StagingPurchaseInvoiceDetail (1:Many)
??? PurchaseInvoice (Approved invoices - Phase B)
    ??? FK ? SupplierId (Supplier)
    ??? FK ? TaxConfigurationId (TaxConfiguration, optional)
    ??? Details ? PurchaseInvoiceDetail (1:Many)
        ??? FK ? ItemId (Item)
```

## Common Operations & Formulas

### Unit Conversion (Base ? Transactional)
```csharp
decimal baseQuantity = transactionalQuantity * conversionFactorQuantity;

// Example: 5 Metros × 40 (factor) = 200 Palas
```

### Unit Conversion (Transactional ? Base)
```csharp
decimal transactionalQuantity = baseQuantity / conversionFactorQuantity;

// Example: 200 Palas ÷ 40 (factor) = 5 Metros
```

### Price Conversion (Base ? Transactional)
```csharp
decimal pricePerBaseUnit = pricePerTransactionalUnit / conversionFactorQuantity;

// Example: $100/Metro ÷ 40 (factor) = $2.50/Pala
```

### Weighted Average Cost (WAC) Calculation
```csharp
decimal newWAC = (oldStockValue + newPurchaseValue) / (oldStockQty + newPurchaseQty);

// Where:
// oldStockValue = Item.CurrentStockQuantity × Item.CostPricePerBaseUnit
// newPurchaseValue = SUM(detail.QuantityInBaseUnit × detail.UnitPricePerBaseUnit)
// newPurchaseQty = SUM(detail.QuantityInBaseUnit)
```

### Invoice Due Date Calculation
```csharp
DateTimeOffset dueDate = invoiceIssueDateTime.AddDays(creditDaysDuration);
```

## DbContext Usage Examples

### 1. Query Items with Units
```csharp
var items = await context.Items
    .Include(i => i.BaseUnitOfMeasure) // Navigation would need to be added if desired
    .Where(i => i.IsActive)
    .ToListAsync();
```

### 2. Find Item by SKU
```csharp
var item = await context.Items
    .FirstOrDefaultAsync(i => i.StockKeepingUnit == "SKU-12345");
```

### 3. Get Supplier by Tax ID
```csharp
var supplier = await context.Suppliers
    .FirstOrDefaultAsync(s => s.NationalTaxIdentificationNumber == "123456789");
```

### 4. Get Item Unit Conversions
```csharp
var conversions = await context.ItemUnitConversions
    .Where(c => c.ItemId == itemId && c.IsActive)
    .ToListAsync();
```

### 5. Query Staging Invoices by Batch
```csharp
var stagingInvoices = await context.StagingPurchaseInvoices
    .Include(s => s.Details)
    .Where(s => s.ImportBatchId == batchId && s.ValidationStatus == StagingValidationStatus.Pending)
    .ToListAsync();
```

### 6. Get Approved Invoices for a Date Range
```csharp
var invoices = await context.PurchaseInvoices
    .Include(p => p.Details)
    .Where(p => p.InvoiceIssueDateTime >= startDate && p.InvoiceIssueDateTime <= endDate)
    .OrderByDescending(p => p.InvoiceIssueDateTime)
    .ToListAsync();
```

## Validation Rules Summary

### Supplier
- `NationalTaxIdentificationNumber`: Must be 9 or 11 digits (Dominican RNC)
- `NationalTaxIdentificationNumber`: Must be unique

### Item
- `StockKeepingUnit`: Must be unique
- `BaseUnitOfMeasureId`: Must reference a valid UnitOfMeasure

### PurchaseInvoice
- `FiscalReceiptNumber`: Format B + Type (2 digits) + Sequence (8 digits)
  - Example: `B0100000123`
- `TotalAmount`: Must equal `SubtotalAmount + TaxAmount`

### StagingPurchaseInvoice
- ValidationStatus flow: `Pending` ? `Validated` ? `Approved`
- Or: `Pending` ? `Invalid` (cannot be approved)
- Or: `Pending` ? `Rejected` (user decision)

## Enum Values Reference

### StagingValidationStatus
- `Pending`: Awaiting validation
- `Validated`: Validation passed, ready for approval
- `Invalid`: Validation failed, cannot be approved
- `Approved`: Approved and posted to live tables
- `Rejected`: Rejected by user

## Database Precision Standards

| Type | Precision | Usage |
|------|-----------|-------|
| Monetary Amounts | `DECIMAL(18,4)` | Prices, costs, invoice totals |
| Quantities | `DECIMAL(18,2)` | Stock, purchase quantities |
| Conversion Factors | `DECIMAL(18,4)` | Unit conversion ratios |
| Tax Percentages | `DECIMAL(5,2)` | Tax rates (e.g., 18.75%) |

## Key Indexes

### Performance Critical
```sql
-- Fast supplier lookup by tax ID
Suppliers.NationalTaxIdentificationNumber (UNIQUE)

-- Fast item lookup by SKU
Items.StockKeepingUnit (UNIQUE)

-- Fast unit lookup by code
UnitsOfMeasure.UnitCode (UNIQUE)

-- Fast staging queries
StagingPurchaseInvoices.ValidationStatus
StagingPurchaseInvoices.ImportBatchId

-- Fast invoice queries
PurchaseInvoices.InvoiceIssueDateTime
PurchaseInvoices.InvoiceDueDate
PurchaseInvoices.SupplierId
```

### Composite Indexes
```sql
-- Item-Supplier relationships
ItemSuppliers (ItemId, SupplierId)

-- Unit conversions for specific items
ItemUnitConversions (ItemId, TransactionalUnitOfMeasureId, IsActive)
```

## Transaction Boundaries

### Critical: Purchase Invoice Approval
This operation MUST be wrapped in a single transaction:

```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // 1. Create PurchaseInvoice from StagingPurchaseInvoice
    var invoice = new PurchaseInvoice(...);
    context.PurchaseInvoices.Add(invoice);

    // 2. Create PurchaseInvoiceDetails from StagingPurchaseInvoiceDetails
    foreach (var detail in stagingDetails)
    {
        context.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail(...));
        
        // 3. Update Item.CurrentStockQuantity
        var item = await context.Items.FindAsync(detail.ItemId);
        item.CurrentStockQuantity += detail.QuantityInBaseUnit;
        
        // 4. Recalculate Item.CostPricePerBaseUnit (WAC)
        decimal oldValue = item.CurrentStockQuantity * item.CostPricePerBaseUnit;
        decimal newValue = detail.QuantityInBaseUnit * detail.UnitPricePerBaseUnit;
        decimal newQty = item.CurrentStockQuantity + detail.QuantityInBaseUnit;
        item.CostPricePerBaseUnit = (oldValue + newValue) / newQty;
        
        // 5. Update ItemSupplier.LastPurchasePrice and LastPurchaseDate
        var itemSupplier = await context.ItemSuppliers
            .FirstOrDefaultAsync(s => s.ItemId == detail.ItemId && s.SupplierId == invoice.SupplierId);
        if (itemSupplier != null)
        {
            itemSupplier.LastPurchasePricePerBaseUnit = detail.UnitPricePerBaseUnit;
            itemSupplier.LastPurchaseDate = DateTimeOffset.UtcNow;
        }
    }

    // 6. Mark staging records as Approved
    stagingInvoice.ValidationStatus = StagingValidationStatus.Approved;
    stagingInvoice.ProcessedAt = DateTimeOffset.UtcNow;
    stagingInvoice.ProcessedByUserId = currentUserId;

    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Concurrency Notes

- **Read Committed Snapshot Isolation (RCSI)**: Designed for non-blocking reads
- **Optimistic Concurrency**: Consider adding `RowVersion` timestamp columns for high-contention tables
- **Staging Pattern**: Minimizes lock time on live inventory tables

## Migration Generation

```powershell
# Add a new migration
dotnet ef migrations add ERP_PersistenceLayer_Initial --project HeuristicLogix.Api --context AppDbContext

# Update database
dotnet ef database update --project HeuristicLogix.Api --context AppDbContext

# Generate SQL script
dotnet ef migrations script --project HeuristicLogix.Api --context AppDbContext --output migration.sql
```

## Common Queries (LINQ)

### Get Low Stock Items
```csharp
var lowStockItems = await context.Items
    .Where(i => i.IsActive && 
                i.MinimumStockLevel.HasValue && 
                i.CurrentStockQuantity < i.MinimumStockLevel.Value)
    .OrderBy(i => i.CurrentStockQuantity)
    .ToListAsync();
```

### Get Items with No Recent Purchases
```csharp
var cutoffDate = DateTimeOffset.UtcNow.AddMonths(-6);
var itemsWithoutRecentPurchases = await context.ItemSuppliers
    .Where(s => s.IsActive && 
                (!s.LastPurchaseDate.HasValue || s.LastPurchaseDate.Value < cutoffDate))
    .Select(s => new { s.ItemId, s.SupplierId, s.LastPurchaseDate })
    .ToListAsync();
```

### Get Overdue Invoices
```csharp
var overdueInvoices = await context.PurchaseInvoices
    .Where(p => p.InvoiceDueDate.HasValue && 
                p.InvoiceDueDate.Value < DateTimeOffset.UtcNow)
    .Include(p => p.Details)
    .OrderBy(p => p.InvoiceDueDate)
    .ToListAsync();
```

---
**Quick Ref Version**: 1.0  
**Last Updated**: January 2025  
**For**: HeuristicLogix ERP - .NET 10 / C# 14
