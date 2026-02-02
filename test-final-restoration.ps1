# HL-FIX-005 Quick Testing Script

Write-Host "=== HL-FIX-005: Final Maintenance Module Testing ===" -ForegroundColor Cyan
Write-Host "Testing dialog visibility and error handling`n" -ForegroundColor Gray

$testsPassed = 0
$testsFailed = 0

# Test 1: MainLayout Providers
Write-Host "[1/6] Checking MainLayout providers..." -ForegroundColor Yellow
$mainLayout = Get-Content "HeuristicLogix.Client\MainLayout.razor" -Raw
if ($mainLayout -match "<MudDialogProvider" -and $mainLayout -match "<MudSnackbarProvider") {
    Write-Host "     ? MudDialogProvider and MudSnackbarProvider present" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "     ? Providers missing or incomplete" -ForegroundColor Red
    $testsFailed++
}

# Test 2: MaintenanceBase Dialog Binding
Write-Host "`n[2/6] Checking MaintenanceBase dialog binding..." -ForegroundColor Yellow
$maintenanceBase = Get-Content "HeuristicLogix.Client\Shared\MaintenanceBase.razor" -Raw
if ($maintenanceBase -match "@bind-IsVisible=`"@DialogVisible`"") {
    Write-Host "     ? Dialog binding correct: @bind-IsVisible=`"@DialogVisible`"" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "     ? Dialog binding incorrect" -ForegroundColor Red
    $testsFailed++
}

# Test 3: Enhanced Error Handling
Write-Host "`n[3/6] Checking HandleHttpError enhancement..." -ForegroundColor Yellow
if ($maintenanceBase -match "JsonDocument.Parse" -and 
    $maintenanceBase -match "foreign key" -and
    $maintenanceBase -match "constraint") {
    Write-Host "     ? Advanced error handling with JSON parsing" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "     ? Error handling not enhanced" -ForegroundColor Red
    $testsFailed++
}

# Test 4: BaseHttpMaintenanceService DELETE
Write-Host "`n[4/6] Checking BaseHttpMaintenanceService DELETE..." -ForegroundColor Yellow
$baseService = Get-Content "HeuristicLogix.Shared\Services\BaseHttpMaintenanceService.cs" -Raw
if ($baseService -match "DELETE.*{id}" -and $baseService -match "throw new HttpRequestException") {
    Write-Host "     ? DELETE includes ID in URL and throws detailed errors" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "     ? DELETE implementation incomplete" -ForegroundColor Red
    $testsFailed++
}

# Test 5: API Accessibility
Write-Host "`n[5/6] Testing API accessibility..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" -Method GET -ErrorAction Stop
    Write-Host "     ? API accessible on port 7086" -ForegroundColor Green
    Write-Host "        Categories found: $($response.Count)" -ForegroundColor Gray
    $testsPassed++
} catch {
    Write-Host "     ? API not accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "        Start API: cd HeuristicLogix.Api; dotnet run" -ForegroundColor Yellow
    $testsFailed++
}

# Test 6: Build Status
Write-Host "`n[6/6] Building solution..." -ForegroundColor Yellow
$buildOutput = dotnet build --no-restore 2>&1 | Out-String
if ($LASTEXITCODE -eq 0) {
    Write-Host "     ? Build successful" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "     ? Build failed" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Red
    $testsFailed++
}

# Summary
Write-Host "`n=== TEST SUMMARY ===" -ForegroundColor Cyan
Write-Host "Tests Passed: $testsPassed/6" -ForegroundColor Green
Write-Host "Tests Failed: $testsFailed/6" -ForegroundColor $(if ($testsFailed -gt 0) { "Red" } else { "Green" })

