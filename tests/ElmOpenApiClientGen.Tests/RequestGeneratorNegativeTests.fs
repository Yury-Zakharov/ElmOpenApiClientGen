module RequestGeneratorNegativeTests

open System
open System.Collections.Generic
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Any
open Xunit
open ElmOpenApiClientGen.Generator.RequestGenerator

let createEmptyDocument () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- ""
    doc.Info.Version <- ""
    doc

let createDocumentWithoutComponents () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- "Test API"
    doc.Info.Version <- "1.0.0"
    doc.Paths <- OpenApiPaths()
    // No Components section
    doc

let createOperationWithoutId () =
    let operation = OpenApiOperation()
    // No OperationId set
    operation.Summary <- "Test operation"
    operation.Responses <- OpenApiResponses()
    operation

let createOperationWithInvalidResponses () =
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    // Add invalid response
    let response = OpenApiResponse()
    response.Description <- "Invalid response"
    operation.Responses.Add("invalid", response)
    operation

[<Fact>]
let ``generateRequest should handle null operation gracefully`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = null
    
    // Act - Should not throw exception, should handle gracefully
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should generate error comment instead of throwing
    Assert.NotNull(signature)
    Assert.Contains("ERROR: Operation is null", signature)

[<Fact>]
let ``generateRequest should handle null document gracefully`` () =
    // Arrange
    let operation = createOperationWithoutId()
    let doc = null
    
    // Act - Should not throw exception, should handle gracefully
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should generate error comment instead of throwing
    Assert.NotNull(signature)
    Assert.Contains("ERROR: Document is null", signature)

[<Fact>]
let ``generateRequest should handle empty operation ID`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithoutId()
    
    // Act
    let signature, definition, errorType = generateRequest "/test/path" "GET" operation doc
    
    // Assert - Should generate fallback function name
    Assert.Contains("GET", signature)
    Assert.Contains("test_path", signature)  // Path-based fallback name

[<Fact>]
let ``generateRequest should handle empty path`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithoutId()
    
    // Act
    let signature, definition, errorType = generateRequest "" "GET" operation doc
    
    // Assert - Should handle empty path
    Assert.NotNull(signature)
    Assert.NotNull(definition)
    Assert.Contains("GET", definition)

[<Fact>]
let ``generateRequest should handle null path`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithoutId()
    
    // Act - Should not throw exception, should handle gracefully
    let signature, definition, errorType = generateRequest null "GET" operation doc
    
    // Assert - Should handle null path gracefully with default
    Assert.NotNull(signature)
    Assert.NotNull(definition)

[<Fact>]
let ``generateRequest should handle invalid HTTP method`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithoutId()
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "INVALID" operation doc
    
    // Assert - Should default to GET
    Assert.Contains("GET", definition)

[<Fact>]
let ``generateRequest should handle empty HTTP method`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithoutId()
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "" operation doc
    
    // Assert - Should default to GET
    Assert.Contains("GET", definition)

[<Fact>]
let ``generateRequest should handle null HTTP method`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithoutId()
    
    // Act - Should not throw exception, should handle gracefully
    let signature, definition, errorType = generateRequest "/test" null operation doc
    
    // Assert - Should default to GET when method is null
    Assert.NotNull(signature)
    Assert.Contains("GET", definition)

[<Fact>]
let ``generateRequest should handle operation without responses`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    // No responses set (null)
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.Contains("testOp", signature)
    Assert.Contains("Decode.Value", signature)  // Default return type

[<Fact>]
let ``generateRequest should handle operation with empty responses`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()  // Empty responses
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.Contains("testOp", signature)
    Assert.Contains("Http.Error", signature)  // Default error type

[<Fact>]
let ``generateRequest should handle malformed path parameters`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    
    // Add malformed path parameter
    let param = OpenApiParameter()
    param.Name <- ""  // Empty name
    param.In <- Nullable(ParameterLocation.Path)
    param.Schema <- OpenApiSchema()
    param.Schema.Type <- "string"
    operation.Parameters <- ResizeArray<OpenApiParameter>([param])
    
    // Act
    let signature, definition, errorType = generateRequest "/test/{id}" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.NotNull(signature)
    Assert.NotNull(definition)

