#!/bin/sh

# Elm Build container entrypoint
# This script validates that generated Elm code compiles correctly

set -e

echo "=== Elm Build Container ==="
echo "Starting Elm compilation validation..."

# Configuration
GENERATED_CODE_DIR="${GENERATED_CODE_DIR:-/generated-code}"
OUTPUT_FILE="${OUTPUT_FILE:-main.js}"

# Wait for generated code to be available
echo "Waiting for generated code in $GENERATED_CODE_DIR..."
max_attempts=30
attempt=1

while [ $attempt -le $max_attempts ]; do
    if [ -d "$GENERATED_CODE_DIR" ] && [ "$(ls -A "$GENERATED_CODE_DIR" 2>/dev/null)" ]; then
        echo "✓ Generated code directory found"
        break
    fi
    
    if [ $attempt -eq $max_attempts ]; then
        echo "✗ Generated code not found after $max_attempts attempts"
        exit 1
    fi
    
    echo "  Attempt $attempt/$max_attempts - waiting for generated code..."
    sleep 2
    attempt=$((attempt + 1))
done

# List generated files
echo "Generated files found:"
find "$GENERATED_CODE_DIR" -name "*.elm" -type f | while read -r file; do
    echo "  - $file ($(wc -l < "$file") lines)"
done

# Update elm.json to include generated source directory
echo "Updating elm.json to include generated code..."
if [ -f elm.json ]; then
    # Create backup
    cp elm.json elm.json.backup
    
    # Update source directories using a simple approach
    # Add the generated code directory if it's not already there
    if ! grep -q "/generated-code" elm.json; then
        # Use sed to add the generated code directory to source-directories
        sed -i 's|\["src"\]|["src", "/generated-code"]|g' elm.json || \
        sed -i 's|"src"|"src",\n        "/generated-code"|g' elm.json
    fi
    
    echo "✓ elm.json updated"
else
    echo "✗ elm.json not found"
    exit 1
fi

# Validate Elm project structure
echo "Validating Elm project..."
if elm --version > /dev/null 2>&1; then
    echo "✓ Elm $(elm --version) is available"
else
    echo "✗ Elm is not available"
    exit 1
fi

# Test 1: Try to compile the main application
echo "Test 1: Compiling main Elm application..."
if elm make src/Main.elm --output="$OUTPUT_FILE" --debug; then
    echo "✓ Main application compiled successfully"
    echo "  Output file: $OUTPUT_FILE ($(wc -c < "$OUTPUT_FILE") bytes)"
else
    echo "✗ Main application compilation failed"
    
    # Restore backup
    if [ -f elm.json.backup ]; then
        mv elm.json.backup elm.json
    fi
    exit 1
fi

# Test 2: Try to compile a test module that imports generated code
echo "Test 2: Testing generated code import..."
cat > test-import.elm << 'EOF'
module TestImport exposing (main)

import Api.Schemas
import Html exposing (Html, text)

main : Html msg
main = 
    text "Generated code import test successful"
EOF

if elm make test-import.elm --output=test-import.js --debug; then
    echo "✓ Generated code can be imported successfully"
    rm -f test-import.elm test-import.js
else
    echo "✗ Generated code import failed"
    rm -f test-import.elm test-import.js
    
    # Restore backup
    if [ -f elm.json.backup ]; then
        mv elm.json.backup elm.json
    fi
    exit 1
fi

# Test 3: Validate generated code structure
echo "Test 3: Validating generated code structure..."
generated_files=$(find "$GENERATED_CODE_DIR" -name "*.elm" | wc -l)
if [ "$generated_files" -gt 0 ]; then
    echo "✓ Found $generated_files generated Elm files"
    
    # Check for expected patterns in generated code
    for file in $(find "$GENERATED_CODE_DIR" -name "*.elm"); do
        if grep -q "module Api\." "$file"; then
            echo "  ✓ $file has correct module declaration"
        else
            echo "  ✗ $file missing proper module declaration"
        fi
        
        if grep -q "type alias\|type " "$file"; then
            echo "  ✓ $file contains type definitions"
        else
            echo "  ⚠ $file does not contain type definitions"
        fi
    done
else
    echo "✗ No generated Elm files found"
    exit 1
fi

# Restore elm.json backup
if [ -f elm.json.backup ]; then
    mv elm.json.backup elm.json
    echo "✓ elm.json restored"
fi

echo "✓ All Elm compilation tests passed successfully"
echo "=== Elm build validation completed ==="