if ($testsFailed -eq 0) {
    Write-Host "`n? All tests passed! Ready for manual testing." -ForegroundColor Green
    
    Write-Host "`n=== CRITICAL FIXES ===" -ForegroundColor Cyan
    Write-Host "1. Dialog Binding: Fixed @bind-IsVisible binding" -ForegroundColor Green
    Write-Host "2. Error Handling: Enhanced with JSON parsing" -ForegroundColor Green
    Write-Host "3. Foreign Key Errors: Show specific server messages" -ForegroundColor Green
    Write-Host "4. DELETE Logic: Includes ID in URL and detailed errors" -ForegroundColor Green
    
    Write-Host "`n=== MANUAL TESTING STEPS ===" -ForegroundColor Cyan
    Write-Host "1. Start API: cd HeuristicLogix.Api; dotnet run" -ForegroundColor White
    Write-Host "2. Start Client: cd HeuristicLogix.Client; dotnet run" -ForegroundColor White
    Write-Host "3. Clear browser cache (Ctrl+Shift+Del)" -ForegroundColor White
    Write-Host "4. Navigate to: https://localhost:5001/inventory/categories" -ForegroundColor White
    Write-Host "5. Open F12 Console" -ForegroundColor White
    
    Write-Host "`n=== TEST SCENARIO 1: Dialog Visibility ===" -ForegroundColor Yellow
    Write-Host "Action: Click 'Nueva Categoría'" -ForegroundColor Gray
    Write-Host "Expected:" -ForegroundColor Gray
    Write-Host "  - Console: [MaintenanceBase-Categoría] OpenCreateDialog called" -ForegroundColor DarkGray
    Write-Host "  - Console: [MaintenanceBase-Categoría] Setting DialogVisible = true" -ForegroundColor DarkGray
    Write-Host "  - UI: Dialog appears with one field" -ForegroundColor DarkGray
    Write-Host "  - UI: Field is empty and functional" -ForegroundColor DarkGray
    
    Write-Host "`n=== TEST SCENARIO 2: Foreign Key Error ===" -ForegroundColor Yellow
    Write-Host "Action: Try to delete a category with associated items" -ForegroundColor Gray
    Write-Host "Expected:" -ForegroundColor Gray
    Write-Host "  - Console: [Category] DELETE api/inventory/categories/X" -ForegroundColor DarkGray
    Write-Host "  - Console: [Category] DELETE Failed: 400" -ForegroundColor DarkGray
    Write-Host "  - Console: [MaintenanceBase] Extracted message: No se puede eliminar..." -ForegroundColor DarkGray
    Write-Host "  - UI: Yellow warning with ?? icon" -ForegroundColor DarkGray
    Write-Host "  - UI: Specific message about foreign key constraint" -ForegroundColor DarkGray
    
    Write-Host "`n=== TEST SCENARIO 3: Successful CRUD ===" -ForegroundColor Yellow
    Write-Host "Create:" -ForegroundColor Gray
    Write-Host "  1. Click 'Nueva Categoría'" -ForegroundColor DarkGray
    Write-Host "  2. Enter 'Electrónicos'" -ForegroundColor DarkGray
    Write-Host "  3. Click 'Crear'" -ForegroundColor DarkGray
    Write-Host "  4. ? Success snackbar appears" -ForegroundColor DarkGray
    Write-Host "  5. ? Table refreshes with new row" -ForegroundColor DarkGray
    
    Write-Host "`nEdit:" -ForegroundColor Gray
    Write-Host "  1. Click pencil icon on any row" -ForegroundColor DarkGray
    Write-Host "  2. Modify name" -ForegroundColor DarkGray
    Write-Host "  3. Click 'Actualizar'" -ForegroundColor DarkGray
    Write-Host "  4. ? Success snackbar appears" -ForegroundColor DarkGray
    Write-Host "  5. ? Changes visible in table" -ForegroundColor DarkGray
    
    Write-Host "`nDelete (no constraints):" -ForegroundColor Gray
    Write-Host "  1. Click trash icon" -ForegroundColor DarkGray
    Write-Host "  2. Confirm deletion" -ForegroundColor DarkGray
    Write-Host "  3. ? Success snackbar appears" -ForegroundColor DarkGray
    Write-Host "  4. ? Row removed from table" -ForegroundColor DarkGray
    
} else {
    Write-Host "`n? Some tests failed. Review errors above." -ForegroundColor Red
}

Write-Host "`n=== KEY CHANGES ===" -ForegroundColor Cyan
Write-Host "1. Dialog Binding:" -ForegroundColor Yellow
Write-Host "   Before: @bind-IsVisible=`"DialogVisible`"  ?" -ForegroundColor Red
Write-Host "   After:  @bind-IsVisible=`"@DialogVisible`" ?" -ForegroundColor Green

Write-Host "`n2. Error Handling:" -ForegroundColor Yellow
Write-Host "   Before: Generic 'Error de validación'" -ForegroundColor Red
Write-Host "   After:  'No se puede eliminar X porque está asociado a Y artículos'" -ForegroundColor Green

Write-Host "`n3. DELETE URL:" -ForegroundColor Yellow
Write-Host "   Format: DELETE api/inventory/categories/{id} ?" -ForegroundColor Green

Write-Host "`nFor detailed documentation, see: HL-FIX-005_FINAL_RESTORATION_COMPLETE.md" -ForegroundColor Gray
