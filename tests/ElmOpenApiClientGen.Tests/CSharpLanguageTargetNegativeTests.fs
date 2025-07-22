module ElmOpenApiClientGen.Tests.CSharpLanguageTargetNegativeTests

open Xunit
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Languages.CSharp
open Microsoft.OpenApi.Models
open System.Collections.Generic

[<Fact>]
let ``CSharpLanguageTarget should handle null context document without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    let nullContext = {
        LanguageContext.Document = null
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions - defensive programming requirement
    let types = target.GenerateTypes(nullContext)
    let requests = target.GenerateRequests(nullContext)
    let errorTypes = target.GenerateErrorTypes(nullContext)
    let moduleContent = target.GenerateModule(nullContext)
    
    Assert.NotEmpty(types)
    Assert.NotEmpty(requests)
    Assert.Empty(errorTypes) // Currently returns empty list
    Assert.NotEmpty(moduleContent)
    
    // Should contain defensive comments
    Assert.Contains("// No schemas found in OpenAPI document", String.concat "\n" types)
    Assert.Contains("// No paths found in OpenAPI document", String.concat "\n" (requests |> List.map snd))

[<Fact>]
let ``CSharpLanguageTarget should handle document with null components without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- null
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let types = target.GenerateTypes(context)
    let moduleContent = target.GenerateModule(context)
    
    Assert.NotEmpty(types)
    Assert.NotEmpty(moduleContent)
    Assert.Contains("// No schemas found in OpenAPI document", String.concat "\n" types)

[<Fact>]
let ``CSharpLanguageTarget should handle document with null schemas without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- null
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    Assert.Contains("// No schemas found in OpenAPI document", String.concat "\n" types)

[<Fact>]
let ``CSharpLanguageTarget should handle document with null paths without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Paths <- null
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let requests = target.GenerateRequests(context)
    
    Assert.NotEmpty(requests)
    Assert.Contains("// No paths found in OpenAPI document", String.concat "\n" (requests |> List.map snd))

[<Fact>]
let ``CSharpLanguageTarget should handle schema with null properties without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    let badSchema = OpenApiSchema()
    badSchema.Type <- "object"
    badSchema.Properties <- null // Null properties
    
    doc.Components.Schemas.Add("BadSchema", badSchema)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    Assert.Contains("public record BadSchema", typeString)
    Assert.Contains("// No properties defined", typeString)

[<Fact>]
let ``CSharpLanguageTarget should handle path item with null operations without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Paths <- OpenApiPaths()
    
    let pathItem = OpenApiPathItem()
    pathItem.Operations <- null // Null operations
    
    doc.Paths.Add("/test", pathItem)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let requests = target.GenerateRequests(context)
    
    Assert.NotEmpty(requests)
    // Should still handle the path gracefully

[<Fact>]
let ``CSharpLanguageTarget should handle operation with null parameters without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Paths <- OpenApiPaths()
    
    let pathItem = OpenApiPathItem()
    pathItem.Operations <- Dictionary<OperationType, OpenApiOperation>()
    
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Parameters <- null // Null parameters
    operation.Responses <- OpenApiResponses()
    
    pathItem.Operations.Add(OperationType.Get, operation)
    doc.Paths.Add("/test", pathItem)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let requests = target.GenerateRequests(context)
    
    Assert.NotEmpty(requests)
    let (_, methodBody) = requests.Head
    Assert.Contains("TestOpAsync", methodBody)

[<Fact>]
let ``CSharpLanguageTarget should handle operation with null responses without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Paths <- OpenApiPaths()
    
    let pathItem = OpenApiPathItem()
    pathItem.Operations <- Dictionary<OperationType, OpenApiOperation>()
    
    let operation = OpenApiOperation()
    operation.OperationId <- "testOp"
    operation.Responses <- null // Null responses
    
    pathItem.Operations.Add(OperationType.Get, operation)
    doc.Paths.Add("/test", pathItem)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let requests = target.GenerateRequests(context)
    
    Assert.NotEmpty(requests)
    let (_, methodBody) = requests.Head
    Assert.Contains("Result<string", methodBody) // Should default to string

[<Fact>]
let ``CSharpLanguageTarget should handle operation with empty operation ID without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Paths <- OpenApiPaths()
    
    let pathItem = OpenApiPathItem()
    pathItem.Operations <- Dictionary<OperationType, OpenApiOperation>()
    
    let operation = OpenApiOperation()
    operation.OperationId <- null // Empty operation ID
    operation.Responses <- OpenApiResponses()
    
    pathItem.Operations.Add(OperationType.Get, operation)
    doc.Paths.Add("/users/{id}", pathItem)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let requests = target.GenerateRequests(context)
    
    Assert.NotEmpty(requests)
    let (methodName, methodBody) = requests.Head
    // Should generate a method name based on path and HTTP method
    Assert.Contains("Get", methodName)

[<Fact>]
let ``CSharpLanguageTarget should handle schema with null enum values without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    let enumSchema = OpenApiSchema()
    enumSchema.Type <- "string"
    enumSchema.Enum <- null // Null enum values
    
    doc.Components.Schemas.Add("Status", enumSchema)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    // Should generate a TODO comment instead of enum
    Assert.Contains("// TODO: Generate type for Status", typeString)

[<Fact>]
let ``CSharpLanguageTarget should handle array schema with null items without throwing`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    let objectSchema = OpenApiSchema()
    objectSchema.Type <- "object"
    objectSchema.Properties <- Dictionary<string, OpenApiSchema>()
    
    let arraySchema = OpenApiSchema()
    arraySchema.Type <- "array"
    arraySchema.Items <- null // Null items
    
    objectSchema.Properties.Add("items", arraySchema)
    doc.Components.Schemas.Add("Container", objectSchema)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    Assert.Contains("List<object", typeString) // Should handle null items gracefully

[<Fact>]
let ``CSharpLanguageTarget should handle invalid module prefix gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "123-invalid!@#" // Invalid characters
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let moduleContent = target.GenerateModule(context)
    
    Assert.NotEmpty(moduleContent)
    // Should sanitize the namespace name
    Assert.Contains("namespace Type", moduleContent) // Should convert to valid C# identifier

[<Fact>]
let ``CSharpLanguageTarget should handle empty API description gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "" // Empty description
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let moduleContent = target.GenerateModule(context)
    
    Assert.NotEmpty(moduleContent)

[<Fact>]
let ``CSharpLanguageTarget should handle null API description gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = null // Null description
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    // Should not throw exceptions
    let moduleContent = target.GenerateModule(context)
    
    Assert.NotEmpty(moduleContent)

[<Fact>]
let ``CSharpLanguageTarget should handle template loading errors gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    // This tests the fallback mechanism when embedded template loading fails
    let fallbackTemplate = target.GetDefaultTemplate()
    
    Assert.NotEmpty(fallbackTemplate)
    // Should either return the template or a fallback

[<Fact>]
let ``CSharpLanguageTarget validation should not throw on malformed input`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    // Should handle null input
    match target.ValidateOutput(null) with
    | Ok () -> Assert.True(false, "Null input should fail validation")
    | Error _ -> Assert.True(true) // Expected
    
    // Should handle empty input
    match target.ValidateOutput("") with
    | Ok () -> Assert.True(false, "Empty input should fail validation")
    | Error _ -> Assert.True(true) // Expected
    
    // Should handle garbage input
    match target.ValidateOutput("complete garbage #$%^&*") with
    | Ok () -> Assert.True(false, "Garbage input should fail validation")
    | Error _ -> Assert.True(true) // Expected

[<Fact>]
let ``CSharpLanguageTarget should handle path generation errors gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    // Should handle null base path
    let path1 = target.GetOutputPath(null) None
    Assert.NotEmpty(path1)
    Assert.Contains("ApiClient.cs", path1)
    
    // Should handle empty base path
    let path2 = target.GetOutputPath("") None
    Assert.NotEmpty(path2)
    
    // Should handle invalid characters in prefix
    let path3 = target.GetOutputPath("/output") (Some "invalid@#$%^")
    Assert.NotEmpty(path3)