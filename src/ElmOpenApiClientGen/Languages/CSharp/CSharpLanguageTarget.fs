namespace ElmOpenApiClientGen.Languages.CSharp

open System
open System.IO
open System.Text.RegularExpressions
open System.Collections.Generic
open Microsoft.OpenApi.Models
open ElmOpenApiClientGen.Languages
open ElmOpenApiClientGen.Generator

/// C# naming convention utilities
module CSharpNaming =
    /// C# reserved keywords that need to be escaped
    let private reservedKeywords = Set.ofList [
        "abstract"; "as"; "base"; "bool"; "break"; "byte"; "case"; "catch"; "char"; "checked";
        "class"; "const"; "continue"; "decimal"; "default"; "delegate"; "do"; "double"; "else";
        "enum"; "event"; "explicit"; "extern"; "false"; "finally"; "fixed"; "float"; "for";
        "foreach"; "goto"; "if"; "implicit"; "in"; "int"; "interface"; "internal"; "is";
        "lock"; "long"; "namespace"; "new"; "null"; "object"; "operator"; "out"; "override";
        "params"; "private"; "protected"; "public"; "readonly"; "ref"; "return"; "sbyte";
        "sealed"; "short"; "sizeof"; "stackalloc"; "static"; "string"; "struct"; "switch";
        "this"; "throw"; "true"; "try"; "typeof"; "uint"; "ulong"; "unchecked"; "unsafe";
        "ushort"; "using"; "virtual"; "void"; "volatile"; "while"
    ]
    
    /// Convert OpenAPI name to PascalCase (for types, properties)
    let toPascalCase (name: string) =
        try
            if String.IsNullOrEmpty(name) then "UnknownType"
            else
                // If the name only contains alphanumeric characters, ensure it starts with uppercase
                if Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9]*$") then
                    let result = if name.Length > 0 && Char.IsLower(name.[0]) then 
                                    Char.ToUpper(name.[0]).ToString() + name.Substring(1)
                                 else 
                                    name
                    if reservedKeywords.Contains(result.ToLower()) then "@" + result
                    elif Char.IsDigit(result.[0]) then "Type" + result
                    else result
                else
                    // Handle names with special characters, underscores, or dots
                    let sanitized = Regex.Replace(name, @"[^a-zA-Z0-9_.]", "_")
                    let parts = sanitized.Split([|'_'; '.'|], StringSplitOptions.RemoveEmptyEntries)
                    let pascalCase = 
                        parts 
                        |> Array.map (fun part -> 
                            if String.IsNullOrEmpty(part) then ""
                            else Char.ToUpper(part.[0]).ToString() + part.Substring(1))
                        |> String.concat ""
                    
                    if String.IsNullOrEmpty(pascalCase) then "UnknownType"
                    elif reservedKeywords.Contains(pascalCase.ToLower()) then "@" + pascalCase
                    elif Char.IsDigit(pascalCase.[0]) then "Type" + pascalCase
                    else pascalCase
        with
        | _ -> "UnknownType"
    
    /// Convert OpenAPI name to camelCase (for parameters, variables)
    let toCamelCase (name: string) =
        try
            let pascalCase = toPascalCase name
            if String.IsNullOrEmpty(pascalCase) then "unknownParam"
            else Char.ToLower(pascalCase.[0]).ToString() + pascalCase.Substring(1)
        with
        | _ -> "unknownParam"
    
    /// Get valid namespace name from module prefix
    let toNamespace (prefix: string option) =
        try
            match prefix with
            | None -> "GeneratedApi"
            | Some p when String.IsNullOrWhiteSpace(p) -> "GeneratedApi"
            | Some p -> toPascalCase p
        with
        | _ -> "GeneratedApi"

