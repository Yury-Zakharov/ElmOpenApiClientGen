# Multi-Language OpenAPI Client Generator

[![Build Status](https://github.com/Yury-Zakharov/ElmOpenApiClientGen/workflows/CI/badge.svg)](https://github.com/Yury-Zakharov/ElmOpenApiClientGen/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![F# Version](https://img.shields.io/badge/F%23-10.0-blue.svg)](https://fsharp.org/)
[![Languages](https://img.shields.io/badge/Targets-Elm%20%7C%20C%23%20%7C%20Haskell-green.svg)](#)
[![Tests](https://img.shields.io/badge/Tests-171%20Passing-brightgreen.svg)](#)

## 💖 Support This Project

If you find this Multi-Language OpenAPI Client Generator useful, please consider supporting it through [GitHub Sponsors](https://github.com/sponsors/Yury-Zakharov).  
Your sponsorship helps cover development time, maintenance, and new feature work.

[Become a sponsor](https://github.com/sponsors/Yury-Zakharov)

A robust, production-ready multi-language code generator that transforms OpenAPI 3.0/3.1 specifications into type-safe HTTP clients. Built with F# and designed for reliability, this tool handles real-world API specifications gracefully while generating clean, idiomatic code for **Elm**, **Haskell**, and **C#**.

## ✨ Key Features

### 🛡️ **Production-Ready Reliability**
- **Never crashes on invalid input** - Graceful error handling with human-readable diagnostic comments
- **Comprehensive defensive programming** - Handles malformed OpenAPI specs, null values, and edge cases
- **Extensive test coverage** - 171 tests covering positive cases, negative cases, and integration scenarios

### 🎯 **Complete OpenAPI Support**
- **OpenAPI 3.0 & 3.1** - Full specification support including JSON Schema draft 2020-12
- **Flexible input sources** - Local files (YAML/JSON) and remote URLs with automatic format detection
- **Advanced schema features** - Recursive types, conditional schemas, discriminators, and pattern properties
- **Security schemes** - API keys, Bearer tokens, Basic auth, and custom authentication
- **Rich type system** - Union types, optional fields, arrays, objects, and custom types

### 🚀 **Developer Experience**
- **Type-safe HTTP clients** - Compile-time guarantees for API interactions  
- **Automatic error handling** - Structured error types for different HTTP status codes
- **Documentation generation** - Inline documentation from OpenAPI descriptions
- **Ecosystem integration** - elm-format, elm-review, GitHub Actions, and editor support

### ⚡ **Performance & Optimization**
- **Tree-shaking support** - Generate only the code you need
- **Lazy loading patterns** - Efficient resource usage
- **Deduplication** - Eliminates redundant type definitions
- **Optimized output** - Clean, minimal generated code

## 🚀 Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- **Target Language Runtime** (for the language you're generating):
  - [Elm 0.19.1](https://guide.elm-lang.org/install/elm.html) or later for Elm clients
  - [GHC 9.0+](https://www.haskell.org/ghc/) for Haskell clients  
  - [.NET 10.0+](https://dotnet.microsoft.com/download/dotnet/10.0) for C# clients

### Installation

```bash
# Clone the repository
git clone https://github.com/Yury-Zakharov/ElmOpenApiClientGen.git
cd ElmOpenApiClientGen

# Build the project
dotnet build

# Run tests to verify installation
dotnet test
```

### Basic Usage

```bash
# Generate Elm client from local OpenAPI file
dotnet run --project src/ElmOpenApiClientGen \
  --input api-spec.yaml \
  --output ./src/Generated \
  --target elm \
  --moduleprefix Api \
  --force

# Generate C# client from remote OpenAPI URL
dotnet run --project src/ElmOpenApiClientGen \
  --input https://api.example.com/openapi.yaml \
  --output ./src/Generated \
  --target csharp \
  --moduleprefix MyApi \
  --force

# Generate Haskell client
dotnet run --project src/ElmOpenApiClientGen \
  --input api-spec.yaml \
  --output ./src/Generated \
  --target haskell \
  --moduleprefix MyApi \
  --force
```

### 🐳 Docker Usage

For containerized environments or CI/CD pipelines:

```bash
# Build the Docker image
docker build -t elm-openapi-gen .

# Generate from remote URL
docker run --rm -v $(pwd)/output:/output elm-openapi-gen \
  --input https://petstore.swagger.io/v2/swagger.json \
  --moduleprefix PetStore --force

# Generate from local file
docker run --rm \
  -v $(pwd)/specs:/input:ro \
  -v $(pwd)/output:/output \
  elm-openapi-gen \
  --input /input/openapi.yaml \
  --moduleprefix MyApi --force

# Using Docker Compose
docker-compose -f docker-compose.production.yml run --rm generate-petstore
```

📋 **[Complete Docker Usage Guide](DOCKER_USAGE.md)** - Comprehensive Docker documentation including:
- Production deployment patterns
- CI/CD integration examples  
- Docker Compose configurations
- Troubleshooting and debugging
- Security considerations

This generates language-specific HTTP clients:

**Elm Output:**
- **Type definitions** in `Generated/Api/Schemas.elm`
- **HTTP client functions** with full type safety
- **JSON encoders/decoders** for all data types
- **Error types** for structured error handling

**C# Output:**
- **ApiClient class** with async HTTP methods
- **Record types** for data models  
- **Result<T,E> pattern** for error handling
- **Nullable reference types** for optional fields

**Haskell Output:**
- **Data types** with automatic JSON instances
- **HTTP client functions** using Servant or similar
- **Type-safe error handling** with Either types
- **Lens support** for data access

## 🌐 URL Input Support

**NEW in v2.1.0**: ElmOpenApiClientGen now supports downloading OpenAPI specifications directly from URLs:

### Remote OpenAPI Specifications

```bash
# Popular API examples (defaults to Elm)
dotnet run --input https://petstore.swagger.io/v2/swagger.json --output ./petstore
dotnet run --input https://api.apis.guru/v2/specs/github.com/1.1.4/openapi.yaml --output ./github --target csharp

# Corporate APIs with different targets
dotnet run --input https://api.company.com/v1/openapi.yaml --output ./company-api --target haskell

# Development servers
dotnet run --input http://localhost:8080/openapi.json --output ./local-api --target elm
```

### Features

- ✅ **Automatic Format Detection** - YAML and JSON support based on URL extension or content
- ✅ **Robust Error Handling** - Clear error messages for network issues, timeouts, and HTTP errors
- ✅ **Security** - 30-second timeout, proper User-Agent headers, HTTPS support
- ✅ **No Dependencies** - Uses built-in .NET HTTP client, no additional installations needed

### URL Input Benefits

1. **Live API Development** - Generate clients directly from your running API server
2. **CI/CD Integration** - Download specs from version control or API gateways
3. **API Discovery** - Work with publicly available API specifications
4. **Team Collaboration** - Share specs via URLs instead of file distribution

## 📖 Detailed Usage

### Command Line Options

```bash
dotnet run --project src/ElmOpenApiClientGen [OPTIONS]

Options:
  --input <path>          Path to OpenAPI spec file (JSON or YAML) or URL to download spec
  --output <directory>    Output directory for generated files
  --target <language>     Target language: elm (default), csharp, haskell
  --moduleprefix <name>   Module name prefix (default: Api)
  --force                 Overwrite existing files
```

### Input Source Examples

The `--input` parameter accepts both local files and remote URLs:

```bash
# Local YAML file
--input ./openapi.yaml
--input /path/to/spec.yml

# Local JSON file  
--input ./openapi.json
--input /path/to/spec.json

# Remote YAML URL
--input https://api.example.com/openapi.yaml
--input https://petstore.swagger.io/v2/swagger.yaml

# Remote JSON URL
--input https://api.example.com/openapi.json
--input https://petstore.swagger.io/v2/swagger.json
```

**Automatic Format Detection**: The tool automatically detects whether the input is YAML or JSON based on:
- File extension (`.yaml`, `.yml`, `.json`)
- Content analysis for URLs
- HTTP Content-Type headers

### Error Handling for URL Input

When using URL input, the tool provides robust error handling:

```bash
# Network errors are handled gracefully
$ dotnet run --input https://invalid-domain.example/openapi.yaml --output ./out
Error: Failed to download from URL https://invalid-domain.example/openapi.yaml: Network error when downloading from URL https://invalid-domain.example/openapi.yaml: No such host is known

# HTTP errors provide clear feedback
$ dotnet run --input https://api.example.com/missing.yaml --output ./out  
Error: HTTP error NotFound (Not Found) when downloading from URL: https://api.example.com/missing.yaml

# Timeout errors are reported clearly
$ dotnet run --input https://slow-api.example.com/openapi.yaml --output ./out
Error: Request timeout (30 seconds) when downloading from URL: https://slow-api.example.com/openapi.yaml
```

**Defensive Programming**: The tool never crashes on network issues - it always provides informative error messages to help diagnose and fix input problems.

## 🏗️ Generated Code Structure

### Type Definitions

```elm
-- Generated/Api/Schemas.elm
module Api.Schemas exposing (..)

import Json.Decode as Decode
import Json.Encode as Encode
import Http
import Task exposing (Task)

{-| User data from the API -}
type alias User =
    { id : Int
    , name : String
    , email : Maybe String
    , createdAt : String
    }

{-| JSON decoder for User -}
decoderUser : Decode.Decoder User
decoderUser =
    Decode.map4 User
        (Decode.field \"id\" Decode.int)
        (Decode.field \"name\" Decode.string)
        (Decode.maybe (Decode.field \"email\" Decode.string))
        (Decode.field \"created_at\" Decode.string)
```

### HTTP Client Functions

```elm
{-| Get user by ID -}
getUser : Config -> Int -> Task GetUserError User
getUser config userId =
    Http.task
        { method = \"GET\"
        , headers = [ Http.header \"Authorization\" (\"Bearer \" ++ config.bearerToken) ]
        , url = config.baseUrl ++ \"/users/\" ++ String.fromInt userId
        , body = Http.emptyBody
        , resolver = Http.stringResolver handleGetUserResponse
        , timeout = config.timeout
        }

{-| Error types for getUser operation -}
type GetUserError
    = GetUserError404 String  -- User not found
    | GetUserError401 String  -- Unauthorized
    | GetUserErrorUnknown String
```

### Usage in Your Elm App

```elm
import Api.Schemas as Api
import Task

-- Configuration
config : Api.Config
config =
    { baseUrl = \"https://api.example.com\"
    , bearerToken = \"your-token-here\"
    , customHeaders = []
    , timeout = Just 30000
    }

-- Using the generated client
fetchUser : Int -> Cmd Msg
fetchUser userId =
    Api.getUser config userId
        |> Task.attempt UserFetched

-- Handle the result
type Msg
    = UserFetched (Result Api.GetUserError Api.User)

update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        UserFetched (Ok user) ->
            ( { model | user = Just user }, Cmd.none )
        
        UserFetched (Err (Api.GetUserError404 _)) ->
            ( { model | error = Just \"User not found\" }, Cmd.none )
        
        UserFetched (Err (Api.GetUserError401 _)) ->
            ( { model | error = Just \"Unauthorized\" }, Cmd.none )
```

## 🌍 Multi-Language Support

### C# Generated Code

```csharp
// Generated/MyApiClient.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyApi
{
    /// <summary>
    /// User data from the API
    /// </summary>
    public record User(
        public int Id { get; init; }
        public string Name { get; init; }
        public string? Email { get; init; }
        public DateTime CreatedAt { get; init; }
    );

    /// <summary>
    /// Generated API client for MyApi
    /// </summary>
    public class MyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MyApiClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<Result<User, ApiError>> GetUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/users/{userId}");
                return await ExecuteAsync<User>(request, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<User, ApiError>.Failure(new ApiError($"Failed to execute GetUser: {ex.Message}"));
            }
        }
    }
}
```

### Haskell Generated Code

```haskell
-- Generated/MyApi/Client.hs
{-# LANGUAGE DataKinds #-}
{-# LANGUAGE DeriveGeneric #-}
{-# LANGUAGE TypeOperators #-}

module MyApi.Client where

import Data.Aeson
import Data.Time
import GHC.Generics
import Network.HTTP.Client (Manager)
import Servant.API
import Servant.Client

-- | User data from the API
data User = User
  { userId :: Int
  , userName :: String
  , userEmail :: Maybe String
  , userCreatedAt :: UTCTime
  } deriving (Show, Eq, Generic)

instance FromJSON User where
  parseJSON = withObject "User" $ \o -> User
    <$> o .: "id"
    <*> o .: "name"
    <*> o .:? "email"
    <*> o .: "created_at"

instance ToJSON User where
  toJSON User{..} = object
    [ "id" .= userId
    , "name" .= userName
    , "email" .= userEmail
    , "created_at" .= userCreatedAt
    ]

-- | API definition
type MyApiAPI = "users" :> Capture "id" Int :> Get '[JSON] User

-- | Generated client functions
getUserById :: Int -> ClientM User
getUserById = client (Proxy :: Proxy MyApiAPI)

-- | Run client with configuration
runMyApiClient :: Manager -> BaseUrl -> ClientM a -> IO (Either ClientError a)
runMyApiClient manager baseUrl = runClientM
```

### Usage Examples by Language

**C# Usage:**
```csharp
// Program.cs
var httpClient = new HttpClient();
var client = new MyApiClient(httpClient, "https://api.example.com");

try 
{
    var result = await client.GetUserAsync(123);
    switch (result)
    {
        case Result<User, ApiError>.Success success:
            Console.WriteLine($"User: {success.Value.Name}");
            break;
        case Result<User, ApiError>.Failure failure:
            Console.WriteLine($"Error: {failure.Error.Message}");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
}
```

**Haskell Usage:**
```haskell
-- Main.hs
import MyApi.Client
import Network.HTTP.Client (newManager, defaultManagerSettings)
import Servant.Client

main :: IO ()
main = do
  manager <- newManager defaultManagerSettings
  let baseUrl = BaseUrl Https "api.example.com" 443 ""
  
  result <- runMyApiClient manager baseUrl (getUserById 123)
  case result of
    Right user -> putStrLn $ "User: " ++ userName user
    Left err -> putStrLn $ "Error: " ++ show err
```

## 🧪 Testing

The project includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test categories
dotnet test --filter \"RequestGeneratorTests\"
dotnet test --filter \"CodegenTests\"
dotnet test --filter \"IntegrationTests\"

# Run integration tests (includes URL input testing)
cd integration-tests
./run-integration-test.sh

# Run containerized integration tests
./run-containerized-test.sh
```

### Test Categories

- **Unit Tests** (171 tests) - Individual component testing with comprehensive coverage for all language targets
- **Integration Tests** (Host-based) - End-to-end pipeline testing with file and URL inputs for Elm, C#, and Haskell
- **Containerized Integration Tests** - Complete workflow validation in isolated containers
- **Negative Tests** - Error handling, network failures, and edge cases across all languages
- **URL Input Tests** - HTTP download validation for both YAML and JSON endpoints
- **Language-Specific Tests** - Target-specific code generation, validation, and defensive programming

### Integration Test Features

The integration tests validate all input scenarios:

| Input Type | Format | Test Coverage |
|------------|--------|---------------|
| **Local File** | YAML | ✅ File system access |
| **Local File** | JSON | ✅ File system access |
| **Remote URL** | YAML | ✅ HTTP download + parsing |
| **Remote URL** | JSON | ✅ HTTP download + parsing |

**Containerized Testing**: Complete Docker-based test environment with no host dependencies:
- OpenAPI service container serving specs
- Code generation containers with URL support  
- Elm compilation validation containers
- Automatic cleanup and health checks

## 🏛️ Architecture

### Core Components

```
src/ElmOpenApiClientGen/
├── Generator/
│   ├── RequestGenerator.fs    # HTTP client generation
│   ├── Codegen.fs            # Main code generation logic
│   └── TemplateRenderer.fs   # Template processing
├── Languages/                # Multi-language support
│   ├── ILanguageTarget.fs    # Common interface for all targets
│   ├── Elm/                  # Elm language target
│   ├── Haskell/              # Haskell language target
│   ├── CSharp/               # C# language target
│   └── GeneratorFactory.fs   # Language target factory
├── Models/                   # Data models and types
├── Utils/                    # Utility functions
└── Program.fs               # CLI entry point
```

### Generation Pipeline

1. **Input** - Accept local file or download from URL (YAML/JSON)
2. **Parse** OpenAPI specification with automatic format detection
3. **Validate** schema and resolve references
4. **Target Selection** - Choose language target (Elm, C#, or Haskell)
5. **Generate** type definitions from schemas using language-specific mapping
6. **Create** HTTP client functions for endpoints with appropriate patterns
7. **Render** using Scriban templates customized for each language
8. **Output** clean, type-safe, idiomatic code in the target language

## 🔧 Advanced Features

### Phantom Types for Type Safety

```elm
{-| Validated user data -}
type ValidatedUser = ValidatedUser User

{-| Authenticate user and return validated data -}
authenticateUser : Config -> User -> Task AuthError ValidatedUser
```

### Middleware Support

```elm
{-| Add request middleware -}
withLogging : (String -> Cmd msg) -> Config -> Config
withRateLimit : Int -> Config -> Config
withRetry : Int -> Config -> Config
```

### Custom Error Handling

```elm
{-| Custom error handling with retry logic -}
withErrorRecovery : (HttpError -> Task HttpError a) -> Task HttpError a -> Task HttpError a
```

## 📦 Ecosystem Integration

### GitHub Actions

The generator can create a complete CI/CD setup:

```yaml
# Generated .github/workflows/ci.yml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: jorelali/setup-elm@v5
      - run: elm-format --validate src/
      - run: elm-review
      - run: elm-test
```

### API Change Tracking

For production applications that need to stay synchronized with API changes, we provide comprehensive GitHub Actions workflows that automatically track OpenAPI specification updates and generate pull requests with updated client code.

📋 **[Complete GitHub Workflows Guide](GITHUB_WORKFLOWS.md)** - Detailed instructions for:

- **External API Tracking** - Monitor third-party API repositories for changes
- **Coordinated Updates** - Synchronize updates when you own both API and client repositories  
- **Automated Pull Requests** - Generate PRs with updated Elm client code
- **Validation Pipelines** - Ensure generated code compiles and passes tests
- **Multiple Repository Management** - Handle updates across multiple client applications

**Quick Example**: Automatically update your Elm app when an external API releases new versions:

```yaml
# .github/workflows/api-update.yml
on:
  schedule:
    - cron: '0 2 * * *'  # Check daily at 2 AM
jobs:
  update-client:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Generate updated client
        run: |
          dotnet run --project elm-codegen \
            --input https://api.example.com/openapi.yaml \
            --output ./src/Generated \
            --moduleprefix Api
      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v5
        # ... configuration
```

### Editor Support

Generated files include:
- **VSCode settings** for Elm development
- **elm.json** with proper dependencies
- **elm-review** configuration
- **elm-format** rules

## 🐛 Error Handling Philosophy

This generator follows a **defensive programming** approach:

- **Never crashes** on invalid input
- **Always generates** some output, even for malformed specs
- **Provides diagnostics** through comments in generated code
- **Graceful degradation** when encountering unsupported features

Example of graceful error handling:

```elm
-- ERROR: Invalid status code 'invalid_status' in OpenAPI spec
-- Generated fallback response handling
-- Please check your OpenAPI specification at path /users/{id} GET operation
getUserById : Config -> Int -> Task Http.Error (Result String User)
```

## 🤝 Contributing

We welcome contributions from the community! Whether you're fixing bugs, adding features, improving documentation, or providing feedback, your contributions are valuable.

### Quick Start for Contributors

1. **Fork the repository** on GitHub
2. **Create a feature branch** from `main`
3. **Make your changes** following our coding standards
4. **Run tests** to ensure everything works (`dotnet test`)
5. **Submit a pull request** with a clear description

### Contribution Guidelines

Please read our [Contributing Guide](CONTRIBUTING.md) for detailed information on:

- 🐛 **Bug reporting** and feature requests
- 💻 **Development setup** and workflow  
- 🧪 **Testing requirements** and guidelines
- 📝 **Documentation standards**
- 🔍 **Code review process**
- 🛡️ **Defensive programming** principles

### What We're Looking For

- **Bug fixes** for existing issues
- **New features** that align with project goals
- **Documentation improvements** 
- **Test coverage** enhancements
- **Performance optimizations**
- **Real-world usage examples**

All pull requests require approval from [@Yury-Zakharov](https://github.com/Yury-Zakharov) or assigned maintainers before merging.

### Development Setup

```bash
# Clone and setup
git clone https://github.com/Yury-Zakharov/ElmOpenApiClientGen.git
cd ElmOpenApiClientGen

# Install dependencies
dotnet restore

# Run tests
dotnet test

# Build
dotnet build
```

### Running Examples

```bash
# Generate Elm client from local files
dotnet run --project src/ElmOpenApiClientGen \
  --input sample/openapi.yaml \
  --output examples/generated \
  --target elm \
  --moduleprefix PetStore

# Generate C# client from remote URLs  
dotnet run --project src/ElmOpenApiClientGen \
  --input https://petstore.swagger.io/v2/swagger.json \
  --output examples/petstore-client \
  --target csharp \
  --moduleprefix PetStore

# Generate Haskell client from GitHub API
dotnet run --project src/ElmOpenApiClientGen \
  --input https://api.github.com/openapi.json \
  --output examples/github-client \
  --target haskell \
  --moduleprefix GitHub
```

## 📋 Roadmap

### Upcoming Features

**Language Targets:**
- [ ] **TypeScript** - Modern TypeScript with fetch API and async/await
- [ ] **Rust** - Type-safe HTTP clients with reqwest and serde
- [ ] **Go** - Idiomatic Go clients with net/http
- [ ] **Python** - Modern Python with type hints and aiohttp
- [ ] **Java** - Spring WebClient and Jackson integration

**Core Features:**
- [ ] **Authentication support for URLs** - API keys and credentials for private OpenAPI specs
- [ ] **OpenAPI 3.2** support with new JSON Schema features
- [ ] **GraphQL** integration for hybrid APIs  
- [ ] **Real-time** WebSocket client generation
- [ ] **Mock server** generation for testing
- [ ] **Performance benchmarks** and optimization
- [ ] **Plugin system** for custom generators

### Version History

- **v3.0.0** - **Multi-language support** with C# and Haskell code generation alongside Elm
- **v2.1.0** - URL input support for remote OpenAPI specifications
- **v2.0.0** - Production-ready with defensive programming and comprehensive testing
- **v1.5.0** - OpenAPI 3.1 support and advanced features
- **v1.0.0** - Initial release with basic OpenAPI 3.0 support

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [OpenAPI Initiative](https://www.openapis.org/) for the specification
- [Elm Community](https://elm-lang.org/community) for inspiration
- [F# Software Foundation](https://fsharp.org/) for the language
- All contributors who have helped improve this project

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/Yury-Zakharov/ElmOpenApiClientGen/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Yury-Zakharov/ElmOpenApiClientGen/discussions)
- **Documentation**: [Wiki](https://github.com/Yury-Zakharov/ElmOpenApiClientGen/wiki)

---

Made with ❤️ by the Elm OpenAPI Generator team