# Test Orchestrator container for coordinating integration tests
FROM alpine:latest

# Install required tools
RUN apk add --no-cache \
    curl \
    bash \
    jq \
    util-linux

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Create working directory
RUN mkdir -p /tests && \
    chown -R appuser:appgroup /tests

# Copy test scripts
COPY --chown=appuser:appgroup docker/test-orchestrator/scripts/ /tests/

# Make scripts executable
RUN chmod +x /tests/*.sh

# Switch to non-root user
USER appuser

# Set working directory
WORKDIR /tests

# Entry point
ENTRYPOINT ["./orchestrator.sh"]