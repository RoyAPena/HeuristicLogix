# HL-FIX-003 Verification Script
# Verifies that all CRUD components are properly configured

Write-Host "=== HL-FIX-003: CRUD Flow Verification ===" -ForegroundColor Cyan
Write-Host "Checking Category and Unit of Measure CRUD implementation...`n" -ForegroundColor Gray

$allPassed = $true

# Check 1: API Running
Write-Host "[1/8] Checking if API is running on port 7086..." -ForegroundColor Yellow
try {
    $apiResponse = Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" -Method GET -ErrorAction Stop
    Write-Host "     ? API is running and responsive" -ForegroundColor Green
    Write-Host "        Categories in database: $($apiResponse.Count)" -ForegroundColor Gray
} catch {
    Write-Host "     ? API not accessible on port 7086" -ForegroundColor Red
    Write-Host "        Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "        Action: cd HeuristicLogix.Api; dotnet run" -ForegroundColor Yellow
    $allPassed = $false
}

# Check 2: Required Files
Write-Host "`n[2/8] Checking required files..." -ForegroundColor Yellow
$requiredFiles = @(
    @{Path="HeuristicLogix.Client\Features\Inventory\Maintenances\CategoryPage.razor"; Name="CategoryPage"},
    @{Path="HeuristicLogix.Client\Features\Inventory\Maintenances\UnitOfMeasurePage.razor"; Name="UnitOfMeasurePage"},
    @{Path="HeuristicLogix.Client\Components\HLTextField.razor"; Name="HLTextField"},
    @{Path="HeuristicLogix.Client\Shared\MaintenanceBase.razor"; Name="MaintenanceBase"},
    @{Path="HeuristicLogix.Client\wwwroot\appsettings.json"; Name="appsettings.json"}
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file.Path) {
        Write-Host "     ? $($file.Name)" -ForegroundColor Green
    } else {
        Write-Host "     ? $($file.Name) - NOT FOUND" -ForegroundColor Red
        $allPassed = $false
    }
}

