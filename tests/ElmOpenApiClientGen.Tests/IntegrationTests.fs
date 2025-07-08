module IntegrationTests

open System
open System.IO
open Microsoft.OpenApi.Readers
open Xunit
open ElmOpenApiClientGen.Generator.RequestGenerator
open ElmOpenApiClientGen.Generator.Codegen

let testOpenApiSpec = """
openapi: 3.0.3
info:
  title: Integration Test API
  description: API for integration testing
  version: 1.0.0
servers:
  - url: https://api.example.com/v1
security:
  - bearerAuth: []
paths:
  /users/{userId}:
    get:
      summary: Get user by ID
      operationId: getUserById
      parameters:
        - name: userId
          in: path
          required: true
          schema:
            type: integer
            minimum: 1
        - name: includeProfile
          in: query
          required: false
          schema:
            type: boolean
      responses:
        '200':
          description: User found
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/User'
        '404':
          description: User not found
  /users:
    post:
      summary: Create user
      operationId: createUser
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UserInput'
      responses:
        '201':
          description: User created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/User'
        '400':
          description: Invalid input

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
      
  schemas:
    User:
      type: object
      required:
        - id
        - email
        - username
      properties:
        id:
          type: integer
          minimum: 1
        email:
          type: string
          format: email
        username:
          type: string
          minLength: 3
          maxLength: 50
        profile:
          $ref: '#/components/schemas/UserProfile'
        
    UserProfile:
      type: object
      properties:
        firstName:
          type: string
          maxLength: 100
        lastName:
          type: string
          maxLength: 100
        bio:
          type: string
          maxLength: 500
          
    UserInput:
      type: object
      required:
        - email
        - username
      properties:
        email:
          type: string
          format: email
        username:
          type: string
          minLength: 3
          maxLength: 50
        password:
          type: string
          minLength: 8
"""

[<Fact>]
let ``Full generation pipeline should produce valid requests`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act - Generate requests
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should generate requests and error types
    Assert.NotEmpty(requests)
    Assert.NotEmpty(errorTypes)
    
    // Verify request signatures contain expected elements
    let requestSignatures = requests |> List.map fst
    Assert.True(requestSignatures |> List.exists _.Contains("getUserById"))
    Assert.True(requestSignatures |> List.exists _.Contains("createUser"))
    Assert.True(requestSignatures |> List.exists _.Contains("Config"))
    Assert.True(requestSignatures |> List.exists _.Contains("Task"))

[<Fact>]
let ``Generated requests should include authentication`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Generated code should include authentication
    let requestDefinitions = requests |> List.map snd |> String.concat "\n"
    Assert.Contains("Authorization", requestDefinitions)
    Assert.Contains("Bearer", requestDefinitions)
    Assert.Contains("config.bearerToken", requestDefinitions)

[<Fact>]
let ``Generated requests should handle path parameters`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle path parameters
    let getUserRequest = requests |> List.find (fun (signature, _) -> signature.Contains("getUserById"))
    let signature, definition = getUserRequest
    
    Assert.Contains("Int", signature)  // userId parameter
    Assert.Contains("Bool", signature)  // includeProfile parameter
    Assert.Contains("String.fromInt", definition)
    Assert.Contains("userId", definition)

[<Fact>]
let ``Generated requests should handle request bodies`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle request bodies
    let createUserRequest = requests |> List.find (fun (signature, _) -> signature.Contains("createUser"))
    let signature, definition = createUserRequest
    
    Assert.Contains("UserInput", signature)
    Assert.Contains("POST", definition)
    Assert.Contains("Http.jsonBody", definition)
    Assert.Contains("encodeUserInput", definition)

[<Fact>]
let ``Generated error types should be properly formatted`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Error types should be generated
    Assert.NotEmpty(errorTypes)
    
    let errorTypesText = String.concat "\n" errorTypes
    Assert.Contains("GetUserByIdError", errorTypesText)
    Assert.Contains("GetUserByIdError404", errorTypesText)
    Assert.Contains("GetUserByIdErrorUnknown", errorTypesText)

[<Fact>]
let ``Ecosystem integration should work with generated content`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    let ciConfig = {
        GenerateGitHubActions = true
        GenerateElmReview = true
        GenerateElmTest = true
        GenerateElmMake = true
    }
    
    // Act
    let ecosystemFiles = generateEcosystemIntegration doc editorConfig ciConfig FormattingOption.Standard
    
    // Assert
    Assert.NotEmpty(ecosystemFiles)
    
    let fileNames = ecosystemFiles |> List.map fst
    Assert.Contains("elm.json", fileNames)
    Assert.Contains(".github/workflows/ci.yml", fileNames)
    
    // Check elm.json content
    let _, elmJsonContent = ecosystemFiles |> List.find (fun (name, _) -> name = "elm.json")
    Assert.Contains("integrationtestapi", elmJsonContent)
    Assert.Contains("Api.Schemas", elmJsonContent)

[<Fact>]
let ``Advanced type system features should work with integration`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act - Use advanced request generation
    let requests, errorTypes, advancedFeatures = generateRequestsWithAdvancedFeatures doc
    
    // Assert
    Assert.NotEmpty(requests)
    Assert.NotEmpty(errorTypes)
    
    // Verify advanced features are integrated
    let requestDefinitions = requests |> List.map snd |> String.concat "\n"
    Assert.Contains("config.baseUrl", requestDefinitions)
    Assert.Contains("config.timeout", requestDefinitions)
    Assert.Contains("config.customHeaders", requestDefinitions)

[<Fact>]
let ``Generated code should include proper function signatures`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Verify function signatures
    let allRequests = requests |> List.map fst |> String.concat "\n"
    Assert.Contains("getUserById", allRequests)
    Assert.Contains("createUser", allRequests)
    Assert.Contains("Config", allRequests)
    Assert.Contains("Task", allRequests)
    Assert.Contains("User", allRequests)

[<Fact>]
let ``Generated code should handle complex schemas properly`` () =
    // Arrange
    let complexOpenApiSpec = testOpenApiSpec.Replace(
        "username:",
        """tags:
          type: array
          items:
            type: string
        metadata:
          type: object
          additionalProperties:
            type: string
        created:
          type: string
          format: date-time
        username:""")
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(complexOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should generate requests without errors
    Assert.NotEmpty(requests)
    let requestDefinitions = requests |> List.map snd |> String.concat "\n"
    Assert.Contains("Http.task", requestDefinitions)
    Assert.Contains("config.baseUrl", requestDefinitions)

[<Fact>]
let ``OpenAPI parsing should succeed without errors`` () =
    // Arrange & Act
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Assert - Document should be parsed successfully
    Assert.NotNull(doc)
    Assert.Equal("Integration Test API", doc.Info.Title)
    Assert.Equal("1.0.0", doc.Info.Version)
    Assert.Equal("API for integration testing", doc.Info.Description)
    Assert.NotEmpty(doc.Paths)
    Assert.True(doc.Paths.ContainsKey("/users/{userId}"))
    Assert.True(doc.Paths.ContainsKey("/users"))

[<Fact>]
let ``Generated requests should include timeout and custom headers`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - All requests should include timeout and custom headers support
    let allDefinitions = requests |> List.map snd |> String.concat "\n"
    Assert.Contains("timeout = config.timeout", allDefinitions)
    Assert.Contains("config.customHeaders", allDefinitions)
    Assert.Contains("List.map", allDefinitions)  // For custom headers processing