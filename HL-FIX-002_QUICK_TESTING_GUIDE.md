# HL-FIX-002 Quick Testing Guide

## ?? Start Testing

### 1. Start API
```powershell
cd HeuristicLogix.Api
dotnet run
```

### 2. Start Client
```powershell
cd HeuristicLogix.Client
dotnet run
```

### 3. Open Browser Console (F12)
Navigate to: `https://localhost:5001/inventory/categories`

---

## ? Test Scenarios

### Test 1: Simple Create ?
1. Click "Nueva Categoría"
2. **Check Console**: Should log `CategoryId=0`
3. Enter: "Electrónicos"
4. Click "Crear"
5. **Expected**: ? Success, new row appears

### Test 2: Simple Edit ?
1. Click edit icon on any row
2. **Check Console**: Should log `CategoryId=[actual ID]`
3. Change name
4. Click "Actualizar"
5. **Expected**: ? Success, changes visible

### Test 3: Create After Edit (CRITICAL ??)
**This was failing before the fix**

1. Click edit on Category ID=3
2. Modify name to "Category 3 Updated"
3. Click "Actualizar" ? ? Works
4. **Immediately** click "Nueva Categoría"
5. **Check Console**: Should show `CategoryId=0` (NOT 3!)
6. Enter: "New After Edit"
7. Click "Crear"
8. **Expected**: ? Success (Before fix: 400 BadRequest)

### Test 4: Edit After Create (CRITICAL ??)
**This was also failing before the fix**

1. Click "Nueva Categoría"
2. Enter: "Fresh Category"
3. Click "Crear" ? ? Creates ID=10
4. **Immediately** click edit on Category ID=5
5. **Check Console**: Should show `CategoryId=5` (NOT 0 or 10!)
6. Change name to "Category 5 Updated"
7. Click "Actualizar"
8. **Expected**: ? Success (Before fix: 400 BadRequest or ID mismatch)

### Test 5: Rapid Operations
1. Create "Cat A" ? ?
2. Edit "Cat A" ? ?
3. Create "Cat B" ? ?
4. Edit "Cat B" ? ?
5. Delete "Cat A" ? ?
6. Create "Cat C" ? ?
7. **Expected**: All operations succeed without errors

---

## ?? Console Output Examples

### Successful Create
```
[CategoryMaintenanceService] POST api/inventory/categories
[CategoryMaintenanceService] DTO: CategoryId=0, CategoryName=Electrónicos
```
? **CategoryId=0** (correct for create)

### Successful Update
```
[CategoryMaintenanceService] PUT api/inventory/categories/5
[CategoryMaintenanceService] ID param: 5
[CategoryMaintenanceService] DTO: CategoryId=5, CategoryName=Updated Name
```
? **ID param and DTO ID match** (correct for update)

### Error Case (Would occur without fix)
```
[CategoryMaintenanceService] POST api/inventory/categories
[CategoryMaintenanceService] DTO: CategoryId=5, CategoryName=New Category
```
? **CategoryId=5 on POST** (incorrect - should be 0!)

---

## ?? What to Look For

### ? Good Signs
- Create always shows `CategoryId=0` in console
- Update shows matching IDs in URL and DTO
- No 400 BadRequest errors
- Table refreshes automatically after each operation
- Success messages appear (green snackbar)

### ? Bad Signs (Before Fix)
- Create shows `CategoryId=[non-zero]` in console
- 400 BadRequest errors
- "ID mismatch between URL and body" error message
- Operations fail after switching between create/edit

---

## ?? Expected Results

| Operation | Before Fix | After Fix |
|-----------|------------|-----------|
| Create | ? 50% success | ? 100% success |
| Edit | ? 50% success | ? 100% success |
| Delete | ? 100% success | ? 100% success |
| Create?Edit | ? Fails | ? Works |
| Edit?Create | ? Fails | ? Works |

---

## ?? Units of Measure Testing

Repeat all tests on: `https://localhost:5001/inventory/units`

1. Create: Name="Kilogramo", Symbol="kg"
2. Edit existing unit
3. Create after edit (critical)
4. Edit after create (critical)
5. Rapid operations

**Console should show**:
```
[UnitOfMeasureMaintenanceService] POST api/inventory/unitsofmeasure
[UnitOfMeasureMaintenanceService] DTO: UnitOfMeasureId=0, Name=Kilogramo, Symbol=kg
```

---

## ? Troubleshooting

### Issue: Still getting 400 BadRequest
**Check**:
1. Clear browser cache (Ctrl+Shift+Del)
2. Hard reload (Ctrl+F5)
3. Restart both API and Client
4. Check console for `CategoryId=0` on create

### Issue: Console logs not appearing
**Check**:
1. Open F12 DevTools
2. Go to Console tab
3. Ensure "Verbose" level is enabled
4. Look for `[CategoryMaintenanceService]` prefix

### Issue: ID not resetting
**Verify**:
1. `ResetEditor()` method exists in page
2. `OnDialogOpening="@ResetEditor"` is wired to MaintenanceBase
3. Build was successful after changes

---

## ?? Success Criteria

All boxes checked = **FIX VERIFIED**

- [ ] Create works reliably
- [ ] Edit works reliably
- [ ] Delete works reliably
- [ ] Create after edit works
- [ ] Edit after create works
- [ ] Console shows correct CategoryId values
- [ ] No 400 BadRequest errors
- [ ] Table refreshes automatically

---

**Status**: Ready for Testing  
**Expected Duration**: 10-15 minutes  
**Priority**: High (Production Blocker Fix)
