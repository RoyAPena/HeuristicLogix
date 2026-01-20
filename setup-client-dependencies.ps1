# HeuristicLogix.Client - Install Dependencies
# Run this script from the repository root

Write-Host "Installing dependencies for HeuristicLogix.Client..." -ForegroundColor Cyan

# Navigate to Client project
Set-Location "HeuristicLogix.Client"

# 1. Add MudBlazor NuGet package
Write-Host "`n1. Installing MudBlazor..." -ForegroundColor Yellow
dotnet add package MudBlazor --version 7.0.0

# 2. Add project reference to Shared
Write-Host "`n2. Adding reference to HeuristicLogix.Shared..." -ForegroundColor Yellow
dotnet add reference ..\HeuristicLogix.Shared\HeuristicLogix.Shared.csproj

# 3. Restore packages
Write-Host "`n3. Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Navigate back
Set-Location ..

Write-Host "`n? Dependencies installed successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Restart Visual Studio or reload the solution"
Write-Host "2. Rebuild the solution (Ctrl+Shift+B)"
Write-Host "3. The import warnings should now be resolved"
