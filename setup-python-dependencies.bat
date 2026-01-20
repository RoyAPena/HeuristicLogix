@echo off
REM Setup Python Dependencies for IntelligenceService
REM Double-click this file to run

echo ========================================
echo  Python Dependencies Setup
echo ========================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Python is not installed or not in PATH
    echo Please install Python 3.10+ from https://www.python.org/
    pause
    exit /b 1
)

echo [OK] Python found
python --version
echo.

REM Navigate to IntelligenceService
cd IntelligenceService

REM Create virtual environment
if not exist "venv" (
    echo Creating virtual environment...
    python -m venv venv
    echo [OK] Virtual environment created
) else (
    echo [OK] Virtual environment already exists
)
echo.

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat
echo.

REM Upgrade pip
echo Upgrading pip...
python -m pip install --upgrade pip --quiet
echo [OK] pip upgraded
echo.

REM Install dependencies
echo Installing Python packages...
echo This may take a few minutes...
pip install -r requirements.txt
echo.
echo [OK] All packages installed!
echo.

REM Navigate back
cd ..

echo ========================================
echo  Setup Complete!
echo ========================================
echo.
echo Installed packages:
echo   - fastapi (Web framework)
echo   - uvicorn (ASGI server)
echo   - pydantic (Data validation)
echo   - aiokafka (Kafka client)
echo   - google-generativeai (Gemini AI)
echo   - sqlalchemy (Database ORM)
echo   - pyodbc (SQL Server driver)
echo.
echo Next steps:
echo   1. Restart your IDE
echo   2. Set Python interpreter to:
echo      IntelligenceService\venv\Scripts\python.exe
echo   3. Import warnings will be resolved
echo.
echo Virtual environment location:
echo %CD%\IntelligenceService\venv
echo.

pause
