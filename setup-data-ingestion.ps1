# Data Ingestion Setup Script
# Adds required NuGet packages for the Data Ingestion System

Write-Host "=== HeuristicLogix Data Ingestion Setup ===" -ForegroundColor Cyan
Write-Host "Adding required NuGet packages..." -ForegroundColor Gray

$ErrorCount = 0

# Add MiniExcel to HeuristicLogix.Api
Write-Host "`n[1/2] Adding MiniExcel to HeuristicLogix.Api..." -ForegroundColor Yellow
try {
    Push-Location "HeuristicLogix.Api"
    $result = dotnet add package MiniExcel --version 1.34.0 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? MiniExcel added successfully" -ForegroundColor Green
    } else {
        Write-Host "? Failed to add MiniExcel" -ForegroundColor Red
        Write-Host $result -ForegroundColor Gray
        $ErrorCount++
    }
    Pop-Location
} catch {
    Write-Host "? Error: $_" -ForegroundColor Red
    Pop-Location
    $ErrorCount++
}

# Verify build
Write-Host "`n[2/2] Verifying build..." -ForegroundColor Yellow
try {
    Push-Location "HeuristicLogix.Api"
    $result = dotnet build --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Build successful" -ForegroundColor Green
    } else {
        Write-Host "? Build failed" -ForegroundColor Red
        Write-Host $result -ForegroundColor Gray
        $ErrorCount++
    }
    Pop-Location
} catch {
    Write-Host "? Build error: $_" -ForegroundColor Red
    Pop-Location
    $ErrorCount++
}

# Create Kafka topic for historic deliveries
Write-Host "`n[3/3] Creating Kafka topic 'historic.deliveries.v1'..." -ForegroundColor Yellow
try {
    $topicExists = docker exec heuristiclogix-kafka kafka-topics --list --bootstrap-server localhost:9092 2>&1 | Select-String "historic.deliveries.v1"
    
    if ($topicExists) {
        Write-Host "? Topic already exists" -ForegroundColor Green
    } else {
        $result = docker exec heuristiclogix-kafka kafka-topics `
            --create `
            --topic historic.deliveries.v1 `
            --bootstrap-server localhost:9092 `
            --partitions 3 `
            --replication-factor 1 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Topic created successfully" -ForegroundColor Green
        } else {
            Write-Host "? Failed to create topic (Kafka may not be running)" -ForegroundColor Yellow
            Write-Host "  Run 'docker-compose up -d' first" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "? Could not create topic: $_" -ForegroundColor Yellow
    Write-Host "  Kafka may not be running. Run 'docker-compose up -d' first" -ForegroundColor Gray
}

# Summary
Write-Host "`n=== Setup Summary ===" -ForegroundColor Cyan
if ($ErrorCount -eq 0) {
    Write-Host "? Data Ingestion System is ready!" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Create a CSV file with historic delivery data" -ForegroundColor White
    Write-Host "  2. Start the API: cd HeuristicLogix.Api && dotnet run" -ForegroundColor White
    Write-Host "  3. Upload file: POST http://localhost:5001/api/ingestion/historic-deliveries" -ForegroundColor White
    Write-Host "  4. Check Python logs: docker logs -f heuristiclogix-intelligence" -ForegroundColor White
    Write-Host "`nTemplate CSV:" -ForegroundColor Yellow
    Write-Host "  GET http://localhost:5001/api/ingestion/template" -ForegroundColor White
} else {
    Write-Host "? Setup completed with $ErrorCount error(s)" -ForegroundColor Red
    Write-Host "Please review the errors above and run this script again" -ForegroundColor Yellow
}

Write-Host "`n" -ForegroundColor White
