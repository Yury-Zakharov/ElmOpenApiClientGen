# ElmOpenApiClientGen Containerized Integration Tests

This directory contains containerized integration tests that provide complete isolation and eliminate host dependencies for testing the ElmOpenApiClientGen tool.

## 🎯 Overview

The containerized integration test suite validates the complete workflow from OpenAPI specification to working Elm application using Docker containers for isolation and consistency.

### ✅ Benefits of Containerized Testing

- **Zero Host Dependencies** - Only Docker required, no Elm, .NET, or Node.js needed on host
- **Consistent Environment** - Same results across different development machines and CI/CD
- **Automatic Cleanup** - Containers are automatically destroyed after testing
- **Isolated Testing** - No interference with host system or other processes
- **Parallel Execution** - Multiple test phases can run concurrently
- **CI/CD Ready** - Perfect for automated testing pipelines

## 🏗️ Architecture

### Container Components

```
┌─────────────────────┐    ┌──────────────────────┐    ┌─────────────────────┐
│   OpenAPI Service   │    │   ElmOpenApiClientGen│    │     Elm Build       │
│                     │    │      Container       │    │    Container        │
│  - Serves YAML/JSON │    │                      │    │                     │
│  - Health checks    │    │  - Downloads specs   │    │  - Compiles Elm     │
│  - Alpine-based     │    │  - Generates code    │    │  - Validates output │
│     (~100MB)        │    │  - .NET SDK-based    │    │  - Node.js-based    │
└─────────────────────┘    │     (~200MB)         │    │     (~50MB)         │
                          └──────────────────────┘    └─────────────────────┘
                                      │                           │
                              ┌───────▼───────┐           ┌──────▼─────┐
                              │ Generated Code │           │Final Results│
                              │    Volume      │           │ Validation  │
                              └───────────────┘           └────────────┘
                                      │                           │
                              ┌───────▼──────────────────────────▼─────┐
                              │         Test Orchestrator              │
                              │                                        │
                              │  - Coordinates workflow                │
                              │  - Validates each phase                │
                              │  - Reports final results               │
                              │  - Alpine-based (~20MB)                │
                              └────────────────────────────────────────┘
```

### Workflow

1. **OpenAPI Service** starts and serves comprehensive OpenAPI specs
2. **Test Orchestrator** validates service availability and spec quality
3. **Code Generation** (YAML) downloads spec and generates Elm code
4. **Code Generation** (JSON) repeats with JSON format for comparison
5. **Elm Build** (YAML) validates generated code compiles correctly
6. **Elm Build** (JSON) validates JSON-generated code compiles correctly
7. **Final Validator** compares results and provides comprehensive report

## 🚀 Quick Start

### Prerequisites

- Docker 20.10+ 
- Docker Compose 1.29+

### Running Tests

```bash
# Navigate to integration tests directory
cd integration-tests

# Run the complete containerized test suite
./run-containerized-test.sh
```

### Advanced Usage

```bash
# Clean up first, then run tests
./run-containerized-test.sh --cleanup

# Run tests and show detailed logs
./run-containerized-test.sh --logs

# Only build containers (useful for debugging)
./run-containerized-test.sh --build-only

# Clean up including dangling images
./run-containerized-test.sh --cleanup-images

# Run without automatic cleanup (for debugging)
./run-containerized-test.sh --no-cleanup
```

## 📋 Container Details

### OpenAPI Service Container
- **Base**: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
- **Size**: ~100MB
- **Purpose**: Serves OpenAPI specifications in YAML and JSON formats
- **Endpoints**: 
  - `/openapi.yaml` - YAML format
  - `/openapi.json` - JSON format  
  - `/health` - Health check
- **Features**: Health checks, non-root user, minimal dependencies

### ElmOpenApiClientGen Container
- **Base**: `mcr.microsoft.com/dotnet/sdk:10.0-alpine`
- **Size**: ~200MB
- **Purpose**: Downloads specs and generates Elm code
- **Environment**:
  - `OPENAPI_SERVICE_URL` - Service URL
  - `OUTPUT_DIR` - Code output directory
  - `MODULE_PREFIX` - Elm module prefix
  - `SPEC_FORMAT` - YAML or JSON
- **Features**: Automatic service discovery, configurable output

### Elm Build Container
- **Base**: `node:alpine` with Elm 0.19.1
- **Size**: ~50MB
- **Purpose**: Validates generated Elm code compiles
- **Tests**:
  - Main application compilation
  - Generated code import validation
  - Code structure verification
- **Features**: Elm compiler, automatic elm.json updates

### Test Orchestrator Container
- **Base**: `alpine:latest`
- **Size**: ~20MB
- **Purpose**: Coordinates test workflow and validation
- **Tools**: curl, bash, jq for API testing and JSON parsing
- **Features**: Service health monitoring, comprehensive reporting

## 🔧 Configuration

### Docker Compose Services

```yaml
services:
  openapi-service:     # Serves OpenAPI specs
  codegen:             # Generates code from YAML
  codegen-json:        # Generates code from JSON  
  elm-build:           # Validates YAML-generated code
  elm-build-json:      # Validates JSON-generated code
  test-orchestrator:   # Pre-validation checks
  final-validator:     # Final results validation
```

### Shared Volumes

- `generated-code` - YAML-generated Elm code
- `generated-code-json` - JSON-generated Elm code

### Network

- `elm-codegen-network` - Internal container communication

## 📊 Test Output

### Successful Test Output

