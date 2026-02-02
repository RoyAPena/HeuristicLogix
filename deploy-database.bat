@echo off
REM ============================================================
REM HeuristicLogix ERP - Quick Database Deployment
REM Target: LAPTOP-7MG6K7RV
REM ============================================================

echo.
echo ============================================================
echo HeuristicLogix ERP - Quick Database Deployment
echo ============================================================
echo.
echo Target Server: LAPTOP-7MG6K7RV
echo Database: HeuristicLogix
echo Auth: Windows Authentication
echo.

REM Check if PowerShell script exists
if not exist "deploy-database-local.ps1" (
    echo ERROR: deploy-database-local.ps1 not found!
    echo Please ensure you're running from the repository root.
    pause
    exit /b 1
)

echo Running automated deployment script...
echo.

REM Execute PowerShell script
powershell.exe -ExecutionPolicy Bypass -File "deploy-database-local.ps1"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================================
    echo Deployment completed successfully!
    echo ============================================================
) else (
    echo.
    echo ============================================================
    echo Deployment failed! Check errors above.
    echo ============================================================
)

echo.
pause
