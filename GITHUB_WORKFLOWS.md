# GitHub Workflows for API Change Tracking

This document provides GitHub Actions workflows for automatically tracking OpenAPI specification changes and updating generated Elm client code in your application.

## Table of Contents

- [Overview](#overview)
- [Scenario 1: External API Repository](#scenario-1-external-api-repository)
- [Scenario 2: Owned API Repository](#scenario-2-owned-api-repository)
- [Setup Instructions](#setup-instructions)
- [Configuration Options](#configuration-options)
- [Troubleshooting](#troubleshooting)

## Overview

When building Elm applications that consume APIs with OpenAPI specifications, you want to automatically update your client code when the API changes. This document provides two workflow approaches:

1. **External API Repository** - Track releases from a third-party API repository
2. **Owned API Repository** - Coordinate updates when you control both the API and client repositories

## Scenario 1: External API Repository

### Use Case
- **Person A**: Owns API service with OpenAPI specs in `repository-a` (public)
- **Person B**: Owns Elm app in `repository-b` that consumes the API
- **Goal**: Automatically update Elm client when `repository-a` releases new API versions

### Workflow: External API Tracking

Create `.github/workflows/api-update.yml` in your Elm app repository:

```yaml
name: Track API Changes and Update Client

on:
  schedule:
    # Check for new releases every day at 2 AM UTC
    - cron: '0 2 * * *'
  workflow_dispatch:
    inputs:
      api_repository:
        description: 'API repository (owner/repo)'
        required: false
        default: 'api-owner/api-repository'
      force_update:
        description: 'Force update even if no new release'
        type: boolean
        default: false

env:
  API_REPOSITORY: ${{ github.event.inputs.api_repository || 'api-owner/api-repository' }}
  ELM_OUTPUT_DIR: 'src/Generated/Api'
  MODULE_PREFIX: 'Api'

jobs:
  check-api-updates:
    runs-on: ubuntu-latest
    outputs:
      has-new-release: ${{ steps.check-release.outputs.has-new-release }}
      latest-tag: ${{ steps.check-release.outputs.latest-tag }}
      release-url: ${{ steps.check-release.outputs.release-url }}
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Check for new API releases
        id: check-release
        run: |
          # Get the latest release from the API repository
          LATEST_RELEASE=$(curl -s "https://api.github.com/repos/$API_REPOSITORY/releases/latest")
          LATEST_TAG=$(echo "$LATEST_RELEASE" | jq -r '.tag_name // empty')
          RELEASE_URL=$(echo "$LATEST_RELEASE" | jq -r '.html_url // empty')
          
          if [ -z "$LATEST_TAG" ] || [ "$LATEST_TAG" = "null" ]; then
            echo "No releases found in $API_REPOSITORY"
            echo "has-new-release=false" >> $GITHUB_OUTPUT
            exit 0
          fi
          
          echo "Latest API release: $LATEST_TAG"
          echo "latest-tag=$LATEST_TAG" >> $GITHUB_OUTPUT
          echo "release-url=$RELEASE_URL" >> $GITHUB_OUTPUT
          
          # Check if we've already processed this release
          LAST_PROCESSED_FILE=".github/last-api-version"
          if [ -f "$LAST_PROCESSED_FILE" ]; then
            LAST_PROCESSED=$(cat "$LAST_PROCESSED_FILE")
            if [ "$LATEST_TAG" = "$LAST_PROCESSED" ] && [ "${{ github.event.inputs.force_update }}" != "true" ]; then
              echo "Already processed release $LATEST_TAG"
              echo "has-new-release=false" >> $GITHUB_OUTPUT
              exit 0
            fi
          fi
          
          echo "New release detected: $LATEST_TAG"
          echo "has-new-release=true" >> $GITHUB_OUTPUT

  update-client-code:
    needs: check-api-updates
    if: needs.check-api-updates.outputs.has-new-release == 'true'
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          fetch-depth: 0
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Clone ElmOpenApiClientGen
        run: |
          git clone https://github.com/Yury-Zakharov/ElmOpenApiClientGen.git elm-codegen
          cd elm-codegen
          dotnet build
      
      - name: Generate OpenAPI spec URL
        id: spec-url
        run: |
          # Try multiple common OpenAPI spec locations
          API_TAG="${{ needs.check-api-updates.outputs.latest-tag }}"
          
          # Common OpenAPI spec URLs to try
          SPEC_URLS=(
            "https://raw.githubusercontent.com/$API_REPOSITORY/$API_TAG/openapi.yaml"
            "https://raw.githubusercontent.com/$API_REPOSITORY/$API_TAG/openapi.json"
            "https://raw.githubusercontent.com/$API_REPOSITORY/$API_TAG/api/openapi.yaml"
            "https://raw.githubusercontent.com/$API_REPOSITORY/$API_TAG/docs/openapi.yaml"
            "https://raw.githubusercontent.com/$API_REPOSITORY/$API_TAG/swagger.yaml"
            "https://raw.githubusercontent.com/$API_REPOSITORY/$API_TAG/swagger.json"
          )
          
          SPEC_URL=""
          for url in "${SPEC_URLS[@]}"; do
            echo "Trying: $url"
            if curl -f -s "$url" > /dev/null; then
              SPEC_URL="$url"
              echo "Found OpenAPI spec at: $url"
              break
            fi
          done
          
          if [ -z "$SPEC_URL" ]; then
            echo "Error: Could not find OpenAPI specification in $API_REPOSITORY at tag $API_TAG"
            echo "Tried the following URLs:"
            printf '%s\n' "${SPEC_URLS[@]}"
            exit 1
          fi
          
          echo "spec-url=$SPEC_URL" >> $GITHUB_OUTPUT
      
      - name: Generate Elm client code
        run: |
          # Clean existing generated code
          rm -rf "$ELM_OUTPUT_DIR"
          mkdir -p "$ELM_OUTPUT_DIR"
          
          # Generate new client code
          cd elm-codegen
          dotnet run --project src/ElmOpenApiClientGen \
            --input "${{ steps.spec-url.outputs.spec-url }}" \
            --output "../$ELM_OUTPUT_DIR" \
            --moduleprefix "$MODULE_PREFIX" \
            --force
      
      - name: Setup Elm
        uses: jorelali/setup-elm@v5
        with:
          elm-version: 0.19.1
      
      - name: Validate generated Elm code
        run: |
          # Check if elm.json exists, create minimal one if not
          if [ ! -f "elm.json" ]; then
            echo "Creating minimal elm.json for validation..."
            cat > elm.json << EOF
          {
            "type": "application",
            "source-directories": ["src"],
            "elm-version": "0.19.1",
            "dependencies": {
              "direct": {
                "elm/browser": "1.0.2",
                "elm/core": "1.0.5",
                "elm/html": "1.0.0",
                "elm/http": "2.0.0",
                "elm/json": "1.1.3",
                "elm/time": "1.0.0",
                "elm/url": "1.0.0"
              },
              "indirect": {
                "elm/bytes": "1.0.8",
                "elm/file": "1.0.5",
                "elm/virtual-dom": "1.0.3"
              }
            },
            "test-dependencies": {
              "direct": {},
              "indirect": {}
            }
          }
          EOF
          fi
          
          # Validate Elm code compiles
          echo "Validating generated Elm code..."
          if ! elm make $ELM_OUTPUT_DIR/*.elm --output=/dev/null; then
            echo "Error: Generated Elm code does not compile"
            exit 1
          fi
          
          echo "Generated Elm code validation successful"
      
      - name: Update tracking file
        run: |
          mkdir -p .github
          echo "${{ needs.check-api-updates.outputs.latest-tag }}" > .github/last-api-version
      
      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v5
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: |
            feat: update API client for ${{ env.API_REPOSITORY }} ${{ needs.check-api-updates.outputs.latest-tag }}
            
            - Updated generated Elm client code
            - API version: ${{ needs.check-api-updates.outputs.latest-tag }}
            - Release: ${{ needs.check-api-updates.outputs.release-url }}
            
            Auto-generated by GitHub Actions
          title: 'Update API client for ${{ needs.check-api-updates.outputs.latest-tag }}'
          body: |
            ## API Client Update
            
            This PR updates the generated Elm client code for the latest API release.
            
            ### Changes
            - **API Repository**: `${{ env.API_REPOSITORY }}`
            - **New Version**: `${{ needs.check-api-updates.outputs.latest-tag }}`
            - **Release Notes**: [${{ needs.check-api-updates.outputs.latest-tag }}](${{ needs.check-api-updates.outputs.release-url }})
            - **Generated Code**: Updated `${{ env.ELM_OUTPUT_DIR }}/` directory
            
            ### Validation
            - ✅ OpenAPI specification downloaded successfully
            - ✅ Elm client code generated without errors
            - ✅ Generated code compiles successfully
            
            ### Next Steps
            1. Review the generated code changes
            2. Test your application with the new client
            3. Update any breaking changes in your app code
            4. Merge when ready
            
            ---
            *This PR was automatically created by the API tracking workflow.*
          branch: api-update/${{ needs.check-api-updates.outputs.latest-tag }}
          branch-suffix: timestamp
          delete-branch: true
          draft: false
          labels: |
            api-update
            auto-generated
```

## Scenario 2: Owned API Repository

### Use Case
- **Person B**: Owns both the API service repository and the Elm app repository
- **Goal**: Coordinate updates across both repositories when API changes

### Workflow A: API Repository (Trigger)

Create `.github/workflows/notify-clients.yml` in your API repository:

```yaml
name: Notify Client Repositories

on:
  release:
    types: [published]
  workflow_dispatch:
    inputs:
      version:
        description: 'API version to broadcast'
        required: true

jobs:
  notify-clients:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        client_repo:
          # Add your client repositories here
          - "your-username/elm-app-1"
          - "your-username/elm-app-2"
          # Add more client repos as needed
    
    steps:
      - name: Trigger client update
        uses: peter-evans/repository-dispatch@v2
        with:
          token: ${{ secrets.CLIENT_UPDATE_TOKEN }}
          repository: ${{ matrix.client_repo }}
          event-type: api-updated
          client-payload: |
            {
              "api_repository": "${{ github.repository }}",
              "api_version": "${{ github.event.release.tag_name || github.event.inputs.version }}",
              "release_url": "${{ github.event.release.html_url || '' }}",
              "openapi_spec_url": "https://raw.githubusercontent.com/${{ github.repository }}/${{ github.event.release.tag_name || github.event.inputs.version }}/openapi.yaml"
            }
```

### Workflow B: Client Repository (Receiver)

Create `.github/workflows/api-updated.yml` in your Elm app repository:

```yaml
name: Update API Client (Repository Dispatch)

on:
  repository_dispatch:
    types: [api-updated]

env:
  ELM_OUTPUT_DIR: 'src/Generated/Api'
  MODULE_PREFIX: 'Api'

jobs:
  update-client:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Clone ElmOpenApiClientGen
        run: |
          git clone https://github.com/Yury-Zakharov/ElmOpenApiClientGen.git elm-codegen
          cd elm-codegen
          dotnet build
      
      - name: Generate Elm client code
        run: |
          API_SPEC_URL="${{ github.event.client_payload.openapi_spec_url }}"
          
          # Clean existing generated code
          rm -rf "$ELM_OUTPUT_DIR"
          mkdir -p "$ELM_OUTPUT_DIR"
          
          # Generate new client code
          cd elm-codegen
          dotnet run --project src/ElmOpenApiClientGen \
            --input "$API_SPEC_URL" \
            --output "../$ELM_OUTPUT_DIR" \
            --moduleprefix "$MODULE_PREFIX" \
            --force
      
      - name: Setup Elm and validate
        uses: jorelali/setup-elm@v5
        with:
          elm-version: 0.19.1
      
      - name: Validate generated code
        run: |
          # Validate Elm code compiles
          if ! elm make $ELM_OUTPUT_DIR/*.elm --output=/dev/null; then
            echo "Error: Generated Elm code does not compile"
            exit 1
          fi
      
      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v5
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: |
            feat: update API client for ${{ github.event.client_payload.api_version }}
            
            - Updated from ${{ github.event.client_payload.api_repository }}
            - Version: ${{ github.event.client_payload.api_version }}
            
            Auto-generated by repository dispatch
          title: 'API Update: ${{ github.event.client_payload.api_version }}'
          body: |
            ## Coordinated API Client Update
            
            This PR was automatically triggered by a release in the API repository.
            
            ### Details
            - **API Repository**: `${{ github.event.client_payload.api_repository }}`
            - **New Version**: `${{ github.event.client_payload.api_version }}`
            - **Release**: [${{ github.event.client_payload.api_version }}](${{ github.event.client_payload.release_url }})
            - **OpenAPI Spec**: `${{ github.event.client_payload.openapi_spec_url }}`
            
            ### Validation
            - ✅ Generated code compiles successfully
            - ✅ Ready for integration testing
            
            *This is a coordinated update from the API repository.*
          branch: api-update/${{ github.event.client_payload.api_version }}
          labels: |
            api-update
            coordinated-update
```

## Setup Instructions

### For External API Tracking (Scenario 1)

1. **Add the workflow** to your Elm app repository at `.github/workflows/api-update.yml`

2. **Configure the workflow**:
   - Update `API_REPOSITORY` environment variable with the actual repository
   - Adjust `ELM_OUTPUT_DIR` to match your project structure
   - Modify `MODULE_PREFIX` as needed
   - Customize the OpenAPI spec URL detection logic if needed

3. **Test the workflow**:
   ```bash
   # Trigger manually to test
   gh workflow run api-update.yml -f force_update=true
   ```

### For Owned Repositories (Scenario 2)

1. **Create a Personal Access Token**:
   - Go to GitHub Settings → Developer settings → Personal access tokens
   - Create a token with `repo` scope
   - Add as `CLIENT_UPDATE_TOKEN` secret in your API repository

2. **Add workflows**:
   - API repo: `.github/workflows/notify-clients.yml`
   - Client repo(s): `.github/workflows/api-updated.yml`

3. **Update repository lists** in the API workflow matrix

## Configuration Options

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `API_REPOSITORY` | Target API repository (owner/repo) | - |
| `ELM_OUTPUT_DIR` | Output directory for generated code | `src/Generated/Api` |
| `MODULE_PREFIX` | Elm module prefix | `Api` |

### Workflow Triggers

- **Schedule**: Daily checks at 2 AM UTC
- **Manual**: `workflow_dispatch` for testing
- **Repository Dispatch**: For owned repository coordination

### OpenAPI Spec Detection

The workflow attempts to find OpenAPI specs at common locations:
- `openapi.yaml` / `openapi.json`
- `api/openapi.yaml`
- `docs/openapi.yaml`
- `swagger.yaml` / `swagger.json`

Customize the `SPEC_URLS` array to match your API repository structure.

## Troubleshooting

### Common Issues

1. **OpenAPI spec not found**
   - Verify the spec file location in the API repository
   - Add custom URLs to the `SPEC_URLS` array
   - Check if the repository is public or requires authentication

2. **Generated code doesn't compile**
   - Ensure your `elm.json` includes all required dependencies
   - Check for breaking changes in the OpenAPI spec
   - Review the generated code for any issues

3. **Workflow permissions**
   - Verify `GITHUB_TOKEN` has sufficient permissions
   - For repository dispatch, ensure `CLIENT_UPDATE_TOKEN` is properly configured

4. **Rate limiting**
   - GitHub API has rate limits; adjust the schedule if needed
   - Use authentication tokens to increase rate limits

### Debugging

Enable detailed logging by adding:

```yaml
- name: Debug information
  run: |
    echo "API Repository: $API_REPOSITORY"
    echo "Latest tag: ${{ needs.check-api-updates.outputs.latest-tag }}"
    echo "Spec URL: ${{ steps.spec-url.outputs.spec-url }}"
```

### Testing

Test workflows manually:

```bash
# Test external API tracking
gh workflow run api-update.yml -f api_repository="owner/repo" -f force_update=true

# Test repository dispatch
gh api repos/OWNER/REPO/dispatches \
  --method POST \
  --field event_type=api-updated \
  --field client_payload='{"api_version":"v1.2.3"}'
```

## Best Practices

1. **Review PRs carefully** - Generated code changes can introduce breaking changes
2. **Test thoroughly** - Always test your application after API updates
3. **Monitor failures** - Set up notifications for workflow failures
4. **Version pinning** - Consider pinning specific API versions for stability
5. **Documentation** - Document any manual steps required after updates

---

These workflows provide automated API change tracking while maintaining control over when changes are integrated into your application. Customize them based on your specific repository structure and requirements.