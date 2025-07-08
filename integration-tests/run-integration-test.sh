#!/bin/bash

# ElmOpenApiClientGen Integration Test Runner
# This script tests the complete workflow:
# 1. Start OpenAPI service
# 2. Run ElmOpenApiClientGen to consume specs over HTTP
# 3. Build Elm app with generated code
# 4. Verify everything works

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
SERVICE_PORT=5000
SERVICE_URL="http://localhost:$SERVICE_PORT"
TIMEOUT=30
PROJECT_ROOT="/var/home/yury/projects/ElmOpenApiClientGen"
INTEGRATION_DIR="$PROJECT_ROOT/integration-tests"
SERVICE_DIR="$INTEGRATION_DIR/OpenApiTestService"
ELM_APP_DIR="$INTEGRATION_DIR/elm-test-app"
GENERATED_DIR="$ELM_APP_DIR/src/Generated"
CODEGEN_TOOL="$PROJECT_ROOT/src/ElmOpenApiClientGen/bin/Debug/net10.0/ElmOpenApiClientGen.dll"

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

# Function to cleanup processes
cleanup() {
    print_status "Cleaning up..."
    
    # Kill the OpenAPI service if it's running
    if [ ! -z "$SERVICE_PID" ]; then
        print_status "Stopping OpenAPI service (PID: $SERVICE_PID)"
        kill $SERVICE_PID 2>/dev/null || true
        wait $SERVICE_PID 2>/dev/null || true
    fi
    
    # Clean up generated files
    if [ -d "$GENERATED_DIR" ]; then
        print_status "Removing generated files"
        rm -rf "$GENERATED_DIR"
    fi
    
    # Clean up elm-stuff
    if [ -d "$ELM_APP_DIR/elm-stuff" ]; then
        rm -rf "$ELM_APP_DIR/elm-stuff"
    fi
}

# Set up cleanup trap
trap cleanup EXIT

print_status "Starting ElmOpenApiClientGen Integration Test"
print_status "========================================"

# Step 1: Build the ElmOpenApiClientGen tool
print_status "Step 1: Building ElmOpenApiClientGen tool"
cd "$PROJECT_ROOT"
dotnet build --configuration Debug

if [ ! -f "$CODEGEN_TOOL" ]; then
    print_error "ElmOpenApiClientGen tool not found at $CODEGEN_TOOL"
    exit 1
fi

print_success "ElmOpenApiClientGen tool built successfully"

# Step 2: Start the OpenAPI service
print_status "Step 2: Starting OpenAPI test service"
cd "$SERVICE_DIR"

# Restore packages
dotnet restore

# Start service in background
dotnet run --urls "http://localhost:$SERVICE_PORT" &
SERVICE_PID=$!

print_status "OpenAPI service started (PID: $SERVICE_PID)"
print_status "Waiting for service to be ready..."

# Wait for service to be ready
for i in $(seq 1 $TIMEOUT); do
    if curl -s "$SERVICE_URL/health" > /dev/null 2>&1; then
        print_success "OpenAPI service is ready"
        break
    fi
    
    if [ $i -eq $TIMEOUT ]; then
        print_error "OpenAPI service failed to start within $TIMEOUT seconds"
        exit 1
    fi
    
    sleep 1
done

# Step 3: Test that OpenAPI endpoints are accessible
print_status "Step 3: Verifying OpenAPI endpoints"

# Test YAML endpoint
if curl -s "$SERVICE_URL/openapi.yaml" | head -1 | grep -q "openapi:"; then
    print_success "OpenAPI YAML endpoint is working"
else
    print_error "OpenAPI YAML endpoint is not working"
    exit 1
fi

# Test JSON endpoint
if curl -s "$SERVICE_URL/openapi.json" | head -1 | grep -q "{"; then
    print_success "OpenAPI JSON endpoint is working"
else
    print_error "OpenAPI JSON endpoint is not working"
    exit 1
fi

