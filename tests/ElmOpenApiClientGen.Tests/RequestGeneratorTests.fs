module RequestGeneratorTests

open System
open System.Collections.Generic
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Any
open Xunit
open ElmOpenApiClientGen.Generator.RequestGenerator

let createTestOperation (operationId: string) (summary: string) (responses: (string * OpenApiResponse) list) =
    let operation = OpenApiOperation()
    operation.OperationId <- operationId
    operation.Summary <- summary
    operation.Responses <- OpenApiResponses()
    
    responses
    |> List.iter (fun (statusCode, response) ->
        operation.Responses.Add(statusCode, response))
    
    operation

let createTestResponse (statusCode: int) (schemaRef: string option) =
    let response = OpenApiResponse()
    response.Description <- $"Status %d{statusCode} response"

    match schemaRef with
    | Some ref ->
        let content = OpenApiMediaType()
        content.Schema <- OpenApiSchema()
        content.Schema.Reference <- OpenApiReference()
        content.Schema.Reference.Id <- ref
        response.Content <- Dictionary<string, OpenApiMediaType>()
        response.Content.Add("application/json", content)
    | None -> ()
    
    response

let createTestDocument () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- "Test API"
    doc.Info.Version <- "1.0.0"
    doc.Paths <- OpenApiPaths()
    
    // Add some security schemes
    doc.Components <- OpenApiComponents()
    doc.Components.SecuritySchemes <- Dictionary<string, OpenApiSecurityScheme>()
    
    let bearerScheme = OpenApiSecurityScheme()
    bearerScheme.Type <- SecuritySchemeType.Http
    bearerScheme.Scheme <- "bearer"
    bearerScheme.BearerFormat <- "JWT"
    doc.Components.SecuritySchemes.Add("bearerAuth", bearerScheme)
    
    doc

[<Fact>]
let ``generateRequest should create basic GET request`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "getUser" "Get user" [("200", response)]
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("getUser", signature)
    Assert.Contains("Config", signature)
    Assert.Contains("Task", signature)
    Assert.Contains("User", signature)
    Assert.Contains("GET", definition)
    Assert.Contains("config.baseUrl", definition)

[<Fact>]
let ``generateRequest should handle POST request with body`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 201 (Some "User")
    let operation = createTestOperation "createUser" "Create user" [("201", response)]
    
    // Add request body
    let requestBody = OpenApiRequestBody()
    requestBody.Required <- true
    requestBody.Content <- Dictionary<string, OpenApiMediaType>()
    let mediaType = OpenApiMediaType()
    mediaType.Schema <- OpenApiSchema()
    mediaType.Schema.Reference <- OpenApiReference()
    mediaType.Schema.Reference.Id <- "UserInput"
    requestBody.Content.Add("application/json", mediaType)
    operation.RequestBody <- requestBody
    
    // Act
    let signature, definition, errorType = generateRequest "/users" "POST" operation doc
    
    // Assert
    Assert.Contains("createUser", signature)
    Assert.Contains("UserInput", signature)
    Assert.Contains("POST", definition)
    Assert.Contains("Http.jsonBody", definition)
    Assert.Contains("encodeUserInput", definition)

[<Fact>]
let ``generateRequest should handle path parameters`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "getUserById" "Get user by ID" [("200", response)]
    
    // Add path parameter
    let pathParam = OpenApiParameter()
    pathParam.Name <- "id"
    pathParam.In <- Nullable(ParameterLocation.Path)
    pathParam.Required <- true
    pathParam.Schema <- OpenApiSchema()
    pathParam.Schema.Type <- "integer"
    operation.Parameters <- ResizeArray<OpenApiParameter>([pathParam])
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("Int", signature)
    Assert.Contains("String.fromInt", definition)
    Assert.Contains("id", definition)

[<Fact>]
let ``generateRequest should handle query parameters`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "searchUsers" "Search users" [("200", response)]
    
    // Add query parameters
    let queryParam1 = OpenApiParameter()
    queryParam1.Name <- "name"
    queryParam1.In <- Nullable(ParameterLocation.Query)
    queryParam1.Schema <- OpenApiSchema()
    queryParam1.Schema.Type <- "string"
    
    let queryParam2 = OpenApiParameter()
    queryParam2.Name <- "limit"
    queryParam2.In <- Nullable(ParameterLocation.Query)
    queryParam2.Schema <- OpenApiSchema()
    queryParam2.Schema.Type <- "integer"
    
    operation.Parameters <- ResizeArray<OpenApiParameter>([queryParam1; queryParam2])
    
    // Act
    let signature, definition, errorType = generateRequest "/users" "GET" operation doc
    
    // Assert
    Assert.Contains("String", signature)
    Assert.Contains("Int", signature)
    Assert.Contains("Url.toQuery", definition)
    Assert.Contains("name", definition)
    Assert.Contains("limit", definition)

[<Fact>]
let ``generateRequest should handle error responses`` () =
    // Arrange
    let doc = createTestDocument()
    let successResponse = createTestResponse 200 (Some "User")
    let errorResponse = createTestResponse 404 None
    let operation = createTestOperation "getUser" "Get user" [("200", successResponse); ("404", errorResponse)]
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("GetUserError", signature)
    Assert.True(errorType.IsSome)
    Assert.Contains("GetUserError404", errorType.Value)
    Assert.Contains("GetUserErrorUnknown", errorType.Value)

[<Fact>]
let ``generateRequest should include authentication headers`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "getUser" "Get user" [("200", response)]
    
    // Add security requirement
    let securityReq = OpenApiSecurityRequirement()
    securityReq.Add(doc.Components.SecuritySchemes["bearerAuth"], ResizeArray<string>())
    operation.Security <- ResizeArray<OpenApiSecurityRequirement>([securityReq])
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("Authorization", definition)
    Assert.Contains("Bearer", definition)
    Assert.Contains("config.bearerToken", definition)

[<Fact>]
let ``generateRequest should include custom headers`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "getUser" "Get user" [("200", response)]
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("config.customHeaders", definition)
    Assert.Contains("List.map", definition)

[<Fact>]
let ``generateRequest should include timeout configuration`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "getUser" "Get user" [("200", response)]
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("timeout = config.timeout", definition)

[<Fact>]
let ``generateRequest should generate documentation`` () =
    // Arrange
    let doc = createTestDocument()
    let response = createTestResponse 200 (Some "User")
    let operation = createTestOperation "getUser" "Get user by ID" [("200", response)]
    
    // Act
    let signature, definition, errorType = generateRequest "/users/{id}" "GET" operation doc
    
    // Assert
    Assert.Contains("{-| Get user by ID -}", signature)