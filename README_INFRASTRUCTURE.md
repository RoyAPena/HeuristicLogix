# HeuristicLogix Infrastructure Setup

## Overview
This document describes the infrastructure components for HeuristicLogix, implementing the Transactional Outbox Pattern with Kafka event streaming and AI-powered intelligence enrichment.

## Architecture Components

### 1. SQL Server 2022
- **Purpose**: Primary data store with Outbox table for transactional event publishing
- **Port**: 1433
- **Credentials**: sa / HeuristicLogix2026!
- **Volume**: `heuristiclogix-sqlserver-data` (persists across restarts)

### 2. Kafka (KRaft Mode)
- **Purpose**: Event streaming platform for `expert.decisions.v1` and `heuristic.telemetry.v1` topics
- **Mode**: KRaft (no ZooKeeper required)
- **Port**: 9092
- **Volume**: `heuristiclogix-kafka-data` (persists across restarts)

### 3. Kafka UI
- **Purpose**: Web-based Kafka monitoring and debugging tool
- **Port**: 8080
- **URL**: http://localhost:8080

### 4. Intelligence Service (Python FastAPI)
- **Purpose**: Consumes Kafka events and enriches them with Gemini 2.5 Flash AI
- **Port**: 8000
- **Health Check**: http://localhost:8000/health
- **API Docs**: http://localhost:8000/docs

## Quick Start

