# Quick Fix: Python Import Warnings

## ?? One-Command Fix

```powershell
# From repository root
.\setup-python-dependencies.ps1
```

Then **restart your IDE**.

---

## ?? What Gets Installed

```
fastapi==0.115.0                # Web framework
uvicorn==0.32.0                 # ASGI server
pydantic==2.9.2                 # Data validation
aiokafka==0.11.0                # Kafka client ? Fixes aiokafka warning
google-generativeai==0.8.3      # Gemini AI ? Fixes google.generativeai warning
sqlalchemy[asyncio]==2.0.36     # Database ORM ? Fixes sqlalchemy warnings
pyodbc==5.1.0                   # SQL Server driver
aioodbc==0.5.0                  # Async ODBC
python-dotenv==1.0.1            # Environment vars
python-json-logger==2.0.7       # JSON logging
```

---

## ?? After Setup

1. **Restart IDE** (important!)
2. **Set Python interpreter**: `.\IntelligenceService\venv\Scripts\python.exe`
3. **Verify**: Open `main.py` - no more red squiggles!

---

## ?? IDE Configuration

### Visual Studio
- View ? Python Environments
- Add existing environment
- Select: `.\IntelligenceService\venv\Scripts\python.exe`

### VS Code
- Press `Ctrl+Shift+P`
- "Python: Select Interpreter"
- Choose: `.\IntelligenceService\venv\Scripts\python.exe`

### PyCharm
- File ? Settings ? Python Interpreter
- Add ? Existing environment
- Select: `.\IntelligenceService\venv\Scripts\python.exe`

---

## ? Done!

All import warnings resolved after:
1. ? Running setup script
2. ? Configuring IDE interpreter
3. ? Restarting IDE

**Total time**: ~2 minutes
