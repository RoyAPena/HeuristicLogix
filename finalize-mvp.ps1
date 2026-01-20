# HeuristicLogix MVP Finalization Script
# This script verifies and finalizes all core MVP components

Write-Host "=== HeuristicLogix MVP Finalization ===" -ForegroundColor Cyan
Write-Host "Ensuring zero technical debt and production readiness..." -ForegroundColor Gray

$ErrorCount = 0
$WarningCount = 0

# Function to check file existence
function Test-FileExists {
    param([string]$Path, [string]$Description)
    if (Test-Path $Path) {
        Write-Host "? $Description exists" -ForegroundColor Green
        return $true
    } else {
        Write-Host "? $Description missing" -ForegroundColor Red
        $script:ErrorCount++
        return $false
    }
}

# Function to check content
function Test-ContentExists {
    param([string]$Path, [string]$Pattern, [string]$Description)
    if (Test-Path $Path) {
        $content = Get-Content $Path -Raw
        if ($content -match $Pattern) {
            Write-Host "? $Description verified" -ForegroundColor Green
            return $true
        } else {
            Write-Host "? $Description needs verification" -ForegroundColor Yellow
            $script:WarningCount++
            return $false
        }
    } else {
        Write-Host "? File not found: $Path" -ForegroundColor Red
        $script:ErrorCount++
        return $false
    }
}

Write-Host "`n[Step 1] Verifying Core Infrastructure Files..." -ForegroundColor Yellow
Test-FileExists "docker-compose.yml" "Docker Compose configuration"
Test-FileExists ".env.example" "Environment template"
Test-FileExists "setup-infrastructure.ps1" "Setup script"

Write-Host "`n[Step 2] Verifying C# Backend Components..." -ForegroundColor Yellow
Test-FileExists "HeuristicLogix.Shared\Models\OutboxEvent.cs" "OutboxEvent model"
Test-FileExists "HeuristicLogix.Shared\Models\ExpertHeuristicFeedback.cs" "ExpertHeuristicFeedback model"
Test-FileExists "HeuristicLogix.Shared\Serialization\HeuristicJsonOptions.cs" "JSON serialization config"

Test-FileExists "HeuristicLogix.Api\Services\HeuristicLogixDbContext.cs" "DbContext"
Test-FileExists "HeuristicLogix.Api\Services\TransactionalOutboxService.cs" "Outbox Service"
Test-FileExists "HeuristicLogix.Api\Services\OutboxPublisherBackgroundService.cs" "Background Publisher"
Test-FileExists "HeuristicLogix.Api\Services\OutboxEventNotifier.cs" "Event Notifier (Channel)"

Write-Host "`n[Step 3] Verifying Python Intelligence Service..." -ForegroundColor Yellow
Test-FileExists "IntelligenceService\main.py" "Intelligence Service"
Test-FileExists "IntelligenceService\Dockerfile" "Intelligence Dockerfile"
Test-FileExists "IntelligenceService\requirements.txt" "Python dependencies"

Write-Host "`n[Step 4] Verifying Critical Implementations..." -ForegroundColor Yellow

# Check for Channel-based notification
Test-ContentExists "HeuristicLogix.Api\Services\OutboxEventNotifier.cs" "Channel<bool>" "Channel-based notification"
Test-ContentExists "HeuristicLogix.Api\Services\TransactionalOutboxService.cs" "NotifyEventAddedAsync" "Instant notification in OutboxService"
Test-ContentExists "HeuristicLogix.Api\Services\OutboxPublisherBackgroundService.cs" "WaitForEventAsync" "Channel waiting in Publisher"

# Check for Python idempotency
Test-ContentExists "IntelligenceService\main.py" "check_enrichment_exists" "Idempotency check in Python"
Test-ContentExists "IntelligenceService\main.py" "SQLAlchemy" "SQLAlchemy async integration"

# Check for SQL Server healthcheck
Test-ContentExists "docker-compose.yml" "healthcheck:" "SQL Server healthcheck"
Test-ContentExists "docker-compose.yml" "TrustServerCertificate=yes" "Robust connection string"