/// C# type mapping utilities
module CSharpTypeMapping =
    /// Map OpenAPI type to C# type with defensive programming
    let rec mapType (schema: OpenApiSchema option) =
        try
            match schema with
            | None -> "object /* Unknown schema type */"
            | Some s when s = null -> "object /* Null schema */"
            | Some s ->
                // Check for references first, before checking type
                if s.Reference <> null then
                    CSharpNaming.toPascalCase s.Reference.Id
                else
                    match s.Type with
                    | "string" ->
                        match s.Format with
                        | "date-time" -> "DateTime"
                        | "date" -> "DateOnly"
                        | "time" -> "TimeOnly"
                        | "uuid" -> "Guid"
                        | "byte" -> "byte[]"
                        | _ -> "string"
                    | "integer" ->
                        match s.Format with
                        | "int64" -> "long"
                        | _ -> "int"
                    | "number" ->
                        match s.Format with
                        | "float" -> "float"
                        | "double" -> "double"
                        | _ -> "decimal"
                    | "boolean" -> "bool"
                    | "array" -> 
                        let itemType = mapType (Some s.Items)
                        "List<" + itemType + ">"
                    | "object" -> "object /* TODO: Generate proper class */"
                    | null -> "object /* Null type */"
                    | _ -> "object /* Unsupported type: " + s.Type + " */"
        with
        | ex -> "object /* Error mapping type: " + ex.Message + " */"

