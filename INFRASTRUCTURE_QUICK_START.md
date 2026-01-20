# HeuristicLogix Infrastructure Quick Reference

## ?? One-Command Deployment

### **Windows**:
```powershell
.\deploy-production.ps1 -Detached -GeminiApiKey "your-key"
```

### **Linux/Mac**:
```bash
./deploy-production.sh --detached --gemini-key "your-key"
```

---

## ?? What Gets Deployed

| Service | Port | Purpose |
|---------|------|---------|
| **SQL Server** | 1433 | Database |
| **Zookeeper** | 2181 | Kafka coordination |
| **Kafka** | 9092 | Event bus |
| **API** | 5000 | .NET 10 backend |
| **UI** | 5001 | Blazor WebAssembly |
| **AI** | 8000 | Python Intelligence |

---

## ?? Health Checks

```bash
curl http://localhost:5000/health  # API
curl http://localhost:5001/health  # UI
curl http://localhost:8000/health  # AI
```

---

## ?? Quick Commands

```bash
# View logs
docker compose -f docker-compose.production.yml logs -f [service]

# Stop all
docker compose -f docker-compose.production.yml down

# Restart service
docker compose -f docker-compose.production.yml restart [service]

# Service status
docker compose -f docker-compose.production.yml ps

# Execute command
docker compose -f docker-compose.production.yml exec [service] [command]
```

---

## ?? Service Names

```
db_sql | zookeeper | kafka_bus | api_logistics | ui_blazor | ai_brain
```

---

## ?? Files Created

- ? `infrastructure/terraform/main.tf` - Terraform config
- ? `docker-compose.production.yml` - Production orchestration
- ? `Dockerfile.api` - .NET API multi-stage
- ? `Dockerfile.ui` - Blazor UI multi-stage
- ? `IntelligenceService/Dockerfile` - Python AI
- ? `deploy-production.ps1` - Windows deployment
- ? `deploy-production.sh` - Linux/Mac deployment
- ? `.env` - Auto-generated config

---

## ? Prerequisites

- ? Docker Desktop / Docker Engine
- ? Docker Compose
- ? Terraform (optional - will create .env manually if missing)

---

## ?? Configuration

Default password: `HeuristicLogix2026!`

Change with:
```powershell
.\deploy-production.ps1 -SqlPassword "YourSecurePassword!"
```

---

## ?? Total Time

**Setup**: ~5-10 minutes (first run with build)  
**Restart**: ~30 seconds (with cached images)

---

**All services production-ready with health checks, resource limits, and network isolation!** ??
