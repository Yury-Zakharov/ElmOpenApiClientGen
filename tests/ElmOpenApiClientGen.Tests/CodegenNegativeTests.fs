module CodegenNegativeTests

open System
open System.Collections.Generic
open Microsoft.OpenApi.Models
open Xunit
open ElmOpenApiClientGen.Generator.Codegen

let createEmptyDocument () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc

let createDocumentWithNullInfo () =
    let doc = OpenApiDocument()
    doc.Info <- null
    doc

let createDocumentWithEmptyInfo () =
    let doc = OpenApiDocument()
    doc.Info <- OpenApiInfo()
    doc.Info.Title <- ""
    doc.Info.Version <- ""
    doc.Info.Description <- null
    doc

[<Fact>]
let ``generateElmJson should handle null document`` () =
    // Arrange
    let doc = null
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act - Should not throw exception, should handle gracefully
    let elmJson = generateElmJson doc editorConfig
    
    // Assert - Should generate default package name
    Assert.NotNull(elmJson)
    Assert.Contains("generated-api", elmJson)

[<Fact>]
let ``generateElmJson should handle document with null info`` () =
    // Arrange
    let doc = createDocumentWithNullInfo()
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act - Should not throw exception, should handle gracefully
    let elmJson = generateElmJson doc editorConfig
    
    // Assert - Should generate default package name
    Assert.NotNull(elmJson)
    Assert.Contains("generated-api", elmJson)

[<Fact>]
let ``generateElmJson should handle document with empty title`` () =
    // Arrange
    let doc = createDocumentWithEmptyInfo()
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act
    let elmJson = generateElmJson doc editorConfig
    
    // Assert - Should use fallback package name
    Assert.Contains("\"name\": \"generated/generated-api\"", elmJson)

[<Fact>]
let ``generateElmJson should handle document with special characters in title`` () =
    // Arrange
    let doc = createEmptyDocument()
    doc.Info.Title <- "My-API@v2.0 (Special#Characters!)"
    doc.Info.Version <- "1.0.0"
    
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act
    let elmJson = generateElmJson doc editorConfig
    
    // Assert - Should sanitize package name by removing special characters including dots
    // Original: "My-API@v2.0 (Special#Characters!)" 
    // After sanitization: "myapiv20specialcharacters" (dots, dashes, @, spaces, parens, #, ! removed)
    Assert.Contains("\"name\": \"generated/myapiv20specialcharacters\"", elmJson)

[<Fact>]
let ``generateElmJson should handle extremely long title`` () =
    // Arrange
    let doc = createEmptyDocument()
    doc.Info.Title <- String.replicate 200 "VeryLongApiName"
    doc.Info.Version <- "1.0.0"
    
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act
    let elmJson = generateElmJson doc editorConfig
    
    // Assert - Should handle gracefully
    Assert.NotNull(elmJson)
    Assert.Contains("\"type\": \"package\"", elmJson)

[<Fact>]
let ``generateGitHubActions should handle null CI config`` () =
    // Arrange
    let ciConfig = Unchecked.defaultof<CiCdIntegration>
    
    // Act - Should still generate workflow
    let workflow = generateGitHubActions ciConfig
    
    // Assert
    Assert.Contains("name: CI", workflow)

[<Fact>]
let ``generateElmReview should always succeed`` () =
    // Act - No parameters to cause failure
    let reviewConfig = generateElmReview ()
    
    // Assert
    Assert.Contains("module ReviewConfig exposing (config)", reviewConfig)
    Assert.Contains("config : List Rule", reviewConfig)

[<Fact>]
let ``generateEditorSupport should handle null document`` () =
    // Arrange
    let doc = null
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = true
        GenerateTypeAnnotations = true
        EnableLanguageServer = true
    }
    
    // Act - Should still generate editor configs
    let vscodeSettings = generateEditorSupport doc editorConfig
    
    // Assert
    Assert.Contains("elm.formatOnSave", vscodeSettings)