# Step 4: Download OpenAPI specs from HTTP endpoints
print_status "Step 4: Downloading OpenAPI specs from HTTP endpoints"

# Create temporary directory for downloaded specs
TEMP_DIR="$INTEGRATION_DIR/temp"
mkdir -p "$TEMP_DIR"

# Download YAML spec
print_status "Downloading YAML spec..."
if curl -s "$SERVICE_URL/openapi.yaml" -o "$TEMP_DIR/downloaded-spec.yaml"; then
    print_success "YAML spec downloaded successfully"
else
    print_error "Failed to download YAML spec"
    exit 1
fi

# Download JSON spec
print_status "Downloading JSON spec..."
if curl -s "$SERVICE_URL/openapi.json" -o "$TEMP_DIR/downloaded-spec.json"; then
    print_success "JSON spec downloaded successfully"
else
    print_error "Failed to download JSON spec"
    exit 1
fi

# Step 5: Run ElmOpenApiClientGen with downloaded specs
print_status "Step 5: Running ElmOpenApiClientGen with downloaded YAML spec"

# Create output directory
mkdir -p "$GENERATED_DIR"

# Test with downloaded YAML spec
cd "$PROJECT_ROOT/src/ElmOpenApiClientGen"
dotnet run \
    --input "$TEMP_DIR/downloaded-spec.yaml" \
    --output "$GENERATED_DIR" \
    --moduleprefix "Api" \
    --force

if [ $? -eq 0 ]; then
    print_success "Code generation from downloaded YAML spec successful"
else
    print_error "Code generation from downloaded YAML spec failed"
    exit 1
fi

# Verify generated files exist
if [ -f "$GENERATED_DIR/Api/Schemas.elm" ]; then
    print_success "Generated Elm files found"
else
    print_error "Generated Elm files not found"
    exit 1
fi

# Step 6: Test with downloaded JSON spec (overwrite previous generation)
print_status "Step 6: Testing with downloaded JSON spec..."
rm -rf "$GENERATED_DIR"
mkdir -p "$GENERATED_DIR"

cd "$PROJECT_ROOT/src/ElmOpenApiClientGen"
dotnet run \
    --input "$TEMP_DIR/downloaded-spec.json" \
    --output "$GENERATED_DIR" \
    --moduleprefix "Api" \
    --force

if [ $? -eq 0 ]; then
    print_success "Code generation from downloaded JSON spec successful"
else
    print_error "Code generation from downloaded JSON spec failed"
    exit 1
fi

# Step 6.5: Test URL input directly (NEW FEATURE)
print_status "Step 6.5: Testing URL input directly with YAML endpoint..."
rm -rf "$GENERATED_DIR"
mkdir -p "$GENERATED_DIR"

cd "$PROJECT_ROOT/src/ElmOpenApiClientGen"
dotnet run \
    --input "$SERVICE_URL/openapi.yaml" \
    --output "$GENERATED_DIR" \
    --moduleprefix "Api" \
    --force

if [ $? -eq 0 ]; then
    print_success "Code generation from YAML URL successful"
else
    print_error "Code generation from YAML URL failed"
    exit 1
fi

# Verify generated files exist
if [ -f "$GENERATED_DIR/Api/Schemas.elm" ]; then
    print_success "Generated Elm files from YAML URL found"
else
    print_error "Generated Elm files from YAML URL not found"
    exit 1
fi

# Step 6.6: Test URL input directly with JSON endpoint
print_status "Step 6.6: Testing URL input directly with JSON endpoint..."
rm -rf "$GENERATED_DIR"
mkdir -p "$GENERATED_DIR"

cd "$PROJECT_ROOT/src/ElmOpenApiClientGen"
dotnet run \
    --input "$SERVICE_URL/openapi.json" \
    --output "$GENERATED_DIR" \
    --moduleprefix "Api" \
    --force

if [ $? -eq 0 ]; then
    print_success "Code generation from JSON URL successful"
else
    print_error "Code generation from JSON URL failed"
    exit 1
fi

