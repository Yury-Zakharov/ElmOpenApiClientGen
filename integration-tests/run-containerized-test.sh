#!/bin/bash

# ElmOpenApiClientGen Containerized Integration Test Runner
# This script orchestrates the complete containerized integration test workflow

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="docker-compose.yml"
PROJECT_NAME="elm-codegen-integration-test"

# Function to print colored output
print_header() {
    echo -e "${PURPLE}$1${NC}"
}

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

print_step() {
    echo -e "${CYAN}[STEP]${NC} $1"
}

# Function to cleanup containers and volumes
cleanup() {
    print_status "Cleaning up containers and volumes..."
    
    # Stop and remove containers
    docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" down --volumes --remove-orphans 2>/dev/null || true
    
    # Remove dangling images (optional)
    if [ "${CLEANUP_IMAGES:-false}" = "true" ]; then
        print_status "Removing dangling images..."
        docker image prune -f 2>/dev/null || true
    fi
    
    print_success "Cleanup completed"
}

# Function to check Docker availability
check_docker() {
    print_step "Checking Docker availability..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed or not in PATH"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running or not accessible"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose is not installed or not in PATH"
        exit 1
    fi
    
    print_success "Docker and Docker Compose are available"
}

# Function to build containers
build_containers() {
    print_step "Building containers..."
    
    print_status "Building all containers (this may take a few minutes)..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" build --parallel; then
        print_success "All containers built successfully"
    else
        print_error "Container build failed"
        exit 1
    fi
}

# Function to run the integration test
run_integration_test() {
    print_step "Running containerized integration test..."
    
    # Start services in dependency order
    print_status "Starting OpenAPI service..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" up -d openapi-service; then
        print_success "OpenAPI service started"
    else
        print_error "Failed to start OpenAPI service"
        return 1
    fi
    
    # Wait for service to be healthy
    print_status "Waiting for OpenAPI service to be healthy..."
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" ps | grep -q "healthy"; then
            print_success "OpenAPI service is healthy"
            break
        fi
        
        if [ $attempt -eq $max_attempts ]; then
            print_error "OpenAPI service failed to become healthy"
            return 1
        fi
        
        echo "  Attempt $attempt/$max_attempts - waiting for health check..."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    # Run test orchestrator first
    print_status "Running test orchestrator..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm test-orchestrator; then
        print_success "Test orchestrator completed successfully"
    else
        print_error "Test orchestrator failed"
        return 1
    fi
    
    # Run code generation (YAML)
    print_status "Running code generation from YAML..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm codegen; then
        print_success "YAML code generation completed successfully"
    else
        print_error "YAML code generation failed"
        return 1
    fi
    
    # Run code generation (JSON)
    print_status "Running code generation from JSON..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm codegen-json; then
        print_success "JSON code generation completed successfully"
    else
        print_error "JSON code generation failed"
        return 1
    fi
    
    # Run code generation from URL (YAML) - NEW FEATURE
    print_status "Running code generation from YAML URL..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm codegen-url-yaml; then
        print_success "YAML URL code generation completed successfully"
    else
        print_error "YAML URL code generation failed"
        return 1
    fi
    
    # Run code generation from URL (JSON) - NEW FEATURE
    print_status "Running code generation from JSON URL..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm codegen-url-json; then
        print_success "JSON URL code generation completed successfully"
    else
        print_error "JSON URL code generation failed"
        return 1
    fi
    
    # Run Elm compilation tests
    print_status "Running Elm compilation validation (YAML-generated code)..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm elm-build; then
        print_success "YAML-generated Elm code compilation successful"
    else
        print_error "YAML-generated Elm code compilation failed"
        return 1
    fi
    
    print_status "Running Elm compilation validation (JSON-generated code)..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm elm-build-json; then
        print_success "JSON-generated Elm code compilation successful"
    else
        print_error "JSON-generated Elm code compilation failed"
        return 1
    fi
    
    # Run final validation
    print_status "Running final validation..."
    if docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" run --rm final-validator; then
        print_success "Final validation completed successfully"
    else
        print_error "Final validation failed"
        return 1
    fi
    
    return 0
}

