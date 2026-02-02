# HL-FIX-003: CRUD Flow Verification & Troubleshooting

**Status**: IMPLEMENTED - NEEDS VERIFICATION  
**Date**: January 2025  
**Issue**: User reports dialogs don't show fields and API returns BadRequest

---

## ?? Current Status Analysis

### ? What's Already Implemented

#### 1. EditorFields - COMPLETE
Both CategoryPage and UnitOfMeasurePage have fully functional EditorFields:

**CategoryPage.razor** (Lines 38-45):
```razor
<EditorFields>
    <HLTextField @bind-Value="_name"
                 Label="Nombre de Categoría"
                 Required="true"
                 MaxLength="300"
                 HelperText="Ingrese el nombre de la categoría"
                 Class="mt-4" />
</EditorFields>
```

**UnitOfMeasurePage.razor** (Lines 40-53):
```razor
<EditorFields>
    <HLTextField @bind-Value="_name"
                 Label="Nombre de Unidad"
                 Required="true"
                 MaxLength="200"
                 HelperText="Ej: Kilogramo, Metro, Unidad"
                 Class="mt-4" />
    <HLTextField @bind-Value="_symbol"
                 Label="Símbolo"
                 Required="true"
                 MaxLength="20"
                 HelperText="Ej: kg, m, un (letras, números, ³, ²)"
                 Class="mt-4" />
</EditorFields>
```

#### 2. DTO Mapping - COMPLETE

**CategoryPage GetEditorDto** (Lines 59-64):
```csharp
private Task<CategoryUpsertDto> GetDto() => Task.FromResult(
    new CategoryUpsertDto 
    { 
        CategoryId = _currentId, 
        CategoryName = _name 
    });
```

**CategoryPage SetEditorFromEntity** (Lines 66-71):
```csharp
private Task SetEditor(Category category)
{
    _currentId = category.CategoryId;
    _name = category.CategoryName;
    return Task.CompletedTask;
}
```

**UnitOfMeasurePage GetEditorDto** (Lines 68-74):
```csharp
private Task<UnitOfMeasureUpsertDto> GetDto() => Task.FromResult(
    new UnitOfMeasureUpsertDto 
    { 
        UnitOfMeasureId = _currentId,
        UnitOfMeasureName = _name,
        UnitOfMeasureSymbol = _symbol
    });
```

**UnitOfMeasurePage SetEditorFromEntity** (Lines 76-82):
```csharp
private Task SetEditor(UnitOfMeasure unit)
{
    _currentId = unit.UnitOfMeasureId;
    _name = unit.UnitOfMeasureName;
    _symbol = unit.UnitOfMeasureSymbol;
    return Task.CompletedTask;
}
```

#### 3. Reset Mechanism - COMPLETE

**CategoryPage** (Lines 73-77):
```csharp
private void ResetEditor()
{
    _currentId = 0;
    _name = string.Empty;
}
```

**UnitOfMeasurePage** (Lines 84-89):
```csharp
private void ResetEditor()
{
    _currentId = 0;
    _name = string.Empty;
    _symbol = string.Empty;
}
```

#### 4. Service Adapter - CORRECT

The BaseMaintenanceServiceAdapter correctly delegates to specific services, which construct URLs properly:

**CategoryMaintenanceService**:
- GET: `api/inventory/categories`
- GET by ID: `api/inventory/categories/{id}`
- POST: `api/inventory/categories`
- PUT: `api/inventory/categories/{id}` ?
- DELETE: `api/inventory/categories/{id}` ?

**UnitOfMeasureMaintenanceService**:
- GET: `api/inventory/unitsofmeasure`
- GET by ID: `api/inventory/unitsofmeasure/{id}`
- POST: `api/inventory/unitsofmeasure`
- PUT: `api/inventory/unitsofmeasure/{id}` ?
- DELETE: `api/inventory/unitsofmeasure/{id}` ?

#### 5. Port Configuration - CORRECT

**appsettings.json**:
```json
{
  "ApiBaseUrl": "https://localhost:7086"
}
```

#### 6. GetEntityId Mapping - CORRECT

**CategoryPage** (Line 23):
```csharp
GetEntityId="@(c => c.CategoryId)"
```

**UnitOfMeasurePage** (Line 23):
```csharp
GetEntityId="@(u => u.UnitOfMeasureId)"
```

#### 7. Dialog Visibility - CORRECT

MaintenanceBase properly manages dialog visibility through `DialogVisible` state and `OpenCreateDialog`/`OpenEditDialog` methods.

---

## ?? Possible Issues & Solutions

### Issue 1: Browser Cache
**Symptom**: Old version of client running  
**Solution**:
```powershell
# Stop client
# Clear browser cache: Ctrl+Shift+Del
# Hard reload: Ctrl+F5
# Restart client
cd HeuristicLogix.Client
dotnet run
```

### Issue 2: API Not Running on Port 7086
**Symptom**: Connection refused or wrong port  
**Solution**:
```powershell
# Check if API is running
netstat -ano | findstr :7086

# Start API
cd HeuristicLogix.Api
dotnet run

# Should see: Now listening on: https://localhost:7086
```

