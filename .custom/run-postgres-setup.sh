#!/bin/bash

##############################################################################
# ABP Next Admin - PostgreSQL Setup & Run Script
# This script automates the entire setup process for running the solution
# with PostgreSQL as the database backend.
#
# Usage: ./run-postgres-setup.sh [options]
#   --help              Show this help message
#   --build-only        Only build the solution, don't run the app
#   --migrate-only      Only run migrations, don't start the app
#   --skip-docker       Skip Docker container creation (assumes already running)
#   --skip-build        Skip building the solution
#   --clean-containers  Stop and remove Docker containers before starting
##############################################################################

set -e

# ============================================================================
# CONFIGURATION - Modify these values as needed
# ============================================================================

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ASPNET_CORE_DIR="${PROJECT_ROOT}/aspnet-core"
SERVICES_DIR="${ASPNET_CORE_DIR}/services/LY.MicroService.Applications.Single"
MIGRATIONS_DIR="${ASPNET_CORE_DIR}/migrations/LY.MicroService.Applications.Single.DbMigrator"

# Database Configuration
DB_HOST="127.0.0.1"
DB_PORT="5432"
DB_NAME="Platform-V70"
DB_USER="postgres"
DB_PASSWORD="123456"
POSTGRES_IMAGE="postgres:15-alpine"
POSTGRES_CONTAINER="postgres-abp"

# Redis Configuration
REDIS_IMAGE="redis:7-alpine"
REDIS_CONTAINER="redis-abp"
REDIS_PORT="6379"

# Application Configuration
APP_PORT="30001"
APP_ENVIRONMENT="Development"
APP_DATABASE_PROVIDER="PostgreSql"
APP_LAUNCH_PROFILE="Single.PostgreSql.Dev"

# Solution Configuration
SOLUTION_FILE="${ASPNET_CORE_DIR}/LINGYUN.MicroService.SingleProject.sln"
BUILD_CONFIG="Debug"

# ============================================================================
# COLORS & FORMATTING
# ============================================================================

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo -e "\n${BLUE}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}\n"
}

