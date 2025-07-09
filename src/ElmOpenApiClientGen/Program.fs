
open Argu

open ElmOpenApiClientGen
open ElmOpenApiClientGen.Generator
open ElmOpenApiClientGen.Languages

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>(
        programName = "openapi-client-gen",
        helpTextMessage = "Generate type-safe HTTP clients from OpenAPI specifications.\n\nSupport the project: https://github.com/sponsors/Yury-Zakharov"
    )

    try
        let results = parser.Parse argv

        let inputPath = results.GetResult <@ Input @>
        let outputDir = results.GetResult <@ Output @>
        let modulePrefix = results.TryGetResult <@ ModulePrefix @>
        let targetStr = results.TryGetResult <@ Target @> |> Option.defaultValue "elm"
        let customTemplate = results.TryGetResult <@ Template @>
        let force = results.Contains <@ Force @>

        // Parse target language
        let targetLanguage = 
            match GeneratorFactory.parseLanguageTarget targetStr with
            | Ok target -> target
            | Error msg -> 
                printfn $"Error: %s{msg}"
                exit 1

        // Step 1: Load and parse OpenAPI spec
        let openApiDoc =
            Parser.loadSpec inputPath
            // Parser.loadSpec : string -> OpenApiDocument

        // Step 2: Create language target
        let languageTarget = GeneratorFactory.createLanguageTarget targetLanguage

        // Step 3: Convert OpenAPI spec into modules using the target language
        let elmModules =
            Codegen.generateModules openApiDoc modulePrefix languageTarget customTemplate
            // Codegen.generateModules : OpenApiDocument -> string option -> ILanguageTarget -> string option -> ElmModule list

        // Step 4: Write generated modules to output directory
        Output.writeModules outputDir elmModules force
        // Output.writeModules : string -> ElmModule list -> bool -> unit

        0
    with
    | :? ArguParseException as ex ->
        printfn $"Error: %s{ex.Message}"
        1
    | ex ->
        printfn $"Unhandled error: %s{ex.Message}"
        2