### Prerequisites
- Docker Desktop installed and running
- .NET 10 SDK installed
- Gemini API Key (get from https://makersuite.google.com/app/apikey)

### Step 1: Configure Environment Variables
```bash
cp .env.example .env
# Edit .env and add your GEMINI_API_KEY
```

### Step 2: Start Infrastructure
```bash
docker-compose up -d
```

This will start:
- SQL Server 2022
- Kafka (KRaft)
- Kafka UI
- Intelligence Service

### Step 3: Verify Services
```bash
# Check all containers are running
docker-compose ps

# Check SQL Server
docker exec -it heuristiclogix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HeuristicLogix2026!" -Q "SELECT @@VERSION" -C

# Check Kafka UI
# Open http://localhost:8080 in browser

# Check Intelligence Service
curl http://localhost:8000/health
```

### Step 4: Initialize Database
```bash
# Navigate to API project
cd HeuristicLogix.Api

# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

### Step 5: Run HeuristicLogix.Api
```bash
cd HeuristicLogix.Api
dotnet run
```

The API will be available at: https://localhost:5001

## C# Components

### OutboxEvent Entity (`HeuristicLogix.Shared\Models\OutboxEvent.cs`)
Represents an event in the transactional outbox pattern:
- `Id`: Unique event identifier
- `EventType`: Type of event (e.g., "expert.decision.created")
- `Topic`: Kafka topic to publish to
- `AggregateId`: Partition key for Kafka ordering
- `PayloadJson`: JSON payload (enums serialized as strings)
- `Status`: Pending, Processing, Published, Failed, Archived

### TransactionalOutboxService (`HeuristicLogix.Api\Services\TransactionalOutboxService.cs`)
Service for managing outbox events:
- `AddEventAsync<T>()`: Add event within current transaction
- `GetPendingEventsAsync()`: Get events ready for publishing
- `MarkAsPublishedAsync()`: Mark event as successfully published
- `MarkAsFailedAsync()`: Mark event as failed after retries

### OutboxPublisherBackgroundService (`HeuristicLogix.Api\Services\OutboxPublisherBackgroundService.cs`)
Background service that:
- Polls outbox table every 5 seconds (configurable)
- Publishes pending events to Kafka
- Implements retry logic with exponential backoff
- Marks events as published or failed

### HeuristicLogixDbContext (`HeuristicLogix.Api\Services\HeuristicLogixDbContext.cs`)
Entity Framework Core DbContext with:
- String-based enum serialization (ARCHITECTURE.md requirement)
- Configured DbSets for all entities
- Proper indexes for query performance

## Python Intelligence Service

### Structure
```
IntelligenceService/
??? main.py              # FastAPI application
??? Dockerfile           # Container definition
??? requirements.txt     # Python dependencies
```

### Key Features
1. **Kafka Consumer**: Consumes from `expert.decisions.v1` and `heuristic.telemetry.v1`
2. **Gemini Integration**: Uses Gemini 2.5 Flash for AI enrichment
3. **SQL Server Storage**: Stores enriched insights in `AIEnrichments` table
4. **Health Checks**: `/health` endpoint for monitoring

### API Endpoints
- `GET /health`: Health check
- `GET /metrics`: Service metrics
- `POST /analyze`: Manual expert decision analysis (for testing)

## Event Flow

### Expert Decision Override Flow
1. User drags Conduce from Pending to a Truck in PlanningDashboard.razor
2. `ExpertHeuristicFeedback` created with OverrideReasonTag
3. API calls `TransactionalOutboxService.AddEventAsync()` within transaction
4. `OutboxEvent` created with topic=`expert.decisions.v1`
5. `OutboxPublisherBackgroundService` polls and publishes to Kafka
6. Python Intelligence Service consumes event
7. Gemini 2.5 Flash analyzes decision and generates tags
8. Enrichment stored in `AIEnrichments` table for ML training

### Telemetry Event Flow
1. System event occurs (route completed, capacity updated, etc.)
2. API creates `OutboxEvent` with topic=`heuristic.telemetry.v1`
3. Event published to Kafka by background service
4. Python service enriches with AI insights
5. Stored for analytics and model training

## Configuration

### API (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=HeuristicLogix;..."
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  },
  "OutboxPublisher": {
    "PollingIntervalSeconds": 5,
    "BatchSize": 100,
    "MaxRetryAttempts": 3
  }
}
```

### Python Service (.env)
```env
GEMINI_API_KEY=your_api_key_here
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
SQLSERVER_CONNECTION_STRING=Server=sqlserver,1433;Database=HeuristicLogix;...
```

## Monitoring

### Kafka UI
- Access: http://localhost:8080
- View topics, messages, consumer groups
- Debug message flow

### Intelligence Service Logs
```bash
docker logs -f heuristiclogix-intelligence
```

### SQL Server Queries
```sql
-- Check outbox events
SELECT * FROM OutboxEvents WHERE Status = 'Pending' ORDER BY CreatedAt DESC;

-- Check AI enrichments
SELECT * FROM AIEnrichments ORDER BY CreatedAt DESC;

-- Check expert feedback
SELECT * FROM ExpertFeedbacks ORDER BY RecordedAt DESC;
```

## Troubleshooting

### Kafka Connection Issues
```bash
# Check Kafka is running
docker exec -it heuristiclogix-kafka kafka-topics --list --bootstrap-server localhost:9092

# Create topics manually if needed
docker exec -it heuristiclogix-kafka kafka-topics --create --topic expert.decisions.v1 --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1
docker exec -it heuristiclogix-kafka kafka-topics --create --topic heuristic.telemetry.v1 --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1
```

### SQL Server Connection Issues
```bash
# Test connection
docker exec -it heuristiclogix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HeuristicLogix2026!" -Q "SELECT 1" -C
```

### Intelligence Service Issues
```bash
# Check logs
docker logs heuristiclogix-intelligence

# Restart service
docker-compose restart intelligence-service
```

## Stopping Infrastructure
```bash
# Stop all containers
docker-compose down

# Stop and remove volumes (WARNING: deletes all data)
docker-compose down -v
```

## Next Steps
1. Implement API controllers for Conduce, Truck, and Route management
2. Add authentication/authorization (Azure AD B2C or Identity Server)
3. Implement ML.NET models for local inference
4. Add monitoring (Application Insights or Prometheus)
5. Implement backup strategy for SQL Server and Kafka

## References
- [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Kafka KRaft Mode](https://kafka.apache.org/documentation/#kraft)
- [Gemini API Documentation](https://ai.google.dev/docs)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
