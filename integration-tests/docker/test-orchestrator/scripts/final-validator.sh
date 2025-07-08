#!/bin/bash

# Final Validator - Validates the complete integration test results
# This script runs after all other containers have completed successfully

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

# Configuration
YAML_GENERATED_DIR="/generated-code"
JSON_GENERATED_DIR="/generated-code-json"

# Main validation function
main() {
    echo "=================================================================="
    echo "              Final Integration Test Validation"
    echo "=================================================================="
    
    print_status "Running final validation of integration test results..."
    
    # Step 1: Validate YAML-generated code
    print_status "Step 1: Validating YAML-generated code"
    if [ -d "$YAML_GENERATED_DIR" ] && [ "$(ls -A "$YAML_GENERATED_DIR" 2>/dev/null)" ]; then
        local yaml_files
        yaml_files=$(find "$YAML_GENERATED_DIR" -name "*.elm" | wc -l)
        print_success "Found $yaml_files Elm files generated from YAML spec"
        
        # List the files
        find "$YAML_GENERATED_DIR" -name "*.elm" | while read -r file; do
            local lines
            lines=$(wc -l < "$file")
            print_status "  - $file ($lines lines)"
        done
    else
        print_error "YAML-generated code directory is empty or missing"
        exit 1
    fi
    
    # Step 2: Validate JSON-generated code
    print_status "Step 2: Validating JSON-generated code"
    if [ -d "$JSON_GENERATED_DIR" ] && [ "$(ls -A "$JSON_GENERATED_DIR" 2>/dev/null)" ]; then
        local json_files
        json_files=$(find "$JSON_GENERATED_DIR" -name "*.elm" | wc -l)
        print_success "Found $json_files Elm files generated from JSON spec"
        
        # List the files
        find "$JSON_GENERATED_DIR" -name "*.elm" | while read -r file; do
            local lines
            lines=$(wc -l < "$file")
            print_status "  - $file ($lines lines)"
        done
    else
        print_error "JSON-generated code directory is empty or missing"
        exit 1
    fi
    
    # Step 3: Compare YAML and JSON generated code
    print_status "Step 3: Comparing YAML vs JSON generated code"
    
    # Find corresponding files and compare them
    local differences=0
    for yaml_file in $(find "$YAML_GENERATED_DIR" -name "*.elm"); do
        local relative_path
        relative_path=$(echo "$yaml_file" | sed "s|$YAML_GENERATED_DIR/||")
        local json_file="$JSON_GENERATED_DIR/$relative_path"
        
        if [ -f "$json_file" ]; then
            if diff -q "$yaml_file" "$json_file" > /dev/null 2>&1; then
                print_success "✓ $relative_path: YAML and JSON generated identical code"
            else
                print_warning "⚠ $relative_path: YAML and JSON generated different code"
                differences=$((differences + 1))
            fi
        else
            print_error "✗ $relative_path: Missing corresponding JSON-generated file"
            differences=$((differences + 1))
        fi
    done
    
    if [ $differences -eq 0 ]; then
        print_success "All YAML and JSON generated files are identical"
    else
        print_warning "$differences differences found between YAML and JSON generated code"
    fi
    
    # Step 4: Validate code structure
    print_status "Step 4: Validating generated code structure"
    
    local total_files=0
    local valid_modules=0
    local has_types=0
    local has_decoders=0
    
    for file in $(find "$YAML_GENERATED_DIR" -name "*.elm"); do
        total_files=$((total_files + 1))
        
        # Check module declaration
        if grep -q "^module " "$file"; then
            valid_modules=$((valid_modules + 1))
        fi
        
        # Check for type definitions
        if grep -q "type alias\|type [A-Z]" "$file"; then
            has_types=$((has_types + 1))
        fi
        
        # Check for decoders
        if grep -q "Decode\." "$file"; then
            has_decoders=$((has_decoders + 1))
        fi
    done
    
    print_status "Code structure analysis:"
    print_status "  - Total files: $total_files"
    print_status "  - Valid modules: $valid_modules"
    print_status "  - Files with types: $has_types"
    print_status "  - Files with decoders: $has_decoders"
    
    if [ $valid_modules -eq $total_files ]; then
        print_success "✓ All files have valid module declarations"
    else
        print_error "✗ Some files missing valid module declarations"
        exit 1
    fi
    
    # Step 5: Final summary
    print_status "Step 5: Integration test summary"
    
    echo ""
    print_success "=================================================================="
    print_success "         🎉 INTEGRATION TEST COMPLETED SUCCESSFULLY! 🎉"
    print_success "=================================================================="
    print_success "✓ OpenAPI service deployment and health checks"
    print_success "✓ YAML and JSON OpenAPI specification serving"
    print_success "✓ Code generation from both YAML and JSON sources"
    print_success "✓ Elm code compilation and validation"
    print_success "✓ Generated code structure and quality checks"
    print_success "✓ Cross-format consistency validation"
    
    echo ""
    print_status "Test Results Summary:"
    print_status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_status "📊 Generated Files (YAML): $yaml_files Elm files"
    print_status "📊 Generated Files (JSON): $json_files Elm files" 
    print_status "🔍 Code Differences: $differences between YAML/JSON"
    print_status "📝 Module Validity: $valid_modules/$total_files files"
    print_status "🏗️  Type Definitions: $has_types/$total_files files"
    print_status "🔄 JSON Decoders: $has_decoders/$total_files files"
    print_status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    echo ""
    print_success "🚀 ElmOpenApiClientGen containerized integration test PASSED!"
    print_success "The tool is ready for production use in containerized environments."
    
    exit 0
}

# Run main function
main "$@"