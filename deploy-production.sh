#!/bin/bash
# HeuristicLogix Complete Infrastructure Setup
# Terraform + Docker Compose Production Deployment

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default parameters
SKIP_TERRAFORM=false
SKIP_BUILD=false
DETACHED=false
SQL_PASSWORD="HeuristicLogix2026!"
GEMINI_API_KEY=""

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-terraform)
            SKIP_TERRAFORM=true
            shift
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --detached|-d)
            DETACHED=true
            shift
            ;;
        --sql-password)
            SQL_PASSWORD="$2"
            shift 2
            ;;
        --gemini-key)
            GEMINI_API_KEY="$2"
            shift 2
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

echo -e "${CYAN}============================================${NC}"
echo -e "${CYAN}  HeuristicLogix Infrastructure Setup${NC}"
echo -e "${CYAN}  Production-Ready Docker Orchestration${NC}"
echo -e "${CYAN}============================================${NC}"
echo ""

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Step 1: Prerequisites Check
echo -e "${YELLOW}[1/5] Checking prerequisites...${NC}"
echo ""

# Check Docker
if command_exists docker; then
    DOCKER_VERSION=$(docker --version)
    echo -e "  ${GREEN}? Docker: $DOCKER_VERSION${NC}"
else
    echo -e "  ${RED}? Docker not found${NC}"
    echo -e "    Install from: https://www.docker.com/products/docker-desktop"
    exit 1
fi

# Check Docker Compose
if docker compose version >/dev/null 2>&1; then
    COMPOSE_VERSION=$(docker compose version)
    echo -e "  ${GREEN}? Docker Compose: $COMPOSE_VERSION${NC}"
else
    echo -e "  ${RED}? Docker Compose not available${NC}"
    exit 1
fi

# Check Terraform
if [ "$SKIP_TERRAFORM" = false ]; then
    if command_exists terraform; then
        TERRAFORM_VERSION=$(terraform version | head -n1)
        echo -e "  ${GREEN}? Terraform: $TERRAFORM_VERSION${NC}"
    else
        echo -e "  ${YELLOW}? Terraform not found - will create .env manually${NC}"
        SKIP_TERRAFORM=true
    fi
fi

# Check .NET SDK (optional)
if command_exists dotnet; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "  ${GREEN}? .NET SDK: $DOTNET_VERSION${NC}"
fi

echo ""

# Step 2: Terraform Infrastructure Provisioning
if [ "$SKIP_TERRAFORM" = false ]; then
    echo -e "${YELLOW}[2/5] Provisioning infrastructure with Terraform...${NC}"
    echo ""
    
    cd infrastructure/terraform
    
    echo -e "  ${CYAN}? Initializing Terraform...${NC}"
    terraform init -upgrade
    
    echo -e "  ${CYAN}? Validating configuration...${NC}"
    terraform validate
    
    echo -e "  ${CYAN}? Creating execution plan...${NC}"
    terraform plan \
        -var="sql_password=$SQL_PASSWORD" \
        -out=tfplan
    
    echo -e "  ${CYAN}? Applying infrastructure...${NC}"
    terraform apply -auto-approve tfplan
    
    echo -e "  ${GREEN}? Infrastructure provisioned${NC}"
    
    cd ../..
else
    echo -e "${YELLOW}[2/5] Creating .env configuration manually...${NC}"
    echo ""
    
    cat > .env << EOF
# HeuristicLogix Environment Configuration
# Generated: $(date '+%Y-%m-%d %H:%M:%S')

# SQL Server Configuration
SQLSERVER_PASSWORD=$SQL_PASSWORD
SQLSERVER_CONNECTION_STRING=Server=db_sql,1433;Database=HeuristicLogixDB;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True;

# Kafka Configuration
KAFKA_BOOTSTRAP_SERVERS=kafka_bus:9092
KAFKA_TOPIC_EXPERT_DECISIONS=expert.decisions.v1
KAFKA_TOPIC_HISTORIC_DELIVERIES=historic.deliveries.v1
KAFKA_CONSUMER_GROUP=heuristiclogix-intelligence

# Zookeeper Configuration
ZOOKEEPER_HOST=zookeeper
ZOOKEEPER_PORT=2181

# AI Configuration
GEMINI_API_KEY=$GEMINI_API_KEY
GEMINI_MODEL=gemini-2.0-flash-exp

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
TZ=America/Santo_Domingo
LOG_LEVEL=Information

# Network
DOCKER_NETWORK=heuristic_net
EOF
    
    echo -e "  ${GREEN}? .env file created${NC}"
fi

echo ""