# Verify generated files exist
if [ -f "$GENERATED_DIR/Api/Schemas.elm" ]; then
    print_success "Generated Elm files from JSON URL found"
else
    print_error "Generated Elm files from JSON URL not found"
    exit 1
fi

# Clean up temp directory
rm -rf "$TEMP_DIR"

# Step 7: Verify generated code quality
print_status "Step 7: Verifying generated code quality"

# Check if generated files contain expected content
if grep -q "module Api.Schemas" "$GENERATED_DIR/Api/Schemas.elm"; then
    print_success "Generated module has correct module declaration"
else
    print_error "Generated module has incorrect module declaration"
    exit 1
fi

# Check for type definitions
if grep -q "type alias" "$GENERATED_DIR/Api/Schemas.elm"; then
    print_success "Generated code contains type aliases"
else
    print_warning "Generated code does not contain type aliases"
fi

# Check for decoders
if grep -q "Decode\." "$GENERATED_DIR/Api/Schemas.elm"; then
    print_success "Generated code contains JSON decoders"
else
    print_warning "Generated code does not contain JSON decoders"
fi

# Step 8: Update Elm app to use generated code
print_status "Step 8: Updating Elm app to use generated code"

# Update elm.json to include generated source directory
cd "$ELM_APP_DIR"
# Create backup
cp elm.json elm.json.backup

# Update elm.json to include generated source
python3 -c "
import json
with open('elm.json', 'r') as f:
    data = json.load(f)
if 'src/Generated' not in data['source-directories']:
    data['source-directories'].append('src/Generated')
with open('elm.json', 'w') as f:
    json.dump(data, f, indent=4)
"

print_success "Updated elm.json to include generated source directory"

# Step 9: Build Elm application
print_status "Step 9: Building Elm application"

# Try to build the Elm app
if elm make src/Main.elm --output=main.js; then
    print_success "Elm application built successfully"
else
    print_error "Elm application build failed"
    
    # Restore elm.json backup
    mv elm.json.backup elm.json
    exit 1
fi

# Step 10: Verify generated code can be imported (compile check)
print_status "Step 10: Verifying generated code import"

# Create a temporary test file that imports generated modules
cat > test-import.elm << 'EOF'
module TestImport exposing (main)

import Api.Schemas
import Html exposing (Html, text)

main : Html msg
main = 
    text "Import test successful"
EOF

if elm make test-import.elm --output=test-import.js; then
    print_success "Generated code can be imported successfully"
    rm -f test-import.elm test-import.js
else
    print_error "Generated code import failed"
    rm -f test-import.elm test-import.js
    exit 1
fi

# Step 11: Run comprehensive validation
print_status "Step 11: Running comprehensive validation"

# Count generated files
GENERATED_FILES=$(find "$GENERATED_DIR" -name "*.elm" | wc -l)
print_status "Generated $GENERATED_FILES Elm files"

# Check file sizes (should not be empty)
for file in $(find "$GENERATED_DIR" -name "*.elm"); do
    if [ -s "$file" ]; then
        print_success "✓ $file ($(wc -l < "$file") lines)"
    else
        print_error "✗ $file is empty"
        exit 1
    fi
done

# Restore elm.json backup
mv elm.json.backup elm.json

print_success "========================================"
print_success "Integration test completed successfully!"
print_success "========================================"
print_success "✓ OpenAPI service started and served specs"
print_success "✓ OpenAPI specs downloaded from HTTP endpoints"
print_success "✓ Code generation worked for both YAML and JSON (file input)"
print_success "✓ Code generation worked for both YAML and JSON (URL input)"
print_success "✓ Generated Elm code compiled successfully"
print_success "✓ Elm application built without errors"
print_success "✓ Generated code can be imported and used"

# Print summary
echo ""
print_status "Summary:"
print_status "- Service URL: $SERVICE_URL"
print_status "- Generated files: $GENERATED_FILES"
print_status "- Output directory: $GENERATED_DIR"
print_status "- Elm app directory: $ELM_APP_DIR"

exit 0