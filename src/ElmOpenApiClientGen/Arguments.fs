namespace ElmOpenApiClientGen

open Argu

type Arguments =
    | [<Mandatory; AltCommandLine("-i")>] Input of path:string
    | [<Mandatory; AltCommandLine("-o")>] Output of path:string
    | [<AltCommandLine("-p")>] ModulePrefix of prefix:string
    | [<AltCommandLine("-f")>] Force
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Input _ -> "Path to OpenAPI spec file (JSON or YAML) or URL to download spec"
            | Output _ -> "Output directory for generated Elm modules"
            | ModulePrefix _ -> "Optional module prefix for generated Elm modules (e.g., Api)"
            | Force -> "Overwrite existing files in output directory"
