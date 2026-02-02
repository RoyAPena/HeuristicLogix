# Item Multi-Dimensional Stock Tracking - Quick Reference

## ?? Four Stock Quantities

| Property | Type | Purpose | Example |
|----------|------|---------|---------|
| **CurrentStockQuantity** | decimal(18,2) | Physically verified warehouse stock | 100 units |
| **ReservedStockQuantity** | decimal(18,4) | Committed to customers (invoiced/conduce) | 30 units |
| **StagingStockQuantity** | decimal(18,4) | Arrived but not yet verified | 50 units |
| **AvailableStockQuantity** | Calculated | Current - Reserved (what can be sold) | 70 units |

## ?? Quick Formulas

```csharp
// Available for sales
AvailableStockQuantity = CurrentStockQuantity - ReservedStockQuantity

// Total physical goods
TotalPhysicalStock = CurrentStockQuantity + StagingStockQuantity

// Total committed
TotalCommittedStock = ReservedStockQuantity
```

## ?? Common Workflows

### Purchase Goods
```csharp
// Goods arrive
item.StagingStockQuantity += 100;

// Quality approved ? Post to inventory
item.CurrentStockQuantity += item.StagingStockQuantity;
item.StagingStockQuantity = 0;
```

### Reserve for Sales
```csharp
// Create sales order
if (item.AvailableStockQuantity >= orderQty)
{
    item.ReservedStockQuantity += orderQty;
}
```

### Deliver Goods
```csharp
// Goods leave warehouse
item.CurrentStockQuantity -= deliveryQty;
item.ReservedStockQuantity -= deliveryQty;
```

## ?? UI Display Pattern

```razor
<MudSimpleTable>
    <tr>
        <td>Current:</td>
        <td>@item.CurrentStockQuantity.ToString("N2")</td>
    </tr>
    <tr>
        <td>Reserved:</td>
        <td><MudText Color="Color.Warning">@item.ReservedStockQuantity.ToString("N2")</MudText></td>
    </tr>
    <tr>
        <td><strong>Available:</strong></td>
        <td><MudText Color="Color.Success">@item.AvailableStockQuantity.ToString("N2")</MudText></td>
    </tr>
    <tr>
        <td>Staging:</td>
        <td><MudText Color="Color.Info">@item.StagingStockQuantity.ToString("N2")</MudText></td>
    </tr>
</MudSimpleTable>
```

## ??? Database Migration

```bash
# Generate migration
dotnet ef migrations add AddMultiDimensionalStockTracking -p HeuristicLogix.Api

# Apply migration
dotnet ef database update -p HeuristicLogix.Api
```

## ? Validation Rules

```csharp
// Prevent negative available stock
CurrentStockQuantity >= ReservedStockQuantity

// All quantities >= 0
CurrentStockQuantity >= 0
ReservedStockQuantity >= 0
StagingStockQuantity >= 0
```

## ?? Optional Fields

| Field | Type | Example | Purpose |
|-------|------|---------|---------|
| **LocationCode** | string? | "P1-T10-A" | Warehouse location |
| **ImageUrl** | string? | "/images/product.jpg" | Product image |

---

**Next**: Generate EF Core migration and apply to database