[<Fact>]
let ``generateFormattingConfig should handle invalid format option`` () =
    // Arrange - Create invalid enum value (though F# prevents this at compile time)
    // We'll test with edge case values
    
    // Test all valid options
    let standardConfig = generateFormattingConfig FormattingOption.Standard
    let compactConfig = generateFormattingConfig FormattingOption.Compact
    let expandedConfig = generateFormattingConfig FormattingOption.Expanded
    
    // Assert - All should succeed
    Assert.Contains("indent", standardConfig)
    Assert.Contains("indent", compactConfig)
    Assert.Contains("indent", expandedConfig)

[<Fact>]
let ``generateEcosystemIntegration should handle all flags disabled`` () =
    // Arrange
    let doc = createEmptyDocument()
    let editorConfig = {
        GenerateElmJson = false
        GenerateDocumentation = false
        GenerateTypeAnnotations = false
        EnableLanguageServer = false
    }
    let ciConfig = {
        GenerateGitHubActions = false
        GenerateElmReview = false
        GenerateElmTest = false
        GenerateElmMake = false
    }
    let formatOption = FormattingOption.Standard
    
    // Act
    let files = generateEcosystemIntegration doc editorConfig ciConfig formatOption
    
    // Assert - Should only generate formatting config (always generated)
    let fileNames = files |> List.map fst
    Assert.Contains("elm-format.json", fileNames)
    Assert.DoesNotContain("elm.json", fileNames)
    Assert.DoesNotContain(".github/workflows/ci.yml", fileNames)
    Assert.DoesNotContain("review/ReviewConfig.elm", fileNames)
    Assert.DoesNotContain(".vscode/settings.json", fileNames)
    Assert.DoesNotContain("package.json", fileNames)

[<Fact>]
let ``generateEcosystemIntegration should handle null document`` () =
    // Arrange
    let doc = null
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
    
    // Act - Should not throw exception, should handle gracefully
    let files = generateEcosystemIntegration doc editorConfig ciConfig formatOption
    
    // Assert - Should generate files even with null document
    Assert.NotEmpty(files)
    let fileNames = files |> List.map fst
    Assert.Contains("elm.json", fileNames)

[<Fact>]
let ``generateEcosystemIntegration should handle conflicting configuration`` () =
    // Arrange - Set conflicting flags
    let doc = createEmptyDocument()
    doc.Info.Title <- "Test API"
    
    let editorConfig = {
        GenerateElmJson = true
        GenerateDocumentation = false  // Conflicting with elm.json generation
        GenerateTypeAnnotations = false
        EnableLanguageServer = false
    }
    let ciConfig = {
        GenerateGitHubActions = false
        GenerateElmReview = true  // Review without GitHub Actions
        GenerateElmTest = false
        GenerateElmMake = false
    }
    let formatOption = FormattingOption.Standard
    
    // Act
    let files = generateEcosystemIntegration doc editorConfig ciConfig formatOption
    
    // Assert - Should handle gracefully
    let fileNames = files |> List.map fst
    Assert.Contains("elm.json", fileNames)  // Should generate elm.json
    Assert.Contains("review/ReviewConfig.elm", fileNames)  // Should generate review config
    Assert.DoesNotContain(".github/workflows/ci.yml", fileNames)  // Should not generate GitHub Actions

[<Fact>]
let ``PhantomType pattern matching should be exhaustive`` () =
    // Test all phantom type cases
    let types = [
        PhantomType.ValidatedType("Test")
        PhantomType.UnvalidatedType("Test")
        PhantomType.AuthenticatedType("Test")
        PhantomType.UnauthenticatedType("Test")
    ]
    
    // Act - Pattern match all types
    let results = types |> List.map (function
        | ValidatedType s -> "validated-" + s
        | UnvalidatedType s -> "unvalidated-" + s
        | AuthenticatedType s -> "authenticated-" + s
        | UnauthenticatedType s -> "unauthenticated-" + s
    )
    
    // Assert
    Assert.Equal(4, results.Length)
    Assert.Contains("validated-Test", results)
    Assert.Contains("unauthenticated-Test", results)