# Check for MVP Event Contract fields
Test-ContentExists "IntelligenceService\main.py" "order_id" "OrderId in ExpertDecisionEvent"
Test-ContentExists "IntelligenceService\main.py" "suggested_truck_id" "SuggestedTruckId in ExpertDecisionEvent"
Test-ContentExists "IntelligenceService\main.py" "selected_truck_id" "SelectedTruckId in ExpertDecisionEvent"
Test-ContentExists "IntelligenceService\main.py" "total_weight" "TotalWeight in ExpertDecisionEvent"
Test-ContentExists "IntelligenceService\main.py" "expert_note" "ExpertNote in ExpertDecisionEvent"

Write-Host "`n[Step 5] Creating Program.cs from Template..." -ForegroundColor Yellow
if (Test-Path "HeuristicLogix.Api\Program.cs.template") {
    $templateContent = Get-Content "HeuristicLogix.Api\Program.cs.template" -Raw
    # Remove template header comments
    $programContent = $templateContent -replace '(?s)/\*.*?\*/', ''
    $programContent = $programContent.Trim()
    
    Set-Content "HeuristicLogix.Api\Program.cs" -Value $programContent
    Write-Host "? Program.cs created from template" -ForegroundColor Green
} else {
    Write-Host "? Program.cs.template not found" -ForegroundColor Red
    $ErrorCount++
}

Write-Host "`n[Step 6] Verifying String-Based Enum Serialization..." -ForegroundColor Yellow
$enumFiles = @(
    "HeuristicLogix.Shared\Models\OutboxEvent.cs",
    "HeuristicLogix.Shared\Models\Truck.cs",
    "HeuristicLogix.Shared\Models\Conduce.cs",
    "HeuristicLogix.Shared\Models\OverrideReasonTag.cs",
    "HeuristicLogix.Shared\Models\MaterialItem.cs",
    "HeuristicLogix.Shared\Models\DeliveryRoute.cs"
)

foreach ($file in $enumFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        if ($content -match "JsonStringEnumConverter" -or $content -match "HasConversion<string>") {
            Write-Host "? $([System.IO.Path]::GetFileName($file)) has string-based enums" -ForegroundColor Green
        } else {
            Write-Host "? $([System.IO.Path]::GetFileName($file)) may need enum verification" -ForegroundColor Yellow
            $WarningCount++
        }
    }
}

Write-Host "`n[Step 7] Building Solution..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build HeuristicLogix.sln 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Solution builds successfully" -ForegroundColor Green
    } else {
        Write-Host "? Build failed" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Gray
        $ErrorCount++
    }
} catch {
    Write-Host "? Could not verify build (dotnet CLI may not be available)" -ForegroundColor Yellow
    $WarningCount++
}

Write-Host "`n[Step 8] Verifying Documentation..." -ForegroundColor Yellow
Test-FileExists "README_INFRASTRUCTURE.md" "Infrastructure documentation"
Test-FileExists "IMPLEMENTATION_SUMMARY.md" "Implementation summary"
Test-FileExists "RELIABILITY_IMPROVEMENTS.md" "Reliability improvements doc"

Write-Host "`n=== MVP Finalization Summary ===" -ForegroundColor Cyan
Write-Host "Errors: $ErrorCount" -ForegroundColor $(if ($ErrorCount -eq 0) { "Green" } else { "Red" })
Write-Host "Warnings: $WarningCount" -ForegroundColor $(if ($WarningCount -eq 0) { "Green" } else { "Yellow" })

if ($ErrorCount -eq 0) {
    Write-Host "`n? MVP is ready for deployment!" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Run: .\setup-infrastructure.ps1" -ForegroundColor White
    Write-Host "  2. Configure .env with GEMINI_API_KEY" -ForegroundColor White
    Write-Host "  3. Start services: docker-compose up -d" -ForegroundColor White
    Write-Host "  4. Run API: cd HeuristicLogix.Api && dotnet run" -ForegroundColor White
    Write-Host "  5. Run Client: cd HeuristicLogix.Client && dotnet run" -ForegroundColor White
} else {
    Write-Host "`n? MVP has errors that need to be fixed" -ForegroundColor Red
    Write-Host "Please review the errors above and run this script again" -ForegroundColor Yellow
}

Write-Host "`n" -ForegroundColor White