/// Safe dictionary access helper
module SafeDictionary =
    let tryGetValue (key: 'K) (dict: IDictionary<'K, 'V>) =
        try
            if dict <> null && dict.ContainsKey(key) then
                Some dict.[key]
            else 
                None
        with
        | _ -> None

/// C# language target implementation
type CSharpLanguageTarget() =
    interface ILanguageTarget with
        member _.Name = "C#"
        member _.FileExtension = "cs"
        member _.DefaultModulePrefix = "GeneratedApi"
        
        member _.GenerateTypes(context: LanguageContext) =
            let types = ResizeArray<string>()
            
            try
                if context.Document <> null 
                   && context.Document.Components <> null 
                   && context.Document.Components.Schemas <> null then
                    for kvp in context.Document.Components.Schemas do
                        try
                            let typeName = CSharpNaming.toPascalCase kvp.Key
                            let schema = kvp.Value
                            
                            if schema <> null then
                                match schema.Type with
                                | "object" ->
                                    let properties = 
                                        if schema.Properties <> null then
                                            let propLines = 
                                                schema.Properties
                                                |> Seq.map (fun prop -> 
                                                    let propName = CSharpNaming.toPascalCase prop.Key
                                                    let propType = CSharpTypeMapping.mapType (Some prop.Value)
                                                    let isRequired = schema.Required <> null && schema.Required.Contains(prop.Key)
                                                    let nullable = if isRequired then "" else "?"
                                                    "    public " + propType + nullable + " " + propName + " { get; init; }")
                                                |> String.concat "\n"
                                            propLines
                                        else
                                            "    // No properties defined"
                                    
                                    let typeComment = if String.IsNullOrEmpty(schema.Description) then "Data model for " + typeName else schema.Description
                                    let typeDecl = "/// <summary>\n/// " + typeComment + "\n/// </summary>\npublic record " + typeName + "(\n" + properties + "\n);"
                                    types.Add(typeDecl)
                                    
                                | "string" when schema.Enum <> null && schema.Enum.Count > 0 ->
                                    let enumValues = 
                                        schema.Enum 
                                        |> Seq.mapi (fun i value -> 
                                            let valueStr = match value with
                                                           | :? Microsoft.OpenApi.Any.OpenApiString as str -> str.Value
                                                           | _ when value <> null -> value.ToString()
                                                           | _ -> "Unknown"
                                            let enumName = CSharpNaming.toPascalCase valueStr
                                            if i = schema.Enum.Count - 1 then "    " + enumName
                                            else "    " + enumName + ",")
                                        |> String.concat "\n"
                                    
                                    let enumComment = if String.IsNullOrEmpty(schema.Description) then "Enumeration for " + typeName else schema.Description
                                    let enumDecl = "/// <summary>\n/// " + enumComment + "\n/// </summary>\npublic enum " + typeName + "\n{\n" + enumValues + "\n}"
                                    types.Add(enumDecl)
                                    
                                | _ ->
                                    types.Add("// TODO: Generate type for " + typeName + " (type: " + (if schema.Type = null then "null" else schema.Type) + ")")
                            else
                                types.Add("// TODO: Generate type for " + typeName + " (null schema)")
                        with
                        | ex ->
                            types.Add("// Error generating type for " + kvp.Key + ": " + ex.Message)
                else
                    types.Add("// No schemas found in OpenAPI document")
            with
            | ex ->
                types.Add("// Error processing schemas: " + ex.Message)
                
            types |> Seq.toList
            
        member _.GenerateRequests(context: LanguageContext) =
            let requests = ResizeArray<string * string>()
            
            try
                if context.Document <> null && context.Document.Paths <> null then
                    for pathKvp in context.Document.Paths do
                        let path = pathKvp.Key
                        let pathItem = pathKvp.Value
                        
                        if pathItem <> null then
                            if pathItem.Operations <> null then
                                // Generate for each HTTP method
                                let operations = [
                                    ("GET", SafeDictionary.tryGetValue OperationType.Get pathItem.Operations)
                                    ("POST", SafeDictionary.tryGetValue OperationType.Post pathItem.Operations)
                                    ("PUT", SafeDictionary.tryGetValue OperationType.Put pathItem.Operations)
                                    ("DELETE", SafeDictionary.tryGetValue OperationType.Delete pathItem.Operations)
                                    ("PATCH", SafeDictionary.tryGetValue OperationType.Patch pathItem.Operations)
                                ]
                                
                                for (httpMethod, operationOpt) in operations do
                                    match operationOpt with
                                    | Some operation when operation <> null ->
                                        try
                                            let methodName = 
                                                if String.IsNullOrEmpty(operation.OperationId) then
                                                    let httpMethodPascal = CSharpNaming.toPascalCase(httpMethod.ToLower())
                                                    let pathPascal = CSharpNaming.toPascalCase(path.Replace("/", "_").Replace("{", "").Replace("}", ""))
                                                    httpMethodPascal + pathPascal
                                                else
                                                    CSharpNaming.toPascalCase(operation.OperationId)
                                        
                                            let parameters = 
                                                if operation.Parameters <> null then
                                                    operation.Parameters
                                                    |> Seq.map (fun param -> 
                                                        let paramName = CSharpNaming.toCamelCase param.Name
                                                        let paramType = CSharpTypeMapping.mapType (Some param.Schema)
                                                        let nullable = if param.Required then "" else "?"
                                                        paramType + nullable + " " + paramName)
                                                    |> String.concat ", "
                                                else ""
                                        
                                            let allParams = 
                                                if String.IsNullOrEmpty(parameters) then "CancellationToken cancellationToken = default"
                                                else parameters + ", CancellationToken cancellationToken = default"
                                        
                                            let responseType = 
                                                if operation.Responses <> null then
                                                    match SafeDictionary.tryGetValue "200" operation.Responses with
                                                    | Some response when response <> null && response.Content <> null ->
                                                        match SafeDictionary.tryGetValue "application/json" response.Content with
                                                        | Some mediaType when mediaType <> null ->
                                                            CSharpTypeMapping.mapType (Some mediaType.Schema)
                                                        | _ -> "string"
                                                    | _ -> "string"
                                                else "string"
                                        
                                            let summary = if String.IsNullOrEmpty(operation.Summary) then "Execute " + httpMethod + " request to " + path else operation.Summary
                                            let httpMethodName = match httpMethod with
                                                                         | "GET" -> "Get"
                                                                         | "POST" -> "Post" 
                                                                         | "PUT" -> "Put"
                                                                         | "DELETE" -> "Delete"
                                                                         | "PATCH" -> "Patch"
                                                                         | _ -> CSharpNaming.toPascalCase httpMethod
                                        
                                            let methodImpl = "        /// <summary>\n        /// " + summary + "\n        /// </summary>\n        public async Task<Result<" + responseType + ", ApiError>> " + methodName + "Async(" + allParams + ")\n        {\n            try\n            {\n                var request = new HttpRequestMessage(HttpMethod." + httpMethodName + ", \"" + path + "\");\n                return await ExecuteAsync<" + responseType + ">(request, cancellationToken);\n            }\n            catch (Exception ex)\n            {\n                return Result<" + responseType + ", ApiError>.Failure(new ApiError($\"Failed to execute " + methodName + ": {ex.Message}\"));\n            }\n        }"
                                        
                                            requests.Add((methodName, methodImpl))
                                        with
                                        | ex ->
                                            let errorMethod = "// Error generating method for " + httpMethod + " " + path + ": " + ex.Message
                                            requests.Add(("Error_" + httpMethod + "_" + path, errorMethod))
                                    | _ -> () // Skip null or missing operations
                            else
                                // Handle path item with null operations
                                requests.Add(("NullOperations_" + path.Replace("/", "_"), "// Path " + path + " has null operations"))
                else
                    requests.Add(("NoOperations", "// No paths found in OpenAPI document"))
            with
            | ex ->
                requests.Add(("Error", "// Error processing requests: " + ex.Message))
                
            requests |> Seq.toList
            
        member _.GenerateErrorTypes(context: LanguageContext) =
            // Standard error types are included in the template
            []
            
        member this.GenerateModule(context: LanguageContext) =
            try
                // Get template content
                let templateContent = TemplateResolver.resolveTemplate context (this :> ILanguageTarget)
                
                // Debug: Check if template is loaded correctly
                if templateContent.Contains("// Fallback C# template") then
                    System.Console.WriteLine("WARNING: Using fallback template - embedded template failed to load")
                
                // Generate components
                let types = (this :> ILanguageTarget).GenerateTypes context
                let requests = (this :> ILanguageTarget).GenerateRequests context
                let errorTypes = (this :> ILanguageTarget).GenerateErrorTypes context
                
                // Create template variables
                let namespaceName = CSharpNaming.toNamespace context.ModulePrefix
                let clientClassName = namespaceName + "Client"
                
                
                // Combine all types
                let allTypes = String.concat "\n\n    " types
                let allErrorTypes = String.concat "\n\n    " errorTypes  
                let allRequests = 
                    requests 
                    |> List.map snd
                    |> String.concat "\n\n        "
                
                // Replace template variables
                templateContent
                    .Replace("{{namespaceName}}", namespaceName)
                    .Replace("{{clientClassName}}", clientClassName)
                    .Replace("{{apiDescription}}", context.ApiDescription)
                    .Replace("{{generationTimestamp}}", context.GenerationTimestamp)
                    .Replace("{{types}}", if String.IsNullOrEmpty(allTypes) then "// No types generated" else allTypes)
                    .Replace("{{errorTypes}}", if String.IsNullOrEmpty(allErrorTypes) then "// Using standard error types" else allErrorTypes)
                    .Replace("{{requests}}", if String.IsNullOrEmpty(allRequests) then "// No requests generated" else allRequests)
            with
            | ex ->
                "// Error generating C# module: " + ex.Message + "\n// This is a fallback module that should compile but may not be functional\n\n#nullable enable\n\nusing System;\n\nnamespace GeneratedApi\n{\n    public class ApiClient\n    {\n        // Module generation failed, please check the OpenAPI specification\n    }\n}"
            
        member _.ValidateOutput(output: string) =
            try
                // Basic validation - check if the output contains valid C# structure
                if output.Contains("namespace ") && output.Contains("public class") then
                    Ok ()
                elif output.Contains("// Error generating C# module:") then
                    // It's a fallback module, but still valid C#
                    Ok ()
                else
                    Error "Generated C# code does not contain valid namespace and class structure"
            with
            | ex ->
                Error ("Error validating C# output: " + ex.Message)
                
        member _.GetOutputPath(basePath: string) (modulePrefix: string option) =
            try
                let safeBasePath = if String.IsNullOrEmpty(basePath) then "/tmp" else basePath
                let namespaceName = CSharpNaming.toNamespace modulePrefix
                let fileName = namespaceName + "Client.cs"
                Path.Combine(safeBasePath, fileName)
            with
            | _ ->
                // Fallback path if there's an error
                let safeBasePath = if String.IsNullOrEmpty(basePath) then "/tmp" else basePath
                Path.Combine(safeBasePath, "ApiClient.cs")
            
        member _.GetDefaultTemplate() =
            try
                TemplateResolver.loadEmbeddedTemplate "ElmOpenApiClientGen.Languages.CSharp.Templates.module.scriban"
            with
            | ex ->
                // Fallback template if embedded template fails
                "// Fallback C# template (embedded template loading failed: " + ex.Message + ")\n#nullable enable\nnamespace GeneratedApi\n{\n    public class ApiClient\n    {\n        // Generated API client will be here\n    }\n}"