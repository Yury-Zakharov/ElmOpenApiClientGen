module ElmOpenApiClientGen.Tests.CSharpIntegrationTests

open System
open System.IO
open Microsoft.OpenApi.Readers
open Xunit
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Languages.CSharp

let testOpenApiSpecCSharp = """
openapi: 3.0.3
info:
  title: C# Integration Test API
  description: API for C# integration testing
  version: 1.0.0
servers:
  - url: https://api.example.com/v1
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
              $ref: '#/components/schemas/CreateUserRequest'
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
  schemas:
    User:
      type: object
      required:
        - id
        - username
        - email
      properties:
        id:
          type: integer
          format: int64
          minimum: 1
        username:
          type: string
          minLength: 3
          maxLength: 50
        email:
          type: string
          format: email
        profile:
          $ref: '#/components/schemas/UserProfile'
        status:
          $ref: '#/components/schemas/UserStatus'
        tags:
          type: array
          items:
            type: string
        metadata:
          type: object
          additionalProperties:
            type: string
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
    UserProfile:
      type: object
      properties:
        firstName:
          type: string
        lastName:
          type: string
        bio:
          type: string
        avatar:
          type: string
          format: uri
    UserStatus:
      type: string
      enum:
        - active
        - inactive
        - pending
        - suspended
      description: Current status of the user account
    CreateUserRequest:
      type: object
      required:
        - username
        - email
      properties:
        username:
          type: string
          minLength: 3
          maxLength: 50
        email:
          type: string
          format: email
        profile:
          $ref: '#/components/schemas/UserProfile'
"""

[<Fact>]
let ``End-to-end C# generation should produce valid compilable code`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpecCSharp)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "TestApi"
        OutputPath = ""
        Force = false
        ApiDescription = "C# Integration Test API"
        GenerationTimestamp = "2024-01-01T00:00:00Z"
        CustomTemplatePath = None
    }
    
    let moduleContent = target.GenerateModule(context)
    
    // Verify basic C# structure
    Assert.NotEmpty(moduleContent)
    Assert.Contains("namespace TestApi", moduleContent)
    Assert.Contains("public class TestApiClient", moduleContent)
    Assert.Contains("#nullable enable", moduleContent)
    
    // Verify HTTP client setup
    Assert.Contains("private readonly HttpClient _httpClient", moduleContent)
    Assert.Contains("JsonSerializerOptions", moduleContent)
    
    // Verify Result type is included
    Assert.Contains("public record Result<T, TError>", moduleContent)
    Assert.Contains("public record ApiError", moduleContent)
    
    // Verify generated types are present
    Assert.Contains("public record User", moduleContent)
    Assert.Contains("public record UserProfile", moduleContent)
    Assert.Contains("public enum UserStatus", moduleContent)
    Assert.Contains("public record CreateUserRequest", moduleContent)

[<Fact>]
let ``Generated C# types should have correct properties and nullability`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpecCSharp)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let types = target.GenerateTypes(context)
    let typesString = String.concat "\n" types
    
    // User type - required fields should not be nullable
    Assert.Contains("public long Id { get; init; }", typesString) // int64 -> long
    Assert.Contains("public string Username { get; init; }", typesString) // required
    Assert.Contains("public string Email { get; init; }", typesString) // required
    
    // User type - optional fields should be nullable
    Assert.Contains("public UserProfile? Profile { get; init; }", typesString)
    Assert.Contains("public UserStatus? Status { get; init; }", typesString)
    Assert.Contains("public List<string>? Tags { get; init; }", typesString)
    Assert.Contains("public DateTime? CreatedAt { get; init; }", typesString) // date-time -> DateTime
    Assert.Contains("public DateTime? UpdatedAt { get; init; }", typesString)
    
    // UserStatus enum
    Assert.Contains("public enum UserStatus", typesString)
    Assert.Contains("Active,", typesString)
    Assert.Contains("Inactive,", typesString)
    Assert.Contains("Pending,", typesString)
    Assert.Contains("Suspended", typesString)

[<Fact>]
let ``Generated C# requests should have correct signatures and error handling`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpecCSharp)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let requests = target.GenerateRequests(context)
    let requestsString = String.concat "\n" (requests |> List.map snd)
    
    // GetUserById method
    Assert.Contains("GetUserByIdAsync", requestsString)
    Assert.Contains("int userId", requestsString)
    Assert.Contains("bool? includeProfile", requestsString)
    Assert.Contains("CancellationToken cancellationToken = default", requestsString)
    Assert.Contains("Result<User", requestsString)
    Assert.Contains("HttpMethod.Get", requestsString)
    
    // CreateUser method
    Assert.Contains("CreateUserAsync", requestsString)
    Assert.Contains("Result<User", requestsString)
    Assert.Contains("HttpMethod.Post", requestsString)
    
    // Error handling
    Assert.Contains("try", requestsString)
    Assert.Contains("catch (Exception ex)", requestsString)
    Assert.Contains("Result<", requestsString)
    Assert.Contains("ApiError", requestsString)

[<Fact>]
let ``Generated C# module should pass validation`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(testOpenApiSpecCSharp)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "ValidationTest"
        OutputPath = ""
        Force = false
        ApiDescription = "Validation Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let moduleContent = target.GenerateModule(context)
    
    match target.ValidateOutput(moduleContent) with
    | Ok () -> Assert.True(true)
    | Error msg -> Assert.True(false, $"Generated C# module should pass validation: {msg}")

