# Inventory Module - Quick Reference

## ?? Quick Start

### Start API
```powershell
cd HeuristicLogix.Api
dotnet run
```

### Test Endpoints

**Categories:**
```bash
# GET all
curl http://localhost:5000/api/inventory/categories

# GET by ID
curl http://localhost:5000/api/inventory/categories/1

# POST create
curl -X POST http://localhost:5000/api/inventory/categories \
  -H "Content-Type: application/json" \
  -d '{"categoryId": 0, "categoryName": "Electronics"}'

# PUT update
curl -X PUT http://localhost:5000/api/inventory/categories/1 \
  -H "Content-Type: application/json" \
  -d '{"categoryId": 1, "categoryName": "Electronics Updated"}'

# DELETE
curl -X DELETE http://localhost:5000/api/inventory/categories/1
```

**Units of Measure:**
```bash
# GET all
curl http://localhost:5000/api/inventory/unitsofmeasure

# POST create
curl -X POST http://localhost:5000/api/inventory/unitsofmeasure \
  -H "Content-Type: application/json" \
  -d '{"unitOfMeasureId": 0, "unitOfMeasureName": "Kilogram", "unitOfMeasureSymbol": "kg"}'
```

## ?? Module Structure

```
HeuristicLogix.Modules.Inventory/
??? Services/
?   ??? ICategoryService.cs
?   ??? CategoryService.cs
?   ??? IUnitOfMeasureService.cs
?   ??? UnitOfMeasureService.cs
??? Validators/
?   ??? CategoryValidator.cs
?   ??? UnitOfMeasureValidator.cs
??? InventoryModuleExtensions.cs
```

## ?? Registration

In `Program.cs`:
```csharp
builder.Services.AddInventoryModule();
```

## ? Features

- [x] CRUD operations
- [x] FluentValidation
- [x] Duplicate checking
- [x] Foreign key validation
- [x] RESTful API
- [x] Hybrid ID architecture (int)

## ?? Validation Rules

**Category:**
- Name: Required, max 300 chars
- Name: Cannot be duplicate

**UnitOfMeasure:**
- Name: Required, max 200 chars
- Symbol: Required, max 20 chars, alphanumeric
- Symbol: Cannot be duplicate

## ?? Status

**Backend:** ? Complete  
**Blazor UI:** ?? Templates provided  
**Testing:** Ready for integration
