namespace ElmOpenApiClientGen.Languages

open System.Collections.Generic
open Microsoft.OpenApi.Models

/// Represents a target language for code generation
type LanguageTarget = 
    | Elm
    | Haskell
    | CSharp

/// Context information passed to language-specific generators
type LanguageContext = {
    Document: OpenApiDocument
    ModulePrefix: string option
    OutputPath: string
    Force: bool
    ApiDescription: string
    GenerationTimestamp: string
    CustomTemplatePath: string option
}

/// Interface for language-specific code generation implementations
type ILanguageTarget =
    /// Human-readable name of the target language
    abstract member Name: string
    
    /// File extension for generated files (e.g., "elm", "hs")
    abstract member FileExtension: string
    
    /// Default module prefix for this language
    abstract member DefaultModulePrefix: string
    
    /// Generate type definitions from OpenAPI document
    abstract member GenerateTypes: LanguageContext -> string list
    
    /// Generate HTTP client request functions from OpenAPI document
    abstract member GenerateRequests: LanguageContext -> (string * string) list
    
    /// Generate error type definitions from OpenAPI document
    abstract member GenerateErrorTypes: LanguageContext -> string list
    
    /// Generate the complete module content
    abstract member GenerateModule: LanguageContext -> string
    
    /// Validate the generated output (syntax checking, etc.)
    abstract member ValidateOutput: string -> Result<unit, string>
    
    /// Get the output file path for a module
    abstract member GetOutputPath: string -> string option -> string
    
    /// Get the default embedded template content for this language
    abstract member GetDefaultTemplate: unit -> string