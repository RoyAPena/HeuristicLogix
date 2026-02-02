# HL-FIX-007 Critical Testing Script

Write-Host "=== HL-FIX-007: JavaScript Initialization Order Fix ===" -ForegroundColor Cyan
Write-Host "CRITICAL FIX: Script loading order correction`n" -ForegroundColor Red

$testsPassed = 0
$testsFailed = 0

# Test 1: Script Order in index.html
Write-Host "[1/3] Checking script loading order..." -ForegroundColor Yellow
$indexHtml = Get-Content "HeuristicLogix.Client\wwwroot\index.html" -Raw

# Find positions of both scripts
$mudBlazorPos = $indexHtml.IndexOf('<script src="_content/MudBlazor/MudBlazor.min.js"></script>')
$blazorWasmPos = $indexHtml.IndexOf('<script src="_framework/blazor.webassembly.js"></script>')

if ($mudBlazorPos -lt $blazorWasmPos -and $mudBlazorPos -gt 0 -and $blazorWasmPos -gt 0) {
    Write-Host "     ? CORRECT ORDER:" -ForegroundColor Green
    Write-Host "        1. MudBlazor.min.js at position: $mudBlazorPos" -ForegroundColor Gray
    Write-Host "        2. blazor.webassembly.js at position: $blazorWasmPos" -ForegroundColor Gray
    $testsPassed++
} else {
    Write-Host "     ? WRONG ORDER or scripts not found!" -ForegroundColor Red
    Write-Host "        MudBlazor at: $mudBlazorPos" -ForegroundColor Red
    Write-Host "        Blazor at: $blazorWasmPos" -ForegroundColor Red
    $testsFailed++
}

# Test 2: InvokeAsync in MaintenanceBase
Write-Host "`n[2/3] Checking InvokeAsync usage..." -ForegroundColor Yellow
$maintenanceBase = Get-Content "HeuristicLogix.Client\Shared\MaintenanceBase.razor" -Raw

if ($maintenanceBase -match "InvokeAsync\(StateHasChanged\)") {
    Write-Host "     ? InvokeAsync(StateHasChanged) found" -ForegroundColor Green
    
    # Count occurrences
    $matches = [regex]::Matches($maintenanceBase, "InvokeAsync\(StateHasChanged\)")
    Write-Host "        Occurrences: $($matches.Count)" -ForegroundColor Gray
    $testsPassed++
} else {
    Write-Host "     ? InvokeAsync(StateHasChanged) NOT found" -ForegroundColor Red
    $testsFailed++
}

# Test 3: Build Status
Write-Host "`n[3/3] Building solution..." -ForegroundColor Yellow
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
Write-Host "Tests Passed: $testsPassed/3" -ForegroundColor Green
Write-Host "Tests Failed: $testsFailed/3" -ForegroundColor $(if ($testsFailed -gt 0) { "Red" } else { "Green" })

