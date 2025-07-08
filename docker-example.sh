#!/bin/bash

# ElmOpenApiClientGen Docker Example
# This script demonstrates how to use the Docker container

set -e

echo "🐳 ElmOpenApiClientGen Docker Example"
echo "======================================"

# Create output directory
mkdir -p docker-output

echo ""
echo "📋 Available Docker commands:"
echo ""

echo "1. Show help:"
echo "   docker run --rm elm-openapi-gen --help"
echo ""

echo "2. Generate from PetStore API (remote URL):"
echo "   docker run --rm -v \$(pwd)/docker-output:/output elm-openapi-gen \\"
echo "     --input https://petstore.swagger.io/v2/swagger.json \\"
echo "     --moduleprefix PetStore --force"
echo ""

echo "3. Generate from local file:"
echo "   docker run --rm \\"
echo "     -v \$(pwd)/sample:/input:ro \\"
echo "     -v \$(pwd)/docker-output:/output \\"
echo "     elm-openapi-gen \\"
echo "     --input /input/openapi.yaml \\"
echo "     --moduleprefix Sample --force"
echo ""

echo "4. Using Docker Compose:"
echo "   docker-compose -f docker-compose.production.yml build"
echo "   docker-compose -f docker-compose.production.yml run --rm generate-petstore"
echo ""

echo "📁 Files created:"
echo "   - Dockerfile (multi-stage production build)"
echo "   - docker-entrypoint.sh (user-friendly entrypoint)" 
echo "   - .dockerignore (optimized build context)"
echo "   - docker-compose.production.yml (usage examples)"
echo "   - DOCKER_USAGE.md (comprehensive documentation)"
echo ""

echo "🚀 To get started:"
echo "   1. docker build -t elm-openapi-gen ."
echo "   2. docker run --rm elm-openapi-gen --help"
echo "   3. Try the examples above!"
echo ""

echo "✅ Ready for containerized code generation!"