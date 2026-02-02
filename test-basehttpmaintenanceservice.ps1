# HL-FIX-004 Quick Testing Script

Write-Host "=== HL-FIX-004: BaseHttpMaintenanceService Testing ===" -ForegroundColor Cyan
Write-Host "Testing direct HTTP communication with API on port 7086`n" -ForegroundColor Gray

$testsPassed = 0
$testsFailed = 0

# Test 1: API Accessibility
Write-Host "[1/5] Testing API accessibility..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" -Method GET -ErrorAction Stop
    Write-Host "     ? API is accessible" -ForegroundColor Green
    Write-Host "        Categories found: $($response.Count)" -ForegroundColor Gray
    $testsPassed++
} catch {
    Write-Host "     ? API not accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "        Start API: cd HeuristicLogix.Api; dotnet run" -ForegroundColor Yellow
    $testsFailed++
}

# Test 2: BaseHttpMaintenanceService File
Write-Host "`n[2/5] Checking BaseHttpMaintenanceService..." -ForegroundColor Yellow
$serviceFile = "HeuristicLogix.Shared\Services\BaseHttpMaintenanceService.cs"
if (Test-Path $serviceFile) {
    Write-Host "     ? BaseHttpMaintenanceService.cs exists" -ForegroundColor Green
    $content = Get-Content $serviceFile -Raw
    if ($content -match "class BaseHttpMaintenanceService" -and 
        $content -match "IBaseMaintenanceService" -and
        $content -match "HttpClient") {
        Write-Host "     ? Implementation looks correct" -ForegroundColor Green
        $testsPassed++
    } else {
        Write-Host "     ? Implementation incomplete" -ForegroundColor Red
        $testsFailed++
    }
} else {
    Write-Host "     ? File not found: $serviceFile" -ForegroundColor Red
    $testsFailed++
}

# Test 3: CategoryPage Updated
Write-Host "`n[3/5] Checking CategoryPage..." -ForegroundColor Yellow
$categoryPage = "HeuristicLogix.Client\Features\Inventory\Maintenances\CategoryPage.razor"
if (Test-Path $categoryPage) {
    $content = Get-Content $categoryPage -Raw
    if ($content -match "@inject HttpClient" -and 
        $content -match "BaseHttpMaintenanceService" -and
        $content -match '"categories"') {
        Write-Host "     ? CategoryPage uses BaseHttpMaintenanceService" -ForegroundColor Green
        $testsPassed++
    } else {
        Write-Host "     ? CategoryPage not updated correctly" -ForegroundColor Red
        $testsFailed++
    }
} else {
    Write-Host "     ? CategoryPage not found" -ForegroundColor Red
    $testsFailed++
}

# Test 4: UnitOfMeasurePage Updated
Write-Host "`n[4/5] Checking UnitOfMeasurePage..." -ForegroundColor Yellow
$unitPage = "HeuristicLogix.Client\Features\Inventory\Maintenances\UnitOfMeasurePage.razor"
if (Test-Path $unitPage) {
    $content = Get-Content $unitPage -Raw
    if ($content -match "@inject HttpClient" -and 
        $content -match "BaseHttpMaintenanceService" -and
        $content -match '"unitsofmeasure"') {
        Write-Host "     ? UnitOfMeasurePage uses BaseHttpMaintenanceService" -ForegroundColor Green
        $testsPassed++
    } else {
        Write-Host "     ? UnitOfMeasurePage not updated correctly" -ForegroundColor Red
        $testsFailed++
    }
} else {
    Write-Host "     ? UnitOfMeasurePage not found" -ForegroundColor Red
    $testsFailed++
}

# Test 5: Build Status
Write-Host "`n[5/5] Building solution..." -ForegroundColor Yellow
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
Write-Host "Tests Passed: $testsPassed/5" -ForegroundColor Green
Write-Host "Tests Failed: $testsFailed/5" -ForegroundColor $(if ($testsFailed -gt 0) { "Red" } else { "Green" })

if ($testsFailed -eq 0) {
    Write-Host "`n? All tests passed! Ready for manual testing." -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Ensure API is running: cd HeuristicLogix.Api; dotnet run" -ForegroundColor White
    Write-Host "2. Start Client: cd HeuristicLogix.Client; dotnet run" -ForegroundColor White
    Write-Host "3. Clear browser cache (Ctrl+Shift+Del)" -ForegroundColor White
    Write-Host "4. Navigate to: https://localhost:5001/inventory/categories" -ForegroundColor White
    Write-Host "5. Open F12 Console" -ForegroundColor White
    Write-Host "6. Click 'Nueva Categoría' and verify:" -ForegroundColor White
    Write-Host "   - Console shows: [MaintenanceBase-Categoría] OpenCreateDialog called" -ForegroundColor Gray
    Write-Host "   - Dialog appears with one field" -ForegroundColor Gray
    Write-Host "   - Field is empty and functional" -ForegroundColor Gray
} else {
    Write-Host "`n? Some tests failed. Review errors above." -ForegroundColor Red
}

# Console Output Examples
Write-Host "`n=== Expected Console Output ===" -ForegroundColor Cyan
Write-Host "When you click 'Nueva Categoría', you should see:" -ForegroundColor Yellow
Write-Host "[MaintenanceBase-Categoría] OpenCreateDialog called" -ForegroundColor Gray
Write-Host "[CategoryPage] ResetEditor called" -ForegroundColor Gray
Write-Host "[MaintenanceBase-Categoría] Setting DialogVisible = true" -ForegroundColor Gray
Write-Host "[MaintenanceBase-Categoría] Dialog should now be visible" -ForegroundColor Gray

Write-Host "`nWhen you create a category, you should see:" -ForegroundColor Yellow
Write-Host "[CategoryPage] GetDto called: ID=0, Name=Test Category" -ForegroundColor Gray
Write-Host "[Category] POST api/inventory/categories" -ForegroundColor Gray
Write-Host "[Category] DTO: {`"categoryId`":0,`"categoryName`":`"Test Category`"}" -ForegroundColor Gray
Write-Host "[Category] POST Success: Entity created" -ForegroundColor Gray

Write-Host "`nFor detailed documentation, see: HL-FIX-004_BASEHTTPMAINTENANCESERVICE_COMPLETE.md" -ForegroundColor Gray