# Step 3: Build Docker Images
if [ "$SKIP_BUILD" = false ]; then
    echo -e "${YELLOW}[3/5] Building Docker images...${NC}"
    echo ""
    
    echo -e "  ${CYAN}? Building API image (api_logistics)...${NC}"
    docker build -t heuristiclogix/api:latest -f Dockerfile.api .
    
    echo ""
    echo -e "  ${CYAN}? Building UI image (ui_blazor)...${NC}"
    docker build -t heuristiclogix/ui:latest -f Dockerfile.ui .
    
    echo ""
    echo -e "  ${CYAN}? Building AI image (ai_brain)...${NC}"
    docker build -t heuristiclogix/ai:latest ./IntelligenceService
    
    echo ""
    echo -e "  ${GREEN}? All images built successfully${NC}"
else
    echo -e "${YELLOW}[3/5] Skipping Docker image build (using cached images)${NC}"
fi

echo ""

# Step 4: Create Docker Network and Volumes
echo -e "${YELLOW}[4/5] Creating Docker resources...${NC}"
echo ""

# Check if network exists
if ! docker network ls | grep -q heuristic_net; then
    echo -e "  ${CYAN}? Creating Docker network (heuristic_net)...${NC}"
    docker network create heuristic_net
    echo -e "  ${GREEN}? Network created${NC}"
else
    echo -e "  ${GREEN}? Network already exists${NC}"
fi

# Create volumes if they don't exist
for VOLUME in sql_data kafka_logs zk_data ai_model_cache; do
    if ! docker volume ls | grep -q $VOLUME; then
        echo -e "  ${CYAN}? Creating volume ($VOLUME)...${NC}"
        docker volume create $VOLUME
        echo -e "  ${GREEN}? Volume created${NC}"
    else
        echo -e "  ${GREEN}? Volume $VOLUME already exists${NC}"
    fi
done

echo ""

# Step 5: Start Services
echo -e "${YELLOW}[5/5] Starting services with Docker Compose...${NC}"
echo ""

COMPOSE_FILE="docker-compose.production.yml"
COMPOSE_CMD="docker compose -f $COMPOSE_FILE up"

if [ "$DETACHED" = true ]; then
    COMPOSE_CMD="$COMPOSE_CMD -d --wait"
fi

echo -e "  ${CYAN}? Starting all services...${NC}"
eval $COMPOSE_CMD

echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}  Setup Complete! ?${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""

# Display service information
echo -e "${CYAN}Services Running:${NC}"
echo -e "  • SQL Server     ? localhost:1433"
echo -e "  • Zookeeper      ? localhost:2181"
echo -e "  • Kafka          ? localhost:9092"
echo -e "  • API (Logistics)? http://localhost:5000"
echo -e "  • UI (Blazor)    ? http://localhost:5001"
echo -e "  • AI (Intelligence)? http://localhost:8000"
echo ""

echo -e "${CYAN}Health Check Endpoints:${NC}"
echo -e "  • API Health     ? http://localhost:5000/health"
echo -e "  • UI Health      ? http://localhost:5001/health"
echo -e "  • AI Health      ? http://localhost:8000/health"
echo ""

echo -e "${CYAN}Quick Commands:${NC}"
echo -e "  ${YELLOW}View logs        ? docker compose -f $COMPOSE_FILE logs -f [service-name]${NC}"
echo -e "  ${YELLOW}Stop all         ? docker compose -f $COMPOSE_FILE down${NC}"
echo -e "  ${YELLOW}Restart service  ? docker compose -f $COMPOSE_FILE restart [service-name]${NC}"
echo -e "  ${YELLOW}Service status   ? docker compose -f $COMPOSE_FILE ps${NC}"
echo -e "  ${YELLOW}Execute command  ? docker compose -f $COMPOSE_FILE exec [service-name] [command]${NC}"
echo ""

echo -e "${CYAN}Service Names:${NC}"
echo -e "  • db_sql | zookeeper | kafka_bus | api_logistics | ui_blazor | ai_brain"
echo ""

# If detached, check health
if [ "$DETACHED" = true ]; then
    echo -e "${YELLOW}Waiting for services to be healthy...${NC}"
    sleep 15
    
    echo ""
    echo -e "${CYAN}Service Status:${NC}"
    docker compose -f $COMPOSE_FILE ps
    
    echo ""
    echo -e "${YELLOW}Testing endpoints...${NC}"
    
    # Test API
    if curl -sf http://localhost:5000/health > /dev/null; then
        echo -e "  ${GREEN}? API is responding${NC}"
    else
        echo -e "  ${YELLOW}? API not ready yet${NC}"
    fi
    
    # Test AI
    if curl -sf http://localhost:8000/health > /dev/null; then
        echo -e "  ${GREEN}? AI service is responding${NC}"
    else
        echo -e "  ${YELLOW}? AI service not ready yet${NC}"
    fi
fi

echo ""
echo -e "${GREEN}?? HeuristicLogix is now running!${NC}"
echo ""