```
==================================================================
     ElmOpenApiClientGen Containerized Integration Test
==================================================================
[SUCCESS] Docker and Docker Compose are available
[SUCCESS] All containers built successfully
[SUCCESS] OpenAPI service is healthy
[SUCCESS] Test orchestrator completed successfully
[SUCCESS] YAML code generation completed successfully
[SUCCESS] JSON code generation completed successfully
[SUCCESS] YAML-generated Elm code compilation successful
[SUCCESS] JSON-generated Elm code compilation successful
[SUCCESS] Final validation completed successfully

==================================================================
🎉 CONTAINERIZED INTEGRATION TEST COMPLETED SUCCESSFULLY! 🎉
==================================================================
✓ All containers built and ran successfully
✓ OpenAPI service deployment and health checks passed
✓ Code generation from YAML and JSON sources completed
✓ Generated Elm code compiled successfully
✓ Cross-format consistency validation passed
✓ Complete workflow validation successful

🚀 ElmOpenApiClientGen is ready for containerized deployment!
```

### Test Phases Validated

1. ✅ **Container Build** - All images build successfully
2. ✅ **Service Health** - OpenAPI service starts and responds
3. ✅ **API Availability** - YAML and JSON endpoints accessible
4. ✅ **Spec Quality** - OpenAPI specification structure validated
5. ✅ **Code Generation** - Both YAML and JSON specs processed
6. ✅ **Code Compilation** - Generated Elm code compiles without errors
7. ✅ **Import Validation** - Generated modules can be imported
8. ✅ **Cross-Format Consistency** - YAML and JSON generate identical code
9. ✅ **Structure Validation** - Code contains expected patterns

## 🔍 Debugging

### Container Logs

```bash
# View logs for specific container
docker-compose -p elm-codegen-integration-test logs openapi-service
docker-compose -p elm-codegen-integration-test logs codegen
docker-compose -p elm-codegen-integration-test logs elm-build

# View all logs
./run-containerized-test.sh --logs
```

### Manual Container Inspection

```bash
# Build containers only
./run-containerized-test.sh --build-only

# Run specific container interactively
docker-compose -p elm-codegen-integration-test run --rm codegen sh

# Inspect generated code
docker-compose -p elm-codegen-integration-test run --rm final-validator sh
```

### Volume Inspection

```bash
# List volumes
docker volume ls | grep elm-codegen

# Inspect generated code
docker run --rm -v elm-codegen-integration-test_generated-code:/code alpine ls -la /code
```

## 🚀 CI/CD Integration

### GitHub Actions Example

```yaml
name: Integration Tests
on: [push, pull_request]

jobs:
  containerized-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Containerized Integration Tests
        run: |
          cd integration-tests
          ./run-containerized-test.sh --cleanup --logs
        env:
          CI: true
```

### Environment Variables

- `CI=true` - Enables CI-specific settings (cleanup images, show logs)
- `CLEANUP_IMAGES=true` - Remove dangling images after cleanup
- `SHOW_LOGS=true` - Show container logs after completion

## 📁 File Structure

```
integration-tests/
├── docker/
│   ├── openapi-service/
│   │   ├── Dockerfile              # Multi-stage OpenAPI service
│   │   └── .dockerignore
│   ├── codegen/
│   │   ├── Dockerfile              # ElmOpenApiClientGen container
│   │   ├── entrypoint.sh           # Code generation script
│   │   └── .dockerignore
│   ├── elm-build/
│   │   ├── Dockerfile              # Elm compilation container
│   │   ├── entrypoint.sh           # Build validation script
│   │   └── .dockerignore
│   └── test-orchestrator/
│       ├── Dockerfile              # Test coordination container
│       └── scripts/
│           ├── orchestrator.sh     # Pre-validation script
│           └── final-validator.sh  # Final validation script
├── docker-compose.yml              # Container orchestration
├── run-containerized-test.sh       # Main test runner
├── OpenApiTestService/             # C# service source
├── elm-test-app/                   # Elm application source
├── comprehensive-openapi.yaml      # Complete OpenAPI spec
└── README-containerized.md         # This file
```

## 🔧 Customization

### Adding New Test Phases

1. Create new container in `docker/` directory
2. Add service to `docker-compose.yml`
3. Update dependencies in compose file
4. Add validation to `final-validator.sh`

### Modifying OpenAPI Spec

1. Edit `comprehensive-openapi.yaml`
2. Rebuild containers: `./run-containerized-test.sh --build-only`
3. Run tests: `./run-containerized-test.sh`

### Container Optimization

- Use multi-stage builds to minimize image sizes
- Add `.dockerignore` files to exclude unnecessary files
- Use Alpine Linux base images for smaller footprint
- Cache package installations in separate layers

## ⚡ Performance

### Build Times
- Initial build: ~5-10 minutes (downloads base images)
- Subsequent builds: ~2-3 minutes (uses cache)
- Test execution: ~3-5 minutes

### Resource Usage
- Memory: ~2GB peak during build, ~500MB during tests
- Disk: ~1GB for all images and volumes
- CPU: Utilizes available cores during parallel builds

### Optimization Tips
- Use `--build-only` for development to avoid repeated builds
- Enable BuildKit for faster Docker builds: `DOCKER_BUILDKIT=1`
- Use Docker layer caching in CI/CD for faster builds

This containerized integration test suite provides a robust, isolated, and comprehensive way to validate ElmOpenApiClientGen functionality across different environments and deployment scenarios.