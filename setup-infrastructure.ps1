# HeuristicLogix Infrastructure Setup Script
# Run this script from the repository root

Write-Host "=== HeuristicLogix Infrastructure Setup ===" -ForegroundColor Cyan

# Step 1: Copy Program.cs template
Write-Host "`n[Step 1] Configuring HeuristicLogix.Api Program.cs..." -ForegroundColor Yellow
if (Test-Path "HeuristicLogix.Api\Program.cs.template") {
    Copy-Item "HeuristicLogix.Api\Program.cs.template" "HeuristicLogix.Api\Program.cs" -Force
    Write-Host "? Program.cs configured" -ForegroundColor Green
} else {
    Write-Host "? Program.cs.template not found" -ForegroundColor Red
}

# Step 2: Copy .env.example to .env
Write-Host "`n[Step 2] Creating .env file..." -ForegroundColor Yellow
if (Test-Path ".env.example") {
    if (-not (Test-Path ".env")) {
        Copy-Item ".env.example" ".env"
        Write-Host "? .env file created" -ForegroundColor Green
        Write-Host "? IMPORTANT: Edit .env and add your GEMINI_API_KEY" -ForegroundColor Magenta
    } else {
        Write-Host "? .env file already exists" -ForegroundColor Green
    }
} else {
    Write-Host "? .env.example not found" -ForegroundColor Red
}

# Step 3: Check Docker
Write-Host "`n[Step 3] Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "? Docker found: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "? Docker not found. Please install Docker Desktop." -ForegroundColor Red
    Write-Host "  Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Step 4: Check .NET SDK
Write-Host "`n[Step 4] Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "? .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "? .NET SDK not found. Please install .NET 10 SDK." -ForegroundColor Red
    exit 1
}

# Step 5: Start Docker infrastructure
Write-Host "`n[Step 5] Starting Docker infrastructure..." -ForegroundColor Yellow
Write-Host "This may take a few minutes on first run..." -ForegroundColor Gray
try {
    docker-compose up -d
    Write-Host "? Docker services started" -ForegroundColor Green
} catch {
    Write-Host "? Failed to start Docker services" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

# Step 6: Wait for services to be healthy
Write-Host "`n[Step 6] Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check SQL Server
Write-Host "  Checking SQL Server..." -ForegroundColor Gray
$sqlRetries = 0
$sqlMaxRetries = 30
$sqlHealthy = $false
while ($sqlRetries -lt $sqlMaxRetries -and -not $sqlHealthy) {
    try {
        $result = docker exec heuristiclogix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HeuristicLogix2026!" -Q "SELECT 1" -C 2>&1
        if ($LASTEXITCODE -eq 0) {
            $sqlHealthy = $true
            Write-Host "  ? SQL Server is ready" -ForegroundColor Green
        } else {
            Start-Sleep -Seconds 2
            $sqlRetries++
        }
    } catch {
        Start-Sleep -Seconds 2
        $sqlRetries++
    }
}

if (-not $sqlHealthy) {
    Write-Host "  ? SQL Server failed to start" -ForegroundColor Red
}

# Check Kafka
Write-Host "  Checking Kafka..." -ForegroundColor Gray
$kafkaRetries = 0
$kafkaMaxRetries = 30
$kafkaHealthy = $false
while ($kafkaRetries -lt $kafkaMaxRetries -and -not $kafkaHealthy) {
    try {
        $result = docker exec heuristiclogix-kafka kafka-broker-api-versions --bootstrap-server localhost:9092 2>&1
        if ($LASTEXITCODE -eq 0) {
            $kafkaHealthy = $true
            Write-Host "  ? Kafka is ready" -ForegroundColor Green
        } else {
            Start-Sleep -Seconds 2
            $kafkaRetries++
        }
    } catch {
        Start-Sleep -Seconds 2
        $kafkaRetries++
    }
}

if (-not $kafkaHealthy) {
    Write-Host "  ? Kafka failed to start" -ForegroundColor Red
}

# Step 7: Create Kafka topics
Write-Host "`n[Step 7] Creating Kafka topics..." -ForegroundColor Yellow
if ($kafkaHealthy) {
    try {
        docker exec heuristiclogix-kafka kafka-topics --create --topic expert.decisions.v1 --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1 --if-not-exists 2>&1 | Out-Null
        docker exec heuristiclogix-kafka kafka-topics --create --topic heuristic.telemetry.v1 --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1 --if-not-exists 2>&1 | Out-Null
        Write-Host "? Kafka topics created" -ForegroundColor Green
    } catch {
        Write-Host "? Failed to create Kafka topics" -ForegroundColor Red
    }
} else {
    Write-Host "? Skipped (Kafka not healthy)" -ForegroundColor Gray
}

# Step 8: Initialize database with EF Core
Write-Host "`n[Step 8] Initializing database..." -ForegroundColor Yellow
if ($sqlHealthy) {
    Write-Host "  Installing EF Core tools..." -ForegroundColor Gray
    try {
        dotnet tool install --global dotnet-ef 2>&1 | Out-Null
    } catch {
        # Tool might already be installed
    }
    
    Write-Host "  Creating initial migration..." -ForegroundColor Gray
    Set-Location "HeuristicLogix.Api"
    try {
        dotnet ef migrations add InitialCreate --force 2>&1 | Out-Null
        Write-Host "  ? Migration created" -ForegroundColor Green
        
        Write-Host "  Applying migration to database..." -ForegroundColor Gray
        dotnet ef database update
        Write-Host "  ? Database initialized" -ForegroundColor Green
    } catch {
        Write-Host "  ? Failed to initialize database" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
    }
    Set-Location ".."
} else {
    Write-Host "? Skipped (SQL Server not healthy)" -ForegroundColor Gray
}

# Summary
Write-Host "`n=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "`nServices:" -ForegroundColor Yellow
Write-Host "  • Kafka UI:              http://localhost:8080" -ForegroundColor White
Write-Host "  • Intelligence Service:  http://localhost:8000/docs" -ForegroundColor White
Write-Host "  • SQL Server:            localhost:1433 (sa/HeuristicLogix2026!)" -ForegroundColor White
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "  1. Edit .env and add your GEMINI_API_KEY" -ForegroundColor White
Write-Host "  2. Restart intelligence service: docker-compose restart intelligence-service" -ForegroundColor White
Write-Host "  3. Run API: cd HeuristicLogix.Api && dotnet run" -ForegroundColor White
Write-Host "  4. Run Client: cd HeuristicLogix.Client && dotnet run" -ForegroundColor White
Write-Host "`nDocumentation:" -ForegroundColor Yellow
Write-Host "  • See README_INFRASTRUCTURE.md for detailed information" -ForegroundColor White
Write-Host "`n" -ForegroundColor White
