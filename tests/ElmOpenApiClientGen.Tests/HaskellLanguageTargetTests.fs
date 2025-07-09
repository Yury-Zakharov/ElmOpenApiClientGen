module ElmOpenApiClientGen.Tests.HaskellLanguageTargetTests

open Xunit
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Languages.Haskell
open Microsoft.OpenApi.Models

[<Fact>]
let ``GeneratorFactory should create Haskell language target`` () =
    let target = GeneratorFactory.createLanguageTarget LanguageTarget.Haskell
    
    Assert.Equal("Haskell", target.Name)
    Assert.Equal("hs", target.FileExtension)
    Assert.Equal("Api", target.DefaultModulePrefix)

[<Fact>]
let ``GeneratorFactory should parse haskell target correctly`` () =
    let result = GeneratorFactory.parseLanguageTarget "haskell"
    
    match result with
    | Ok target -> Assert.Equal(LanguageTarget.Haskell, target)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``GeneratorFactory should parse hs target correctly`` () =
    let result = GeneratorFactory.parseLanguageTarget "hs"
    
    match result with
    | Ok target -> Assert.Equal(LanguageTarget.Haskell, target)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

[<Fact>]
let ``GeneratorFactory should include haskell in available targets`` () =
    let targets = GeneratorFactory.getAvailableTargets()
    
    Assert.NotEmpty(targets)
    Assert.Contains(LanguageTarget.Haskell, targets)

[<Fact>]
let ``HaskellLanguageTarget should generate module with correct structure`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
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
    Assert.Contains("import GHC.Generics", moduleCode)
    Assert.Contains("import Data.Aeson", moduleCode)

[<Fact>]
let ``HaskellLanguageTarget should validate output correctly`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
    
    let validOutput = "module Test.Schemas where\n\nimport GHC.Generics\n-- Valid Haskell module"
    let invalidOutput = "invalid output"
    
    match target.ValidateOutput validOutput with
    | Ok _ -> Assert.True(true)
    | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")
    
    match target.ValidateOutput invalidOutput with
    | Ok _ -> Assert.True(false, "Expected Error but got Ok")
    | Error _ -> Assert.True(true)

[<Fact>]
let ``HaskellLanguageTarget should generate correct output path`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
    
    let pathWithPrefix = target.GetOutputPath "/base" (Some "Custom")
    let pathWithoutPrefix = target.GetOutputPath "/base" None
    
    Assert.Contains("Custom", pathWithPrefix)
    Assert.Contains("Api", pathWithoutPrefix)
    Assert.Contains("Schemas.hs", pathWithPrefix)
    Assert.Contains("Schemas.hs", pathWithoutPrefix)

[<Fact>]
let ``HaskellLanguageTarget should generate types placeholder`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
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
    
    let types = target.GenerateTypes context
    
    Assert.Single(types)
    Assert.Contains("Types will be generated here", types.[0])

[<Fact>]
let ``HaskellLanguageTarget should generate requests placeholder`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
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
    
    let requests = target.GenerateRequests context
    
    Assert.Single(requests)
    let (requestCode, requestBody) = requests.[0]
    Assert.Contains("Requests will be generated here", requestCode)
    Assert.Contains("Request body", requestBody)

[<Fact>]
let ``HaskellLanguageTarget should generate error types placeholder`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
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
    
    let errorTypes = target.GenerateErrorTypes context
    
    Assert.Single(errorTypes)
    Assert.Contains("Error types will be generated here", errorTypes.[0])

[<Fact>]
let ``HaskellLanguageTarget should load embedded template`` () =
    let target = HaskellLanguageTarget() :> ILanguageTarget
    
    let template = target.GetDefaultTemplate()
    
    Assert.NotNull(template)
    Assert.NotEmpty(template)
    Assert.Contains("module {{moduleName}}", template)
    Assert.Contains("import GHC.Generics", template)
    Assert.Contains("import Data.Aeson", template)