# Fix Python Import Warnings - IntelligenceService

## ? Quick Fix (Recommended)

### Step 1: Run Setup Script

From the repository root in PowerShell:

```powershell
.\setup-python-dependencies.ps1
```

This will:
- ? Create a Python virtual environment in `IntelligenceService/venv`
- ? Activate the virtual environment
- ? Install all required packages from `requirements.txt`

### Step 2: Configure Your IDE

#### **Visual Studio**:
1. Close and reopen Visual Studio
2. The Python environment should be detected automatically

#### **Visual Studio Code**:
1. Press `Ctrl+Shift+P`
2. Type "Python: Select Interpreter"
3. Choose: `.\IntelligenceService\venv\Scripts\python.exe`
4. Reload window (`Ctrl+Shift+P` ? "Developer: Reload Window")

#### **PyCharm**:
1. Go to File ? Settings ? Project ? Python Interpreter
2. Click gear icon ? Add
3. Select "Existing environment"
4. Choose: `.\IntelligenceService\venv\Scripts\python.exe`
5. Click OK

### Step 3: Verify Installation

Activate the virtual environment and check:

```powershell
cd IntelligenceService
.\venv\Scripts\Activate.ps1
python -c "import fastapi, uvicorn, aiokafka, google.generativeai, sqlalchemy; print('? All imports work!')"
```

---

## ?? Manual Installation (Alternative)

If you prefer to install manually:

```powershell
cd IntelligenceService

# Create virtual environment
python -m venv venv

# Activate (Windows)
.\venv\Scripts\Activate.ps1

# Activate (Linux/Mac)
source venv/bin/activate

# Upgrade pip
python -m pip install --upgrade pip

# Install all dependencies
pip install -r requirements.txt
```

---

## ?? Required Packages (from requirements.txt)

| Package | Version | Purpose |
|---------|---------|---------|
| **fastapi** | 0.115.0 | Web framework for API endpoints |
| **uvicorn[standard]** | 0.32.0 | ASGI server for FastAPI |
| **pydantic** | 2.9.2 | Data validation and serialization |
| **python-dotenv** | 1.0.1 | Environment variable management |
| **aiokafka** | 0.11.0 | Async Kafka client |
| **google-generativeai** | 0.8.3 | Google Gemini AI SDK |
| **sqlalchemy[asyncio]** | 2.0.36 | Database ORM with async support |
| **aioodbc** | 0.5.0 | Async ODBC driver |
| **pyodbc** | 5.1.0 | SQL Server Python driver |
| **python-json-logger** | 2.0.7 | JSON logging |

---

## ?? Python Version Requirements

- **Minimum**: Python 3.10
- **Recommended**: Python 3.11 or 3.12
- **Check your version**: `python --version`

If you need to install Python:
- **Windows**: https://www.python.org/downloads/
- **macOS**: `brew install python@3.12`
- **Linux**: `sudo apt install python3.12 python3.12-venv`

---

## ?? Common Issues & Solutions

### Issue 1: "python: command not found"
**Solution**: Python not installed or not in PATH
```powershell
# Windows - Download from python.org and check "Add to PATH" during installation
# Or use winget:
winget install Python.Python.3.12
```

### Issue 2: "No module named 'venv'"
**Solution**: Install python3-venv
```bash
# Linux
sudo apt install python3-venv
```

### Issue 3: Import warnings still showing in IDE
**Solution**: Restart IDE after installing packages
1. Close all Python files
2. Exit IDE completely
3. Reopen IDE
4. Wait for indexing to complete
5. Reopen `main.py`

### Issue 4: "pip is not recognized"
**Solution**: Use python -m pip
```powershell
python -m pip install --upgrade pip
python -m pip install -r requirements.txt
```

### Issue 5: SQL Server driver errors
**Solution**: Install ODBC Driver for SQL Server

**Windows**:
```powershell
# Download from Microsoft:
# https://learn.microsoft.com/en-us/sql/connect/odbc/download-odbc-driver-for-sql-server
```

**Linux**:
```bash
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt update
sudo ACCEPT_EULA=Y apt install -y msodbcsql18
```

---

## ? Verification Checklist

After running the setup script, verify:

