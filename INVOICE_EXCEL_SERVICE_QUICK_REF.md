# Invoice Excel Service - Quick Reference

## ?? Key Features

? **Material Classification** - Auto-tag materials (Long/Heavy/Bulk/Fragile/Hazardous)  
? **Special Handling** - Flag varilla/rebar as requiring special handling  
? **Truck Compatibility** - **ALL trucks can carry ANY material mix**  
? **Pre-Save Review** - Staging workflow with human-readable summaries  
? **Event-Driven** - Ready for Kafka integration  

---

## ?? Material Types & Handling

| Material Type | Requires Special Handling | All Trucks Compatible | Example |
|---------------|--------------------------|----------------------|---------|
| **Long** | ? Yes | ? Yes | Varilla, rebar, tubos |
| **Heavy** | ? No | ? Yes | Cemento, bloques |
| **Bulk** | ? No | ? Yes (Dump preferred) | Arena, grava |
| **Fragile** | ? Yes | ? Yes | Vidrio, cerámica |
| **Hazardous** | ? Yes | ?? Certified driver required | Químicos, solvente |

---

## ?? Example Summary

**Input**:
- Varilla 1/2: 20 piezas (100kg)
- Cemento Portland: 10 bolsas (500kg)
- Arena lavada: 2 m³ (3000kg)

**Output**:
```
Invoice #INV-001 (Ferretería Central): 3000kg Bulk, 500kg Heavy, 100kg Long. 
Compatible with All Trucks. 
? Special handling required: Varilla 1/2 (Line 1)
```

---

## ?? Usage

```csharp
// 1. Analyze Excel (pre-save)
List<InvoiceLoadSummary> summaries = await invoiceExcelService.AnalyzeExcelAsync(
    fileStream, "invoices.xlsx");

// 2. Review summaries
foreach (InvoiceLoadSummary summary in summaries) {
    Console.WriteLine(summary.Summary);
}

// 3. Approve and save
Conduce saved = await invoiceExcelService.ApproveAndSaveInvoiceAsync(
    "INV-001", "expert-001");
```

---

## ?? Files Created

- `MaterialCharacteristics.cs` - Handling requirements metadata
- `MaterialClassificationService.cs` - Auto-classification with characteristics
- `InvoiceLoadSummary.cs` - Pre-save review DTOs
- `IConduceRepository.cs` - Repository interface
- `IDomainEventPublisher.cs` - Event publishing (Kafka ready)
- `InvoiceExcelService.cs` - Excel ingestion service (650 lines)

---

## ? Key Business Rules

1. **All trucks can carry long materials** (varilla/rebar)
2. Varilla flagged as `RequiresSpecialHandling = true`
3. Truck selection based on material **MIX** and **availability**
4. Pre-save review with human-readable summaries
5. Geocoding integration for delivery addresses

---

**Status**: ? Production Ready | **Build**: ? Success