print_step() {
    echo -e "${GREEN}▶ $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

# ============================================================================
# UTILITY FUNCTIONS
# ============================================================================

show_help() {
    grep "^#" "$0" | grep -v "^#!/bin/bash" | sed 's/^# //' | sed 's/^#//'
    exit 0
}

check_command() {
    if ! command -v "$1" &> /dev/null; then
        print_error "$1 is not installed. Please install it first."
        exit 1
    fi
}

check_prerequisites() {
    print_header "Checking Prerequisites"
    
    print_step "Checking for required commands..."
    check_command "dotnet"
    check_command "docker"
    check_command "curl"
    check_command "grep"
    
    print_success "All prerequisites installed"
}

wait_for_port() {
    local port=$1
    local timeout=120
    local elapsed=0
    
    print_step "Waiting for port $port to be ready (timeout: ${timeout}s)..."
    
    while [ $elapsed -lt $timeout ]; do
        if lsof -i :$port &>/dev/null; then
            print_success "Port $port is ready"
            return 0
        fi
        sleep 2
        elapsed=$((elapsed + 2))
        printf "."
    done
    
    echo ""
    print_error "Port $port did not become ready within ${timeout}s"
    return 1
}

# ============================================================================
# DOCKER FUNCTIONS
# ============================================================================

setup_postgres() {
    print_step "Setting up PostgreSQL container..."
    
    # Check if container already exists
    if docker ps -a --format '{{.Names}}' | grep -q "^${POSTGRES_CONTAINER}$"; then
        if docker ps --format '{{.Names}}' | grep -q "^${POSTGRES_CONTAINER}$"; then
            print_warning "PostgreSQL container already running, skipping creation"
            return 0
        else
            print_step "Removing stopped PostgreSQL container..."
            docker rm "$POSTGRES_CONTAINER" > /dev/null 2>&1 || true
        fi
    fi
    
    print_step "Creating and starting PostgreSQL container..."
    docker run \
        --name "$POSTGRES_CONTAINER" \
        -p "$DB_PORT:5432" \
        -e "POSTGRES_PASSWORD=$DB_PASSWORD" \
        -d "$POSTGRES_IMAGE" > /dev/null
    
    print_success "PostgreSQL container started"
    sleep 3
}

setup_redis() {
    print_step "Setting up Redis container..."
    
    # Check if container already exists
    if docker ps -a --format '{{.Names}}' | grep -q "^${REDIS_CONTAINER}$"; then
        if docker ps --format '{{.Names}}' | grep -q "^${REDIS_CONTAINER}$"; then
            print_warning "Redis container already running, skipping creation"
            return 0
        else
            print_step "Removing stopped Redis container..."
            docker rm "$REDIS_CONTAINER" > /dev/null 2>&1 || true
        fi
    fi
    
    print_step "Creating and starting Redis container..."
    docker run \
        --name "$REDIS_CONTAINER" \
        -p "$REDIS_PORT:6379" \
        -d "$REDIS_IMAGE" > /dev/null
    
    print_success "Redis container started"
    sleep 2
}

create_database() {
    print_step "Creating database '$DB_NAME'..."
    
    docker exec "$POSTGRES_CONTAINER" psql -U "$DB_USER" -c "CREATE DATABASE \"$DB_NAME\";" 2>&1 || \
    print_warning "Database might already exist, continuing..."
    
    print_success "Database ready"
}

stop_containers() {
    print_step "Stopping Docker containers..."
    
    docker stop "$POSTGRES_CONTAINER" 2>/dev/null || true
    docker stop "$REDIS_CONTAINER" 2>/dev/null || true
    
    print_success "Containers stopped"
}

# ============================================================================
# INFRASTRUCTURE FILE CREATION
# ============================================================================

create_infrastructure_files() {
    print_header "Creating Infrastructure Files"
    
    # Create database directory if needed
    mkdir -p "${PROJECT_ROOT}/database"
    mkdir -p "${PROJECT_ROOT}/gateway"
    
    # Create init-databases.sql
    if [ ! -f "${PROJECT_ROOT}/database/init-databases.sql" ]; then
        print_step "Creating database/init-databases.sql..."
        cat > "${PROJECT_ROOT}/database/init-databases.sql" << 'SQLEOF'
-- Create Platform-V70 database for ABP Next Admin
-- PostgreSQL initialization script

-- Drop existing database if needed (commented out for safety)
-- DROP DATABASE IF EXISTS "Platform-V70";

-- Create the main database
CREATE DATABASE "Platform-V70"
    WITH 
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;

-- Connect to the new database and create extensions
\c "Platform-V70"

-- Create necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

SQLEOF
        print_success "Created database/init-databases.sql"
    else
        print_warning "database/init-databases.sql already exists, skipping"
    fi
    
    # Create init-data.sql
    if [ ! -f "${PROJECT_ROOT}/database/init-data.sql" ]; then
        print_step "Creating database/init-data.sql..."
        cat > "${PROJECT_ROOT}/database/init-data.sql" << 'SQLEOF'
-- Initial data seeding script for Platform-V70
-- Run this after migrations to populate default data

-- Note: Most initial data is seeded by Entity Framework migrations
-- Use this file for any additional seed data needed

-- Example: Add default settings, configurations, or lookup values
-- INSERT INTO your_table VALUES (...);

SQLEOF
        print_success "Created database/init-data.sql"
    else
        print_warning "database/init-data.sql already exists, skipping"
    fi
    
    # Create docker-compose.production.postgresql.yml
    if [ ! -f "${PROJECT_ROOT}/docker-compose.production.postgresql.yml" ]; then
        print_step "Creating docker-compose.production.postgresql.yml..."
        cat > "${PROJECT_ROOT}/docker-compose.production.postgresql.yml" << 'YAMLEOF'
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: postgres-abp-prod
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${DB_PASSWORD:-123456}
      POSTGRES_DB: ${DB_NAME:-Platform-V70}
    ports:
      - "${DB_PORT:-5432}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/init-databases.sql:/docker-entrypoint-initdb.d/01-init-databases.sql:ro
      - ./database/init-data.sql:/docker-entrypoint-initdb.d/02-init-data.sql:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - abp-network
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    container_name: redis-abp-prod
    ports:
      - "${REDIS_PORT:-6379}:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - abp-network
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:

networks:
  abp-network:
    driver: bridge
YAMLEOF
        print_success "Created docker-compose.production.postgresql.yml"
    else
        print_warning "docker-compose.production.postgresql.yml already exists, skipping"
    fi
    
    # Create gateway/nginx.conf
    if [ ! -f "${PROJECT_ROOT}/gateway/nginx.conf" ]; then
        print_step "Creating gateway/nginx.conf..."
        cat > "${PROJECT_ROOT}/gateway/nginx.conf" << 'CONFEOF'
# Nginx configuration for ABP Next Admin API Gateway
# This configuration proxies requests to the backend application

user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
    use epoll;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';

    access_log /var/log/nginx/access.log main;

    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;
    client_max_body_size 20M;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1000;
    gzip_types text/plain text/css text/xml text/javascript 
               application/x-javascript application/xml+rss 
               application/json application/javascript;

    # Upstream backend
    upstream backend {
        server app:30001;
    }

    # Redirect HTTP to HTTPS (optional, uncomment for production)
    # server {
    #     listen 80;
    #     server_name _;
    #     return 301 https://$host$request_uri;
    # }

    # Main API server
    server {
        listen 80;
        server_name _;

        # API endpoints
        location / {
            proxy_pass http://backend;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_connect_timeout 60s;
            proxy_send_timeout 60s;
            proxy_read_timeout 60s;
        }

        # Health check endpoint
        location /healthz {
            proxy_pass http://backend;
            access_log off;
        }
    }
}
CONFEOF
        print_success "Created gateway/nginx.conf"
    else
        print_warning "gateway/nginx.conf already exists, skipping"
    fi
}