# Check 3: EditorFields in CategoryPage
Write-Host "`n[3/8] Verifying EditorFields in CategoryPage..." -ForegroundColor Yellow
$categoryContent = Get-Content "HeuristicLogix.Client\Features\Inventory\Maintenances\CategoryPage.razor" -Raw
if ($categoryContent -match "<EditorFields>" -and $categoryContent -match "HLTextField" -and $categoryContent -match "@bind-Value=`"_name`"") {
    Write-Host "     ? EditorFields properly implemented" -ForegroundColor Green
} else {
    Write-Host "     ? EditorFields missing or incomplete" -ForegroundColor Red
    $allPassed = $false
}

# Check 4: EditorFields in UnitOfMeasurePage
Write-Host "`n[4/8] Verifying EditorFields in UnitOfMeasurePage..." -ForegroundColor Yellow
$unitContent = Get-Content "HeuristicLogix.Client\Features\Inventory\Maintenances\UnitOfMeasurePage.razor" -Raw
if ($unitContent -match "<EditorFields>" -and $unitContent -match "HLTextField.*_name" -and $unitContent -match "HLTextField.*_symbol") {
    Write-Host "     ? EditorFields properly implemented (2 fields)" -ForegroundColor Green
} else {
    Write-Host "     ? EditorFields missing or incomplete" -ForegroundColor Red
    $allPassed = $false
}

# Check 5: DTO Mapping in CategoryPage
Write-Host "`n[5/8] Verifying DTO mapping in CategoryPage..." -ForegroundColor Yellow
if ($categoryContent -match "GetDto\(\)" -and $categoryContent -match "SetEditor\(Category category\)" -and $categoryContent -match "ResetEditor\(\)") {
    Write-Host "     ? GetDto, SetEditor, and ResetEditor implemented" -ForegroundColor Green
} else {
    Write-Host "     ? DTO mapping methods missing" -ForegroundColor Red
    $allPassed = $false
}

# Check 6: DTO Mapping in UnitOfMeasurePage
Write-Host "`n[6/8] Verifying DTO mapping in UnitOfMeasurePage..." -ForegroundColor Yellow
if ($unitContent -match "GetDto\(\)" -and $unitContent -match "SetEditor\(UnitOfMeasure unit\)" -and $unitContent -match "ResetEditor\(\)") {
    Write-Host "     ? GetDto, SetEditor, and ResetEditor implemented" -ForegroundColor Green
} else {
    Write-Host "     ? DTO mapping methods missing" -ForegroundColor Red
    $allPassed = $false
}

# Check 7: API Configuration
Write-Host "`n[7/8] Verifying API configuration..." -ForegroundColor Yellow
$appsettings = Get-Content "HeuristicLogix.Client\wwwroot\appsettings.json" | ConvertFrom-Json
if ($appsettings.ApiBaseUrl -eq "https://localhost:7086") {
    Write-Host "     ? API URL correctly configured: $($appsettings.ApiBaseUrl)" -ForegroundColor Green
} else {
    Write-Host "     ? API URL incorrect: $($appsettings.ApiBaseUrl)" -ForegroundColor Red
    Write-Host "        Expected: https://localhost:7086" -ForegroundColor Yellow
    $allPassed = $false
}

# Check 8: Build Status
Write-Host "`n[8/8] Building solution..." -ForegroundColor Yellow
$buildOutput = dotnet build --no-restore 2>&1 | Out-String
if ($LASTEXITCODE -eq 0) {
    Write-Host "     ? Build successful" -ForegroundColor Green
} else {
    Write-Host "     ? Build failed" -ForegroundColor Red
    Write-Host "        See errors above" -ForegroundColor Yellow
    $allPassed = $false
}

# Summary
Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "? All checks passed! CRUD flow is properly implemented." -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Ensure API is running: cd HeuristicLogix.Api; dotnet run" -ForegroundColor White
    Write-Host "2. Start Client: cd HeuristicLogix.Client; dotnet run" -ForegroundColor White
    Write-Host "3. Clear browser cache (Ctrl+Shift+Del)" -ForegroundColor White
    Write-Host "4. Navigate to: https://localhost:5001/inventory/categories" -ForegroundColor White
    Write-Host "5. Open F12 DevTools Console" -ForegroundColor White
    Write-Host "6. Test: Create ? Edit ? Delete" -ForegroundColor White
} else {
    Write-Host "? Some checks failed. Please review the errors above." -ForegroundColor Red
    Write-Host "`nRecommended actions:" -ForegroundColor Cyan
    Write-Host "1. Review HL-FIX-003_VERIFICATION_GUIDE.md" -ForegroundColor White
    Write-Host "2. Ensure all files are present" -ForegroundColor White
    Write-Host "3. Run: dotnet clean; dotnet build" -ForegroundColor White
    Write-Host "4. Start API and Client" -ForegroundColor White
}

Write-Host "`n=== Additional Diagnostics ===" -ForegroundColor Cyan

# Check if Units of Measure endpoint works
try {
    $unitsResponse = Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/unitsofmeasure" -Method GET -ErrorAction Stop
    Write-Host "? Units of Measure API: $($unitsResponse.Count) units found" -ForegroundColor Green
} catch {
    Write-Host "? Units of Measure API: Not accessible" -ForegroundColor Red
}

# Check for seed data
try {
    $categoriesResponse = Invoke-RestMethod -Uri "https://localhost:7086/api/inventory/categories" -Method GET -ErrorAction Stop
    if ($categoriesResponse.Count -eq 0) {
        Write-Host "??  Database is empty. Run seed: Invoke-RestMethod -Uri 'https://localhost:7086/api/seed/run' -Method POST" -ForegroundColor Yellow
    }
} catch {
    # Already reported above
}

Write-Host "`nFor detailed troubleshooting, see: HL-FIX-003_VERIFICATION_GUIDE.md" -ForegroundColor Gray
Write-Host "For quick testing steps, see: HL-FIX-002_QUICK_TESTING_GUIDE.md" -ForegroundColor Gray
