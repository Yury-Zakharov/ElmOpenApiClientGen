module ElmOpenApiClientGen.Tests.TemplateResolverTests

open Xunit
open System.IO
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Languages.Elm
open Microsoft.OpenApi.Models

[<Fact>]
let ``loadEmbeddedTemplate should load embedded resource`` () =
    let template = TemplateResolver.loadEmbeddedTemplate "ElmOpenApiClientGen.Languages.Elm.Templates.module.scriban"
    
    Assert.NotNull(template)
    Assert.NotEmpty(template)
    Assert.Contains("module {{moduleName}}", template)
    Assert.Contains("{{apiDescription}}", template)
    Assert.Contains("{{generationTimestamp}}", template)

[<Fact>]
let ``loadEmbeddedTemplate should throw for non-existent resource`` () =
    Assert.Throws<System.Exception>(fun () ->
        TemplateResolver.loadEmbeddedTemplate "NonExistent.Template" |> ignore
    )

[<Fact>]
let ``loadCustomTemplate should load file when it exists`` () =
    let tempFile = Path.GetTempFileName()
    let testContent = "module {{moduleName}} exposing (..)\n-- Test template"
    
    try
        File.WriteAllText(tempFile, testContent)
        let result = TemplateResolver.loadCustomTemplate tempFile
        
        Assert.Equal(testContent, result)
    finally
        File.Delete(tempFile)

[<Fact>]
let ``loadCustomTemplate should throw for non-existent file`` () =
    Assert.Throws<System.Exception>(fun () ->
        TemplateResolver.loadCustomTemplate "/path/to/non/existent/file.template" |> ignore
    )

[<Fact>]
let ``resolveTemplate should use custom template when provided`` () =
    let tempFile = Path.GetTempFileName()
    let testContent = "module {{moduleName}} exposing (..)\n-- Custom template"
    
    try
        File.WriteAllText(tempFile, testContent)
        
        let doc = OpenApiDocument()
        doc.Info <- OpenApiInfo()
        doc.Info.Title <- "Test API"
        
        let context = {
            LanguageContext.Document = doc
            ModulePrefix = Some "Test"
            OutputPath = ""
            Force = false
            ApiDescription = "Test API"
            GenerationTimestamp = "2023-01-01"
            CustomTemplatePath = Some tempFile
        }
        
        let target = ElmLanguageTarget() :> ILanguageTarget
        let result = TemplateResolver.resolveTemplate context target
        
        Assert.Equal(testContent, result)
    finally
        File.Delete(tempFile)

[<Fact>]
let ``resolveTemplate should use default template when custom not provided`` () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- "Test API"
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "Test"
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2023-01-01"
        CustomTemplatePath = None
    }
    
    let target = ElmLanguageTarget() :> ILanguageTarget
    let result = TemplateResolver.resolveTemplate context target
    
    Assert.NotNull(result)
    Assert.NotEmpty(result)
    Assert.Contains("module {{moduleName}}", result)

[<Fact>]
let ``validateTemplate should pass for valid template`` () =
    let template = "module {{moduleName}} exposing (..)\n-- {{apiDescription}}"
    let requiredPlaceholders = ["{{moduleName}}"; "{{apiDescription}}"]
    
    match TemplateResolver.validateTemplate template requiredPlaceholders with
    | Ok _ -> Assert.True(true)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``validateTemplate should fail for missing placeholders`` () =
    let template = "module Test exposing (..)\n-- Missing placeholders"
    let requiredPlaceholders = ["{{moduleName}}"; "{{apiDescription}}"]
    
    match TemplateResolver.validateTemplate template requiredPlaceholders with
    | Ok _ -> Assert.True(false, "Expected Error but got Ok")
    | Error msg -> 
        Assert.Contains("missing required placeholders", msg)
        Assert.Contains("{{moduleName}}", msg)
        Assert.Contains("{{apiDescription}}", msg)