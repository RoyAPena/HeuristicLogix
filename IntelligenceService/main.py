"""
HeuristicLogix Intelligence Service
FastAPI service for consuming Kafka events and enriching them with Gemini 2.5 Flash AI.

Architecture:
- Consumes events from Kafka topics: expert.decisions.v1, heuristic.telemetry.v1
- Uses Gemini 2.5 Flash for real-time telemetry labeling and decision analysis
- Writes enriched insights back to SQL Server for ML training pipelines
- Implements idempotency checks to prevent duplicate processing
- Uses SQLAlchemy async for database operations
"""

import asyncio
import json
import logging
import os
import time
from contextlib import asynccontextmanager
from datetime import datetime
from typing import Any, Dict, List, Optional
from urllib.parse import quote_plus

import google.generativeai as genai
from aiokafka import AIOKafkaConsumer
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from sqlalchemy import Column, DateTime, Float, String, Text, select
from sqlalchemy.dialects.mssql import UNIQUEIDENTIFIER
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine
from sqlalchemy.orm import declarative_base, sessionmaker

# Configure logging
logging.basicConfig(
    level=os.getenv("LOG_LEVEL", "INFO"),
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# Environment configuration
KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092")
KAFKA_CONSUMER_GROUP = os.getenv("KAFKA_CONSUMER_GROUP", "intelligence-workers")
KAFKA_TOPICS = os.getenv("KAFKA_TOPICS", "expert.decisions.v1,heuristic.telemetry.v1").split(",")
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
GEMINI_MODEL = os.getenv("GEMINI_MODEL", "gemini-2.5-flash")
SQLSERVER_CONNECTION_STRING = os.getenv("SQLSERVER_CONNECTION_STRING")

# Build async SQLAlchemy connection string
if SQLSERVER_CONNECTION_STRING:
    # Parse connection string and build async version with TrustServerCertificate
    # Example: Server=sqlserver,1433;Database=HeuristicLogix;User Id=sa;Password=xxx;TrustServerCertificate=True
    conn_parts = dict(part.split('=', 1) for part in SQLSERVER_CONNECTION_STRING.split(';') if '=' in part)
    server = conn_parts.get('Server', 'localhost,1433')
    database = conn_parts.get('Database', 'HeuristicLogix')
    user = conn_parts.get('User Id', 'sa')
    password = conn_parts.get('Password', '')
    
    # Async connection string for aioodbc driver
    ASYNC_DATABASE_URL = (
        f"mssql+aioodbc://{user}:{quote_plus(password)}@{server}/{database}"
        f"?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes"
    )
    
    # Create async engine
    async_engine = create_async_engine(
        ASYNC_DATABASE_URL,
        echo=False,
        pool_pre_ping=True,
        pool_recycle=3600
    )
    
    # Create async session factory
    AsyncSessionLocal = sessionmaker(
        async_engine,
        class_=AsyncSession,
        expire_on_commit=False
    )
    
    logger.info(f"Database configured: {server}/{database}")
else:
    logger.warning("SQLSERVER_CONNECTION_STRING not set. Database operations will be disabled.")
    async_engine = None
    AsyncSessionLocal = None

# SQLAlchemy ORM Models
Base = declarative_base()

class AIEnrichment(Base):
    """SQLAlchemy model for AIEnrichments table."""
    __tablename__ = "AIEnrichments"
    
    Id = Column(UNIQUEIDENTIFIER, primary_key=True, server_default="NEWID()")
    EventId = Column(String(450), nullable=False, index=True, unique=True)
    EventType = Column(String(100), nullable=False)
    AITags = Column(Text)
    AIConfidenceScore = Column(Float)
    AIReasoning = Column(Text)
    AISuggestedActions = Column(Text)
    ProcessingTimeMs = Column(Float)
    CreatedAt = Column(DateTime, server_default="SYSDATETIMEOFFSET()")

# Initialize Gemini AI
if GEMINI_API_KEY:
    genai.configure(api_key=GEMINI_API_KEY)
    gemini_model = genai.GenerativeModel(GEMINI_MODEL)
    logger.info(f"Gemini AI initialized with model: {GEMINI_MODEL}")
else:
    logger.warning("GEMINI_API_KEY not set. AI enrichment will be disabled.")
    gemini_model = None



class ExpertDecisionEvent(BaseModel):
"""
Event schema for expert decision overrides.
MVP Contract: Includes all essential fields for logistics intelligence.
"""
# Core identifiers
feedback_id: str
order_id: str  # MVP: OrderId for tracking
conduce_id: str
    
# Truck assignment decision
suggested_truck_id: Optional[str] = None  # MVP: AI suggestion
selected_truck_id: str  # MVP: Expert selection
expert_assigned_truck_id: str  # Deprecated: use selected_truck_id
ai_suggested_truck_id: Optional[str] = None  # Deprecated: use suggested_truck_id
    
# Decision context
primary_reason: str
secondary_reasons: List[str] = Field(default_factory=list)
decision_time_seconds: float
expert_note: Optional[str] = None  # MVP: ExpertNote
expert_notes: Optional[str] = None  # Deprecated: use expert_note
    
# Order details
total_weight: Optional[float] = None  # MVP: TotalWeight in kg
    
# Metadata
recorded_at_utc: str
    
def __post_init__(self):
    """Backwards compatibility mapping."""
    # Map deprecated fields to new MVP fields
    if not hasattr(self, 'order_id') or not self.order_id:
        self.order_id = self.conduce_id
    if not hasattr(self, 'selected_truck_id') or not self.selected_truck_id:
        self.selected_truck_id = self.expert_assigned_truck_id
    if not hasattr(self, 'suggested_truck_id') or not self.suggested_truck_id:
        self.suggested_truck_id = self.ai_suggested_truck_id
    if not hasattr(self, 'expert_note') or not self.expert_note:
        self.expert_note = self.expert_notes


class TelemetryEvent(BaseModel):
    """Event schema for heuristic telemetry data."""
    event_id: str
    event_type: str
    aggregate_id: str
    timestamp_utc: str
    payload: Dict[str, Any]


class IntelligenceEnrichment(BaseModel):
    """Enriched insights from AI analysis."""
    event_id: str
    event_type: str
    ai_tags: List[str] = Field(default_factory=list)
    ai_confidence_score: float
    ai_reasoning: str
    ai_suggested_actions: List[str] = Field(default_factory=list)
    processing_time_ms: float


# Global Kafka consumer
kafka_consumer: Optional[AIOKafkaConsumer] = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Lifecycle manager for FastAPI - starts/stops Kafka consumer and initializes database."""
    global kafka_consumer
    
    # Startup: Initialize database
    logger.info("Initializing database...")
    await initialize_database()
    
    # Startup: Initialize Kafka consumer
    logger.info("Starting Kafka consumer...")
    kafka_consumer = AIOKafkaConsumer(
        *KAFKA_TOPICS,
        bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
        group_id=KAFKA_CONSUMER_GROUP,
        auto_offset_reset="earliest",
        enable_auto_commit=True,
        value_deserializer=lambda m: json.loads(m.decode("utf-8"))
    )
    
    await kafka_consumer.start()
    logger.info(f"Kafka consumer started. Subscribed to topics: {KAFKA_TOPICS}")
    
    # Start background task for consuming messages
    consumer_task = asyncio.create_task(consume_kafka_messages())
    
    yield
    
    # Shutdown: Stop Kafka consumer
    logger.info("Stopping Kafka consumer...")
    consumer_task.cancel()
    await kafka_consumer.stop()
    logger.info("Kafka consumer stopped.")


app = FastAPI(
    title="HeuristicLogix Intelligence Service",
    description="AI-powered event enrichment for logistics optimization",
    version="1.0.0",
    lifespan=lifespan
)


async def consume_kafka_messages():
    """Background task to consume and process Kafka messages."""
    try:
        async for message in kafka_consumer:
            topic = message.topic
            event_data = message.value
            
            logger.info(f"Received event from topic '{topic}': {event_data.get('event_type', 'unknown')}")
            
            try:
                if topic == "expert.decisions.v1":
                    await process_expert_decision(event_data)
                elif topic == "heuristic.telemetry.v1":
                    await process_telemetry_event(event_data)
                else:
                    logger.warning(f"Unknown topic: {topic}")
            except Exception as e:
                logger.error(f"Error processing message from topic '{topic}': {e}", exc_info=True)
    except asyncio.CancelledError:
        logger.info("Kafka consumer task cancelled.")
    except Exception as e:
        logger.error(f"Fatal error in Kafka consumer: {e}", exc_info=True)



async def check_enrichment_exists(event_id: str) -> bool:
    """
    Check if enrichment already exists for the given event ID (idempotency).
    
    Args:
        event_id: The event identifier to check
        
    Returns:
        True if enrichment exists, False otherwise
    """
    if not AsyncSessionLocal:
        return False
    
    try:
        async with AsyncSessionLocal() as session:
            stmt = select(AIEnrichment).where(AIEnrichment.EventId == event_id)
            result = await session.execute(stmt)
            enrichment = result.scalar_one_or_none()
            return enrichment is not None
    except Exception as error:
        logger.error(f"Error checking enrichment existence for {event_id}: {error}", exc_info=True)
        return False


async def process_expert_decision(event_data: Dict[str, Any]):
    """Process expert decision event with AI enrichment (idempotent)."""
    start_time = time.time()
    
    try:
        decision = ExpertDecisionEvent(**event_data)
        
        # IDEMPOTENCY CHECK: Skip if already processed
        if await check_enrichment_exists(decision.feedback_id):
            logger.warning(
                f"Skipping expert decision {decision.feedback_id} - already enriched (idempotency)"
            )
            return
        
        # Generate AI insights using Gemini
        if gemini_model:
            prompt = f"""
Analyze this expert logistics decision override:

Primary Reason: {decision.primary_reason}
Secondary Reasons: {', '.join(decision.secondary_reasons) if decision.secondary_reasons else 'None'}
Decision Time: {decision.decision_time_seconds} seconds
Expert Notes: {decision.expert_notes or 'None'}

Task: Provide actionable tags and insights for ML training. Focus on:
1. Pattern identification (e.g., "quick_override", "capacity_misjudgment")
2. Confidence score (0-100) on whether this was a clear AI error
3. Suggested model improvements

Format your response as JSON with keys: tags, confidence_score, reasoning, suggested_actions
"""
            
            response = await asyncio.to_thread(
                gemini_model.generate_content,
                prompt
            )
            
            # Parse AI response
            ai_response = json.loads(response.text)
            
            processing_time_ms = (time.time() - start_time) * 1000
            
            enrichment = IntelligenceEnrichment(
                event_id=decision.feedback_id,
                event_type="expert.decision",
                ai_tags=ai_response.get("tags", []),
                ai_confidence_score=ai_response.get("confidence_score", 0.0),
                ai_reasoning=ai_response.get("reasoning", ""),
                ai_suggested_actions=ai_response.get("suggested_actions", []),
                processing_time_ms=processing_time_ms
            )
            
            # Store enrichment in SQL Server
            await store_enrichment(enrichment)
            
            logger.info(
                f"Expert decision {decision.feedback_id} enriched with AI tags: {enrichment.ai_tags} "
                f"(processing time: {processing_time_ms:.2f}ms)"
            )
        else:
            logger.warning("AI enrichment skipped (Gemini not configured)")
    except Exception as error:
        logger.error(f"Error processing expert decision: {error}", exc_info=True)


async def process_telemetry_event(event_data: Dict[str, Any]):
    """Process telemetry event with AI enrichment (idempotent)."""
    start_time = time.time()
    
    try:
        telemetry = TelemetryEvent(**event_data)
        
        # IDEMPOTENCY CHECK: Skip if already processed
        if await check_enrichment_exists(telemetry.event_id):
            logger.warning(
                f"Skipping telemetry event {telemetry.event_id} - already enriched (idempotency)"
            )
            return
        
        # Generate AI insights using Gemini
        if gemini_model:
            prompt = f"""
Analyze this logistics telemetry event:

Event Type: {telemetry.event_type}
Aggregate ID: {telemetry.aggregate_id}
Payload: {json.dumps(telemetry.payload, indent=2)}

Task: Provide telemetry tags and anomaly detection insights. Focus on:
1. Event classification tags (e.g., "route_optimization", "capacity_utilization")
2. Anomaly detection (any unusual patterns?)
3. Predictive insights for future optimizations

Format your response as JSON with keys: tags, confidence_score, reasoning, suggested_actions
"""
            
            response = await asyncio.to_thread(
                gemini_model.generate_content,
                prompt
            )
            
            # Parse AI response
            ai_response = json.loads(response.text)
            
            processing_time_ms = (time.time() - start_time) * 1000
            
            enrichment = IntelligenceEnrichment(
                event_id=telemetry.event_id,
                event_type=telemetry.event_type,
                ai_tags=ai_response.get("tags", []),
                ai_confidence_score=ai_response.get("confidence_score", 0.0),
                ai_reasoning=ai_response.get("reasoning", ""),
                ai_suggested_actions=ai_response.get("suggested_actions", []),
                processing_time_ms=processing_time_ms
            )
            
            # Store enrichment in SQL Server
            await store_enrichment(enrichment)
            
            logger.info(
                f"Telemetry event {telemetry.event_id} enriched with AI tags: {enrichment.ai_tags} "
                f"(processing time: {processing_time_ms:.2f}ms)"
            )
        else:
            logger.warning("AI enrichment skipped (Gemini not configured)")
    except Exception as error:
        logger.error(f"Error processing telemetry event: {error}", exc_info=True)





async def store_enrichment(enrichment: IntelligenceEnrichment):
    """Store AI enrichment results in SQL Server using SQLAlchemy async."""
    if not AsyncSessionLocal:
        logger.warning("SQL Server not configured. Skipping storage.")
        return
    
    try:
        async with AsyncSessionLocal() as session:
            # Create AIEnrichment record
            db_enrichment = AIEnrichment(
                EventId=enrichment.event_id,
                EventType=enrichment.event_type,
                AITags=json.dumps(enrichment.ai_tags),
                AIConfidenceScore=enrichment.ai_confidence_score,
                AIReasoning=enrichment.ai_reasoning,
                AISuggestedActions=json.dumps(enrichment.ai_suggested_actions),
                ProcessingTimeMs=enrichment.processing_time_ms
            )
            
            session.add(db_enrichment)
            await session.commit()
            
            logger.info(f"Enrichment stored for event {enrichment.event_id}")
    except Exception as error:
        logger.error(f"Error storing enrichment: {error}", exc_info=True)


async def initialize_database():
    """Initialize database tables if they don't exist."""
    if not async_engine:
        return
    
    try:
        async with async_engine.begin() as conn:
            await conn.run_sync(Base.metadata.create_all)
        logger.info("Database tables initialized")
    except Exception as error:
        logger.error(f"Error initializing database: {error}", exc_info=True)

    
    try:
        def insert_enrichment():
            conn = pyodbc.connect(SQLSERVER_CONNECTION_STRING)
            cursor = conn.cursor()
            
            cursor.execute("""
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AIEnrichments')
                CREATE TABLE AIEnrichments (
                    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    EventId NVARCHAR(450) NOT NULL,
                    EventType NVARCHAR(100) NOT NULL,
                    AITags NVARCHAR(MAX),


@app.get("/health")
async def health_check():
    """Health check endpoint for container orchestration."""
    kafka_status = "connected" if kafka_consumer and not kafka_consumer._closed else "disconnected"
    gemini_status = "configured" if gemini_model else "not_configured"
    database_status = "configured" if AsyncSessionLocal else "not_configured"
    
    return {
        "status": "healthy",
        "service": "intelligence-service",
        "version": "1.1.0",
        "kafka": kafka_status,
        "gemini": gemini_status,
        "database": database_status
    }


@app.get("/metrics")
async def get_metrics():
    """Endpoint for service metrics (for monitoring)."""
    # TODO: Implement metrics collection
    return {
        "events_processed": 0,
        "ai_enrichments_generated": 0,
        "average_processing_time_ms": 0
    }


@app.post("/analyze")
async def analyze_decision(decision: ExpertDecisionEvent):
    """Manual endpoint for analyzing expert decisions (for testing)."""
    if not gemini_model:
        raise HTTPException(status_code=503, detail="Gemini AI not configured")
    
    await process_expert_decision(decision.model_dump())
    return {"status": "analyzed", "decision_id": decision.feedback_id}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
