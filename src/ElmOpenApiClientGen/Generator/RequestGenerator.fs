namespace ElmOpenApiClientGen.Generator

open System.Collections.Generic
open Microsoft.OpenApi.Models

module RequestGenerator =

    /// Safely normalize operation ID with comprehensive defensive programming
    let private normalizeOperationId (opId: string) =
        try
            if System.String.IsNullOrWhiteSpace(opId) then
                "unknownOperation"
            else
                // Remove all special characters and keep only alphanumeric
                let normalized = System.Text.RegularExpressions.Regex.Replace(opId, @"[^a-zA-Z0-9]", "")
                if System.String.IsNullOrEmpty(normalized) then 
                    "unknownOperation" 
                else 
                    normalized
        with
        | _ -> "unknownOperation"

    /// Safely determine Elm type for schema with null checks
    let private elmTypeForSchema (schema: OpenApiSchema) =
        try
            if schema = null then "String" else
            match schema.Type with
            | null -> "String"
            | "integer" -> "Int"
            | "number" -> "Float"  
            | "boolean" -> "Bool"
            | "string" -> "String"
            | _ -> "String"
        with
        | _ -> "String"

    /// Safely determine encoding function for schema with null checks
    let private encodeFnForSchema (schema: OpenApiSchema) =
        try
            if schema = null then "identity" else
            match schema.Type with
            | null -> "identity"
            | "integer" -> "String.fromInt"
            | "number" -> "String.fromFloat"
            | "boolean" -> "(\\b -> if b then \"true\" else \"false\")"
            | "string" -> "identity"
            | _ -> "identity"
        with
        | _ -> "identity"

    /// Safely build URL expression with defensive programming
    let private buildUrl (path: string) (pathParams: OpenApiParameter list) =
        try
            let safePath = if System.String.IsNullOrEmpty(path) then "" else path
            let safePathParams = pathParams
            
            if System.String.IsNullOrEmpty(safePath) then
                "\"\""
            else
                let segments = safePath.Split('/')
                let parts =
                    segments
                    |> Array.map (fun segment ->
                        try
                            if System.String.IsNullOrEmpty(segment) then
                                ""
                            elif segment.StartsWith("{") && segment.EndsWith("}") then
                                let paramName = segment.Trim([| '{'; '}' |])
                                let paramType = 
                                    safePathParams 
                                    |> List.tryFind (fun p -> p <> null && p.Name = paramName)
                                    |> Option.bind (fun p -> 
                                        if p.Schema = null then None 
                                        else 
                                            try Some p.Schema.Type
                                            with _ -> None)
                                    |> Option.defaultValue "string"
                                let encoder = 
                                    match paramType with
                                    | "integer" -> "String.fromInt"
                                    | "number" -> "String.fromFloat"
                                    | "boolean" -> "(\\b -> if b then \"true\" else \"false\")"
                                    | _ -> "identity"
                                $"\"/\" ++ {encoder} {paramName}"
                            else
                                $"\"/{segment}\""
                        with
                        | _ -> $"\"/{segment}\""
                    )
                    |> Array.filter (fun s -> not (System.String.IsNullOrEmpty(s)))
                    
                if parts.Length = 0 then
                    "\"\""
                else
                    String.concat " ++ " parts
        with
        | _ -> "\"\""

    /// Safely generate parameter declarations with comprehensive null checks
    let private generateParams (parameters: OpenApiParameter list) (requestBody: OpenApiRequestBody option) =
        try
            let safeParameters = parameters
            let pathParams = 
                safeParameters 
                |> List.choose (fun p -> 
                    if p = null then None
                    else
                        try Some (elmTypeForSchema p.Schema)
                        with _ -> Some "String")
                        
            let bodyParam = 
                match requestBody with
                | Some body when body <> null ->
                    try
                        if body.Content <> null && body.Content.ContainsKey("application/json") then
                            let media = body.Content["application/json"]
                            if media = null || media.Schema = null then ["Json.Encode.Value"]
                            else
                                match media.Schema.Reference with
                                | null -> ["Json.Encode.Value"]
                                | ref when not (System.String.IsNullOrEmpty(ref.Id)) -> [ref.Id]
                                | _ -> ["Json.Encode.Value"]
                        else ["Json.Encode.Value"]
                    with
                    | _ -> ["Json.Encode.Value"]
                | _ -> []
            
            // Include query parameters in function signatures for proper type safety
            let queryParamTypes = 
                safeParameters 
                |> List.choose (fun p -> 
                    if p = null then None
                    else
                        try 
                            if p.In.HasValue && p.In.Value = ParameterLocation.Query then
                                Some (elmTypeForSchema p.Schema)
                            else None
                        with _ -> None)
            
            let allParams = pathParams @ queryParamTypes @ bodyParam
            if allParams.IsEmpty then "" else String.concat " -> " allParams
        with
        | _ -> ""

    /// Safely generate function arguments with null checks
    let private generateArgs (parameters: OpenApiParameter list) (hasBody: bool) =
        try
            let safeParameters = parameters
            let pathArgs = 
                safeParameters 
                |> List.choose (fun p -> 
                    if p = null then Some "unknownParam"
                    else
                        try 
                            if System.String.IsNullOrEmpty(p.Name) then Some "unknownParam" 
                            else Some p.Name
                        with _ -> Some "unknownParam")
                        
            let bodyArg = if hasBody then ["body"] else []
            let allArgs = pathArgs @ bodyArg
            String.concat " " allArgs
        with
        | _ -> ""

    /// Safely parse status code with TryParse instead of Parse
    let private tryParseStatusCode (statusCodeStr: string) =
        if System.String.IsNullOrEmpty(statusCodeStr) then None 
        else
            try
                match System.Int32.TryParse(statusCodeStr) with
                | true, code -> Some code
                | false, _ -> None
            with
            | _ -> None

    /// Main request generation function with comprehensive defensive programming
    let generateRequest (path: string) (method: string) (operation: OpenApiOperation) (doc: OpenApiDocument) =
        try
            // Defensive programming: handle all null inputs gracefully
            let safePath = if System.String.IsNullOrEmpty(path) then "/unknown-path" else path
            let safeMethod = if System.String.IsNullOrEmpty(method) then "GET" else method
            
            if operation = null then
                let errorComment = $"-- ERROR: Operation is null for path %s{safePath} %s{safeMethod}"
                (errorComment, errorComment + "\n-- Please check your OpenAPI specification", None)
            elif doc = null then
                let errorComment = $"-- ERROR: Document is null for %s{safeMethod} %s{safePath}"
                (errorComment, errorComment + "\n-- Please provide a valid OpenAPI document", None)
            else
                let pathParams =
                    try
                        if operation.Parameters = null then []
                        else
                            operation.Parameters
                            |> Seq.filter (fun p -> 
                                p <> null && 
                                try p.In.HasValue && p.In.Value = ParameterLocation.Path
                                with _ -> false)
                            |> Seq.toList
                    with
                    | _ -> []

                let queryParams =
                    try
                        if operation.Parameters = null then []
                        else
                            operation.Parameters
                            |> Seq.filter (fun p -> 
                                p <> null && 
                                try p.In.HasValue && p.In.Value = ParameterLocation.Query
                                with _ -> false)
                            |> Seq.toList
                    with
                    | _ -> []

                let hasRequestBody = 
                    try operation.RequestBody <> null
                    with _ -> false

                // Analyze security requirements with defensive programming
                let securityRequirements = 
                    try
                        if operation.Security <> null && operation.Security.Count > 0 then
                            operation.Security |> Seq.toList
                        elif doc.SecurityRequirements <> null && doc.SecurityRequirements.Count > 0 then
                            doc.SecurityRequirements |> Seq.toList
                        else
                            []
                    with
                    | _ -> []

                // Always include Config for production features (custom headers, timeout, base URL)
                let needsConfig = true
                let configParam = if needsConfig then ["Config"] else []
                let configArg = if needsConfig then ["config"] else []

                let generateHeaders () =
                    try
                        let headers = ResizeArray<string>()
                        
                        // Add authentication headers if security requirements exist
                        if securityRequirements.Length > 0 then
                            try
                                if doc.Components <> null && doc.Components.SecuritySchemes <> null then
                                    for schemeEntry in doc.Components.SecuritySchemes do
                                        try
                                            if schemeEntry.Value <> null then
                                                match schemeEntry.Value.Type with
                                                | SecuritySchemeType.ApiKey when schemeEntry.Value.In = ParameterLocation.Header ->
                                                    let headerName = 
                                                        if System.String.IsNullOrEmpty(schemeEntry.Value.Name) then "X-API-Key" 
                                                        else schemeEntry.Value.Name
                                                    headers.Add($"Http.header \"{headerName}\" config.apiKey")
                                                | SecuritySchemeType.Http when not (System.String.IsNullOrEmpty(schemeEntry.Value.Scheme)) && schemeEntry.Value.Scheme.ToLower() = "bearer" ->
                                                    headers.Add("Http.header \"Authorization\" (\"Bearer \" ++ config.bearerToken)")
                                                | SecuritySchemeType.Http when not (System.String.IsNullOrEmpty(schemeEntry.Value.Scheme)) && schemeEntry.Value.Scheme.ToLower() = "basic" ->
                                                    headers.Add("Http.header \"Authorization\" (\"Basic \" ++ config.basicAuth)")
                                                | _ -> ()
                                        with
                                        | _ -> ()
                            with
                            | _ -> ()
                        
                        // Always add custom headers from config for production features
                        headers.Add("List.map (\\(name, value) -> Http.header name value) config.customHeaders")
                        
                        if headers.Count = 0 then "[]"
                        elif headers.Count = 1 && not (headers[0].StartsWith("List.map")) then 
                            "[" + String.concat ", " headers + "]"
                        else 
                            let fixedHeaders = headers |> Seq.filter (fun h -> not (h.StartsWith("List.map"))) |> List.ofSeq
                            let customHeadersExpr = headers |> Seq.tryFind _.StartsWith("List.map")
                            match customHeadersExpr with
                            | Some customExpr ->
                                if fixedHeaders.IsEmpty then customExpr
                                else "[" + String.concat ", " fixedHeaders + "] ++ " + customExpr
                            | None ->
                                if fixedHeaders.IsEmpty then "[]"
                                else "[" + String.concat ", " fixedHeaders + "]"
                    with
                    | _ -> "[]"

                // Analyze all responses to determine success and error types with defensive programming
                let analyzeResponses (responses: IDictionary<string, OpenApiResponse>) =
                    try
                        if responses = null then ([], []) else
                        
                        let successResponses = 
                            try
                                responses
                                |> Seq.filter (fun (kvp: KeyValuePair<string, OpenApiResponse>) -> 
                                    kvp.Value <> null &&
                                    match tryParseStatusCode kvp.Key with
                                    | Some statusCode -> statusCode >= 200 && statusCode < 300
                                    | None -> false)
                                |> Seq.toList
                            with
                            | _ -> []
                        
                        let errorResponses = 
                            try
                                responses
                                |> Seq.filter (fun (kvp: KeyValuePair<string, OpenApiResponse>) -> 
                                    kvp.Value <> null &&
                                    match tryParseStatusCode kvp.Key with
                                    | Some statusCode -> statusCode >= 400
                                    | None -> false)
                                |> Seq.toList
                            with
                            | _ -> []
                        
                        (successResponses, errorResponses)
                    with
                    | _ -> ([], [])
                
                let successResponses, errorResponses = 
                    try
                        if operation.Responses = null then 
                            ([], [])
                        else 
                            analyzeResponses operation.Responses
                    with
                    | _ -> ([], [])
                
                let funcName =
                    try
                        if System.String.IsNullOrWhiteSpace(operation.OperationId) then
                            let cleanPath = safePath.Replace("/", "_").Replace("{", "").Replace("}", "")
                            $"{safeMethod}{cleanPath}"
                        else
                            normalizeOperationId operation.OperationId
                    with
                    | _ -> "unknownOperation"
                
                // Determine primary success response type with defensive programming
                let successType =
                    try
                        let primarySuccessResponse = 
                            successResponses
                            |> List.tryFind (fun (kvp: KeyValuePair<string, OpenApiResponse>) -> kvp.Key = "200" || kvp.Key = "201")
                            |> Option.orElse (successResponses |> List.tryHead)
                        
                        match primarySuccessResponse with
                        | Some kvp when kvp.Value <> null ->
                            try
                                if kvp.Value.Content <> null && kvp.Value.Content.ContainsKey("application/json") then
                                    let media = kvp.Value.Content["application/json"]
                                    if media = null || media.Schema = null then "Decode.Value"
                                    else
                                        match media.Schema.Reference with
                                        | null -> "Decode.Value"
                                        | ref when not (System.String.IsNullOrEmpty(ref.Id)) -> ref.Id
                                        | _ -> "Decode.Value"
                                elif kvp.Key = "204" then
                                    "()"
                                else
                                    "Decode.Value"
                            with
                            | _ -> "Decode.Value"
                        | _ -> "Decode.Value"
                    with
                    | _ -> "Decode.Value"
                
                // Generate error type based on error responses
                let errorType = 
                    try
                        if errorResponses.IsEmpty then
                            "Http.Error"
                        else
                            let capitalizedFuncName = 
                                if funcName.Length > 0 then
                                    funcName.Substring(0,1).ToUpper() + (if funcName.Length > 1 then funcName.Substring(1) else "")
                                else
                                    "Unknown"
                            $"{capitalizedFuncName}Error"
                    with
                    | _ -> "Http.Error"
                
                let paramDecl = generateParams (pathParams @ queryParams) (Option.ofObj operation.RequestBody)
                let fullParamDecl = 
                    try
                        let baseParams = if System.String.IsNullOrEmpty paramDecl then [] else [paramDecl]
                        let allParams = (configParam @ baseParams)
                        if allParams.IsEmpty then "" else String.concat " -> " allParams
                    with
                    | _ -> ""
                        
                let args = generateArgs (pathParams @ queryParams) hasRequestBody  
                let fullArgs = 
                    try
                        String.concat " " (configArg @ (if System.String.IsNullOrEmpty args then [] else [args]))
                    with
                    | _ -> ""
                    
                let pathExpr = buildUrl path pathParams
                let urlExpr = $"config.baseUrl ++ {pathExpr}"

                let queryParamExpr = 
                    try
                        if queryParams.IsEmpty then ""
                        else 
                            let queryItems = 
                                queryParams
                                |> List.choose (fun p -> 
                                    if p = null then None
                                    else
                                        try
                                            let paramName = if System.String.IsNullOrEmpty(p.Name) then "unknownParam" else p.Name
                                            let encoder = encodeFnForSchema p.Schema
                                            Some $"Url.string \"{paramName}\" ({encoder} {paramName})"
                                        with
                                        | _ -> None)
                                |> String.concat ", "
                            if System.String.IsNullOrEmpty(queryItems) then ""
                            else $" ++ Url.toQuery [{queryItems}]"
                    with
                    | _ -> ""

                let bodyExpr = 
                    try
                        if hasRequestBody && operation.RequestBody <> null then
                            try
                                if operation.RequestBody.Content <> null then
                                    match operation.RequestBody.Content.TryGetValue("application/json") with
                                    | true, media when media <> null ->
                                        try
                                            if media.Schema <> null then
                                                match media.Schema.Reference with
                                                | null -> "Http.jsonBody body"
                                                | ref when not (System.String.IsNullOrEmpty(ref.Id)) -> $"Http.jsonBody (encode{ref.Id} body)"
                                                | _ -> "Http.jsonBody body"
                                            else
                                                "Http.jsonBody body"
                                        with
                                        | _ -> "Http.jsonBody body"
                                    | _ -> "Http.emptyBody"
                                else "Http.emptyBody"
                            with
                            | _ -> "Http.emptyBody"
                        else "Http.emptyBody"
                    with
                    | _ -> "Http.emptyBody"

                let httpMethodString = 
                    try
                        match safeMethod.ToUpper() with
                        | "GET" -> "GET"
                        | "POST" -> "POST"
                        | "PUT" -> "PUT"
                        | "DELETE" -> "DELETE"
                        | "PATCH" -> "PATCH"
                        | _ -> "GET"
                    with
                    | _ -> "GET"

                // Generate enhanced HTTP resolver that handles multiple status codes
                let generateResolver() =
                    try
                        let allStatusCodes = 
                            try
                                (successResponses @ errorResponses) |> List.map _.Key |> List.distinct
                            with
                            | _ -> []
                        
                        let generateStatusCase (statusCode: string) =
                            try
                                let successMatch = successResponses |> List.tryFind (fun kvp -> kvp.Key = statusCode)
                                let errorMatch = errorResponses |> List.tryFind (fun kvp -> kvp.Key = statusCode)
                                
                                match successMatch, errorMatch with
                                | Some successKvp, _ ->
                                    try
                                        // Success case
                                        if successKvp.Value <> null && successKvp.Value.Content <> null && successKvp.Value.Content.ContainsKey("application/json") then
                                            let media = successKvp.Value.Content["application/json"]
                                            let decoderName = 
                                                if media = null || media.Schema = null then "Decode.value"
                                                else
                                                    match media.Schema.Reference with
                                                    | null -> "Decode.value"
                                                    | ref when not (System.String.IsNullOrEmpty(ref.Id)) -> $"decoder{ref.Id}"
                                                    | _ -> "Decode.value"
                                            $"                {statusCode} ->\n                    case Decode.decodeString {decoderName} responseBody of\n                        Ok value -> Ok value\n                        Err err -> Err (Http.BadBody (Decode.errorToString err))"
                                        else
                                            $"                {statusCode} -> Ok ()" // No content responses
                                    with
                                    | _ -> $"                {statusCode} -> Ok ()"
                                | None, Some errorKvp ->
                                    try
                                        // Error case
                                        if errorKvp.Value <> null && errorKvp.Value.Content <> null && errorKvp.Value.Content.ContainsKey("application/json") then
                                            let media = errorKvp.Value.Content["application/json"]
                                            let errorDecoderName = 
                                                if media = null || media.Schema = null then "Decode.value"
                                                else
                                                    match media.Schema.Reference with
                                                    | null -> "Decode.value"
                                                    | ref when not (System.String.IsNullOrEmpty(ref.Id)) -> $"decoder{ref.Id}"
                                                    | _ -> "Decode.value"
                                            let capitalizedFuncName = 
                                                if funcName.Length > 0 then
                                                    funcName.Substring(0,1).ToUpper() + (if funcName.Length > 1 then funcName.Substring(1) else "")
                                                else
                                                    "Unknown"
                                            $"                {statusCode} ->\n                    case Decode.decodeString {errorDecoderName} responseBody of\n                        Ok errorData -> Err ({capitalizedFuncName}Error{statusCode} errorData)\n                        Err _ -> Err ({capitalizedFuncName}ErrorUnknown responseBody)"
                                        else
                                            let capitalizedFuncName = 
                                                if funcName.Length > 0 then
                                                    funcName.Substring(0,1).ToUpper() + (if funcName.Length > 1 then funcName.Substring(1) else "")
                                                else
                                                    "Unknown"
                                            $"                {statusCode} -> Err ({capitalizedFuncName}Error{statusCode} \"\")"
                                    with
                                    | _ -> $"                {statusCode} -> Err (Http.BadStatus {statusCode})"
                                | None, None -> ""
                            with
                            | _ -> ""
                        
                        let allCasesWithDefault = 
                            try
                                let cases = allStatusCodes |> List.map generateStatusCase |> List.filter (fun s -> s <> "") |> String.concat "\n"
                                cases + "\n                _ -> Err (Http.BadStatus metadata.statusCode)"
                            with
                            | _ -> "                _ -> Err (Http.BadStatus metadata.statusCode)"
                        
                        $"""Http.stringResolver (\\response -> 
            case response of
                Http.BadUrl_ url -> Err (Http.BadUrl url)
                Http.Timeout_ -> Err Http.Timeout
                Http.NetworkError_ -> Err Http.NetworkError
                Http.BadStatus_ metadata responseBody ->
                    case metadata.statusCode of
{allCasesWithDefault}
                Http.GoodStatus_ metadata responseBody ->
                    case metadata.statusCode of
{allCasesWithDefault}
            )"""
                    with
                    | _ -> "Http.stringResolver (\\response -> Ok \"\")"
                
                // Generate simple documentation for the function
                let funcDoc = 
                    try
                        if not (System.String.IsNullOrEmpty(operation.Summary)) then
                            $"{{-| %s{operation.Summary} -}}"
                        elif not (System.String.IsNullOrEmpty(operation.Description)) then
                            $"{{-| %s{operation.Description} -}}"
                        else ""
                    with
                    | _ -> ""

                let funcSig =
                    try
                        let baseSig = 
                            if System.String.IsNullOrEmpty fullParamDecl then
                                $"%s{funcName} : Task %s{errorType} %s{successType}"
                            else
                                $"%s{funcName} : %s{fullParamDecl} -> Task %s{errorType} %s{successType}"

                        if funcDoc <> "" then
                            $"%s{funcDoc}\n%s{baseSig}"
                        else
                            baseSig
                    with
                    | _ -> $"%s{funcName} : Task Http.Error String"

                let resolver = generateResolver()
                let headers = generateHeaders()
                
                let funcDef =
                    try
                        if System.String.IsNullOrEmpty fullArgs then
                            $"""
{funcName} =
    Http.task
        {{ method = "{httpMethodString}"
        , headers = {headers}
        , url = {urlExpr}{queryParamExpr}
        , body = {bodyExpr}
        , resolver = {resolver}
        , timeout = config.timeout
        }}
"""
                        else
                            $"""
{funcName} {fullArgs} =
    Http.task
        {{ method = "{httpMethodString}"
        , headers = {headers}
        , url = {urlExpr}{queryParamExpr}
        , body = {bodyExpr}
        , resolver = {resolver}
        , timeout = config.timeout
        }}
"""
                    with
                    | _ -> $"""
{funcName} = -- ERROR: Could not generate function definition
    Http.task
        {{ method = "GET"
        , headers = []
        , url = ""
        , body = Http.emptyBody
        , resolver = Http.stringResolver (\\response -> Ok "")
        , timeout = Nothing
        }}
"""
                
                // Generate enhanced error type definition with documentation
                let errorTypeDef = 
                    try
                        if errorResponses.IsEmpty then
                            None
                        else
                            let capitalizedFuncName = 
                                if funcName.Length > 0 then
                                    funcName.Substring(0,1).ToUpper() + (if funcName.Length > 1 then funcName.Substring(1) else "")
                                else
                                    "Unknown"
                            
                            // Generate simple documentation for error type
                            let errorDoc = $"{{-| Error types for %s{funcName} operation -}}"

                            let errorCases = 
                                try
                                    errorResponses
                                    |> List.mapi (fun _ (kvp: KeyValuePair<string, OpenApiResponse>) ->
                                        try
                                            let statusCode = kvp.Key
                                            if kvp.Value <> null && kvp.Value.Content <> null && kvp.Value.Content.ContainsKey("application/json") then
                                                let media = kvp.Value.Content["application/json"]
                                                if media = null || media.Schema = null then
                                                    $"    %s{capitalizedFuncName}Error%s{statusCode} String"
                                                // Generic error
                                                else
                                                    match media.Schema.Reference with
                                                    | null -> $"    %s{capitalizedFuncName}Error%s{statusCode} String"
                                                    // Generic error
                                                    | ref when not (System.String.IsNullOrEmpty(ref.Id)) ->
                                                        $"    %s{capitalizedFuncName}Error%s{statusCode} %s{ref.Id}"
                                                    // Typed error
                                                    | _ -> $"    %s{capitalizedFuncName}Error%s{statusCode} String"
                                            // Generic error
                                            else
                                                $"    %s{capitalizedFuncName}Error%s{statusCode} String"
                                        // No content error
                                        with
                                        | _ -> $"    %s{capitalizedFuncName}ErrorUnknown String"
                                    )
                                    |> fun cases -> cases @ [ $"    %s{capitalizedFuncName}ErrorUnknown String" ]
                                    |> List.mapi (fun i case -> if i = 0 then
                                                                    $"%s{errorDoc}\ntype %s{errorType} =\n%s{case}" else
                                                                $"    | %s{case}")
                                    |> String.concat "\n"
                                with
                                | _ -> $"%s{errorDoc}\ntype %s{errorType} =\n    %s{capitalizedFuncName}ErrorUnknown String"

                            Some errorCases
                    with
                    | _ -> None
                
                funcSig, funcDef, errorTypeDef
        with
        | ex -> 
            let errorComment = $"-- ERROR: Exception in generateRequest: %s{ex.Message}"
            (errorComment, errorComment + "\n-- Please check your inputs and try again", None)

    /// Enhanced request generation with type-safe URLs and middleware (defensive wrapper)
    let generateEnhancedRequest (path: string) (method: string) (operation: OpenApiOperation) (doc: OpenApiDocument) =
        try
            let basicSig, basicDef, errorType = generateRequest path method operation doc
            
            // For now, return the basic request - advanced features integration will be done in Codegen module
            let enhancedComponents : string list = []
            let enhancedDef = basicDef
            
            (basicSig, enhancedDef, errorType, enhancedComponents)
        with
        | ex ->
            let errorComment = $"-- ERROR: Exception in generateEnhancedRequest: %s{ex.Message}"
            (errorComment, errorComment, None, [])

    /// Generate all requests from document with comprehensive defensive programming
    let generateRequests (doc: OpenApiDocument) =
        try
            if doc = null then
                ([], [])
            elif doc.Paths = null then
                ([], [])
            else
                try
                    for kvp in doc.Paths do
                        printfn $" - %s{kvp.Key}"
                with
                | _ -> ()

                let results =
                    try
                        doc.Paths
                        |> Seq.collect (fun kvp ->
                            try
                                let path = kvp.Key
                                let item = kvp.Value
                                if item = null then
                                    []
                                else
                                    [ OperationType.Get, "get"
                                      OperationType.Post, "post"
                                      OperationType.Put, "put"
                                      OperationType.Delete, "delete"
                                      OperationType.Patch, "patch" ]
                                    |> List.choose (fun (opType, methodName) ->
                                        try
                                            match item.Operations.TryGetValue(opType) with
                                            | true, op -> Some (generateRequest path methodName op doc)
                                            | _ -> None
                                        with
                                        | _ -> None)
                            with
                            | _ -> []
                        )
                        |> Seq.filter (fun (signature, _, _) -> not (System.String.IsNullOrEmpty signature))
                        |> Seq.toList
                    with
                    | _ -> []

                let requests = results |> List.map (fun (signature, def, _) -> (signature, def))
                let errorTypes = results |> List.choose (fun (_, _, errorType) -> errorType)

                (requests, errorTypes)
        with
        | ex ->
            let errorComment = $"-- ERROR: Exception in generateRequests: %s{ex.Message}"
            ([(errorComment, errorComment)], [])
        
    /// Enhanced version that includes advanced type system features (defensive wrapper)
    let generateRequestsWithAdvancedFeatures (doc: OpenApiDocument) =
        try
            if doc = null then
                ([], [], [])
            elif doc.Paths = null then
                ([], [], [])
            else
                try
                    for kvp in doc.Paths do
                        printfn $" - %s{kvp.Key}"
                with
                | _ -> ()

                let results =
                    try
                        doc.Paths
                        |> Seq.collect (fun kvp ->
                            try
                                let path = kvp.Key
                                let item = kvp.Value
                                if item = null then
                                    []
                                else
                                    [ OperationType.Get, "get"
                                      OperationType.Post, "post"
                                      OperationType.Put, "put"
                                      OperationType.Delete, "delete"
                                      OperationType.Patch, "patch" ]
                                    |> List.choose (fun (opType, methodName) ->
                                        try
                                            match item.Operations.TryGetValue(opType) with
                                            | true, op -> Some (generateEnhancedRequest path methodName op doc)
                                            | _ -> None
                                        with
                                        | _ -> None)
                            with
                            | _ -> []
                        )
                        |> Seq.filter (fun (signature, _, _, _) -> not (System.String.IsNullOrEmpty signature))
                        |> Seq.toList
                    with
                    | _ -> []

                let requests = results |> List.map (fun (signature, def, _, _) -> (signature, def))
                let errorTypes = results |> List.choose (fun (_, _, errorType, _) -> errorType)
                let advancedFeatures = results |> List.collect (fun (_, _, _, features) -> features)

                (requests, errorTypes, advancedFeatures)
        with
        | ex ->
            let errorComment = $"-- ERROR: Exception in generateRequestsWithAdvancedFeatures: %s{ex.Message}"
            ([(errorComment, errorComment)], [], [])