module ElmOpenApiClientGen.Tests.CSharpLanguageTargetTests

open Xunit
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Languages.CSharp
open Microsoft.OpenApi.Models
open System.Collections.Generic

[<Fact>]
let ``GeneratorFactory should create CSharp language target`` () =
    let target = GeneratorFactory.createLanguageTarget LanguageTarget.CSharp
    
    Assert.Equal("C#", target.Name)
    Assert.Equal("cs", target.FileExtension)
    Assert.Equal("GeneratedApi", target.DefaultModulePrefix)

[<Fact>]
let ``GeneratorFactory should parse csharp target correctly`` () =
    let result = GeneratorFactory.parseLanguageTarget "csharp"
    
    match result with
    | Ok target -> Assert.Equal(LanguageTarget.CSharp, target)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``GeneratorFactory should parse cs target correctly`` () =
    let result = GeneratorFactory.parseLanguageTarget "cs"
    
    match result with
    | Ok target -> Assert.Equal(LanguageTarget.CSharp, target)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``GeneratorFactory should parse c# target correctly`` () =
    let result = GeneratorFactory.parseLanguageTarget "c#"
    
    match result with
    | Ok target -> Assert.Equal(LanguageTarget.CSharp, target)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``GeneratorFactory should include csharp in available targets`` () =
    let targets = GeneratorFactory.getAvailableTargets()
    
    Assert.NotEmpty(targets)
    Assert.Contains(LanguageTarget.CSharp, targets)

[<Fact>]
let ``CSharpLanguageTarget should generate module with correct structure`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- "Test API"
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "TestApi"
        OutputPath = ""
        Force = false
        ApiDescription = "Test API Description"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let moduleContent = target.GenerateModule(context)
    
    
    Assert.NotEmpty(moduleContent)
    Assert.Contains("namespace TestApi", moduleContent)
    Assert.Contains("public class TestApiClient", moduleContent)
    Assert.Contains("#nullable enable", moduleContent)
    Assert.Contains("using System;", moduleContent)

[<Fact>]
let ``CSharpLanguageTarget should generate types for object schemas`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    let userSchema = OpenApiSchema()
    userSchema.Type <- "object"
    userSchema.Properties <- Dictionary<string, OpenApiSchema>()
    userSchema.Required <- HashSet<string>()
    
    let idSchema = OpenApiSchema()
    idSchema.Type <- "integer"
    userSchema.Properties.Add("id", idSchema)
    userSchema.Required.Add("id")
    
    let nameSchema = OpenApiSchema()
    nameSchema.Type <- "string"
    userSchema.Properties.Add("name", nameSchema)
    
    doc.Components.Schemas.Add("User", userSchema)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    Assert.Contains("public record User", typeString)
    Assert.Contains("public int Id { get; init; }", typeString)
    Assert.Contains("public string? Name { get; init; }", typeString)

[<Fact>]
let ``CSharpLanguageTarget should generate enums for string schemas with enum values`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    let statusSchema = OpenApiSchema()
    statusSchema.Type <- "string"
    statusSchema.Enum <- List<Microsoft.OpenApi.Any.IOpenApiAny>()
    statusSchema.Enum.Add(Microsoft.OpenApi.Any.OpenApiString("active"))
    statusSchema.Enum.Add(Microsoft.OpenApi.Any.OpenApiString("inactive"))
    statusSchema.Enum.Add(Microsoft.OpenApi.Any.OpenApiString("pending"))
    
    doc.Components.Schemas.Add("Status", statusSchema)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    Assert.Contains("public enum Status", typeString)
    Assert.Contains("Active,", typeString)
    Assert.Contains("Inactive,", typeString)
    Assert.Contains("Pending", typeString)

[<Fact>]
let ``CSharpLanguageTarget should generate requests for API paths`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Paths <- OpenApiPaths()
    
    let pathItem = OpenApiPathItem()
    pathItem.Operations <- Dictionary<OperationType, OpenApiOperation>()
    
    let getOperation = OpenApiOperation()
    getOperation.OperationId <- "getUser"
    getOperation.Summary <- "Get user by ID"
    getOperation.Responses <- OpenApiResponses()
    
    let response = OpenApiResponse()
    response.Content <- Dictionary<string, OpenApiMediaType>()
    let mediaType = OpenApiMediaType()
    let schema = OpenApiSchema()
    schema.Type <- "object"
    mediaType.Schema <- schema
    response.Content.Add("application/json", mediaType)
    getOperation.Responses.Add("200", response)
    
    pathItem.Operations.Add(OperationType.Get, getOperation)
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
    
    let requests = target.GenerateRequests(context)
    
    Assert.NotEmpty(requests)
    let (methodName, methodBody) = requests.Head
    Assert.Equal("GetUser", methodName)
    Assert.Contains("GetUserAsync", methodBody)
    Assert.Contains("HttpMethod.Get", methodBody)
    Assert.Contains("Result<object", methodBody)

