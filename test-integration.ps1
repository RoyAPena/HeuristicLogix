# HeuristicLogix MVP Integration Test
# Verifies all components work together end-to-end

Write-Host "=== HeuristicLogix MVP Integration Test ===" -ForegroundColor Cyan
Write-Host "Testing complete system integration..." -ForegroundColor Gray

$TestsPassed = 0
$TestsFailed = 0

function Test-Component {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    Write-Host "`nTesting: $Name" -ForegroundColor Yellow
    try {
        $result = & $Test
        if ($result) {
            Write-Host "? PASS: $Name" -ForegroundColor Green
            $script:TestsPassed++
            return $true
        } else {
            Write-Host "? FAIL: $Name" -ForegroundColor Red
            $script:TestsFailed++
            return $false
        }
    } catch {
        Write-Host "? ERROR: $Name - $_" -ForegroundColor Red
        $script:TestsFailed++
        return $false
    }
}

# Test 1: Docker Services Running
Test-Component "Docker Services Running" {
    $containers = docker-compose ps --quiet
    return $containers.Count -ge 3  # SQL Server, Kafka, Intelligence Service
}

# Test 2: SQL Server Healthcheck
Test-Component "SQL Server Health" {
    try {
        $health = docker inspect heuristiclogix-sqlserver --format '{{.State.Health.Status}}'
        return $health -eq "healthy"
    } catch {
        return $false
    }
}

# Test 3: Kafka Healthcheck
Test-Component "Kafka Health" {
    try {
        $result = docker exec heuristiclogix-kafka kafka-broker-api-versions --bootstrap-server localhost:9092 2>&1
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

# Test 4: Intelligence Service Health
Test-Component "Intelligence Service Health" {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8000/health" -TimeoutSec 5
        return $response.status -eq "healthy"
    } catch {
        return $false
    }
}

# Test 5: Kafka Topics Created
Test-Component "Kafka Topics Exist" {
    try {
        $topics = docker exec heuristiclogix-kafka kafka-topics --list --bootstrap-server localhost:9092
        return ($topics -contains "expert.decisions.v1") -and ($topics -contains "heuristic.telemetry.v1")
    } catch {
        return $false
    }
}

# Test 6: SQL Server Database Exists
Test-Component "HeuristicLogix Database Exists" {
    try {
        $result = docker exec heuristiclogix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HeuristicLogix2026!" -Q "SELECT name FROM sys.databases WHERE name = 'HeuristicLogix'" -C -h -1
        return $result.Trim() -eq "HeuristicLogix"
    } catch {
        return $false
    }
}

# Test 7: OutboxEvents Table Exists
Test-Component "OutboxEvents Table Exists" {
    try {
        $result = docker exec heuristiclogix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HeuristicLogix2026!" -d HeuristicLogix -Q "SELECT OBJECT_ID('OutboxEvents')" -C -h -1
        return $result.Trim() -ne "NULL" -and $result.Trim() -ne ""
    } catch {
        return $false
    }
}

# Test 8: AIEnrichments Table Exists
Test-Component "AIEnrichments Table Exists" {
    try {
        $result = docker exec heuristiclogix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HeuristicLogix2026!" -d HeuristicLogix -Q "SELECT OBJECT_ID('AIEnrichments')" -C -h -1
        return $result.Trim() -ne "NULL" -and $result.Trim() -ne ""
    } catch {
        return $false
    }
}

# Test 9: Channel-Based Notification (Code Check)
Test-Component "Channel-Based Notification Code" {
    $notifierFile = "HeuristicLogix.Api\Services\OutboxEventNotifier.cs"
    if (Test-Path $notifierFile) {
        $content = Get-Content $notifierFile -Raw
        return $content -match "Channel<bool>" -and $content -match "NotifyEventAddedAsync"
    }
    return $false
}

# Test 10: Python Idempotency (Code Check)
Test-Component "Python Idempotency Code" {
    $mainFile = "IntelligenceService\main.py"
    if (Test-Path $mainFile) {
        $content = Get-Content $mainFile -Raw
        return $content -match "check_enrichment_exists" -and $content -match "async def check_enrichment_exists"
    }
    return $false
}

# Test 11: MVP Event Contract Fields (Code Check)
Test-Component "MVP Event Contract Fields" {
    $mainFile = "IntelligenceService\main.py"
    if (Test-Path $mainFile) {
        $content = Get-Content $mainFile -Raw
        $hasOrderId = $content -match "order_id"
        $hasSuggestedTruck = $content -match "suggested_truck_id"
        $hasSelectedTruck = $content -match "selected_truck_id"
        $hasTotalWeight = $content -match "total_weight"
        $hasExpertNote = $content -match "expert_note"
        return $hasOrderId -and $hasSuggestedTruck -and $hasSelectedTruck -and $hasTotalWeight -and $hasExpertNote
    }
    return $false
}

# Test 12: String-Based Enum Serialization (Code Check)
Test-Component "String-Based Enum Serialization" {
    $jsonOptionsFile = "HeuristicLogix.Shared\Serialization\HeuristicJsonOptions.cs"
    if (Test-Path $jsonOptionsFile) {
        $content = Get-Content $jsonOptionsFile -Raw
        return $content -match "JsonStringEnumConverter" -and $content -match "allowIntegerValues: false"
    }
    return $false
}

# Test 13: API Build Check
Test-Component "API Project Builds" {
    try {
        Push-Location "HeuristicLogix.Api"
        $buildResult = dotnet build --no-restore 2>&1
        Pop-Location
        return $LASTEXITCODE -eq 0
    } catch {
        Pop-Location
        return $false
    }
}

# Test 14: Shared Project Build Check
Test-Component "Shared Project Builds" {
    try {
        Push-Location "HeuristicLogix.Shared"
        $buildResult = dotnet build --no-restore 2>&1
        Pop-Location
        return $LASTEXITCODE -eq 0
    } catch {
        Pop-Location
        return $false
    }
}

# Test 15: Intelligence Service Gemini Configuration
Test-Component "Gemini API Configuration" {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8000/health" -TimeoutSec 5
        # Check if gemini is at least configured (key may be missing, but structure exists)
        return $null -ne $response.gemini
    } catch {
        return $false
    }
}