### 1. Virtual Environment Created
```powershell
Test-Path IntelligenceService\venv
# Should return: True
```

### 2. Packages Installed
```powershell
cd IntelligenceService
.\venv\Scripts\Activate.ps1
pip list
```

You should see all packages from requirements.txt listed.

### 3. Imports Work in Python REPL
```powershell
python
>>> import fastapi
>>> import uvicorn
>>> import aiokafka
>>> import google.generativeai
>>> import sqlalchemy
>>> from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER
>>> from sqlalchemy.ext.asyncio import AsyncSession
>>> from sqlalchemy.orm import declarative_base
>>> print("? All imports successful!")
>>> exit()
```

### 4. IDE Recognizes Packages
Open `main.py` in your IDE:
- ? No red squiggly lines under imports
- ? IntelliSense/autocomplete works
- ? Type hints display correctly

---

## ?? Expected Output

After running `setup-python-dependencies.ps1`:

```
Setting up Python environment for IntelligenceService...

Checking Python installation...
? Found: Python 3.12.0

Creating virtual environment...
? Virtual environment created

Activating virtual environment...
? Virtual environment activated

Upgrading pip...
? pip upgraded

Installing Python packages from requirements.txt...
? All packages installed successfully

? Python environment setup complete!

Installed packages:
  • fastapi (0.115.0) - Web framework
  • uvicorn (0.32.0) - ASGI server
  • pydantic (2.9.2) - Data validation
  • aiokafka (0.11.0) - Kafka client
  • google-generativeai (0.8.3) - Gemini AI
  • sqlalchemy (2.0.36) - Database ORM
  • pyodbc (5.1.0) - SQL Server driver

Next steps:
1. Restart your IDE/editor to pick up the new Python environment
2. Set your IDE's Python interpreter to: .\IntelligenceService\venv\Scripts\python.exe
3. The import warnings should now be resolved
```

---

## ?? IDE-Specific Configuration

### **Visual Studio 2022**

1. **Open Python Environments**:
   - View ? Other Windows ? Python Environments

2. **Add Environment**:
   - Click "+ Add Environment"
   - Select "Existing environment"
   - Browse to: `C:\Repository\HeuristicLogix\IntelligenceService\venv\Scripts\python.exe`

3. **Set as Default**:
   - Right-click the environment ? "Make this the default environment"

### **Visual Studio Code**

Create/update `.vscode/settings.json`:

```json
{
  "python.defaultInterpreterPath": "${workspaceFolder}/IntelligenceService/venv/Scripts/python.exe",
  "python.venvPath": "${workspaceFolder}/IntelligenceService",
  "python.terminal.activateEnvironment": true,
  "python.linting.enabled": true,
  "python.linting.pylintEnabled": false,
  "python.linting.flake8Enabled": true,
  "python.formatting.provider": "black"
}
```

### **PyCharm**

1. File ? Settings (Ctrl+Alt+S)
2. Project: HeuristicLogix ? Python Interpreter
3. Click gear icon ? Add
4. Choose "Virtualenv Environment"
5. Select "Existing environment"
6. Interpreter: `.\IntelligenceService\venv\Scripts\python.exe`
7. Make available to all projects: ? (optional)
8. Click OK

---

## ?? Summary

**Issue**: Import warnings for Python packages in `main.py`

**Cause**: Python packages not installed in the environment

**Solution**: 
1. ? Run `.\setup-python-dependencies.ps1`
2. ? Configure IDE to use `.\IntelligenceService\venv\Scripts\python.exe`
3. ? Restart IDE

**Time**: ~2-5 minutes

**Result**: All import warnings resolved! ?

---

## ?? Still Having Issues?

If imports still don't work after following all steps:

1. **Check Python version**: `python --version` (must be 3.10+)
2. **Verify pip works**: `python -m pip --version`
3. **Reinstall in verbose mode**: 
   ```powershell
   pip install -r requirements.txt -v
   ```
4. **Check IDE Python interpreter**: Make sure it points to the venv
5. **Try in a fresh terminal**: Close all terminals and open a new one

If problems persist, check the error messages in the terminal output - they usually indicate what's missing (like ODBC driver for SQL Server).
