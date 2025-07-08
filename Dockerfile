# ElmOpenApiClientGen - Production Container
# 
# This Dockerfile creates a production-ready container for ElmOpenApiClientGen
# that can generate Elm client code from OpenAPI specifications.
#
# Usage examples:
#   docker build -t elm-openapi-gen .
#   docker run --rm -v $(pwd)/output:/output elm-openapi-gen --input https://api.example.com/openapi.yaml
#   docker run --rm -v $(pwd)/specs:/input -v $(pwd)/output:/output elm-openapi-gen --input /input/spec.yaml

# === Build Stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Install dependencies for downloading and processing OpenAPI specs
RUN apk add --no-cache \
    curl \
    ca-certificates \
    && rm -rf /var/cache/apk/*

# Set working directory
WORKDIR /src

# Copy solution and project files first for better Docker layer caching
COPY ElmOpenApiClientGen.sln ./
COPY src/ElmOpenApiClientGen/ElmOpenApiClientGen.fsproj ./src/ElmOpenApiClientGen/
COPY tests/ElmOpenApiClientGen.Tests/ElmOpenApiClientGen.Tests.fsproj ./tests/ElmOpenApiClientGen.Tests/

# Restore NuGet packages for the main project only
RUN dotnet restore src/ElmOpenApiClientGen/ElmOpenApiClientGen.fsproj

# Copy source code
COPY src/ ./src/

# Build the application in Release mode
RUN dotnet build --configuration Release --no-restore

# Publish as self-contained single file for better performance and smaller runtime image
RUN dotnet publish src/ElmOpenApiClientGen/ElmOpenApiClientGen.fsproj \
    --configuration Release \
    --output /app \
    --no-restore \
    --self-contained false

# === Runtime Stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Install runtime dependencies
RUN apk add --no-cache \
    curl \
    ca-certificates \
    && rm -rf /var/cache/apk/*

# Create non-root user for security
RUN addgroup -g 1001 -S elmgen && \
    adduser -S elmgen -u 1001 -G elmgen -h /home/elmgen

# Create working directories
RUN mkdir -p /app /input /output && \
    chown -R elmgen:elmgen /app /input /output

# Copy the published application
COPY --from=build --chown=elmgen:elmgen /app /app

# Copy entrypoint script
COPY --chown=elmgen:elmgen docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

# Switch to non-root user
USER elmgen

# Set working directory
WORKDIR /app

# Expose volumes for input specifications and output code
VOLUME ["/input", "/output"]

# Set default environment variables
ENV INPUT_DIR="/input" \
    OUTPUT_DIR="/output" \
    MODULE_PREFIX="Api"

# Health check to verify the application is working
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet /app/ElmOpenApiClientGen.dll --help > /dev/null || exit 1

# Set entrypoint
ENTRYPOINT ["docker-entrypoint.sh"]

# Default command - show help
CMD ["--help"]

# Metadata
LABEL org.opencontainers.image.title="ElmOpenApiClientGen" \
      org.opencontainers.image.description="Generate type-safe Elm HTTP clients from OpenAPI specifications" \
      org.opencontainers.image.version="2.1.0" \
      org.opencontainers.image.vendor="ElmOpenApiClientGen Project" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.source="https://github.com/Yury-Zakharov/ElmOpenApiClientGen" \
      org.opencontainers.image.documentation="https://github.com/Yury-Zakharov/ElmOpenApiClientGen/blob/main/README.md"