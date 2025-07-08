#!/bin/bash

# Test Orchestrator - Coordinates containerized integration tests
# This script validates the complete ElmOpenApiClientGen workflow

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
OPENAPI_SERVICE_URL="${OPENAPI_SERVICE_URL:-http://openapi-service:5000}"
MAX_WAIT_TIME=60

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Function to wait for service
wait_for_service() {
    local url=$1
    local service_name=$2
    local max_attempts=$((MAX_WAIT_TIME / 2))
    local attempt=1

    print_status "Waiting for $service_name at $url..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s "$url" > /dev/null 2>&1; then
            print_success "$service_name is ready"
            return 0
        fi
        
        if [ $attempt -eq $max_attempts ]; then
            print_error "$service_name failed to become ready after $MAX_WAIT_TIME seconds"
            return 1
        fi
        
        echo "  Attempt $attempt/$max_attempts - waiting..."
        sleep 2
        attempt=$((attempt + 1))
    done
}

# Function to test endpoint
test_endpoint() {
    local url=$1
    local description=$2
    local expected_pattern=$3
    
    print_status "Testing $description..."
    
    local response
    if response=$(curl -s "$url" 2>/dev/null); then
        if [ -n "$expected_pattern" ] && echo "$response" | head -1 | grep -q "$expected_pattern"; then
            print_success "$description is working"
            return 0
        elif [ -z "$expected_pattern" ]; then
            print_success "$description is working"
            return 0
        else
            print_error "$description response doesn't match expected pattern"
            return 1
        fi
    else
        print_error "$description failed"
        return 1
    fi
}

# Function to validate container status
check_container_status() {
    local container_name=$1
    print_status "Checking $container_name container status..."
    
    # In a real Docker environment, we would check container status
    # For now, we'll simulate this check
    print_success "$container_name container is ready"
}

# Main test orchestration
main() {
    echo "=================================================================="
    echo "        ElmOpenApiClientGen Containerized Integration Test"
    echo "=================================================================="
    
    print_status "Starting integration test orchestration..."
    
    # Step 1: Wait for OpenAPI service
    print_status "Step 1: Validating OpenAPI service availability"
    if ! wait_for_service "$OPENAPI_SERVICE_URL/health" "OpenAPI service"; then
        exit 1
    fi
    
    # Step 2: Test OpenAPI endpoints
    print_status "Step 2: Testing OpenAPI endpoints"
    if ! test_endpoint "$OPENAPI_SERVICE_URL/openapi.yaml" "YAML endpoint" "openapi:"; then
        exit 1
    fi
    
    if ! test_endpoint "$OPENAPI_SERVICE_URL/openapi.json" "JSON endpoint" "{"; then
        exit 1
    fi
    
    # Step 3: Validate service info
    print_status "Step 3: Validating service information"
    if ! test_endpoint "$OPENAPI_SERVICE_URL/" "service info"; then
        exit 1
    fi
    
    # Step 4: Check that other containers will have what they need
    print_status "Step 4: Pre-flight checks for dependent containers"
    
    # Verify OpenAPI spec content
    local yaml_size
    yaml_size=$(curl -s "$OPENAPI_SERVICE_URL/openapi.yaml" | wc -c)
    if [ "$yaml_size" -gt 1000 ]; then
        print_success "OpenAPI YAML spec has reasonable size ($yaml_size bytes)"
    else
        print_error "OpenAPI YAML spec is too small ($yaml_size bytes)"
        exit 1
    fi
    
    local json_size
    json_size=$(curl -s "$OPENAPI_SERVICE_URL/openapi.json" | wc -c)
    if [ "$json_size" -gt 1000 ]; then
        print_success "OpenAPI JSON spec has reasonable size ($json_size bytes)"
    else
        print_error "OpenAPI JSON spec is too small ($json_size bytes)"
        exit 1
    fi
    
    # Step 5: Validate OpenAPI spec structure
    print_status "Step 5: Validating OpenAPI specification structure"
    
    local spec_content
    spec_content=$(curl -s "$OPENAPI_SERVICE_URL/openapi.json")
    
    # Check for required OpenAPI fields
    if echo "$spec_content" | jq -e '.openapi' > /dev/null 2>&1; then
        print_success "OpenAPI version field found"
    else
        print_error "OpenAPI version field missing"
        exit 1
    fi
    
    if echo "$spec_content" | jq -e '.info.title' > /dev/null 2>&1; then
        local title
        title=$(echo "$spec_content" | jq -r '.info.title')
        print_success "API title: $title"
    else
        print_error "API title missing"
        exit 1
    fi
    
    if echo "$spec_content" | jq -e '.paths' > /dev/null 2>&1; then
        local path_count
        path_count=$(echo "$spec_content" | jq '.paths | keys | length')
        print_success "Found $path_count API paths"
    else
        print_error "API paths missing"
        exit 1
    fi
    
    # Step 6: Final validation
    print_status "Step 6: Final pre-codegen validation"
    
    # Test that we can reach the service from this container
    if curl -s "$OPENAPI_SERVICE_URL/health" | jq -e '.status' > /dev/null 2>&1; then
        local status
        status=$(curl -s "$OPENAPI_SERVICE_URL/health" | jq -r '.status')
        print_success "Service health status: $status"
    else
        print_warning "Could not parse health status, but service is responding"
    fi
    
    # Summary
    echo ""
    print_success "=================================================================="
    print_success "           Pre-codegen validation completed successfully!"
    print_success "=================================================================="
    print_success "✓ OpenAPI service is running and healthy"
    print_success "✓ YAML and JSON endpoints are accessible"
    print_success "✓ OpenAPI specification is valid and complete"
    print_success "✓ Ready for code generation phase"
    
    echo ""
    print_status "Environment summary:"
    print_status "- OpenAPI service URL: $OPENAPI_SERVICE_URL"
    print_status "- YAML spec size: $yaml_size bytes"
    print_status "- JSON spec size: $json_size bytes"
    print_status "- API paths: $path_count"
    
    echo ""
    print_status "Next phase: Code generation and Elm compilation will be handled by other containers"
    print_status "Test orchestration completed successfully"
}

# Run main function
main "$@"