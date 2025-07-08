module CodegenTests

open System
open System.Collections.Generic
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Any
open Xunit
open ElmOpenApiClientGen.Generator.Codegen

let createTestSchema (schemaType: string) (format: string option) =
    let schema = OpenApiSchema()
    schema.Type <- schemaType
    match format with
    | Some f -> schema.Format <- f
    | None -> ()
    schema

let createEnumSchema (enumValues: string list) =
    let schema = OpenApiSchema()
    schema.Type <- "string"
    schema.Enum <- ResizeArray<IOpenApiAny>()
    enumValues
    |> List.iter (fun value ->
        let enumValue = OpenApiString(value)
        schema.Enum.Add(enumValue))
    schema

let createTestDocument () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- "Test API"
    doc.Info.Version <- "1.0.0"
    doc

[<Fact>]
let ``generateElmJson should create valid elm.json`` () =
    // Arrange
    let doc = createTestDocument()
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act
    let elmJson = generateElmJson doc editorConfig
    
    // Assert
    Assert.Contains("\"type\": \"package\"", elmJson)
    Assert.Contains("\"name\": \"generated/testapi\"", elmJson)
    Assert.Contains("\"elm/core\"", elmJson)
    Assert.Contains("\"elm/json\"", elmJson)
    Assert.Contains("\"elm/http\"", elmJson)
    Assert.Contains("Api.Schemas", elmJson)

[<Fact>]
let ``generateGitHubActions should create valid workflow`` () =
    // Arrange
    let ciConfig = {
        GenerateGitHubActions = true
        GenerateElmReview = true
        GenerateElmTest = true
        GenerateElmMake = true
    }
    
    // Act
    let workflow = generateGitHubActions ciConfig
    
    // Assert
    Assert.Contains("name: CI", workflow)
    Assert.Contains("on:", workflow)
    Assert.Contains("push:", workflow)
    Assert.Contains("pull_request:", workflow)
    Assert.Contains("Setup Elm", workflow)
    Assert.Contains("elm-format", workflow)
    Assert.Contains("elm-review", workflow)
    Assert.Contains("elm-test", workflow)

[<Fact>]
let ``generateElmReview should create valid ReviewConfig`` () =
    // Arrange & Act
    let reviewConfig = generateElmReview ()
    
    // Assert
    Assert.Contains("module ReviewConfig exposing (config)", reviewConfig)
    Assert.Contains("import Review.Rule exposing (Rule)", reviewConfig)
    Assert.Contains("NoExposingEverything.rule", reviewConfig)
    Assert.Contains("NoUnused.Variables.rule", reviewConfig)
    Assert.Contains("config : List Rule", reviewConfig)

[<Fact>]
let ``generateEditorSupport should create valid editor configurations`` () =
    // Arrange
    let doc = createTestDocument()
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act
    let vscodeSettings = generateEditorSupport doc editorConfig
    
    // Assert
    Assert.Contains("elm.formatOnSave", vscodeSettings)
    Assert.Contains("./node_modules/.bin/elm", vscodeSettings)

[<Fact>]
let ``generateFormattingConfig should create valid formatting options`` () =
    // Test Standard formatting
    let standardConfig = generateFormattingConfig FormattingOption.Standard
    Assert.Contains("\"indent\": 4", standardConfig)
    Assert.Contains("\"maxLineLength\": 120", standardConfig)
    
    // Test Compact formatting
    let compactConfig = generateFormattingConfig FormattingOption.Compact
    Assert.Contains("\"indent\": 2", compactConfig)
    Assert.Contains("\"maxLineLength\": 80", compactConfig)
    
    // Test Expanded formatting
    let expandedConfig = generateFormattingConfig FormattingOption.Expanded
    Assert.Contains("\"indent\": 4", expandedConfig)
    Assert.Contains("\"maxLineLength\": 160", expandedConfig)

[<Fact>]
let ``generateEcosystemIntegration should create all requested files`` () =
    // Arrange
    let doc = createTestDocument()
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
    let formatOption = FormattingOption.Standard
    
    // Act
    let files = generateEcosystemIntegration doc editorConfig ciConfig formatOption
    
    // Assert
    let fileNames = files |> List.map fst
    Assert.Contains("elm.json", fileNames)
    Assert.Contains(".github/workflows/ci.yml", fileNames)
    Assert.Contains("review/ReviewConfig.elm", fileNames)
    Assert.Contains(".vscode/settings.json", fileNames)
    Assert.Contains("elm-format.json", fileNames)
    Assert.Contains("package.json", fileNames)

