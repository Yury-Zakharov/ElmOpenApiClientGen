namespace ElmOpenApiClientGen.Languages.Elm

open System.IO
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Generator

/// Elm language target implementation
type ElmLanguageTarget() =
    interface ILanguageTarget with
        member _.Name = "Elm"
        member _.FileExtension = "elm"
        member _.DefaultModulePrefix = "Api"
        
        member _.GenerateTypes(context: LanguageContext) =
            // For Phase 1, return placeholder types
            ["-- Types will be generated here"]
            
        member _.GenerateRequests(context: LanguageContext) =
            // For Phase 1, return placeholder requests
            [("-- Requests will be generated here", "-- Request body")]
            
        member _.GenerateErrorTypes(context: LanguageContext) =
            // For Phase 1, return placeholder error types
            ["-- Error types will be generated here"]
            
        member this.GenerateModule(context: LanguageContext) =
            // Get template content (either custom or embedded default)
            let templateContent = TemplateResolver.resolveTemplate context (this :> ILanguageTarget)
            
            // Create simple template variables for rendering
            let moduleName = 
                match context.ModulePrefix with
                | Some prefix -> $"{prefix}.Schemas"
                | None -> "Api.Schemas"
            
            // For now, do simple string replacements in the template
            templateContent
                .Replace("{{moduleName}}", moduleName)
                .Replace("{{apiDescription}}", context.ApiDescription)
                .Replace("{{generationTimestamp}}", context.GenerationTimestamp)
                .Replace("{{types}}", "-- Types will be generated here")
                .Replace("{{decoders}}", "-- Decoders will be generated here")
                .Replace("{{encoders}}", "-- Encoders will be generated here")
                .Replace("{{requests}}", "-- Requests will be generated here")
            
        member _.ValidateOutput(output: string) =
            // Basic validation - check if the output contains basic Elm module structure
            if output.Contains("module ") && output.Contains("exposing") then
                Ok ()
            else
                Error "Generated Elm code does not contain valid module structure"
                
        member _.GetOutputPath(basePath: string) (modulePrefix: string option) =
            let prefix = modulePrefix |> Option.defaultValue "Api"
            Path.Combine(basePath, prefix, "Schemas.elm")
            
        member _.GetDefaultTemplate() =
            TemplateResolver.loadEmbeddedTemplate "ElmOpenApiClientGen.Languages.Elm.Templates.module.scriban"