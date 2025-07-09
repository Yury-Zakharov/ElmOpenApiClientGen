namespace ElmOpenApiClientGen.Languages.Haskell

open System.IO
open ElmOpenApiClientGen.Languages
open Microsoft.OpenApi.Models

/// Haskell language target implementation
type HaskellLanguageTarget() =
    interface ILanguageTarget with
        member _.Name = "Haskell"
        
        member _.FileExtension = "hs"
        
        member _.DefaultModulePrefix = "Api"
        
        member _.GenerateTypes(context: LanguageContext) =
            // For Phase 2, return placeholder types
            ["-- Types will be generated here"]
            
        member _.GenerateRequests(context: LanguageContext) =
            // For Phase 2, return placeholder requests
            [("-- Requests will be generated here", "-- Request body")]
            
        member _.GenerateErrorTypes(context: LanguageContext) =
            // For Phase 2, return placeholder error types
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
                .Replace("{{jsonInstances}}", "-- JSON instances will be generated here")
                .Replace("{{httpFunctions}}", "-- HTTP functions will be generated here")
            
        member _.ValidateOutput(output: string) =
            // Basic validation - check if the output contains basic Haskell module structure
            if output.Contains("module ") && output.Contains("where") then
                Ok ()
            else
                Error "Generated Haskell code does not contain valid module structure"
                
        member _.GetOutputPath(basePath: string) (modulePrefix: string option) =
            let prefix = modulePrefix |> Option.defaultValue "Api"
            Path.Combine(basePath, prefix, "Schemas.hs")
            
        member _.GetDefaultTemplate() =
            TemplateResolver.loadEmbeddedTemplate "ElmOpenApiClientGen.Languages.Haskell.Templates.module.scriban"