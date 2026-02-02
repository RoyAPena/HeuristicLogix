# HL-FIX-001 Testing Guide

## ?? Quick Start

### 1. Start the API
```powershell
cd HeuristicLogix.Api
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7086
```

### 2. Start the Client (new terminal)
```powershell
cd HeuristicLogix.Client
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### 3. Open Browser
Navigate to: `https://localhost:5001`

---

## ? Verification Steps

### Navigation Menu
- [x] Sidebar shows "Módulos ERP" header
- [x] Only one group visible: "Inventario"
- [x] Two links visible:
  - Categorías (Category icon)
  - Unidades de Medida (Scale icon)
- [x] No other modules visible (Compras, Ventas, Logística removed)

### Categories Page (`/inventory/categories`)

#### Initial Load
- [x] Page loads without errors
- [x] Table displays with headers: ID, Nombre de Categoría, Acciones
- [x] ID column uses monospace font (Roboto Mono)
- [x] Table is dense (8px cell padding)
- [x] Rows have hover effect
- [x] Alternating row colors (striped)

#### Create Operation
1. Click **"Nueva Categoría"** button (Amber color)
2. Dialog opens with title "Nuevo Categoría"
3. Enter name: `"Electrónicos"`
4. Click **"Crear"** button
5. **Expected**:
   - Dialog closes
   - Green success message appears
   - Table refreshes automatically
   - New row appears in table

#### Edit Operation
1. Click **pencil icon** on any row
2. Dialog opens with title "Editar Categoría"
3. Name field populated with current value
4. Modify name (e.g., add " Actualizado")
5. Click **"Actualizar"** button
6. **Expected**:
   - Dialog closes
   - Green success message appears
   - Table refreshes automatically
   - Changes visible in table

#### Delete Operation
1. Click **trash icon** on any row
2. Confirmation dialog appears: "¿Eliminar '[Name]'?"
3. Click **"Eliminar"** button
4. **Expected**:
   - Dialog closes
   - Green success message appears
   - Table refreshes automatically
   - Row removed from table

### Units of Measure Page (`/inventory/units`)

#### Initial Load
- [x] Page loads without errors
- [x] Table displays with headers: ID, Nombre, Símbolo, Acciones
- [x] ID column uses monospace font
- [x] Símbolo column uses monospace font
- [x] Table is dense, hover, striped

#### Create Operation
1. Click **"Nueva Unidad"** button (Amber color)
2. Dialog opens with title "Nuevo Unidad de Medida"
3. Enter:
   - Nombre: `"Kilogramo"`
   - Símbolo: `"kg"`
4. Click **"Crear"** button
5. **Expected**:
   - Dialog closes
   - Success message appears
   - Table refreshes
   - New row visible

#### Edit Operation
1. Click **pencil icon** on any row
2. Dialog opens with both fields populated
3. Modify values
4. Click **"Actualizar"**
5. **Expected**:
   - Updates saved
   - Table refreshes
   - Changes visible

#### Delete Operation
1. Click **trash icon**
2. Confirm deletion
3. **Expected**:
   - Row removed
   - Table refreshes

---

## ?? API Direct Testing (Optional)

### Using PowerShell / curl

#### Get All Categories
```powershell
Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" -Method GET
```

#### Create Category
```powershell
$body = @{
    categoryName = "Electrónicos"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

#### Update Category
```powershell
$body = @{
    categoryId = 1
    categoryName = "Electrónicos Actualizado"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories/1" `
    -Method PUT `
    -Body $body `
    -ContentType "application/json"
```

#### Delete Category
```powershell
Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories/1" -Method DELETE
```

### Expected HTTP Status Codes
- **200 OK**: GET, PUT successful
- **201 Created**: POST successful
- **204 No Content**: DELETE successful
- **400 Bad Request**: Validation failed
- **404 Not Found**: Entity doesn't exist
- **409 Conflict**: Duplicate name or in use

---

## ?? Visual Standards Verification

### Typography
- [x] Regular text: **Inter** font
- [x] ID fields: **Roboto Mono** (monospace)
- [x] Symbol/Code fields: **Roboto Mono** (monospace)

### Colors
- [x] Primary buttons: **#FF8F00** (Alert Amber)
- [x] Secondary buttons: **#283593** (Deep Steel Blue)
- [x] AppBar: **#1A237E** (Midnight Navy)
- [x] Drawer: **#F8F9FA** (Technical Grey)

### Spacing
- [x] Table cell padding: **8px**
- [x] Input fields: **Dense** (compact)
- [x] Card padding: **20px**

### Components
- [x] All inputs use `HLTextField` (auto-styled)
- [x] No manual `Variant.Outlined` needed
- [x] Buttons follow Industrial Steel theme

---

## ? Common Issues & Solutions

### Issue: API doesn't start
**Solution**: 
```powershell
# Check if port 7086 is in use
netstat -ano | findstr :7086

# Kill process if needed
taskkill /PID <PID> /F
```

### Issue: Client can't connect to API
**Check**:
1. API is running on port 7086
2. `appsettings.json` has correct URL
3. CORS is enabled in API

### Issue: Table doesn't refresh after save
**Check**:
1. Network tab shows successful API call (200/201)
2. `LoadItems()` is called in `MaintenanceBase`
3. No JavaScript errors in console

### Issue: Validation errors don't show
**Check**:
1. Validator is injected in page
2. Validator is passed to `MaintenanceBase`
3. FluentValidation rules are defined

---

## ? Success Criteria

All boxes checked = **READY FOR PRODUCTION**

### Functional
- [ ] Categories: Create, Read, Update, Delete work
- [ ] Units: Create, Read, Update, Delete work
- [ ] Tables refresh automatically after operations
- [ ] Validation messages display correctly
- [ ] Error messages are user-friendly

### Visual
- [ ] Navigation shows only 2 items
- [ ] Tables use dense spacing (8px padding)
- [ ] IDs/Codes use monospace font
- [ ] Colors match HL-UI-001 spec
- [ ] Buttons use correct colors (Amber/Blue)

### Technical
- [ ] API responds on port 7086
- [ ] Client connects to API correctly
- [ ] REST endpoints follow standards
- [ ] DTOs validated before entity mapping
- [ ] No console errors or warnings

---

## ?? Performance Benchmarks

### Expected Response Times
- **GetAll**: < 500ms
- **GetById**: < 200ms
- **Create**: < 500ms
- **Update**: < 500ms
- **Delete**: < 300ms

### Load Testing (Optional)
```powershell
# Test 100 concurrent requests
for ($i=1; $i -le 100; $i++) {
    Start-Job -ScriptBlock {
        Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories"
    }
}
```

---

## ?? Next Testing Phase

After basic CRUD verified:
1. **Integration Tests**: Automated API tests
2. **E2E Tests**: Playwright/Selenium UI tests
3. **Load Tests**: JMeter/k6 performance tests
4. **Security Tests**: OWASP ZAP vulnerability scan
5. **Accessibility**: WCAG 2.1 AA compliance

---

**Status**: Ready for Testing  
**Last Updated**: January 2025  
**Standards**: HL-UI-001 + HL-FIX-001
