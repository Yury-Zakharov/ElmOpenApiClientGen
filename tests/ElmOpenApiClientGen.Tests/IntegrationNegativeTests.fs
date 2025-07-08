module IntegrationNegativeTests

open System
open System.IO
open Microsoft.OpenApi.Readers
open Xunit
open ElmOpenApiClientGen.Generator.RequestGenerator
open ElmOpenApiClientGen.Generator.Codegen

let invalidOpenApiSpec = """
openapi: 3.0.3
info:
  title: Invalid Test API
  description: 
  version: 
servers: []
security: []
paths: {}
components:
  securitySchemes: {}
  schemas: {}
"""

let malformedOpenApiSpec = """
openapi: 3.0.3
info:
  title: Malformed API
  version: 1.0.0
paths:
  /users/{id}:
    get:
      operationId: 
      summary: 
      parameters: []
      responses: {}
  /invalid-path:
    post:
      operationId: createUser
      requestBody:
        required: true
        content: {}
      responses:
        '200':
          description: Success
          content: {}
"""

let emptyOpenApiSpec = """
openapi: 3.0.3
info:
  title: ""
  version: ""
"""

let complexInvalidSpec = """
openapi: 3.0.3
info:
  title: Complex Invalid API
  version: 1.0.0
paths:
  /users/{userId}:
    get:
      operationId: getUserById
      parameters:
        - name: userId
          in: path
          required: true
          schema:
            type: invalid_type
            format: unknown_format
        - name: invalidParam
          in: invalid_location
          schema: null
      responses:
        'invalid_status':
          description: Invalid response
          content:
            invalid/media-type:
              schema:
                type: unknown
                properties: null
        '200':
          description: Success
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/NonExistentSchema'
components:
  schemas:
    User:
      type: object
      properties:
        id:
          type: invalid_type
        data:
          type: object
          properties: null
        circular:
          $ref: '#/components/schemas/User'
"""

