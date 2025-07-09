namespace ElmOpenApiClientGen

open Argu

type Arguments =
    | [<Mandatory; AltCommandLine("-i")>] Input of path:string
    | [<Mandatory; AltCommandLine("-o")>] Output of path:string
    | [<AltCommandLine("-p")>] ModulePrefix of prefix:string
    | [<AltCommandLine("-t")>] Target of language:string
    | [<AltCommandLine("-T")>] Template of path:string
    | [<AltCommandLine("-f")>] Force
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Input _ -> "Path to OpenAPI spec file (JSON or YAML) or URL to download spec"
            | Output _ -> "Output directory for generated modules"
            | ModulePrefix _ -> "Optional module prefix for generated modules (e.g., Api)"
            | Target _ -> "Target language for code generation (default: elm)"
            | Template _ -> "Path to custom template file (uses embedded default if not specified)"
            | Force -> "Overwrite existing files in output directory"