if ($testsFailed -eq 0) {
    Write-Host "`n? All tests passed! Ready for browser testing." -ForegroundColor Green
    
    Write-Host "`n=== CRITICAL FIX APPLIED ===" -ForegroundColor Cyan
    Write-Host "Issue: MudDialog not rendering despite DialogVisible = true" -ForegroundColor Yellow
    Write-Host "Root Cause: JavaScript loading order incorrect" -ForegroundColor Yellow
    
    Write-Host "`nBEFORE (BROKEN):" -ForegroundColor Red
    Write-Host "  <script src=`"_framework/blazor.webassembly.js`"></script>" -ForegroundColor Red
    Write-Host "  <script src=`"_content/MudBlazor/MudBlazor.min.js`"></script>" -ForegroundColor Red
    Write-Host "  ? Blazor initializes BEFORE MudBlazor JS is loaded" -ForegroundColor Red
    Write-Host "  ? mudDialog.open() undefined" -ForegroundColor Red
    Write-Host "  ? Dialog doesn't render (silent failure)" -ForegroundColor Red
    
    Write-Host "`nAFTER (FIXED):" -ForegroundColor Green
    Write-Host "  <script src=`"_content/MudBlazor/MudBlazor.min.js`"></script>" -ForegroundColor Green
    Write-Host "  <script src=`"_framework/blazor.webassembly.js`"></script>" -ForegroundColor Green
    Write-Host "  ? MudBlazor JS registers interop FIRST" -ForegroundColor Green
    Write-Host "  ? THEN Blazor initializes" -ForegroundColor Green
    Write-Host "  ? Dialog renders correctly" -ForegroundColor Green
    
    Write-Host "`n=== WHY THIS MATTERS ===" -ForegroundColor Cyan
    Write-Host "MudDialog requires JavaScript interop for:" -ForegroundColor White
    Write-Host "  • Dialog overlay creation" -ForegroundColor Gray
    Write-Host "  • Semi-transparent backdrop" -ForegroundColor Gray
    Write-Host "  • Focus trap management" -ForegroundColor Gray
    Write-Host "  • Scroll locking" -ForegroundColor Gray
    Write-Host "  • ESC key handling" -ForegroundColor Gray
    
    Write-Host "`nIf MudBlazor JS loads AFTER Blazor:" -ForegroundColor White
    Write-Host "  ? Components try to call mudDialog.open()" -ForegroundColor Gray
    Write-Host "  ? Function doesn't exist yet" -ForegroundColor Gray
    Write-Host "  ? Silent failure (no error, no dialog)" -ForegroundColor Gray
    
    Write-Host "`n=== CRITICAL: HARD REFRESH REQUIRED ===" -ForegroundColor Red -BackgroundColor Yellow
    Write-Host "Browser caches the old script order!" -ForegroundColor Yellow
    Write-Host "`nSTEPS TO TEST:" -ForegroundColor Cyan
    Write-Host "1. CLOSE ALL BROWSER WINDOWS" -ForegroundColor White
    Write-Host "2. Ctrl+Shift+Del ? Clear cache ? All time" -ForegroundColor White
    Write-Host "3. Start API: cd HeuristicLogix.Api; dotnet run" -ForegroundColor White
    Write-Host "4. Start Client: cd HeuristicLogix.Client; dotnet run" -ForegroundColor White
    Write-Host "5. Open NEW browser window" -ForegroundColor White
    Write-Host "6. Navigate: https://localhost:5001/inventory/categories" -ForegroundColor White
    Write-Host "7. F12 ? Console tab" -ForegroundColor White
    Write-Host "8. Click 'Nueva Categoría'" -ForegroundColor White
    
    Write-Host "`n=== EXPECTED RESULTS ===" -ForegroundColor Yellow
    Write-Host "Console Verification:" -ForegroundColor Gray
    Write-Host "  Type: console.log(typeof window.mudDialog)" -ForegroundColor DarkGray
    Write-Host "  Expected: 'object' (not 'undefined')" -ForegroundColor DarkGray
    
    Write-Host "`nConsole Logs:" -ForegroundColor Gray
    Write-Host "  [MaintenanceBase-Categoría] OpenCreateDialog called" -ForegroundColor DarkGray
    Write-Host "  [CategoryPage] ResetEditor called" -ForegroundColor DarkGray
    Write-Host "  [MaintenanceBase-Categoría] Setting DialogVisible = true" -ForegroundColor DarkGray
    Write-Host "  [MaintenanceBase-Categoría] Dialog should now be visible" -ForegroundColor DarkGray
    
    Write-Host "`nVisual Results:" -ForegroundColor Gray
    Write-Host "  ? Dialog appears immediately" -ForegroundColor DarkGray
    Write-Host "  ? Smooth fade-in + slide-up animation" -ForegroundColor DarkGray
    Write-Host "  ? Semi-transparent backdrop" -ForegroundColor DarkGray
    Write-Host "  ? Title: 'Nuevo Categoría'" -ForegroundColor DarkGray
    Write-Host "  ? One field visible and focused" -ForegroundColor DarkGray
    Write-Host "  ? Can type in field" -ForegroundColor DarkGray
    Write-Host "  ? ESC key closes dialog" -ForegroundColor DarkGray
    Write-Host "  ? Clicking backdrop closes dialog" -ForegroundColor DarkGray
    
    Write-Host "`n=== TEST FULL CRUD ===" -ForegroundColor Yellow
    Write-Host "CREATE:" -ForegroundColor Gray
    Write-Host "  1. Dialog opens ?" -ForegroundColor DarkGray
    Write-Host "  2. Enter 'Electrónicos'" -ForegroundColor DarkGray
    Write-Host "  3. Click 'Crear'" -ForegroundColor DarkGray
    Write-Host "  4. Dialog closes with animation ?" -ForegroundColor DarkGray
    Write-Host "  5. Green toast appears ?" -ForegroundColor DarkGray
    Write-Host "  6. Table refreshes ?" -ForegroundColor DarkGray
    
    Write-Host "`nEDIT:" -ForegroundColor Gray
    Write-Host "  1. Click pencil icon" -ForegroundColor DarkGray
    Write-Host "  2. Dialog opens ?" -ForegroundColor DarkGray
    Write-Host "  3. Title: 'Editar Categoría'" -ForegroundColor DarkGray
    Write-Host "  4. Field populated ?" -ForegroundColor DarkGray
    Write-Host "  5. Modify and save" -ForegroundColor DarkGray
    Write-Host "  6. Changes visible ?" -ForegroundColor DarkGray
    
    Write-Host "`nDELETE:" -ForegroundColor Gray
    Write-Host "  1. Click trash icon" -ForegroundColor DarkGray
    Write-Host "  2. Confirmation dialog ?" -ForegroundColor DarkGray
    Write-Host "  3. Confirm deletion" -ForegroundColor DarkGray
    Write-Host "  4. Success toast ?" -ForegroundColor DarkGray
    Write-Host "  5. Row removed ?" -ForegroundColor DarkGray
    
    Write-Host "`n=== TROUBLESHOOTING ===" -ForegroundColor Yellow
    Write-Host "If dialog STILL doesn't appear:" -ForegroundColor Gray
    
    Write-Host "`n1. Verify script order in browser:" -ForegroundColor White
    Write-Host "   F12 ? Elements ? Inspect <body> ? Look at <script> tags" -ForegroundColor DarkGray
    Write-Host "   Should see MudBlazor.min.js ABOVE blazor.webassembly.js" -ForegroundColor DarkGray
    
    Write-Host "`n2. Check browser console:" -ForegroundColor White
    Write-Host "   console.log(typeof window.mudDialog)" -ForegroundColor DarkGray
    Write-Host "   If 'undefined': Cache not cleared or wrong order" -ForegroundColor DarkGray
    
    Write-Host "`n3. Try incognito mode:" -ForegroundColor White
    Write-Host "   Ctrl+Shift+N (Chrome/Edge)" -ForegroundColor DarkGray
    Write-Host "   Navigate to app" -ForegroundColor DarkGray
    Write-Host "   If works: Cache issue" -ForegroundColor DarkGray
    
    Write-Host "`n4. Check Network tab:" -ForegroundColor White
    Write-Host "   F12 ? Network ? Reload" -ForegroundColor DarkGray
    Write-Host "   Verify both scripts load: 200 OK" -ForegroundColor DarkGray
    Write-Host "   Verify order: MudBlazor first, Blazor second" -ForegroundColor DarkGray
    
} else {
    Write-Host "`n? Some tests failed. Fix errors above before testing." -ForegroundColor Red
}