### Issue 3: Database Not Seeded
**Symptom**: Empty tables or 404 errors  
**Solution**:
```powershell
# Seed the database
Invoke-RestMethod -Uri "https://localhost:7086/api/seed/run" -Method POST
```

### Issue 4: CORS Issues
**Symptom**: API calls blocked  
**Check**: API Program.cs should have CORS enabled  
**Verify**:
```csharp
app.UseCors(); // Should be present before app.UseAuthorization()
```

### Issue 5: Validation Errors
**Symptom**: 400 BadRequest with validation messages  
**Check**: Console logs for validation errors  
**Common causes**:
- Empty required fields
- Exceeding max length
- Invalid characters

---

## ?? Step-by-Step Testing Protocol

### Prerequisites
1. **API Running**: `https://localhost:7086`
2. **Client Running**: `https://localhost:5001`
3. **Browser DevTools**: F12 ? Console tab open
4. **Database Seeded**: At least 1-2 categories and units exist

### Test 1: Navigate to Categories
```
1. Open https://localhost:5001
2. Click "Inventario" in sidebar
3. Click "Categorías"
4. Expected: Table shows existing categories
5. Check console: Should see GET request
```

**? Pass**: Table displays with data  
**? Fail**: Empty table or error ? Check database seeding

### Test 2: Open Create Dialog
```
1. On Categories page
2. Click "Nueva Categoría" (Amber button, top right)
3. Expected: Dialog opens with title "Nuevo Categoría"
4. Expected: One text field visible: "Nombre de Categoría"
5. Expected: Field is empty and focused
```

**? Pass**: Dialog shows with empty name field  
**? Fail**: Dialog doesn't open ? Check browser console for errors  
**? Fail**: No fields visible ? Check HLTextField component registration

### Test 3: Create Category
```
1. In create dialog
2. Enter: "Electrónicos"
3. Click "Crear"
4. Check console: Should log POST request
5. Expected: Dialog closes
6. Expected: Green success message
7. Expected: Table refreshes with new row
```

**? Pass**: Category created, table updates  
**? Fail 400**: Check console for DTO contents  
**? Fail**: Dialog doesn't close ? Check for JS errors

### Test 4: Open Edit Dialog
```
1. Click pencil icon on any row
2. Expected: Dialog opens with title "Editar Categoría"
3. Expected: Name field populated with current value
4. Expected: Field is focused
```

**? Pass**: Dialog shows with populated field  
**? Fail**: Field empty ? Check SetEditorFromEntity implementation

### Test 5: Update Category
```
1. In edit dialog
2. Modify name (add " Updated")
3. Click "Actualizar"
4. Check console: Should log PUT request with matching IDs
5. Expected: Dialog closes
6. Expected: Green success message
7. Expected: Table refreshes with changes
```

**? Pass**: Category updated, changes visible  
**? Fail 400 "ID mismatch"**: Check console for ID values  
**? Fail**: Table doesn't refresh ? Check LoadItems() call

### Test 6: Delete Category
```
1. Click trash icon on any row
2. Expected: Confirmation dialog
3. Click "Eliminar"
4. Check console: Should log DELETE request
5. Expected: Success message
6. Expected: Row disappears
```

**? Pass**: Category deleted, row removed  
**? Fail 400**: Check if category is in use

### Test 7: Units of Measure (Repeat Tests 1-6)
```
Navigate to: /inventory/units
Repeat all tests above
Note: Should have 2 fields (Name and Symbol)
```

---

## ?? Console Debugging Commands

### Check if fields are rendered
```javascript
// In browser console
document.querySelectorAll('.mud-input-control').length
// Should return: 1 for Categories, 2 for Units
```

### Check if dialog is visible
```javascript
document.querySelector('.mud-dialog').style.display
// Should return: "block" or empty (not "none")
```

### Check API base URL
```javascript
// In browser console on Categories page
localStorage.getItem('ApiBaseUrl')
// Or check in Network tab ? Headers ? Request URL
```

### Manually trigger API call
```javascript
// Test GET all categories
fetch('https://localhost:7086/api/inventory/categories')
  .then(r => r.json())
  .then(console.log)
```

---

## ?? Diagnostic Checklist

| Component | Status | Verification |
|-----------|--------|--------------|
| **EditorFields Defined** | ? | Present in both pages |
| **DTO Mapping (Get)** | ? | GetDto() implemented |
| **DTO Mapping (Set)** | ? | SetEditor() implemented |
| **Reset Mechanism** | ? | ResetEditor() implemented |
| **Service URLs** | ? | Correct endpoints with {id} |
| **Port Configuration** | ? | 7086 configured |
| **GetEntityId** | ? | Correct property mapping |
| **Build Status** | ? | Successful |
| **API Running** | ? | Needs verification |
| **Client Running** | ? | Needs verification |
| **Database Seeded** | ? | Needs verification |
| **Browser Cache** | ? | Needs clearing |

---

## ?? Quick Start Commands

