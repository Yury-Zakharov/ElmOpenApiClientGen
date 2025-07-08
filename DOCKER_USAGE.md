# Docker Usage Guide

This guide explains how to use ElmOpenApiClientGen in Docker containers for generating Elm HTTP clients from OpenAPI specifications.

## Table of Contents

- [Quick Start](#quick-start)
- [Building the Image](#building-the-image)
- [Usage Examples](#usage-examples)
- [Docker Compose](#docker-compose)
- [Advanced Configuration](#advanced-configuration)
- [Troubleshooting](#troubleshooting)

## Quick Start

### Build and Run

```bash
# Build the Docker image
docker build -t elm-openapi-gen .

# Generate from a remote URL
docker run --rm -v $(pwd)/output:/output elm-openapi-gen \
  --input https://petstore.swagger.io/v2/swagger.json \
  --moduleprefix PetStore \
  --force

# Check generated files
ls -la output/
```

### Using Pre-built Images

```bash
# Pull from registry (once published)
docker pull elm-openapi-gen:latest

# Or use the local build
docker tag elm-openapi-gen:latest elm-openapi-gen:local
```

## Building the Image

### Standard Build

```bash
# Build with default settings
docker build -t elm-openapi-gen .

# Build with specific tag
docker build -t elm-openapi-gen:v2.1.0 .

# Build with build arguments (if supported)
docker build --build-arg DOTNET_VERSION=8.0 -t elm-openapi-gen .
```

### Multi-platform Build

```bash
# Build for multiple architectures
docker buildx build --platform linux/amd64,linux/arm64 -t elm-openapi-gen .
```

## Usage Examples

### 1. Generate from Remote URL

```bash
# PetStore API example
docker run --rm -v $(pwd)/output:/output elm-openapi-gen \
  --input https://petstore.swagger.io/v2/swagger.json \
  --moduleprefix PetStore

# GitHub API example  
docker run --rm -v $(pwd)/output:/output elm-openapi-gen \
  --input https://raw.githubusercontent.com/github/rest-api-description/main/descriptions/api.github.com/api.github.com.json \
  --moduleprefix GitHub \
  --force

# Custom API example
docker run --rm -v $(pwd)/output:/output elm-openapi-gen \
  --input https://api.your-company.com/openapi.yaml \
  --moduleprefix CompanyApi \
  --force
```

### 2. Generate from Local File

```bash
# Create input directory and add your OpenAPI spec
mkdir -p input output
cp your-openapi-spec.yaml input/

# Generate from local file
docker run --rm \
  -v $(pwd)/input:/input:ro \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input /input/your-openapi-spec.yaml \
  --moduleprefix MyApi \
  --force

# Alternative: mount specific file
docker run --rm \
  -v $(pwd)/openapi.yaml:/tmp/openapi.yaml:ro \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input /tmp/openapi.yaml \
  --moduleprefix MyApi
```

### 3. Interactive Usage

```bash
# Run container interactively
docker run -it --rm \
  -v $(pwd)/input:/input \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  /bin/sh

# Inside container, run multiple generations
/app $ dotnet ElmOpenApiClientGen.dll --input /input/spec1.yaml --output /output --moduleprefix Api1
/app $ dotnet ElmOpenApiClientGen.dll --input /input/spec2.json --output /output --moduleprefix Api2
```

### 4. Using Environment Variables

```bash
# Set default values with environment variables
docker run --rm \
  -v $(pwd)/output:/output \
  -e MODULE_PREFIX=CustomApi \
  -e OUTPUT_DIR=/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml
```

## Docker Compose

### Basic Compose Setup

Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  elm-codegen:
    build: .
    volumes:
      - ./input:/input:ro
      - ./output:/output
    environment:
      - MODULE_PREFIX=Api
```

### Using the Production Compose File

```bash
# Use the provided production compose file
docker-compose -f docker-compose.production.yml build

# Generate from PetStore
docker-compose -f docker-compose.production.yml run --rm generate-petstore

# Generate from local file
cp your-spec.yaml input/openapi.yaml
docker-compose -f docker-compose.production.yml run --rm generate-from-file

# Custom generation
docker-compose -f docker-compose.production.yml run --rm elm-openapi-gen \
  --input https://api.custom.com/openapi.json \
  --moduleprefix Custom
```

### Advanced Compose with Multiple APIs

```yaml
version: '3.8'

services:
  generate-petstore:
    build: .
    volumes:
      - ./output/petstore:/output
    command:
      - "--input"
      - "https://petstore.swagger.io/v2/swagger.json"
      - "--moduleprefix"
      - "PetStore"

  generate-github:
    build: .
    volumes:
      - ./output/github:/output
    command:
      - "--input"
      - "https://api.github.com/openapi.json"
      - "--moduleprefix"  
      - "GitHub"

  generate-local:
    build: .
    volumes:
      - ./specs:/input:ro
      - ./output/local:/output
    command:
      - "--input"
      - "/input/company-api.yaml"
      - "--moduleprefix"
      - "CompanyApi"
```

## Advanced Configuration

### Health Checks

```bash
# Check container health
docker run --rm elm-openapi-gen --help

# Use health check endpoint
docker run -d --name elm-gen elm-openapi-gen sleep 3600
docker exec elm-gen dotnet /app/ElmOpenApiClientGen.dll --help
```

### Resource Limits

```bash
# Limit memory and CPU usage
docker run --rm \
  --memory=512m \
  --cpus=1.0 \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml
```

### Custom Networking

```bash
# Use custom network for API access
docker network create codegen-network
docker run --rm \
  --network codegen-network \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input http://api-server:8080/openapi.json
```

### Security Considerations

```bash
# Run with read-only root filesystem
docker run --rm \
  --read-only \
  --tmpfs /tmp \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml

# Run with non-root user (already default)
docker run --rm \
  --user 1001:1001 \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml
```

## Troubleshooting

### Common Issues

#### 1. Permission Errors

```bash
# Problem: Permission denied writing to output
# Solution: Fix ownership of output directory
sudo chown -R 1001:1001 output/

# Or run with current user
docker run --rm \
  --user $(id -u):$(id -g) \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml
```

#### 2. Network Issues

```bash
# Problem: Cannot access URL
# Solution: Check network connectivity
docker run --rm elm-openapi-gen curl -I https://api.example.com/openapi.yaml

# Use host networking if needed
docker run --rm \
  --network host \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://localhost:8080/openapi.yaml
```

#### 3. File Not Found

```bash
# Problem: Input file not found
# Solution: Check volume mounts
docker run --rm \
  -v $(pwd)/specs:/input:ro \
  elm-openapi-gen \
  ls -la /input

# Verify file exists
ls -la specs/openapi.yaml
```

#### 4. Empty Output

```bash
# Problem: No files generated
# Solution: Check logs and permissions
docker run --rm \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml

# Check output directory
ls -la output/
```

### Debugging

```bash
# Run with debug output
docker run --rm \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml \
  --force

# Inspect generated files
docker run --rm \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  sh -c "find /output -name '*.elm' -exec head -20 {} \;"

# Interactive debugging
docker run -it --rm \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  /bin/sh
```

### Performance Optimization

```bash
# Use specific .NET runtime options
docker run --rm \
  -e DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
  -e DOTNET_EnableDiagnostics=0 \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input https://api.example.com/openapi.yaml

# Pre-pull base images
docker pull mcr.microsoft.com/dotnet/aspnet:8.0-alpine
```

## Integration with CI/CD

### GitHub Actions

```yaml
name: Generate Elm Client
on:
  push:
    paths: ['api-spec.yaml']

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Generate Elm Client
        run: |
          docker build -t elm-openapi-gen .
          docker run --rm \
            -v ${{ github.workspace }}/generated:/output \
            elm-openapi-gen \
            --input ${{ github.workspace }}/api-spec.yaml \
            --moduleprefix Api \
            --force
      
      - name: Commit generated code
        run: |
          git config --local user.email "action@github.com"
          git config --local user.name "GitHub Action"
          git add generated/
          git commit -m "Update generated Elm client" || exit 0
          git push
```

### GitLab CI

```yaml
generate-elm-client:
  image: docker:latest
  services:
    - docker:dind
  script:
    - docker build -t elm-openapi-gen .
    - mkdir -p generated
    - docker run --rm -v $(pwd)/generated:/output elm-openapi-gen
        --input https://api.example.com/openapi.yaml
        --moduleprefix Api --force
  artifacts:
    paths:
      - generated/
```

---

For more information, see the main [README.md](README.md) and [project documentation](https://github.com/Yury-Zakharov/ElmOpenApiClientGen).