Write-Host "`n=== TECHNICAL DETAILS ===" -ForegroundColor Cyan
Write-Host "Loading Timeline (CORRECT):" -ForegroundColor Yellow
Write-Host "  T0: Browser parses HTML" -ForegroundColor White
Write-Host "  T1: MudBlazor.min.js downloads and executes" -ForegroundColor White
Write-Host "  T2: mudDialog, mudPopover, etc. registered globally" -ForegroundColor White
Write-Host "  T3: blazor.webassembly.js downloads and executes" -ForegroundColor White
Write-Host "  T4: Blazor WebAssembly runtime initializes" -ForegroundColor White
Write-Host "  T5: Components can safely use MudBlazor interop ?" -ForegroundColor Green

Write-Host "`nLoading Timeline (WRONG - Before Fix):" -ForegroundColor Yellow
Write-Host "  T0: Browser parses HTML" -ForegroundColor White
Write-Host "  T1: blazor.webassembly.js downloads and executes" -ForegroundColor White
Write-Host "  T2: Blazor runtime initializes" -ForegroundColor White
Write-Host "  T3: Components try to use mudDialog ? undefined" -ForegroundColor Red
Write-Host "  T4: MudBlazor.min.js finally loads ? TOO LATE" -ForegroundColor Red

Write-Host "`nFor detailed documentation, see: HL-FIX-007_JAVASCRIPT_INITIALIZATION_ORDER.md" -ForegroundColor Gray
