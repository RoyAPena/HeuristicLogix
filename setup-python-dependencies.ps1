# Setup Python Dependencies for IntelligenceService
# Run this script from the repository root

Write-Host "Setting up Python environment for IntelligenceService..." -ForegroundColor Cyan

# Check if Python is installed
Write-Host "`nChecking Python installation..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    Write-Host "? Found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "? Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Python 3.10+ from https://www.python.org/" -ForegroundColor Yellow
    exit 1
}

# Navigate to IntelligenceService directory
Set-Location "IntelligenceService"

# Create virtual environment if it doesn't exist
if (-not (Test-Path "venv")) {
    Write-Host "`nCreating virtual environment..." -ForegroundColor Yellow
    python -m venv venv
    Write-Host "? Virtual environment created" -ForegroundColor Green
} else {
    Write-Host "`n? Virtual environment already exists" -ForegroundColor Green
}

# Activate virtual environment
Write-Host "`nActivating virtual environment..." -ForegroundColor Yellow
if ($IsWindows -or $env:OS -eq "Windows_NT") {
    .\venv\Scripts\Activate.ps1
} else {
    . ./venv/bin/activate
}
Write-Host "? Virtual environment activated" -ForegroundColor Green

# Upgrade pip
Write-Host "`nUpgrading pip..." -ForegroundColor Yellow
python -m pip install --upgrade pip
Write-Host "? pip upgraded" -ForegroundColor Green

# Install dependencies
Write-Host "`nInstalling Python packages from requirements.txt..." -ForegroundColor Yellow
pip install -r requirements.txt
Write-Host "? All packages installed successfully" -ForegroundColor Green

# Navigate back
Set-Location ..

Write-Host "`n? Python environment setup complete!" -ForegroundColor Green
Write-Host "`nInstalled packages:" -ForegroundColor Cyan
Write-Host "  • fastapi (0.115.0) - Web framework"
Write-Host "  • uvicorn (0.32.0) - ASGI server"
Write-Host "  • pydantic (2.9.2) - Data validation"
Write-Host "  • aiokafka (0.11.0) - Kafka client"
Write-Host "  • google-generativeai (0.8.3) - Gemini AI"
Write-Host "  • sqlalchemy (2.0.36) - Database ORM"
Write-Host "  • pyodbc (5.1.0) - SQL Server driver"

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Restart your IDE/editor to pick up the new Python environment"
Write-Host "2. Set your IDE's Python interpreter to: .\IntelligenceService\venv\Scripts\python.exe"
Write-Host "3. The import warnings should now be resolved"

Write-Host "`nTo activate the virtual environment manually:" -ForegroundColor Yellow
Write-Host "  cd IntelligenceService"
Write-Host "  .\venv\Scripts\Activate.ps1"