[<Fact>]
let ``generateRequest should handle parameters without schema`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    
    // Add parameter without schema
    let param = OpenApiParameter()
    param.Name <- "testParam"
    param.In <- Nullable(ParameterLocation.Query)
    // No schema set (null)
    operation.Parameters <- ResizeArray<OpenApiParameter>([param])
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.NotNull(signature)
    Assert.NotNull(definition)

[<Fact>]
let ``generateRequest should handle request body without content`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    
    // Add request body without content
    let requestBody = OpenApiRequestBody()
    requestBody.Required <- true
    // No content set (null)
    operation.RequestBody <- requestBody
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "POST" operation doc
    
    // Assert - Should handle gracefully and use empty body
    Assert.Contains("POST", definition)
    Assert.Contains("Http.emptyBody", definition)

[<Fact>]
let ``generateRequest should handle document without security schemes`` () =
    // Arrange
    let doc = createDocumentWithoutComponents()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    
    // Add security requirement but no schemes in document
    let securityReq = OpenApiSecurityRequirement()
    operation.Security <- ResizeArray<OpenApiSecurityRequirement>([securityReq])
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.NotNull(signature)
    Assert.NotNull(definition)
    Assert.Contains("config.customHeaders", definition)  // Should still include custom headers

[<Fact>]
let ``generateRequest should handle responses with null content`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    
    // Add response with null content
    let response = OpenApiResponse()
    response.Description <- "Success"
    // No content set (null)
    operation.Responses.Add("200", response)
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.Contains("testOp", signature)
    Assert.Contains("200 -> Ok ()", definition)  // No content response

[<Fact>]
let ``generateRequest should handle responses with empty content`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- OpenApiResponses()
    
    // Add response with empty content
    let response = OpenApiResponse()
    response.Description <- "Success"
    response.Content <- Dictionary<string, OpenApiMediaType>()  // Empty content
    operation.Responses.Add("200", response)
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.Contains("testOp", signature)

[<Fact>]
let ``generateRequest should handle non-numeric status codes`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = createOperationWithInvalidResponses()
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully by skipping invalid codes
    Assert.NotNull(signature)
    Assert.NotNull(definition)

[<Fact>]
let ``generateRequest should handle extremely long operation ID`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- String.replicate 1000 "verylongoperationid"  // Very long ID
    operation.Responses <- OpenApiResponses()
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle gracefully
    Assert.NotNull(signature)
    Assert.NotNull(definition)

[<Fact>]
let ``generateRequest should handle special characters in operation ID`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "test-operation_with.special@chars#"
    operation.Responses <- OpenApiResponses()
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should normalize special characters
    Assert.Contains("testoperationwithspecialchars", signature)

[<Fact>]
let ``generateRequest should handle Unicode characters in operation ID`` () =
    // Arrange
    let doc = createEmptyDocument()
    let operation = OpenApiOperation()
    operation.OperationId <- "testOperationαβγ中文"
    operation.Responses <- OpenApiResponses()
    
    // Act
    let signature, definition, errorType = generateRequest "/test" "GET" operation doc
    
    // Assert - Should handle Unicode gracefully
    Assert.NotNull(signature)
    Assert.NotNull(definition)

[<Fact>]
let ``generateRequests should handle empty document`` () =
    // Arrange
    let doc = createEmptyDocument()
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should return empty results
    Assert.Empty(requests)
    Assert.Empty(errorTypes)

[<Fact>]
let ``generateRequests should handle document with null paths`` () =
    // Arrange
    let doc = createEmptyDocument()
    doc.Paths <- null
    
    // Act - Should not throw exception, should handle gracefully
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should return empty results for null paths
    Assert.Empty(requests)
    Assert.Empty(errorTypes)

[<Fact>]
let ``generateRequests should handle paths with null operations`` () =
    // Arrange
    let doc = createEmptyDocument()
    doc.Paths <- OpenApiPaths()
    let pathItem = OpenApiPathItem()
    // Add path with null operations (default state)
    doc.Paths.Add("/test", pathItem)
    
    // Act
    let requests, errorTypes = generateRequests doc
    
    // Assert - Should handle gracefully
    Assert.Empty(requests)  // No operations to generate
    Assert.Empty(errorTypes)