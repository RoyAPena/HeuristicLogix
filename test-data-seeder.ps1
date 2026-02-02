# ============================================================
# HeuristicLogix ERP - Test Data Seeder
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "HeuristicLogix ERP - Data Seeder Test" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$apiUrl = "http://localhost:5000"
$seedEndpoint = "$apiUrl/api/seed"
$statusEndpoint = "$apiUrl/api/seed/status"

Write-Host "Step 1: Building API..." -ForegroundColor Green
Push-Location HeuristicLogix.Api
try {
    dotnet build --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   ? Build failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Write-Host "   ? Build successful" -ForegroundColor White
} finally {
    Pop-Location
}
Write-Host ""

Write-Host "Step 2: Start API (Ctrl+C to stop after testing)..." -ForegroundColor Yellow
Write-Host "   Run in separate terminal: cd HeuristicLogix.Api && dotnet run" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter when API is running"

Write-Host "Step 3: Checking seed status..." -ForegroundColor Green
try {
    $statusResponse = Invoke-RestMethod -Uri $statusEndpoint -Method Get
    Write-Host "   Current status:" -ForegroundColor White
    Write-Host "   - Is Seeded: $($statusResponse.isSeeded)" -ForegroundColor Cyan
    Write-Host "   - Categories: $($statusResponse.recordCounts.categories)" -ForegroundColor Cyan
    Write-Host "   - Items: $($statusResponse.recordCounts.items)" -ForegroundColor Cyan
} catch {
    Write-Host "   ??  Could not connect to API. Is it running?" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "Step 4: Seeding database..." -ForegroundColor Green
try {
    $seedResponse = Invoke-RestMethod -Uri $seedEndpoint -Method Post
    
    if ($seedResponse.success) {
        Write-Host "   ? Seeding successful!" -ForegroundColor Green
        
        if ($seedResponse.alreadySeeded) {
            Write-Host "   ??  Database was already seeded" -ForegroundColor Cyan
        } else {
            Write-Host "   ?? Seeded Records:" -ForegroundColor Cyan
            Write-Host "      - Tax Configurations: $($seedResponse.details.taxConfigurations)" -ForegroundColor White
            Write-Host "      - Units of Measure: $($seedResponse.details.unitsOfMeasure)" -ForegroundColor White
            Write-Host "      - Categories: $($seedResponse.details.categories)" -ForegroundColor White
            Write-Host "      - Brands: $($seedResponse.details.brands)" -ForegroundColor White
            Write-Host "      - Items: $($seedResponse.details.items)" -ForegroundColor White
            Write-Host "      - Item Unit Conversions: $($seedResponse.details.itemUnitConversions)" -ForegroundColor White
            Write-Host "      - Suppliers: $($seedResponse.details.suppliers)" -ForegroundColor White
            Write-Host "      - Item-Supplier Links: $($seedResponse.details.itemSuppliers)" -ForegroundColor White
            Write-Host "   Total: $($seedResponse.totalRecords) records" -ForegroundColor Green
            
            Write-Host ""
            Write-Host "   ? Verification:" -ForegroundColor Green
            Write-Host "      $($seedResponse.verification.hybridIDs)" -ForegroundColor White
            Write-Host "      $($seedResponse.verification.foreignKeys)" -ForegroundColor White
            Write-Host "      $($seedResponse.verification.decimalPrecision)" -ForegroundColor White
            Write-Host "      $($seedResponse.verification.compositePKs)" -ForegroundColor White
        }
    } else {
        Write-Host "   ? Seeding failed: $($seedResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ? Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "Step 5: Verifying seeded data..." -ForegroundColor Green
try {
    $statusResponse = Invoke-RestMethod -Uri $statusEndpoint -Method Get
    Write-Host "   Final status:" -ForegroundColor White
    Write-Host "   - Is Seeded: $($statusResponse.isSeeded)" -ForegroundColor Cyan
    Write-Host "   - Tax Configurations: $($statusResponse.recordCounts.taxConfigurations)" -ForegroundColor White
    Write-Host "   - Units of Measure: $($statusResponse.recordCounts.unitsOfMeasure)" -ForegroundColor White
    Write-Host "   - Categories: $($statusResponse.recordCounts.categories)" -ForegroundColor White
    Write-Host "   - Brands: $($statusResponse.recordCounts.brands)" -ForegroundColor White
    Write-Host "   - Items: $($statusResponse.recordCounts.items)" -ForegroundColor White
    Write-Host "   - Item Unit Conversions: $($statusResponse.recordCounts.itemUnitConversions)" -ForegroundColor White
    Write-Host "   - Suppliers: $($statusResponse.recordCounts.suppliers)" -ForegroundColor White
    Write-Host "   - Item-Supplier Links: $($statusResponse.recordCounts.itemSuppliers)" -ForegroundColor White
} catch {
    Write-Host "   ??  Could not verify status" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "? Data Seeder Test Complete!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Run database verification script" -ForegroundColor Yellow
Write-Host "   sqlcmd -S LAPTOP-7MG6K7RV -d HeuristicLogix -E -i verify-database-schema.sql" -ForegroundColor Cyan
Write-Host ""
