
open Argu

open ElmOpenApiClientGen
open ElmOpenApiClientGen.Generator

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>(
        programName = "elm-openapi-gen",
        helpTextMessage = "Generate type-safe Elm HTTP clients from OpenAPI specifications.\n\nSupport the project: https://github.com/sponsors/Yury-Zakharov"
    )

    try
        let results = parser.Parse argv

        let inputPath = results.GetResult <@ Input @>
        let outputDir = results.GetResult <@ Output @>
        let modulePrefix = results.TryGetResult <@ ModulePrefix @>
        let force = results.Contains <@ Force @>

        // Step 1: Load and parse OpenAPI spec
        let openApiDoc =
            Parser.loadSpec inputPath
            // Parser.loadSpec : string -> OpenApiDocument

        // Step 2: Convert OpenAPI spec into Elm modules
        let elmModules =
            Codegen.generateModules openApiDoc modulePrefix
            // Codegen.generateModules : OpenApiDocument -> string option -> ElmModule list

        // Step 3: Write generated Elm modules to output directory
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
