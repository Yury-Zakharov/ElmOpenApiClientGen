namespace ElmOpenApiClientGen.Languages

open System.IO
open System.Reflection

/// Utilities for resolving template content from embedded resources or custom files
module TemplateResolver =
    
    /// Load template content from embedded resource
    let loadEmbeddedTemplate (resourceName: string) : string =
        try
            let assembly = Assembly.GetExecutingAssembly()
            use stream = assembly.GetManifestResourceStream(resourceName)
            if stream = null then
                failwith $"Embedded resource '{resourceName}' not found"
            use reader = new StreamReader(stream)
            reader.ReadToEnd()
        with
        | ex -> failwith $"Failed to load embedded template '{resourceName}': {ex.Message}"
    
    /// Load template content from custom file path
    let loadCustomTemplate (filePath: string) : string =
        try
            if not (File.Exists(filePath)) then
                failwith $"Custom template file '{filePath}' not found"
            File.ReadAllText(filePath)
        with
        | ex -> failwith $"Failed to load custom template '{filePath}': {ex.Message}"
    
    /// Resolve template content based on context - either custom file or embedded default
    let resolveTemplate (context: LanguageContext) (languageTarget: ILanguageTarget) : string =
        match context.CustomTemplatePath with
        | Some customPath -> loadCustomTemplate customPath
        | None -> languageTarget.GetDefaultTemplate()
    
    /// Validate template content contains required placeholders
    let validateTemplate (templateContent: string) (requiredPlaceholders: string list) : Result<unit, string> =
        let missing = 
            requiredPlaceholders
            |> List.filter (fun placeholder -> not (templateContent.Contains(placeholder)))
        
        if missing.IsEmpty then
            Ok ()
        else
            let missingStr = String.concat ", " missing
            Error $"Template is missing required placeholders: {missingStr}"