# ============================================================================
# BUILD FUNCTIONS
# ============================================================================

build_solution() {
    print_header "Building Solution"
    
    if [ ! -f "$SOLUTION_FILE" ]; then
        print_error "Solution file not found: $SOLUTION_FILE"
        exit 1
    fi
    
    print_step "Building with configuration: $BUILD_CONFIG"
    cd "$ASPNET_CORE_DIR"
    
    if dotnet build "$SOLUTION_FILE" -c "$BUILD_CONFIG" 2>&1 | grep -E "Build (FAILED|succeeded)"; then
        print_success "Solution built successfully"
    else
        print_error "Build failed"
        exit 1
    fi
}

# ============================================================================
# MIGRATION FUNCTIONS
# ============================================================================

run_migrations() {
    print_header "Running Database Migrations"
    
    if [ ! -d "$MIGRATIONS_DIR" ]; then
        print_error "Migrations directory not found: $MIGRATIONS_DIR"
        exit 1
    fi
    
    print_step "Running database migrator..."
    cd "$MIGRATIONS_DIR"
    
    ASPNETCORE_ENVIRONMENT="$APP_DATABASE_PROVIDER" \
    APPLICATION_DATABASE_PROVIDER="$APP_DATABASE_PROVIDER" \
    dotnet run 2>&1 | tail -30
    
    print_success "Migrations completed"
}

# ============================================================================
# APPLICATION FUNCTIONS
# ============================================================================

stop_application() {
    print_step "Stopping any running application instances..."
    pkill -f "dotnet run" 2>/dev/null || true
    sleep 2
}

start_application() {
    print_header "Starting Application"
    
    if [ ! -f "$SERVICES_DIR/LY.MicroService.Applications.Single.csproj" ]; then
        print_error "Project file not found: $SERVICES_DIR/LY.MicroService.Applications.Single.csproj"
        exit 1
    fi
    
    print_step "Starting application on port $APP_PORT..."
    cd "$SERVICES_DIR"
    
    ASPNETCORE_ENVIRONMENT="$APP_ENVIRONMENT" \
    APPLICATION_DATABASE_PROVIDER="$APP_DATABASE_PROVIDER" \
    nohup dotnet run --launch-profile "$APP_LAUNCH_PROFILE" > /tmp/abp-app.log 2>&1 &
    
    local app_pid=$!
    print_success "Application started (PID: $app_pid)"
    print_step "Log file: /tmp/abp-app.log"
}

