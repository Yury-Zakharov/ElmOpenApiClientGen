# ElmOpenApiClientGen Integration Tests

This directory contains comprehensive integration tests for the ElmOpenApiClientGen tool, designed to validate the complete workflow from OpenAPI specification to working Elm application.

## Overview

The integration test suite consists of:

1. **OpenAPI Test Service** (`OpenApiTestService/`) - A C# HTTP service that serves comprehensive OpenAPI specifications
2. **Elm Test Application** (`elm-test-app/`) - A simple Elm application that consumes generated code
3. **Integration Test Runner** (`run-integration-test.sh`) - Orchestrates the complete testing pipeline

## Architecture

```
integration-tests/
├── OpenApiTestService/           # C# HTTP service
│   ├── Program.cs               # Service implementation
│   ├── OpenApiTestService.csproj # Project file
│   └── ...
├── elm-test-app/                # Elm test application
│   ├── src/
│   │   └── Main.elm            # Main Elm application
│   └── elm.json                # Elm project configuration
├── comprehensive-openapi.yaml   # Complete OpenAPI spec with all features
├── run-integration-test.sh      # Main integration test script
└── README.md                   # This file
```

## Test Coverage

### OpenAPI Features Tested

The comprehensive OpenAPI specification covers all features that ElmOpenApiClientGen can handle:

- **OpenAPI 3.1** specification format
- **Multiple HTTP methods** (GET, POST, PUT, DELETE)
- **Path parameters** with different types (string, UUID, integer)
- **Query parameters** with various constraints
- **Request bodies** (JSON, multipart/form-data)
- **Response schemas** with different status codes
- **Complex data types**:
  - Object types with required/optional fields
  - Array types
  - Enum types
  - Nested objects
  - Union types (allOf, oneOf, anyOf)
  - Nullable fields
  - Additional properties
- **Authentication schemes**:
  - API Key authentication
  - Bearer token authentication
  - Basic authentication
- **Advanced schema features**:
  - Pattern validation
  - Min/max length constraints
  - Min/max value constraints
  - Format validation (email, uri, uuid, date-time)
  - Default values
- **References** ($ref) to reusable components
- **Multiple content types**
- **File uploads** (binary data)
- **Comprehensive error handling**

### Test Workflow

The integration test validates the complete pipeline:

1. **Service Startup** - Start OpenAPI HTTP service
2. **Endpoint Validation** - Verify YAML and JSON endpoints are accessible
3. **HTTP Code Generation** - Run ElmOpenApiClientGen with HTTP URLs as input
4. **Output Validation** - Verify generated Elm code structure and content
5. **Compilation Test** - Ensure generated code compiles without errors
6. **Import Test** - Verify generated modules can be imported
7. **Application Build** - Build complete Elm application using generated code

## Running the Tests

### Prerequisites

- .NET 10.0 or later
- Elm 0.19.1
- curl (for HTTP testing)
- Python 3 (for JSON manipulation)

### Quick Start

```bash
# Navigate to integration tests directory
cd integration-tests

# Run the complete integration test suite
./run-integration-test.sh
```

### Manual Testing

You can also run components individually:

```bash
# Start the OpenAPI service
cd OpenApiTestService
dotnet run --urls "http://localhost:5000"

# In another terminal, test the endpoints
curl http://localhost:5000/openapi.yaml
curl http://localhost:5000/openapi.json
curl http://localhost:5000/health

# Run code generation
cd ..
dotnet run --project ../src/ElmOpenApiClientGen \
    --input "http://localhost:5000/openapi.yaml" \
    --output "./elm-test-app/src/Generated" \
    --moduleprefix "Api" \
    --force

# Build Elm application
cd elm-test-app
elm make src/Main.elm --output=main.js
```

## Test Output

The integration test provides detailed output including:

- ✅ **Success indicators** for each test phase
- 📊 **Statistics** about generated files
- 🔍 **Validation results** for generated code
- 🚨 **Error messages** with specific failure details
- 📋 **Summary** of all test results

### Expected Output

```
[INFO] Starting ElmOpenApiClientGen Integration Test
[INFO] ========================================
[SUCCESS] ElmOpenApiClientGen tool built successfully
[SUCCESS] OpenAPI service is ready
[SUCCESS] OpenAPI YAML endpoint is working
[SUCCESS] OpenAPI JSON endpoint is working
[SUCCESS] Code generation from YAML endpoint successful
[SUCCESS] Code generation from JSON endpoint successful
[SUCCESS] Generated module has correct module declaration
[SUCCESS] Generated code contains type aliases
[SUCCESS] Generated code contains JSON decoders
[SUCCESS] Elm application built successfully
[SUCCESS] Generated code can be imported successfully
[SUCCESS] ========================================
[SUCCESS] Integration test completed successfully!
[SUCCESS] ========================================
```

## Maintenance

### Adding New Features

When new capabilities are added to ElmOpenApiClientGen:

1. **Update OpenAPI spec** (`comprehensive-openapi.yaml`) to include new features
2. **Add validation** to the integration test script
3. **Update Elm test app** if new generated code patterns need testing
4. **Keep unit tests unchanged** (as per requirements)

### Test Structure

The tests are designed to be:

- **Comprehensive** - Cover all ElmOpenApiClientGen features
- **Automated** - Run without manual intervention
- **Maintainable** - Easy to update when new features are added
- **Reliable** - Consistent results across environments
- **Fast** - Complete in under 60 seconds

## CI/CD Integration

The integration tests can be integrated into CI/CD pipelines:

```yaml
# Example GitHub Actions step
- name: Run Integration Tests
  run: |
    cd integration-tests
    ./run-integration-test.sh
```

## Troubleshooting

### Common Issues

1. **Port conflicts** - Change SERVICE_PORT in the script if 5000 is in use
2. **Elm not found** - Ensure Elm is installed and in PATH
3. **.NET build errors** - Ensure .NET 10.0 SDK is installed
4. **Permission errors** - Ensure the script is executable (`chmod +x`)

### Debug Mode

For debugging, you can run individual components and inspect their output:

```bash
# Check service logs
cd OpenApiTestService
dotnet run --urls "http://localhost:5000" --verbosity detailed

# Check generated code
find elm-test-app/src/Generated -name "*.elm" -exec echo "=== {} ===" \; -exec cat {} \;

# Check Elm compilation errors
cd elm-test-app
elm make src/Main.elm --output=main.js --debug
```

This integration test suite ensures that ElmOpenApiClientGen works correctly in real-world scenarios and maintains high quality standards for generated code.