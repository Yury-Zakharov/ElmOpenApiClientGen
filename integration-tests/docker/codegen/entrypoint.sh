#!/bin/sh

# ElmOpenApiClientGen container entrypoint
# This script downloads OpenAPI specs and generates Elm code

set -e

echo "=== ElmOpenApiClientGen Container ==="
echo "Starting code generation process..."

# Configuration
OPENAPI_SERVICE_URL="${OPENAPI_SERVICE_URL:-http://openapi-service:5000}"
OUTPUT_DIR="${OUTPUT_DIR:-/output}"
MODULE_PREFIX="${MODULE_PREFIX:-Api}"
SPEC_FORMAT="${SPEC_FORMAT:-yaml}"
INPUT_MODE="${INPUT_MODE:-file}"  # 'file' or 'url'

# Wait for OpenAPI service to be ready
echo "Waiting for OpenAPI service at $OPENAPI_SERVICE_URL..."
max_attempts=30
attempt=1

while [ $attempt -le $max_attempts ]; do
    if curl -s "$OPENAPI_SERVICE_URL/health" > /dev/null 2>&1; then
        echo "✓ OpenAPI service is ready"
        break
    fi
    
    if [ $attempt -eq $max_attempts ]; then
        echo "✗ OpenAPI service failed to become ready after $max_attempts attempts"
        exit 1
    fi
    
    echo "  Attempt $attempt/$max_attempts - waiting..."
    sleep 2
    attempt=$((attempt + 1))
done

# Determine input source based on INPUT_MODE
if [ "$INPUT_MODE" = "url" ]; then
    # Use URL input directly (NEW FEATURE)
    INPUT_SOURCE="$OPENAPI_SERVICE_URL/openapi.$SPEC_FORMAT"
    echo "Using URL input: $INPUT_SOURCE"
else
    # Download OpenAPI specification to file (existing behavior)
    echo "Downloading OpenAPI specification ($SPEC_FORMAT format)..."
    TEMP_SPEC="/tmp/openapi-spec.$SPEC_FORMAT"

    if curl -s "$OPENAPI_SERVICE_URL/openapi.$SPEC_FORMAT" -o "$TEMP_SPEC"; then
        echo "✓ OpenAPI spec downloaded successfully"
        
        # Verify the downloaded file
        if [ -s "$TEMP_SPEC" ]; then
            echo "  File size: $(wc -c < "$TEMP_SPEC") bytes"
        else
            echo "✗ Downloaded file is empty"
            exit 1
        fi
    else
        echo "✗ Failed to download OpenAPI spec"
        exit 1
    fi
    
    INPUT_SOURCE="$TEMP_SPEC"
fi

# Ensure output directory exists
mkdir -p "$OUTPUT_DIR"

# Run ElmOpenApiClientGen
echo "Running ElmOpenApiClientGen..."
echo "  Input mode: $INPUT_MODE"
echo "  Input source: $INPUT_SOURCE"
echo "  Output: $OUTPUT_DIR"
echo "  Module prefix: $MODULE_PREFIX"

if dotnet run -- \
    --input "$INPUT_SOURCE" \
    --output "$OUTPUT_DIR" \
    --moduleprefix "$MODULE_PREFIX" \
    --force; then
    echo "✓ Code generation completed successfully"
    
    # List generated files
    echo "Generated files:"
    find "$OUTPUT_DIR" -name "*.elm" -type f | while read -r file; do
        echo "  - $file ($(wc -l < "$file") lines)"
    done
    
else
    echo "✗ Code generation failed"
    exit 1
fi

echo "=== Code generation process completed ==="