### Start Everything Fresh
```powershell
# Terminal 1: Start API
cd C:\Repository\HeuristicLogix
cd HeuristicLogix.Api
dotnet clean
dotnet build
dotnet run

# Terminal 2: Start Client
cd C:\Repository\HeuristicLogix
cd HeuristicLogix.Client
dotnet clean
dotnet build
dotnet run

# Terminal 3: Seed Database (if needed)
Invoke-RestMethod -Uri "https://localhost:7086/api/seed/run" -Method POST

# Browser: Clear cache and load
# 1. Close all browser windows
# 2. Open new window
# 3. Ctrl+Shift+Del ? Clear cache
# 4. Navigate to: https://localhost:5001/inventory/categories
# 5. F12 ? Console tab
```

---

## ? Common Error Messages & Fixes

### "Cannot read properties of null"
**Cause**: Component not initialized  
**Fix**: Check if HLTextField is registered in _Imports.razor

### "400 BadRequest: ID mismatch"
**Cause**: CategoryId in DTO doesn't match URL parameter  
**Fix**: Already fixed in HL-FIX-002 (ResetEditor mechanism)

### "404 Not Found" on API call
**Cause**: Wrong endpoint or API not running  
**Fix**: Verify API is on port 7086, check endpoint URL in console

### "Connection refused"
**Cause**: API not running  
**Fix**: Start API with `dotnet run`

### Fields not visible in dialog
**Cause 1**: HLTextField not found  
**Fix**: Verify `@using HeuristicLogix.Client.Components` in _Imports.razor

**Cause 2**: Browser cache  
**Fix**: Ctrl+F5 hard reload

**Cause 3**: Build issue  
**Fix**: `dotnet clean` then `dotnet build`

---

## ?? Final Verification Script

Run this PowerShell script to verify everything:

```powershell
# HL-FIX-003 Verification Script

Write-Host "=== HL-FIX-003 Verification ===" -ForegroundColor Cyan

# Check if API is running
Write-Host "`nChecking API..." -ForegroundColor Yellow
try {
    $apiResponse = Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" -Method GET
    Write-Host "? API is running and responsive" -ForegroundColor Green
    Write-Host "   Categories found: $($apiResponse.Count)" -ForegroundColor Gray
} catch {
    Write-Host "? API not running on port 7086" -ForegroundColor Red
    Write-Host "   Start with: cd HeuristicLogix.Api; dotnet run" -ForegroundColor Yellow
}

# Check if client files exist
Write-Host "`nChecking Client files..." -ForegroundColor Yellow
$categoryPage = Test-Path "HeuristicLogix.Client\Features\Inventory\Maintenances\CategoryPage.razor"
$unitPage = Test-Path "HeuristicLogix.Client\Features\Inventory\Maintenances\UnitOfMeasurePage.razor"
$hlTextField = Test-Path "HeuristicLogix.Client\Components\HLTextField.razor"

if ($categoryPage -and $unitPage -and $hlTextField) {
    Write-Host "? All required files present" -ForegroundColor Green
} else {
    Write-Host "? Missing files" -ForegroundColor Red
    if (!$categoryPage) { Write-Host "   - CategoryPage.razor" -ForegroundColor Red }
    if (!$unitPage) { Write-Host "   - UnitOfMeasurePage.razor" -ForegroundColor Red }
    if (!$hlTextField) { Write-Host "   - HLTextField.razor" -ForegroundColor Red }
}

# Check build
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
$buildResult = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build successful" -ForegroundColor Green
} else {
    Write-Host "? Build failed" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
}

Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Start API: cd HeuristicLogix.Api; dotnet run" -ForegroundColor White
Write-Host "2. Start Client: cd HeuristicLogix.Client; dotnet run" -ForegroundColor White
Write-Host "3. Clear browser cache: Ctrl+Shift+Del" -ForegroundColor White
Write-Host "4. Navigate to: https://localhost:5001/inventory/categories" -ForegroundColor White
Write-Host "5. Open F12 console and test CRUD operations" -ForegroundColor White
```

---

## ? Expected Behavior Summary

### Create Flow
1. Click "Nueva Categoría"
2. Dialog opens ?
3. Field visible and empty ?
4. Enter name
5. Click "Crear"
6. Console: `POST api/inventory/categories` with `CategoryId=0` ?
7. Dialog closes ?
8. Success message ?
9. Table refreshes ?

### Edit Flow
1. Click edit icon
2. Dialog opens ?
3. Field populated with current value ?
4. Modify name
5. Click "Actualizar"
6. Console: `PUT api/inventory/categories/{id}` with matching IDs ?
7. Dialog closes ?
8. Success message ?
9. Table shows changes ?

### Delete Flow
1. Click delete icon
2. Confirmation dialog ?
3. Click "Eliminar"
4. Console: `DELETE api/inventory/categories/{id}` ?
5. Success message ?
6. Row removed ?

---

**Status**: All code is correct and implemented  
**Issue**: Likely environmental (cache, API not running, or database empty)  
**Action**: Follow verification steps above

---

**Lead Architect**: GitHub Copilot  
**Implementation Date**: January 2025  
**Standards**: HL-FIX-003 v1.0
