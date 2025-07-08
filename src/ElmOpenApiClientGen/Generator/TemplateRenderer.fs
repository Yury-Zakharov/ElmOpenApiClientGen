namespace ElmOpenApiClientGen.Generator

open System.IO
open Scriban

module TemplateRenderer =

    type ElmTemplateContext =
        {
            ModuleName: string
            Types: string list
            Decoders: string list
            Encoders: string list
            Requests: string list
            ApiDescription: string
            GenerationTimestamp: string
            TypeDocumentation: string list
            FunctionDocumentation: string list
            // Import optimization flags
            NeedsDict: bool
            NeedsHttp: bool
            NeedsJsonDecode: bool
            NeedsAndMap: bool
            NeedsJsonEncode: bool
            NeedsTask: bool
            NeedsUrl: bool
        }

    let private loadTemplate (path: string) : Template =
        let resolvedPath = 
            if File.Exists(path) then 
                path
            else
                // Try to resolve relative to the application's directory
                let appDir = System.AppDomain.CurrentDomain.BaseDirectory
                let appRelativePath = Path.Combine(appDir, path)
                if File.Exists(appRelativePath) then
                    appRelativePath
                else
                    // Try to resolve relative to the current working directory
                    let workingDir = Directory.GetCurrentDirectory()
                    let workingRelativePath = Path.Combine(workingDir, path)
                    if File.Exists(workingRelativePath) then
                        workingRelativePath
                    else
                        // If all else fails, use the original path and let it fail with a clear error
                        path
        
        try
            let content = File.ReadAllText resolvedPath
            Template.Parse(content)
        with
        | :? FileNotFoundException ->
            failwith $"Template file not found: {path}. Searched in: {resolvedPath}"
        | ex ->
            failwith $"Error loading template from {resolvedPath}: {ex.Message}"

    open Scriban.Runtime

    /// Renders a single Elm module using the Scriban template and provided context
    let renderModule (templatePath: string) (context: ElmTemplateContext) : string =
        let template = loadTemplate templatePath

        let scriptObj = ScriptObject()
        scriptObj.Add("moduleName", context.ModuleName)
        scriptObj.Add("types", context.Types)
        scriptObj.Add("decoders", context.Decoders)
        scriptObj.Add("encoders", context.Encoders)
        scriptObj.Add("requests", context.Requests)
        scriptObj.Add("apiDescription", context.ApiDescription)
        scriptObj.Add("generationTimestamp", context.GenerationTimestamp)
        scriptObj.Add("typeDocumentation", context.TypeDocumentation)
        scriptObj.Add("functionDocumentation", context.FunctionDocumentation)
        // Import optimization flags
        scriptObj.Add("needsDict", context.NeedsDict)
        scriptObj.Add("needsHttp", context.NeedsHttp)
        scriptObj.Add("needsJsonDecode", context.NeedsJsonDecode)
        scriptObj.Add("needsAndMap", context.NeedsAndMap)
        scriptObj.Add("needsJsonEncode", context.NeedsJsonEncode)
        scriptObj.Add("needsTask", context.NeedsTask)
        scriptObj.Add("needsUrl", context.NeedsUrl)

        
        let scribanContext = TemplateContext()
        scribanContext.PushGlobal(scriptObj)

        template.Render(scribanContext)