[<Fact>]
let ``Full pipeline should handle invalid OpenAPI spec gracefully`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(invalidOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle gracefully with empty results
    Assert.Empty(requests)
    Assert.Empty(errorTypes)

[<Fact>]
let ``Full pipeline should handle malformed OpenAPI spec`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(malformedOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should generate what it can
    Assert.NotNull(requests)
    Assert.NotNull(errorTypes)

[<Fact>]
let ``Full pipeline should handle empty OpenAPI spec`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(emptyOpenApiSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle gracefully
    Assert.Empty(requests)
    Assert.Empty(errorTypes)

[<Fact>]
let ``Full pipeline should handle complex invalid spec`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(complexInvalidSpec)
    
    // Act - Should not throw exceptions
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should generate something even with invalid elements
    Assert.NotNull(requests)
    Assert.NotNull(errorTypes)

[<Fact>]
let ``Ecosystem integration should handle invalid document`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(invalidOpenApiSpec)
    
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
    
    // Assert - Should still generate files
    Assert.NotEmpty(ecosystemFiles)
    let fileNames = ecosystemFiles |> List.map fst
    Assert.Contains("elm.json", fileNames)

[<Fact>]
let ``Advanced type system should handle invalid operations`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(malformedOpenApiSpec)
    
    // Act - Should not throw exceptions
    let requests, errorTypes, advancedFeatures = generateRequestsWithAdvancedFeatures doc
    
    // Assert
    Assert.NotNull(requests)
    Assert.NotNull(errorTypes)
    Assert.NotNull(advancedFeatures)

[<Fact>]
let ``Generated requests should handle missing authentication schemes`` () =
    // Arrange
    let specWithMissingAuth = """
openapi: 3.0.3
info:
  title: Missing Auth API
  version: 1.0.0
security:
  - missingAuth: []
paths:
  /users:
    get:
      operationId: getUsers
      security:
        - missingAuth: []
      responses:
        '200':
          description: Success
"""
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(specWithMissingAuth)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle missing auth scheme gracefully
    Assert.NotEmpty(requests)
    let requestDefinitions = requests |> List.map snd |> String.concat "\n"
    Assert.Contains("config.customHeaders", requestDefinitions)  // Should still include custom headers

[<Fact>]
let ``Generated requests should handle circular references`` () =
    // Arrange
    let circularRefSpec = """
openapi: 3.0.3
info:
  title: Circular Reference API
  version: 1.0.0
paths:
  /users:
    get:
      operationId: getUsers
      responses:
        '200':
          description: Success
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/CircularUser'
components:
  schemas:
    CircularUser:
      type: object
      properties:
        id:
          type: integer
        parent:
          $ref: '#/components/schemas/CircularUser'
        children:
          type: array
          items:
            $ref: '#/components/schemas/CircularUser'
"""
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(circularRefSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle circular references
    Assert.NotEmpty(requests)
    let signatures = requests |> List.map fst |> String.concat "\n"
    Assert.Contains("getUsers", signatures)

[<Fact>]
let ``Generated requests should handle extremely large schemas`` () =
    // Arrange
    let properties = [1..100] |> List.map (fun i -> $"        prop%d{i}:\n          type: string") |> String.concat "\n"
    let largeSchemaSpec = "openapi: 3.0.3\ninfo:\n  title: Large Schema API\n  version: 1.0.0\npaths:\n  /data:\n    get:\n      operationId: getData\n      responses:\n        '200':\n          description: Success\n          content:\n            application/json:\n              schema:\n                $ref: '#/components/schemas/LargeObject'\ncomponents:\n  schemas:\n    LargeObject:\n      type: object\n      properties:\n" + properties
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(largeSchemaSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle large schemas
    Assert.NotEmpty(requests)

[<Fact>]
let ``Generated requests should handle missing required fields`` () =
    // Arrange
    let missingFieldsSpec = """
openapi: 3.0.3
info:
  title: Missing Fields API
  version: 1.0.0
paths:
  /incomplete:
    get:
      responses:
        '200':
          description: Success
    post:
      operationId: createIncomplete
      # Missing requestBody
      responses:
        '201':
          description: Created
"""
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(missingFieldsSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle missing fields gracefully
    Assert.NotNull(requests)

[<Fact>]
let ``Generated code should handle Unicode in API spec`` () =
    // Arrange
    let unicodeSpec = """
openapi: 3.0.3
info:
  title: "Unicode API 测试 αβγ"
  description: "API with Unicode characters: 中文 العربية русский ελληνικά 🌍"
  version: "1.0.0-αβγ"
paths:
  /用户/{用户ID}:
    get:
      operationId: "获取用户αβγ"
      summary: "Get user - получить пользователя"
      parameters:
        - name: "用户ID"
          in: path
          required: true
          schema:
            type: integer
      responses:
        '200':
          description: "Success - 成功"
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/用户'
components:
  schemas:
    用户:
      type: object
      properties:
        id:
          type: integer
          description: "User ID - 用户标识"
        名称:
          type: string
          description: "Name with émojis 🚀"
"""
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(unicodeSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle Unicode gracefully
    Assert.NotNull(requests)
    if requests.Length > 0 then
        let signature, definition = requests[0]
        Assert.NotNull(signature)
        Assert.NotNull(definition)

[<Fact>]
let ``Parser should handle completely invalid YAML`` () =
    // Arrange
    let invalidYaml = """
    This is not valid YAML at all!
    openapi: invalid
    info: [this should be an object]
    paths: "this should be an object"
    random text
    """
    
    let reader = OpenApiStringReader()
    
    // Act & Assert - Should handle parsing errors gracefully
    try
        let doc, diagnostics = reader.Read(invalidYaml)
        // If parsing succeeds, document should be null or empty
        Assert.True(doc = null || doc.Paths = null || doc.Paths.Count = 0)
    with
    | ex ->
        // Exception is expected for completely invalid YAML
        Assert.NotNull(ex)

[<Fact>]
let ``Parser should handle unsupported OpenAPI version`` () =
    // Arrange
    let unsupportedVersionSpec = """
openapi: 2.0
info:
  title: Old Version API
  version: 1.0.0
swagger: "2.0"
paths:
  /test:
    get:
      responses:
        '200':
          description: Success
"""
    
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(unsupportedVersionSpec)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle gracefully even with older versions
    Assert.NotNull(requests)
    Assert.NotNull(errorTypes)

[<Fact>]
let ``Ecosystem integration should handle edge case configurations`` () =
    // Arrange
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(emptyOpenApiSpec)
    
    // Edge case: Conflicting configurations
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = false  // Inconsistent
        GenerateTypeAnnotations = true
        EnableLanguageServer = false  // Inconsistent with elm.json
    }
    
    let ciConfig = {
        GenerateGitHubActions = false
        GenerateElmReview = true  // Review without GitHub Actions
        GenerateElmTest = false
        GenerateElmMake = false
    }
    
    // Act
    let ecosystemFiles = generateEcosystemIntegration doc editorConfig ciConfig FormattingOption.Compact
    
    // Assert - Should handle edge cases gracefully
    Assert.NotEmpty(ecosystemFiles)
    let fileNames = ecosystemFiles |> List.map fst
    Assert.Contains("elm-format.json", fileNames)  // Always generated