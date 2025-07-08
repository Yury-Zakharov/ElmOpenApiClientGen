# Multi-stage build for minimal OpenAPI service container
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Set working directory
WORKDIR /src

# Copy project files
COPY OpenApiTestService/*.csproj ./OpenApiTestService/
COPY comprehensive-openapi.yaml ./

# Restore dependencies
RUN cd OpenApiTestService && dotnet restore

# Copy source code
COPY OpenApiTestService/ ./OpenApiTestService/

# Clean any existing build artifacts and build
RUN cd OpenApiTestService && \
    rm -rf obj bin && \
    dotnet restore && \
    dotnet publish -c Release -o /app/publish --no-restore --self-contained false

# Runtime stage - minimal Alpine image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Install curl for health checks
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Set working directory
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .
COPY --from=build /src/comprehensive-openapi.yaml .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=5s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "OpenApiTestService.dll"]