[<Fact>]
let ``Generated C# output path should be correct`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    let path1 = target.GetOutputPath("/output") (Some "MyCompany.Api")
    let path2 = target.GetOutputPath("/output") None
    
    Assert.Contains("MyCompany", path1)
    Assert.Contains("Api", path1)
    Assert.Contains("Client.cs", path1)
    Assert.EndsWith("Client.cs", path1)
    
    Assert.Contains("GeneratedApi", path2)
    Assert.Contains("Client.cs", path2)

[<Fact>]
let ``C# generation should handle complex nested schemas`` () =
    let complexSpec = """
openapi: 3.0.3
info:
  title: Complex API
  version: 1.0.0
paths:
  /complex:
    get:
      responses:
        '200':
          description: Success
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ComplexResponse'
components:
  schemas:
    ComplexResponse:
      type: object
      required:
        - data
        - meta
      properties:
        data:
          type: array
          items:
            $ref: '#/components/schemas/DataItem'
        meta:
          $ref: '#/components/schemas/MetaInfo'
        pagination:
          $ref: '#/components/schemas/PaginationInfo'
    DataItem:
      type: object
      required:
        - id
        - type
      properties:
        id:
          type: string
          format: uuid
        type:
          type: string
        attributes:
          type: object
        relationships:
          type: array
          items:
            $ref: '#/components/schemas/Relationship'
    Relationship:
      type: object
      properties:
        id:
          type: string
        type:
          type: string
    MetaInfo:
      type: object
      properties:
        total:
          type: integer
        version:
          type: string
    PaginationInfo:
      type: object
      properties:
        page:
          type: integer
        size:
          type: integer
        hasNext:
          type: boolean
"""
    
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(complexSpec)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "ComplexApi"
        OutputPath = ""
        Force = false
        ApiDescription = "Complex API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let moduleContent = target.GenerateModule(context)
    let types = target.GenerateTypes(context)
    let typesString = String.concat "\n" types
    
    // Should generate all types
    Assert.Contains("public record ComplexResponse", typesString)
    Assert.Contains("public record DataItem", typesString)
    Assert.Contains("public record Relationship", typesString)
    Assert.Contains("public record MetaInfo", typesString)
    Assert.Contains("public record PaginationInfo", typesString)
    
    // Should handle nested arrays and references
    Assert.Contains("List<DataItem>", typesString)
    Assert.Contains("List<Relationship>", typesString)
    Assert.Contains("Guid", typesString) // UUID format
    
    // Required vs optional fields
    Assert.Contains("public List<DataItem> Data { get; init; }", typesString)
    Assert.Contains("public MetaInfo Meta { get; init; }", typesString)
    Assert.Contains("public PaginationInfo? Pagination { get; init; }", typesString)
    
    Assert.Contains("public Guid Id { get; init; }", typesString)
    Assert.Contains("public string Type { get; init; }", typesString)

[<Fact>]
let ``C# generation should handle edge cases gracefully`` () =
    let edgeCaseSpec = """
openapi: 3.0.3
info:
  title: Edge Case API
  version: 1.0.0
paths:
  /edge-cases/{param-with-dashes}:
    post:
      parameters:
        - name: param-with-dashes
          in: path
          required: true
          schema:
            type: string
        - name: query_with_underscores
          in: query
          schema:
            type: integer
      responses:
        '200':
          description: Success
components:
  schemas:
    WeirdTypeName123:
      type: object
      properties:
        field-with-dashes:
          type: string
        field_with_underscores:
          type: integer
        123field:
          type: boolean
        class:
          type: string
        enum:
          type: string
    Status:
      type: string
      enum:
        - value-with-dashes
        - value_with_underscores
        - 123value
"""
    
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(edgeCaseSpec)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "EdgeCase"
        OutputPath = ""
        Force = false
        ApiDescription = "Edge Case API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let moduleContent = target.GenerateModule(context)
    let types = target.GenerateTypes(context)
    let requests = target.GenerateRequests(context)
    
    // Should not throw exceptions and generate something
    Assert.NotEmpty(moduleContent)
    Assert.NotEmpty(types)
    Assert.NotEmpty(requests)
    
    let typesString = String.concat "\n" types
    let requestsString = String.concat "\n" (requests |> List.map snd)
    
    // Should handle naming conversion
    Assert.Contains("WeirdTypeName", typesString) // Should clean up the name
    Assert.Contains("FieldWithDashes", typesString)
    Assert.Contains("FieldWithUnderscores", typesString)
    
    // Should handle reserved keywords
    Assert.Contains("@Class", typesString) // Should escape reserved keywords
    Assert.Contains("@Enum", typesString)
    
    // Should handle parameter naming
    Assert.Contains("paramWithDashes", requestsString)
    Assert.Contains("queryWithUnderscores", requestsString)

[<Fact>]
let ``C# generation should work with minimal OpenAPI spec`` () =
    let minimalSpec = """
openapi: 3.0.3
info:
  title: Minimal API
  version: 1.0.0
paths:
  /ping:
    get:
      responses:
        '200':
          description: Pong
"""
    
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let reader = OpenApiStringReader()
    let doc, diagnostics = reader.Read(minimalSpec)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Minimal API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let moduleContent = target.GenerateModule(context)
    let types = target.GenerateTypes(context)
    let requests = target.GenerateRequests(context)
    
    // Should still generate valid C# even with minimal spec
    Assert.NotEmpty(moduleContent)
    Assert.Contains("namespace GeneratedApi", moduleContent)
    Assert.Contains("public class GeneratedApiClient", moduleContent)
    
    // Should handle no components gracefully
    Assert.Contains("// No schemas found", String.concat "\n" types)
    
    // Should generate at least one request
    Assert.NotEmpty(requests)
    let requestsString = String.concat "\n" (requests |> List.map snd)
    Assert.Contains("Async", requestsString)
    Assert.Contains("Result<string", requestsString) // Default response type