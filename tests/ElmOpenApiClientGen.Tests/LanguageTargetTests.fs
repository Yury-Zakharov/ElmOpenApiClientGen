module ElmOpenApiClientGen.Tests.LanguageTargetTests

open Xunit
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Languages.Elm
open Microsoft.OpenApi.Models

[<Fact>]
let ``GeneratorFactory should create Elm language target`` () =
    let target = GeneratorFactory.createLanguageTarget LanguageTarget.Elm
    
    Assert.Equal("Elm", target.Name)
    Assert.Equal("elm", target.FileExtension)
    Assert.Equal("Api", target.DefaultModulePrefix)

[<Fact>]
let ``GeneratorFactory should parse elm target correctly`` () =
    let result = GeneratorFactory.parseLanguageTarget "elm"
    
    match result with
    | Ok target -> Assert.Equal(LanguageTarget.Elm, target)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``GeneratorFactory should return error for invalid target`` () =
    let result = GeneratorFactory.parseLanguageTarget "invalid"
    
    match result with
    | Ok _ -> Assert.True(false, "Expected Error but got Ok")
    | Error msg -> Assert.Contains("Unsupported target language", msg)

[<Fact>]
let ``GeneratorFactory should return elm as default target`` () =
    let target = GeneratorFactory.getDefaultTarget()
    
    Assert.Equal(LanguageTarget.Elm, target)

[<Fact>]
let ``GeneratorFactory should return available targets`` () =
    let targets = GeneratorFactory.getAvailableTargets()
    
    Assert.NotEmpty(targets)
    Assert.Contains(LanguageTarget.Elm, targets)

[<Fact>]
let ``ElmLanguageTarget should generate module with correct structure`` () =
    let target = ElmLanguageTarget() :> ILanguageTarget
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- "Test API"
    
    let context = {
        LanguageContext.Document = doc
        ModulePrefix = Some "Test"
        OutputPath = ""
        Force = false
        ApiDescription = "Test API"
        GenerationTimestamp = "2023-01-01 00:00:00"
        CustomTemplatePath = None
    }
    
    let moduleCode = target.GenerateModule context
    
    Assert.Contains("module Test.Schemas", moduleCode)
    Assert.Contains("Test API", moduleCode)
    Assert.Contains("2023-01-01 00:00:00", moduleCode)

[<Fact>]
let ``ElmLanguageTarget should validate output correctly`` () =
    let target = ElmLanguageTarget() :> ILanguageTarget
    
    let validOutput = "module Test.Schemas exposing (..)\n\n-- Valid Elm module"
    let invalidOutput = "invalid output"
    
    match target.ValidateOutput validOutput with
    | Ok _ -> Assert.True(true)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")
    
    match target.ValidateOutput invalidOutput with
    | Ok _ -> Assert.True(false, "Expected Error but got Ok")
    | Error _ -> Assert.True(true)

[<Fact>]
let ``ElmLanguageTarget should generate correct output path`` () =
    let target = ElmLanguageTarget() :> ILanguageTarget
    
    let pathWithPrefix = target.GetOutputPath "/base" (Some "Custom")
    let pathWithoutPrefix = target.GetOutputPath "/base" None
    
    Assert.Contains("Custom", pathWithPrefix)
    Assert.Contains("Api", pathWithoutPrefix)
    Assert.Contains("Schemas.elm", pathWithPrefix)
    Assert.Contains("Schemas.elm", pathWithoutPrefix)