[<Fact>]
let ``CSharpLanguageTarget should validate generated output correctly`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    let validCSharp = "namespace GeneratedApi { public class ApiClient { } }"
    let invalidCSharp = "this is not valid C#"
    let fallbackCSharp = "// Error generating C# module: test error\nnamespace GeneratedApi { public class ApiClient { } }"
    
    match target.ValidateOutput(validCSharp) with
    | Ok () -> Assert.True(true)
    | Error msg -> Assert.True(false, $"Valid C# should pass validation: {msg}")
    
    match target.ValidateOutput(invalidCSharp) with
    | Ok () -> Assert.True(false, "Invalid C# should fail validation")
    | Error _ -> Assert.True(true)
    
    match target.ValidateOutput(fallbackCSharp) with
    | Ok () -> Assert.True(true)
    | Error msg -> Assert.True(false, $"Fallback C# should pass validation: {msg}")

[<Fact>]
let ``CSharpLanguageTarget should get correct output path`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    let pathWithPrefix = target.GetOutputPath("/output" ) (Some "MyApi")
    let pathWithoutPrefix = target.GetOutputPath("/output") None
    
    Assert.Contains("MyApiClient.cs", pathWithPrefix)
    Assert.Contains("GeneratedApiClient.cs", pathWithoutPrefix)

[<Fact>]
let ``CSharpLanguageTarget should handle null document gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    
    let context = {
        LanguageContext.Document = null
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let types = target.GenerateTypes(context)
    let requests = target.GenerateRequests(context)
    let moduleContent = target.GenerateModule(context)
    
    Assert.NotEmpty(types)
    Assert.Contains("// No schemas found in OpenAPI document", String.concat "\n" types)
    
    Assert.NotEmpty(requests)
    Assert.Contains("// No paths found in OpenAPI document", String.concat "\n" (requests |> List.map snd))
    
    Assert.NotEmpty(moduleContent)
    // Should still generate a valid C# module even with null document

[<Fact>]
let ``CSharpLanguageTarget should handle malformed schemas gracefully`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    // Add a null schema to test defensive programming
    doc.Components.Schemas.Add("BadSchema", null)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    Assert.Contains("// TODO: Generate type for BadSchema (null schema)", typeString)

[<Fact>]
let ``CSharpLanguageTarget should handle complex type mappings`` () =
    let target = CSharpLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Components <- OpenApiComponents()
    doc.Components.Schemas <- Dictionary<string, OpenApiSchema>()
    
    let complexSchema = OpenApiSchema()
    complexSchema.Type <- "object"
    complexSchema.Properties <- Dictionary<string, OpenApiSchema>()
    complexSchema.Required <- HashSet<string>()
    
    // Test different data types
    let stringSchema = OpenApiSchema(Type = "string")
    let intSchema = OpenApiSchema(Type = "integer")
    let longSchema = OpenApiSchema(Type = "integer", Format = "int64")
    let floatSchema = OpenApiSchema(Type = "number", Format = "float")
    let boolSchema = OpenApiSchema(Type = "boolean")
    let dateTimeSchema = OpenApiSchema(Type = "string", Format = "date-time")
    let uuidSchema = OpenApiSchema(Type = "string", Format = "uuid")
    
    // Test array type
    let arraySchema = OpenApiSchema()
    arraySchema.Type <- "array"
    arraySchema.Items <- stringSchema
    
    complexSchema.Properties.Add("name", stringSchema)
    complexSchema.Properties.Add("id", intSchema)
    complexSchema.Properties.Add("bigId", longSchema)
    complexSchema.Properties.Add("score", floatSchema)
    complexSchema.Properties.Add("active", boolSchema)
    complexSchema.Properties.Add("createdAt", dateTimeSchema)
    complexSchema.Properties.Add("uuid", uuidSchema)
    complexSchema.Properties.Add("tags", arraySchema)
    
    // Make some fields required
    complexSchema.Required.Add("id")
    complexSchema.Required.Add("name")
    
    doc.Components.Schemas.Add("ComplexType", complexSchema)
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = None
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2024-01-01"
        CustomTemplatePath = None
    }
    
    let types = target.GenerateTypes(context)
    
    Assert.NotEmpty(types)
    let typeString = String.concat "\n" types
    
    // Check type mappings
    Assert.Contains("public string Name { get; init; }", typeString)
    Assert.Contains("public int Id { get; init; }", typeString)
    Assert.Contains("public long? BigId { get; init; }", typeString)
    Assert.Contains("public float? Score { get; init; }", typeString)
    Assert.Contains("public bool? Active { get; init; }", typeString)
    Assert.Contains("public DateTime? CreatedAt { get; init; }", typeString)
    Assert.Contains("public Guid? Uuid { get; init; }", typeString)
    Assert.Contains("public List<string>? Tags { get; init; }", typeString)