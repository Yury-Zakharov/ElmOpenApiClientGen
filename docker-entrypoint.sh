#!/bin/sh

# ElmOpenApiClientGen Docker Entrypoint
# 
# This script provides a user-friendly interface for running ElmOpenApiClientGen
# in a Docker container with support for both local files and remote URLs.

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Print colored output
print_header() {
    echo -e "${PURPLE}$1${NC}"
}

print_info() {
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

# Display banner
print_header "=================================================================="
print_header "                ElmOpenApiClientGen Container                      "
print_header "    Generate type-safe Elm HTTP clients from OpenAPI specs        "
print_header "=================================================================="

# Function to show usage
show_usage() {
    cat << EOF

Usage: docker run [docker-options] elm-openapi-gen [OPTIONS]

ElmOpenApiClientGen Options:
  --input <path>          Path to OpenAPI spec file (JSON or YAML) or URL to download spec
  --output <directory>    Output directory for generated Elm files (default: /output)
  --moduleprefix <name>   Module name prefix (default: Api)
  --force                 Overwrite existing files
  --help                  Show this help message

Docker Usage Examples:

1. Generate from remote URL:
   docker run --rm -v \$(pwd)/output:/output elm-openapi-gen \\
     --input https://petstore.swagger.io/v2/swagger.json \\
     --moduleprefix PetStore

2. Generate from local file:
   docker run --rm -v \$(pwd)/specs:/input -v \$(pwd)/output:/output elm-openapi-gen \\
     --input /input/openapi.yaml \\
     --moduleprefix MyApi

3. Generate with custom module prefix:
   docker run --rm -v \$(pwd)/output:/output elm-openapi-gen \\
     --input https://api.github.com/openapi.json \\
     --moduleprefix GitHub \\
     --force

Environment Variables:
  INPUT_DIR               Default input directory (default: /input)
  OUTPUT_DIR              Default output directory (default: /output)
  MODULE_PREFIX           Default module prefix (default: Api)

Volumes:
  /input                  Mount directory containing OpenAPI specification files
  /output                 Mount directory for generated Elm code

Support the project: https://github.com/sponsors/Yury-Zakharov

EOF
}

# Check if no arguments provided or help requested
if [ $# -eq 0 ] || [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
    show_usage
    exit 0
fi

# Parse arguments to check for input and output
has_input=false
has_output=false
for arg in "$@"; do
    if [ "$prev_arg" = "--input" ] || [ "$prev_arg" = "-i" ]; then
        has_input=true
        input_value="$arg"
    elif [ "$prev_arg" = "--output" ] || [ "$prev_arg" = "-o" ]; then
        has_output=true
        output_value="$arg"
    fi
    prev_arg="$arg"
done

# Validate required arguments
if [ "$has_input" = false ]; then
    print_error "Missing required argument: --input"
    print_info "Use --help to see usage examples"
    exit 1
fi

# Determine if input is URL or file path
is_url=false
if echo "$input_value" | grep -E '^https?://' > /dev/null; then
    is_url=true
    print_info "Input detected as URL: $input_value"
else
    print_info "Input detected as file path: $input_value"
    
    # Check if file exists when it's a local path
    if [ ! -f "$input_value" ]; then
        print_warning "Input file not found: $input_value"
        print_info "Make sure to mount the directory containing your OpenAPI spec:"
        print_info "  docker run -v /path/to/specs:/input ..."
    fi
fi

# Ensure output directory exists
output_dir="${OUTPUT_DIR:-/output}"
mkdir -p "$output_dir"

# Pre-flight checks
print_info "Pre-flight checks..."

# Check .NET runtime
if ! dotnet --info > /dev/null 2>&1; then
    print_error ".NET runtime not available"
    exit 1
fi

# Check application
if [ ! -f "/app/ElmOpenApiClientGen.dll" ]; then
    print_error "ElmOpenApiClientGen application not found"
    exit 1
fi

print_success "Pre-flight checks completed"

# Show configuration
print_info "Configuration:"
if [ "$is_url" = true ]; then
    print_info "  Input type: URL"
    print_info "  URL: $input_value"
else
    print_info "  Input type: File"
    print_info "  File: $input_value"
fi
print_info "  Output directory: $output_dir"
print_info "  Working directory: $(pwd)"

# Test URL connectivity if input is a URL
if [ "$is_url" = true ]; then
    print_info "Testing URL connectivity..."
    
    if curl -s --head --max-time 30 "$input_value" > /dev/null; then
        print_success "URL is accessible"
    else
        print_warning "URL connectivity test failed - proceeding anyway"
        print_info "The URL might require authentication or have connectivity issues"
    fi
fi

# Run ElmOpenApiClientGen
print_info "Starting code generation..."

# Add default output parameter if not provided
if [ "$has_output" = false ]; then
    set -- "$@" "--output" "$output_dir"
fi

if dotnet /app/ElmOpenApiClientGen.dll "$@"; then
    print_success "Code generation completed successfully!"
    
    # Show generated files
    generated_files=$(find "$output_dir" -name "*.elm" -type f 2>/dev/null | wc -l)
    if [ "$generated_files" -gt 0 ]; then
        print_info "Generated files:"
        find "$output_dir" -name "*.elm" -type f | while read -r file; do
            lines=$(wc -l < "$file" 2>/dev/null || echo "?")
            size=$(wc -c < "$file" 2>/dev/null || echo "?")
            print_info "  📄 $file ($lines lines, $size bytes)"
        done
    else
        print_warning "No .elm files found in output directory"
    fi
    
    # Show summary
    print_header ""
    print_success "🎉 ElmOpenApiClientGen completed successfully!"
    print_info "Generated Elm client code is available in: $output_dir"
    
    if [ "$is_url" = true ]; then
        print_info "Next steps:"
        print_info "  1. Copy the generated files to your Elm project"
        print_info "  2. Update your elm.json dependencies"
        print_info "  3. Import and use the generated API modules"
    fi
    
else
    print_error "Code generation failed!"
    print_info "Common issues:"
    print_info "  - Invalid OpenAPI specification format"
    print_info "  - Network connectivity issues (for URLs)"
    print_info "  - Insufficient permissions for output directory"
    print_info "  - Missing input file (check volume mounts)"
    
    exit 1
fi

print_header "=================================================================="