# HeuristicLogix Complete Infrastructure Setup
# Terraform + Docker Compose Production Deployment

param(
    [switch]$SkipTerraform,
    [switch]$SkipBuild,
    [switch]$Detached,
    [string]$SqlPassword = "HeuristicLogix2026!",
    [string]$GeminiApiKey = ""
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  HeuristicLogix Infrastructure Setup" -ForegroundColor Cyan
Write-Host "  Production-Ready Docker Orchestration" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Function to check command exists
function Test-CommandExists {
    param($Command)
    try {
        if (Get-Command $Command -ErrorAction Stop) {
            return $true
        }
    } catch {
        return $false
    }
}

# Step 1: Prerequisites Check
Write-Host "[1/5] Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

# Check Docker
if (Test-CommandExists docker) {
    $dockerVersion = docker --version
    Write-Host "  ? Docker: $dockerVersion" -ForegroundColor Green
} else {
    Write-Host "  ? Docker not found" -ForegroundColor Red
    Write-Host "    Install from: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Check Docker Compose
if (Test-CommandExists "docker compose") {
    $composeVersion = docker compose version
    Write-Host "  ? Docker Compose: $composeVersion" -ForegroundColor Green
} else {
    Write-Host "  ? Docker Compose not available" -ForegroundColor Red
    exit 1
}

# Check Terraform
if (-not $SkipTerraform) {
    if (Test-CommandExists terraform) {
        $terraformVersion = terraform version | Select-Object -First 1
        Write-Host "  ? Terraform: $terraformVersion" -ForegroundColor Green
    } else {
        Write-Host "  ? Terraform not found - will create .env manually" -ForegroundColor Yellow
        $SkipTerraform = $true
    }
}

# Check .NET SDK (optional)
if (Test-CommandExists dotnet) {
    $dotnetVersion = dotnet --version
    Write-Host "  ? .NET SDK: $dotnetVersion" -ForegroundColor Green
}

Write-Host ""

# Step 2: Terraform Infrastructure Provisioning
if (-not $SkipTerraform) {
    Write-Host "[2/5] Provisioning infrastructure with Terraform..." -ForegroundColor Yellow
    Write-Host ""
    
    Push-Location "infrastructure/terraform"
    
    try {
        Write-Host "  ? Initializing Terraform..." -ForegroundColor Cyan
        terraform init -upgrade
        
        Write-Host "  ? Validating configuration..." -ForegroundColor Cyan
        terraform validate
        
        Write-Host "  ? Creating execution plan..." -ForegroundColor Cyan
        terraform plan `
            -var="sql_password=$SqlPassword" `
            -out=tfplan
        
        Write-Host "  ? Applying infrastructure..." -ForegroundColor Cyan
        terraform apply -auto-approve tfplan
        
        Write-Host "  ? Infrastructure provisioned" -ForegroundColor Green
    }
    catch {
        Write-Host "  ? Terraform failed: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host "[2/5] Creating .env configuration manually..." -ForegroundColor Yellow
    Write-Host ""
    
    $envContent = @"
# HeuristicLogix Environment Configuration
# Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

# SQL Server Configuration
SQLSERVER_PASSWORD=$SqlPassword
SQLSERVER_CONNECTION_STRING=Server=db_sql,1433;Database=HeuristicLogixDB;User Id=sa;Password=$SqlPassword;TrustServerCertificate=True;MultipleActiveResultSets=True;

# Kafka Configuration
KAFKA_BOOTSTRAP_SERVERS=kafka_bus:9092
KAFKA_TOPIC_EXPERT_DECISIONS=expert.decisions.v1
KAFKA_TOPIC_HISTORIC_DELIVERIES=historic.deliveries.v1
KAFKA_CONSUMER_GROUP=heuristiclogix-intelligence

# Zookeeper Configuration
ZOOKEEPER_HOST=zookeeper
ZOOKEEPER_PORT=2181

# AI Configuration
GEMINI_API_KEY=$GeminiApiKey
GEMINI_MODEL=gemini-2.0-flash-exp

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
TZ=America/Santo_Domingo
LOG_LEVEL=Information

# Network
DOCKER_NETWORK=heuristic_net
"@
    
    $envContent | Out-File -FilePath ".env" -Encoding utf8 -NoNewline
    Write-Host "  ? .env file created" -ForegroundColor Green
}

Write-Host ""

# Step 3: Build Docker Images
if (-not $SkipBuild) {
    Write-Host "[3/5] Building Docker images..." -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "  ? Building API image (api_logistics)..." -ForegroundColor Cyan
    docker build -t heuristiclogix/api:latest -f Dockerfile.api . | Out-Host
    
    Write-Host ""
    Write-Host "  ? Building UI image (ui_blazor)..." -ForegroundColor Cyan
    docker build -t heuristiclogix/ui:latest -f Dockerfile.ui . | Out-Host
    
    Write-Host ""
    Write-Host "  ? Building AI image (ai_brain)..." -ForegroundColor Cyan
    docker build -t heuristiclogix/ai:latest ./IntelligenceService | Out-Host
    
    Write-Host ""
    Write-Host "  ? All images built successfully" -ForegroundColor Green
} else {
    Write-Host "[3/5] Skipping Docker image build (using cached images)" -ForegroundColor Yellow
}

Write-Host ""

# Step 4: Create Docker Network and Volumes
Write-Host "[4/5] Creating Docker resources..." -ForegroundColor Yellow
Write-Host ""

# Check if network exists
$networkExists = docker network ls --filter name=heuristic_net --format "{{.Name}}"
if (-not $networkExists) {
    Write-Host "  ? Creating Docker network (heuristic_net)..." -ForegroundColor Cyan
    docker network create heuristic_net
    Write-Host "  ? Network created" -ForegroundColor Green
} else {
    Write-Host "  ? Network already exists" -ForegroundColor Green
}

# Create volumes if they don't exist
$volumes = @("sql_data", "kafka_logs", "zk_data", "ai_model_cache")
foreach ($volume in $volumes) {
    $volumeExists = docker volume ls --filter name=$volume --format "{{.Name}}"
    if (-not $volumeExists) {
        Write-Host "  ? Creating volume ($volume)..." -ForegroundColor Cyan
        docker volume create $volume
        Write-Host "  ? Volume created" -ForegroundColor Green
    } else {
        Write-Host "  ? Volume $volume already exists" -ForegroundColor Green
    }
}

Write-Host ""

# Step 5: Start Services
Write-Host "[5/5] Starting services with Docker Compose..." -ForegroundColor Yellow
Write-Host ""

$composeFile = "docker-compose.production.yml"
$composeArgs = @("compose", "-f", $composeFile, "up")

if ($Detached) {
    $composeArgs += "-d"
    $composeArgs += "--wait"
}

Write-Host "  ? Starting all services..." -ForegroundColor Cyan
& docker $composeArgs

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Setup Complete! ?" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Display service information
Write-Host "Services Running:" -ForegroundColor Cyan
Write-Host "  • SQL Server     ? localhost:1433" -ForegroundColor White
Write-Host "  • Zookeeper      ? localhost:2181" -ForegroundColor White
Write-Host "  • Kafka          ? localhost:9092" -ForegroundColor White
Write-Host "  • API (Logistics)? http://localhost:5000" -ForegroundColor White
Write-Host "  • UI (Blazor)    ? http://localhost:5001" -ForegroundColor White
Write-Host "  • AI (Intelligence)? http://localhost:8000" -ForegroundColor White
Write-Host ""

Write-Host "Health Check Endpoints:" -ForegroundColor Cyan
Write-Host "  • API Health     ? http://localhost:5000/health" -ForegroundColor White
Write-Host "  • UI Health      ? http://localhost:5001/health" -ForegroundColor White
Write-Host "  • AI Health      ? http://localhost:8000/health" -ForegroundColor White
Write-Host ""

Write-Host "Quick Commands:" -ForegroundColor Cyan
Write-Host "  View logs        ? docker compose -f $composeFile logs -f [service-name]" -ForegroundColor Yellow
Write-Host "  Stop all         ? docker compose -f $composeFile down" -ForegroundColor Yellow
Write-Host "  Restart service  ? docker compose -f $composeFile restart [service-name]" -ForegroundColor Yellow
Write-Host "  Service status   ? docker compose -f $composeFile ps" -ForegroundColor Yellow
Write-Host "  Execute command  ? docker compose -f $composeFile exec [service-name] [command]" -ForegroundColor Yellow
Write-Host ""

Write-Host "Service Names:" -ForegroundColor Cyan
Write-Host "  • db_sql | zookeeper | kafka_bus | api_logistics | ui_blazor | ai_brain" -ForegroundColor White
Write-Host ""

# If detached, check health
if ($Detached) {
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    
    Write-Host ""
    Write-Host "Service Status:" -ForegroundColor Cyan
    docker compose -f $composeFile ps
    
    Write-Host ""
    Write-Host "Testing endpoints..." -ForegroundColor Yellow
    
    # Test API
    try {
        $apiHealth = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 5
        Write-Host "  ? API is responding" -ForegroundColor Green
    } catch {
        Write-Host "  ? API not ready yet" -ForegroundColor Yellow
    }
    
    # Test AI
    try {
        $aiHealth = Invoke-RestMethod -Uri "http://localhost:8000/health" -TimeoutSec 5
        Write-Host "  ? AI service is responding" -ForegroundColor Green
    } catch {
        Write-Host "  ? AI service not ready yet" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "?? HeuristicLogix is now running!" -ForegroundColor Green
Write-Host ""
