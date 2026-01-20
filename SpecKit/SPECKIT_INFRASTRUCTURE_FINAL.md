# SPECKIT_INFRASTRUCTURE_FINAL.md

## 1. STRATEGY & ENVIRONMENT (JAN 2026)
- **Deployment Model:** Hybrid On-Premise (Local Server + Cloud AI Bridge).
- **IaC Engine:** Terraform (Local Volume Provisioning & Secret Management).
- **Orchestration:** Docker Compose (Zero-install developer/server experience).
- **Network Architecture:** Isolated bridge network `heuristic_net`.

## 2. CONTAINER TOPOLOGY & PORTS
| Service | Image/Base Stack | Port (Host) | Volume / Persistence |
| :--- | :--- | :--- | :--- |
| **db_sql** | mssql/server:2022-latest | 1433 | `sql_data:/var/opt/mssql` |
| **kafka_bus** | confluentinc/cp-kafka:latest | 9092 | `kafka_logs:/var/lib/kafka/data` |
| **zookeeper** | confluentinc/cp-zookeeper:latest | 2181 | `zk_data:/var/lib/zookeeper/data` |
| **api_logistics** | .NET 10 (ASP.NET Core) | 5000 | N/A (Stateless) |
| **ui_blazor** | .NET 10 (Blazor Server/WASM) | 5001 | N/A (Stateless) |
| **ai_brain** | Python 3.11 + FastAPI | 8000 | `ai_model_cache:/app/cache` |

## 3. INTER-SERVICE DEPENDENCIES (HEALTH CHECKS)
1. **Infrastructure Tier:** `zookeeper` must be healthy before `kafka_bus`.
2. **Persistence Tier:** `db_sql` and `kafka_bus` must be healthy before `api_logistics`.
3. **Intelligence Tier:** `ai_brain` depends on `kafka_bus` (Consumer) and `api_logistics` (Optional REST Callback).

## 4. DOCKER SPECIFICATIONS
- **Multi-stage Build:** Mandatory for .NET and Python to minimize attack surface and image size.
- **Restart Policy:** `unless-stopped` (Resilience against local power cycles).
- **Environment Management:** - Terraform generates `.env` from secure variables.
    - SQL Server requires `ACCEPT_EULA=Y` and `MSSQL_SA_PASSWORD`.

## 5. PROVISIONING WORKFLOW (CLI)
1. `cd infrastructure/terraform && terraform apply -auto-approve`
2. `cd ../.. && docker-compose up -d --build`
3. `docker ps` (Verification of all 6 agents in position).