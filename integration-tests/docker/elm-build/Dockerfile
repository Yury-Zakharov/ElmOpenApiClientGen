# Elm Build container for validating generated code
FROM node:alpine AS elm-installer

# Install Elm binary
RUN npm install --global elm@0.19.1-5 --unsafe-perm=true

# Create minimal runtime image
FROM alpine:latest AS runtime

# Install Node.js runtime (needed for Elm)
RUN apk add --no-cache nodejs npm

# Copy Elm binary from installer stage
COPY --from=elm-installer /usr/local/bin/elm /usr/local/bin/elm
COPY --from=elm-installer /usr/local/lib/node_modules/elm /usr/local/lib/node_modules/elm

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Create working directories
RUN mkdir -p /app /generated-code && \
    chown -R appuser:appgroup /app /generated-code

# Copy Elm test application
COPY --chown=appuser:appgroup elm-test-app/ /app/

# Switch to non-root user
USER appuser

# Set working directory
WORKDIR /app

# Entry point script
COPY --chown=appuser:appgroup docker/elm-build/entrypoint.sh /entrypoint.sh
USER root
RUN chmod +x /entrypoint.sh
USER appuser

ENTRYPOINT ["/entrypoint.sh"]