[<Fact>]
let ``generateEcosystemIntegration should respect configuration flags`` () =
    // Arrange
    let doc = createTestDocument()
    let editorConfig = {
        GenerateElmJson = false  // Disabled
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = false  // Disabled
    }
    let ciConfig = {
        GenerateGitHubActions = false  // Disabled
        GenerateElmReview = false  // Disabled
        GenerateElmTest = true
        GenerateElmMake = true
    }
    let formatOption = FormattingOption.Standard
    
    // Act
    let files = generateEcosystemIntegration doc editorConfig ciConfig formatOption
    
    // Assert
    let fileNames = files |> List.map fst
    Assert.DoesNotContain("elm.json", fileNames)
    Assert.DoesNotContain(".github/workflows/ci.yml", fileNames)
    Assert.DoesNotContain("review/ReviewConfig.elm", fileNames)
    Assert.DoesNotContain(".vscode/settings.json", fileNames)
    Assert.Contains("elm-format.json", fileNames)  // Always generated
    Assert.Contains("package.json", fileNames)  // Generated because test flags are true

[<Fact>]
let ``PhantomType should have correct cases`` () =
    // Test phantom type definitions
    let validatedType = PhantomType.ValidatedType("TestType")
    let unvalidatedType = PhantomType.UnvalidatedType("TestType")
    let authenticatedType = PhantomType.AuthenticatedType("TestType")
    let unauthenticatedType = PhantomType.UnauthenticatedType("TestType")
    
    // Assert types can be created
    Assert.True(true)  // If we reach here, the types are valid

[<Fact>]
let ``UrlSegment should have correct cases`` () =
    // Test URL segment definitions
    let staticSegment = UrlSegment.StaticSegment("users")
    let parameterSegment = UrlSegment.ParameterSegment("id", "Int")
    let querySegment = UrlSegment.QuerySegment("filter", "String", true)
    
    // Assert types can be created
    Assert.True(true)  // If we reach here, the types are valid

[<Fact>]
let ``MiddlewareType should have correct cases`` () =
    // Test middleware type definitions
    let authMiddleware = MiddlewareType.AuthenticationMiddleware
    let validationMiddleware = MiddlewareType.ValidationMiddleware
    let loggingMiddleware = MiddlewareType.LoggingMiddleware
    let rateLimitingMiddleware = MiddlewareType.RateLimitingMiddleware
    let cachingMiddleware = MiddlewareType.CachingMiddleware
    
    // Assert types can be created
    Assert.True(true)  // If we reach here, the types are valid

[<Fact>]
let ``FormattingOption should have correct cases`` () =
    // Test formatting option definitions
    let standard = FormattingOption.Standard
    let compact = FormattingOption.Compact
    let expanded = FormattingOption.Expanded
    
    // Assert types can be created
    Assert.True(true)  // If we reach here, the types are valid

[<Fact>]
let ``EditorSupport should have correct properties`` () =
    // Test editor support record
    let editorSupport = {
        GenerateElmJson = true
        GenerateDocumentation = false
        GenerateTypeAnnotations = true
        EnableLanguageServer = false
    }
    
    // Assert properties can be accessed
    Assert.True(editorSupport.GenerateElmJson)
    Assert.False(editorSupport.GenerateDocumentation)
    Assert.True(editorSupport.GenerateTypeAnnotations)
    Assert.False(editorSupport.EnableLanguageServer)

[<Fact>]
let ``CiCdIntegration should have correct properties`` () =
    // Test CI/CD integration record
    let ciCdIntegration = {
        GenerateGitHubActions = true
        GenerateElmReview = false
        GenerateElmTest = true
        GenerateElmMake = false
    }
    
    // Assert properties can be accessed
    Assert.True(ciCdIntegration.GenerateGitHubActions)
    Assert.False(ciCdIntegration.GenerateElmReview)
    Assert.True(ciCdIntegration.GenerateElmTest)
    Assert.False(ciCdIntegration.GenerateElmMake)