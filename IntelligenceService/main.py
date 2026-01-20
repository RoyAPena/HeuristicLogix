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








class HistoricDeliveryEvent(BaseModel):
    """
Event schema for unit-aware historic delivery ingestion (batch mode).
Used for building the AI knowledge base from past deliveries.
Supports quantity/unit parsing and taxonomy-based weight calculation.
"""
delivery_date: str
client_name: str
raw_description: str  # Product description (may include quantity/unit if not parsed)
quantity: Optional[float] = None  # Parsed quantity
raw_unit: Optional[str] = None  # Parsed unit (BAG, M3, TON, etc.)
calculated_weight: Optional[float] = None  # Weight from taxonomy (Quantity * WeightFactor)
total_weight_kg: Optional[float] = None  # Final weight (calculated or provided)
is_weight_calculated: bool = False  # Whether weight was calculated from taxonomy
taxonomy_id: Optional[str] = None  # Product taxonomy ID if found
is_taxonomy_verified: bool = False  # Whether taxonomy is expert-verified
delivery_address: str
latitude: float
longitude: float
truck_license_plate: str
service_time_minutes: float
expert_notes: Optional[str] = None
override_reason: Optional[str] = None
is_historic: bool = True
ingestion_batch_id: str


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
            
            logger.info(f"Received event from topic '{topic}': {event_data.get('type', event_data.get('event_type', 'unknown'))}")
            
            try:
                # Check if this is a CloudEvent (has 'type' field) or legacy event
                if "type" in event_data:
                    # CloudEvent format
                    await process_cloud_event(topic, event_data)
                elif topic == "expert.decisions.v1":
                    await process_expert_decision(event_data)
                elif topic == "heuristic.telemetry.v1":
                    await process_telemetry_event(event_data)
                elif topic == "historic.deliveries.v1":
                    await process_historic_delivery(event_data)
                else:
                    logger.warning(f"Unknown topic: {topic}")
            except Exception as e:
                logger.error(f"Error processing message from topic '{topic}': {e}", exc_info=True)
    except asyncio.CancelledError:
        logger.info("Kafka consumer task cancelled.")
    except Exception as e:
        logger.error(f"Fatal error in Kafka consumer: {e}", exc_info=True)


async def process_cloud_event(topic: str, cloud_event: Dict[str, Any]):
    """Process CloudEvent format messages."""
    event_type = cloud_event.get("type", "")
    event_data = cloud_event.get("data", {})
    extensions = cloud_event.get("extensions", {})
    
    # Check if this is a historic event (batch mode)
    is_historic = extensions.get("is_historic", "false").lower() == "true"
    
    if event_type == "HistoricDeliveryIngested":
        await process_historic_delivery(event_data, is_historic)
    elif "expert" in event_type.lower() or "decision" in event_type.lower():
        await process_expert_decision(event_data)
    elif "telemetry" in event_type.lower():
        await process_telemetry_event(event_data)
    else:
        logger.warning(f"Unknown CloudEvent type: {event_type}")




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