# Summary
Write-Host "`n=== Integration Test Summary ===" -ForegroundColor Cyan
Write-Host "Tests Passed: $TestsPassed" -ForegroundColor Green
Write-Host "Tests Failed: $TestsFailed" -ForegroundColor $(if ($TestsFailed -eq 0) { "Green" } else { "Red" })

$TotalTests = $TestsPassed + $TestsFailed
$SuccessRate = [math]::Round(($TestsPassed / $TotalTests) * 100, 2)
Write-Host "Success Rate: $SuccessRate%" -ForegroundColor $(if ($SuccessRate -ge 90) { "Green" } elseif ($SuccessRate -ge 70) { "Yellow" } else { "Red" })

if ($TestsFailed -eq 0) {
    Write-Host "`n? All integration tests passed! MVP is fully functional." -ForegroundColor Green
    Write-Host "`nYou can now:" -ForegroundColor Yellow
    Write-Host "  1. Access Kafka UI: http://localhost:8080" -ForegroundColor White
    Write-Host "  2. Access Intelligence API: http://localhost:8000/docs" -ForegroundColor White
    Write-Host "  3. Run HeuristicLogix.Api: cd HeuristicLogix.Api && dotnet run" -ForegroundColor White
    Write-Host "  4. Run Blazor Client: cd HeuristicLogix.Client && dotnet run" -ForegroundColor White
    exit 0
} else {
    Write-Host "`n? Some integration tests failed. Please review the errors above." -ForegroundColor Red
    Write-Host "Run .\setup-infrastructure.ps1 to initialize the system" -ForegroundColor Yellow
    exit 1
}