# Function to show test results
show_results() {
    print_step "Collecting test results..."
    
    # Show container status
    print_status "Final container status:"
    docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" ps
    
    # Show any logs if there were issues
    if [ "${SHOW_LOGS:-false}" = "true" ]; then
        print_status "Container logs:"
        docker-compose -f "$COMPOSE_FILE" -p "$PROJECT_NAME" logs
    fi
}

# Function to display usage
show_usage() {
    cat << EOF
ElmOpenApiClientGen Containerized Integration Test Runner

Usage: $0 [OPTIONS]

Options:
    -h, --help          Show this help message
    -c, --cleanup       Clean up containers and volumes before running
    -l, --logs          Show container logs after test completion
    -i, --cleanup-images Remove dangling images after cleanup
    --build-only        Only build containers, don't run tests
    --no-cleanup        Don't clean up after test completion

Examples:
    $0                  Run the full integration test
    $0 --cleanup        Clean up first, then run tests
    $0 --logs           Run tests and show logs
    $0 --build-only     Only build the containers

Environment Variables:
    CLEANUP_IMAGES      Set to 'true' to remove dangling images during cleanup
    SHOW_LOGS          Set to 'true' to show container logs
EOF
}

# Main function
main() {
    local cleanup_first=false
    local build_only=false
    local no_cleanup=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_usage
                exit 0
                ;;
            -c|--cleanup)
                cleanup_first=true
                shift
                ;;
            -l|--logs)
                export SHOW_LOGS=true
                shift
                ;;
            -i|--cleanup-images)
                export CLEANUP_IMAGES=true
                shift
                ;;
            --build-only)
                build_only=true
                shift
                ;;
            --no-cleanup)
                no_cleanup=true
                shift
                ;;
            *)
                print_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Set up cleanup trap (unless disabled)
    if [ "$no_cleanup" != "true" ]; then
        trap cleanup EXIT
    fi
    
    # Display header
    print_header "=================================================================="
    print_header "     ElmOpenApiClientGen Containerized Integration Test"
    print_header "=================================================================="
    
    # Check prerequisites
    check_docker
    
    # Optional initial cleanup
    if [ "$cleanup_first" = "true" ]; then
        cleanup
    fi
    
    # Build containers
    build_containers
    
    # If build-only mode, exit here
    if [ "$build_only" = "true" ]; then
        print_success "Build completed. Use '$0' to run the tests."
        exit 0
    fi
    
    # Run the integration test
    if run_integration_test; then
        show_results
        
        print_header ""
        print_header "=================================================================="
        print_success "🎉 CONTAINERIZED INTEGRATION TEST COMPLETED SUCCESSFULLY! 🎉"
        print_header "=================================================================="
        print_success "✓ All containers built and ran successfully"
        print_success "✓ OpenAPI service deployment and health checks passed"
        print_success "✓ Code generation from YAML and JSON sources completed (file input)"
        print_success "✓ Code generation from YAML and JSON URLs completed (URL input)"
        print_success "✓ Generated Elm code compiled successfully"
        print_success "✓ Cross-format consistency validation passed"
        print_success "✓ Complete workflow validation successful"
        print_header ""
        print_success "🚀 ElmOpenApiClientGen is ready for containerized deployment!"
        
    else
        show_results
        
        print_header ""
        print_error "=================================================================="
        print_error "❌ CONTAINERIZED INTEGRATION TEST FAILED"
        print_error "=================================================================="
        print_error "One or more test phases failed. Check the logs above for details."
        print_error "Use '$0 --logs' to see detailed container logs."
        
        exit 1
    fi
}

# Check if running in CI environment
if [ "${CI:-false}" = "true" ]; then
    print_status "Running in CI environment"
    export CLEANUP_IMAGES=true
    export SHOW_LOGS=true
fi

# Run main function with all arguments
main "$@"