async def process_historic_delivery(event_data: Dict[str, Any], is_historic: bool = True):
    """
    Process unit-aware historic delivery event with pattern recognition focus (batch mode).
    Handles quantity/unit parsing and taxonomy-based weight calculations.
    
    Args:
        event_data: The historic delivery event data
        is_historic: Whether this is a historic event (batch mode)
    """
    start_time = time.time()
    
    try:
        delivery = HistoricDeliveryEvent(**event_data)
        
        # Generate unique event ID from batch ID and delivery details
        event_id = f"{delivery.ingestion_batch_id}_{delivery.truck_license_plate}_{delivery.delivery_date}"
        
        # IDEMPOTENCY CHECK: Skip if already processed
        if await check_enrichment_exists(event_id):
            logger.warning(
                f"Skipping historic delivery {event_id} - already enriched (idempotency)"
            )
            return
        
        # Determine if we have quantity/unit data
        has_quantity_data = delivery.quantity is not None and delivery.raw_unit is not None
        has_taxonomy = delivery.taxonomy_id is not None
        weight_source = "calculated" if delivery.is_weight_calculated else "provided" if delivery.total_weight_kg else "missing"
        
        # BATCH MODE: Pattern recognition for historic data with unit awareness
        if gemini_model and is_historic:
            prompt = f"""
Analyze this HISTORIC delivery record for pattern recognition and AI training (UNIT-AWARE):

Product: {delivery.raw_description}
Quantity: {delivery.quantity if delivery.quantity else 'Not provided'} {delivery.raw_unit if delivery.raw_unit else ''}
Weight: {delivery.total_weight_kg if delivery.total_weight_kg else 'Not provided'} kg ({weight_source})
Taxonomy Status: {'Verified' if delivery.is_taxonomy_verified else 'Pending' if has_taxonomy else 'Not found'}

Delivery Date: {delivery.delivery_date}
Client: {delivery.client_name}
Address: {delivery.delivery_address}
Truck: {delivery.truck_license_plate}
Service Time: {delivery.service_time_minutes} minutes
Expert Notes: {delivery.expert_notes or 'None'}
Override Reason: {delivery.override_reason or 'None'}

Task: Extract patterns and insights for AI knowledge base training with UNIT AWARENESS:

1. Product identification and categorization
   - What product category is "{delivery.raw_description}"? (AGGREGATE, CEMENT, STEEL, REBAR, etc.)
   - Are there standard units for this product? (BAG, M3, TON, KG, PIECE, METER)
   
2. Weight calculation validation (if weight is {'calculated' if delivery.is_weight_calculated else 'provided'})
   - Does the weight seem reasonable for this product and quantity?
   - Estimate typical weight per unit for this product category
   - Any anomalies in weight data?

3. Quantity/Unit patterns
   - Is the quantity typical for this product type?
   - Is the unit measurement standard for this category?
   - Any unit conversion insights?

4. Capacity utilization by product
   - Weight vs service time correlation for "{delivery.raw_description}"
   - Truck type effectiveness for this product/quantity

5. Expert decision patterns
   - Why did expert assign this truck for "{delivery.raw_description}"?
   - Any product-specific routing considerations?

6. Taxonomy recommendations (if {'taxonomy exists but unverified' if has_taxonomy and not delivery.is_taxonomy_verified else 'no taxonomy'})
   - Suggest standard product description
   - Recommend weight factor (kg per unit)
   - Suggest product category

IMPORTANT: This is historic data for pattern learning, NOT real-time alerts.
Focus on unit-aware product analysis and taxonomy building.

Format your response as JSON with keys: 
- product_category: str (AGGREGATE, CEMENT, STEEL, etc.)
- suggested_standard_description: str (standardized product name)
- suggested_weight_factor: float (kg per unit, or 0 if not applicable)
- suggested_unit: str (BAG, M3, TON, etc.)
- patterns: List[str] (identified patterns)
- insights: str (key insights for training)
- training_recommendations: List[str] (how to use this data)
- capacity_score: float (0-100, how well utilized was the truck)
- weight_validation: str (assessment of weight data quality)
"""
            
            logger.info(
                f"Processing unit-aware historic delivery (batch mode): {event_id} "
                f"[Product: {delivery.raw_description}, Qty: {delivery.quantity} {delivery.raw_unit}, "
                f"Weight: {weight_source}]"
            )
            
            response = await asyncio.to_thread(
                gemini_model.generate_content,
                prompt
            )
            
            # Parse AI response
            ai_response = json.loads(response.text)
            
            processing_time_ms = (time.time() - start_time) * 1000
            
            # Extract patterns as tags, including product category and unit info
            patterns = ai_response.get("patterns", [])
            patterns.insert(0, f"PRODUCT:{delivery.raw_description}")
            if delivery.quantity and delivery.raw_unit:
                patterns.insert(1, f"UNIT:{delivery.raw_unit}")
            if has_taxonomy:
                patterns.insert(2, f"TAXONOMY:{'VERIFIED' if delivery.is_taxonomy_verified else 'PENDING'}")
            
            insights = ai_response.get("insights", "")
            recommendations = ai_response.get("training_recommendations", [])
            capacity_score = ai_response.get("capacity_score", 50.0)
            weight_validation = ai_response.get("weight_validation", "")
            
            # Extract taxonomy recommendations from AI
            suggested_description = ai_response.get("suggested_standard_description", delivery.raw_description)
            suggested_weight_factor = ai_response.get("suggested_weight_factor", 0.0)
            suggested_unit = ai_response.get("suggested_unit", delivery.raw_unit or "")
            product_category = ai_response.get("product_category", "OTHER")
            
            # Combine insights with unit-aware analysis
            full_reasoning = f"""Product: {delivery.raw_description}
Category: {product_category}
Quantity/Unit: {delivery.quantity} {delivery.raw_unit if delivery.raw_unit else 'N/A'}
Weight: {delivery.total_weight_kg}kg ({weight_source})
Taxonomy: {'Verified' if delivery.is_taxonomy_verified else 'Pending' if has_taxonomy else 'Not found'}

Insights: {insights}

Weight Validation: {weight_validation}

AI Taxonomy Suggestions:
- Standard Description: {suggested_description}
- Weight Factor: {suggested_weight_factor} kg/unit
- Standard Unit: {suggested_unit}
"""
            
            enrichment = IntelligenceEnrichment(
                event_id=event_id,
                event_type="historic_delivery_unit_aware",
                ai_tags=patterns,
                ai_confidence_score=capacity_score,
                ai_reasoning=full_reasoning,
                ai_suggested_actions=recommendations,
                processing_time_ms=processing_time_ms
            )
            
            # Store enrichment in SQL Server
            await store_enrichment(enrichment)
            
            logger.info(
                f"Unit-aware historic delivery {event_id} enriched with {len(patterns)} patterns "
                f"(Product: {delivery.raw_description}, Category: {product_category}, "
                f"processing time: {processing_time_ms:.2f}ms, capacity score: {capacity_score})"
            )
        else:
            logger.warning("AI enrichment skipped (Gemini not configured or not in batch mode)")
    except Exception as error:
        logger.error(f"Error processing unit-aware historic delivery: {error}", exc_info=True)


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
