# ElmOpenApiClientGen container for code generation
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Install curl for downloading OpenAPI specs
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Set working directory
WORKDIR /src

# Copy project files first for better caching
COPY ElmOpenApiClientGen.sln ./
COPY src/ElmOpenApiClientGen/*.fsproj ./src/ElmOpenApiClientGen/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ ./src/

# Build the application
RUN dotnet build --configuration Release --no-restore

# Create output directory and set permissions
RUN mkdir -p /output && \
    chown -R appuser:appgroup /src /output

# Switch to non-root user
USER appuser

# Set working directory to the project
WORKDIR /src/src/ElmOpenApiClientGen

# Entry point script to run code generation
COPY --chown=appuser:appgroup integration-tests/docker/codegen/entrypoint.sh /entrypoint.sh
USER root
RUN chmod +x /entrypoint.sh
USER appuser

ENTRYPOINT ["/entrypoint.sh"]