#r "src/ElmOpenApiClientGen/bin/Debug/net10.0/ElmOpenApiClientGen.dll"
#r "src/ElmOpenApiClientGen/bin/Debug/net10.0/Microsoft.OpenApi.dll"
#r "src/ElmOpenApiClientGen/bin/Debug/net10.0/Microsoft.OpenApi.Readers.dll"

open System.IO
open Microsoft.OpenApi.Readers
open ElmOpenApiClientGen.Generator.Codegen

// Load the OpenAPI spec
let openApiSpec = File.ReadAllText("sample/test-advanced-type-system.yaml")
let reader = OpenApiStringReader()
let (doc, diagnostics) = reader.Read(openApiSpec)

// Configure ecosystem integration
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

// Generate ecosystem integration files
let ecosystemFiles = generateEcosystemIntegration doc editorConfig ciConfig formatOption

// Display the generated files
printfn "Generated ecosystem integration files:"
for (filename, content) in ecosystemFiles do
    printfn "\n=== %s ===" filename
    printfn "%s" content
    printfn ""