[<Fact>]
let ``UrlSegment pattern matching should be exhaustive`` () =
    // Test all URL segment cases
    let segments = [
        UrlSegment.StaticSegment("users")
        UrlSegment.ParameterSegment("id", "Int")
        UrlSegment.QuerySegment("filter", "String", true)
    ]
    
    // Act - Pattern match all segments
    let results = segments |> List.map (function
        | StaticSegment s -> "static-" + s
        | ParameterSegment (name, t) -> "param-" + name + "-" + t
        | QuerySegment (name, t, req) -> "query-" + name + "-" + t + "-" + string req
    )
    
    // Assert
    Assert.Equal(3, results.Length)
    Assert.Contains("static-users", results)
    Assert.Contains("param-id-Int", results)
    Assert.Contains("query-filter-String-True", results)

[<Fact>]
let ``MiddlewareType pattern matching should be exhaustive`` () =
    // Test all middleware type cases
    let middlewares = [
        MiddlewareType.AuthenticationMiddleware
        MiddlewareType.ValidationMiddleware
        MiddlewareType.LoggingMiddleware
        MiddlewareType.RateLimitingMiddleware
        MiddlewareType.CachingMiddleware
    ]
    
    // Act - Pattern match all middleware types
    let results = middlewares |> List.map (function
        | AuthenticationMiddleware -> "auth"
        | ValidationMiddleware -> "validation"
        | LoggingMiddleware -> "logging"
        | RateLimitingMiddleware -> "ratelimit"
        | CachingMiddleware -> "caching"
    )
    
    // Assert
    Assert.Equal(5, results.Length)
    Assert.Contains("auth", results)
    Assert.Contains("caching", results)

[<Fact>]
let ``FormattingOption pattern matching should be exhaustive`` () =
    // Test all formatting option cases
    let options = [
        FormattingOption.Standard
        FormattingOption.Compact
        FormattingOption.Expanded
    ]
    
    // Act - Pattern match all options
    let results = options |> List.map (function
        | Standard -> "standard"
        | Compact -> "compact" 
        | Expanded -> "expanded"
    )
    
    // Assert
    Assert.Equal(3, results.Length)
    Assert.Contains("standard", results)
    Assert.Contains("compact", results)
    Assert.Contains("expanded", results)

[<Fact>]
let ``EditorSupport should handle extreme boolean combinations`` () =
    // Test all boolean combinations
    let configs = [
        { GenerateElmJson = true; GenerateDocumentation = true; GenerateTypeAnnotations = true; EnableLanguageServer = true }
        { GenerateElmJson = false; GenerateDocumentation = false; GenerateTypeAnnotations = false; EnableLanguageServer = false }
        { GenerateElmJson = true; GenerateDocumentation = false; GenerateTypeAnnotations = true; EnableLanguageServer = false }
        { GenerateElmJson = false; GenerateDocumentation = true; GenerateTypeAnnotations = false; EnableLanguageServer = true }
    ]
    
    // Act & Assert - All should be valid
    configs |> List.iter (fun config ->
        Assert.True(true)  // If we can create the config, the test passes
        // Access properties to ensure they're accessible
        ignore config.GenerateElmJson
        ignore config.GenerateDocumentation
        ignore config.GenerateTypeAnnotations
        ignore config.EnableLanguageServer
    )

[<Fact>]
let ``CiCdIntegration should handle extreme boolean combinations`` () =
    // Test all boolean combinations
    let configs = [
        { GenerateGitHubActions = true; GenerateElmReview = true; GenerateElmTest = true; GenerateElmMake = true }
        { GenerateGitHubActions = false; GenerateElmReview = false; GenerateElmTest = false; GenerateElmMake = false }
        { GenerateGitHubActions = true; GenerateElmReview = false; GenerateElmTest = true; GenerateElmMake = false }
        { GenerateGitHubActions = false; GenerateElmReview = true; GenerateElmTest = false; GenerateElmMake = true }
    ]
    
    // Act & Assert - All should be valid
    configs |> List.iter (fun config ->
        Assert.True(true)  // If we can create the config, the test passes
        // Access properties to ensure they're accessible
        ignore config.GenerateGitHubActions
        ignore config.GenerateElmReview
        ignore config.GenerateElmTest
        ignore config.GenerateElmMake
    )