verify_application() {
    print_header "Verifying Application"
    
    if wait_for_port "$APP_PORT"; then
        print_step "Testing API endpoints..."
        
        if curl -s "http://127.0.0.1:$APP_PORT/swagger/index.html" > /dev/null 2>&1; then
            print_success "Swagger UI is accessible"
            echo -e "\n${GREEN}✓ Application is running successfully!${NC}"
            echo -e "${BLUE}Access the application at:${NC}"
            echo -e "  ${YELLOW}Swagger UI: http://127.0.0.1:$APP_PORT/swagger/index.html${NC}"
            echo -e "  ${YELLOW}API Base: http://127.0.0.1:$APP_PORT${NC}\n"
            return 0
        else
            print_error "Failed to access Swagger UI"
            return 1
        fi
    else
        print_error "Application failed to start"
        echo -e "\n${YELLOW}Check the log file for details:${NC}"
        echo -e "  tail -100 /tmp/abp-app.log\n"
        return 1
    fi
}

# ============================================================================
# CLEANUP FUNCTIONS
# ============================================================================

cleanup_on_exit() {
    if [ $? -ne 0 ]; then
        print_error "Script failed. Check the error messages above."
        echo -e "\n${YELLOW}Useful debug commands:${NC}"
        echo "  docker logs $POSTGRES_CONTAINER"
        echo "  docker logs $REDIS_CONTAINER"
        echo "  tail -100 /tmp/abp-app.log"
        echo ""
    fi
}

trap cleanup_on_exit EXIT

# ============================================================================
# MAIN EXECUTION
# ============================================================================

main() {
    print_header "ABP Next Admin - PostgreSQL Setup & Run"
    
    # Parse command line arguments
    BUILD_ONLY=false
    MIGRATE_ONLY=false
    SKIP_DOCKER=false
    SKIP_BUILD=false
    CLEAN_CONTAINERS=false
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --help)
                show_help
                ;;
            --build-only)
                BUILD_ONLY=true
                shift
                ;;
            --migrate-only)
                MIGRATE_ONLY=true
                shift
                ;;
            --skip-docker)
                SKIP_DOCKER=true
                shift
                ;;
            --skip-build)
                SKIP_BUILD=true
                shift
                ;;
            --clean-containers)
                CLEAN_CONTAINERS=true
                shift
                ;;
            *)
                print_error "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done
    
    # Verify project structure
    if [ ! -d "$ASPNET_CORE_DIR" ]; then
        print_error "ASP.NET Core directory not found: $ASPNET_CORE_DIR"
        print_step "Make sure you're running this script from the repository root"
        exit 1
    fi
    
    # Check prerequisites
    check_prerequisites
    
    # Clean containers if requested
    if [ "$CLEAN_CONTAINERS" = true ]; then
        print_header "Cleaning Containers"
        stop_containers
    fi
    
    # Create infrastructure files if missing
    create_infrastructure_files
    
    # Setup Docker containers
    if [ "$SKIP_DOCKER" = false ]; then
        print_header "Setting Up Docker Containers"
        setup_postgres
        setup_redis
        create_database
    fi
    
    # Build solution
    if [ "$SKIP_BUILD" = false ] && [ "$MIGRATE_ONLY" = false ]; then
        build_solution
    fi
    
    # Run migrations
    if [ "$BUILD_ONLY" = false ]; then
        run_migrations
    fi
    
    # Start application
    if [ "$BUILD_ONLY" = false ] && [ "$MIGRATE_ONLY" = false ]; then
        stop_application
        start_application
        sleep 5
        verify_application
    else
        print_success "Completed requested operations"
    fi
}

# Run main function
main "$@"
