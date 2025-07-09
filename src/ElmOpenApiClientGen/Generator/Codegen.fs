namespace ElmOpenApiClientGen.Generator

open System.Collections.Generic
open Microsoft.OpenApi.Any
open Microsoft.OpenApi.Models
open ElmOpenApiClientGen.Generator
open ElmOpenApiClientGen.Languages

module Codegen =

    open TemplateRenderer
    
    // Advanced Type System Features for Step 5
    
    // Phantom type definitions for compile-time safety
    type PhantomType = 
        | ValidatedType of string
        | UnvalidatedType of string
        | AuthenticatedType of string
        | UnauthenticatedType of string
        
    // Type-safe URL builder patterns
    type UrlSegment =
        | StaticSegment of string
        | ParameterSegment of string * string // name, type
        | QuerySegment of string * string * bool // name, type, required
        
    // Middleware patterns for request/response processing
    type MiddlewareType =
        | AuthenticationMiddleware
        | ValidationMiddleware
        | LoggingMiddleware
        | RateLimitingMiddleware
        | CachingMiddleware
        
    // Ecosystem Integration Features for Step 7
    
    // Elm-format integration
    type FormattingOption =
        | Standard
        | Compact
        | Expanded
        
    // Editor support configuration
    type EditorSupport = {
        GenerateElmJson: bool
        GenerateDocumentation: bool
        GenerateTypeAnnotations: bool
        EnableLanguageServer: bool
    }
    
    // CI/CD integration options
    type CiCdIntegration = {
        GenerateGitHubActions: bool
        GenerateElmReview: bool
        GenerateElmTest: bool
        GenerateElmMake: bool
    }
    
    // Ecosystem integration generation functions
    
    // Generate elm.json configuration
    let generateElmJson (doc: OpenApiDocument) (config: EditorSupport) =
        let packageName = 
            if doc = null then
                "generated-api"
            elif doc.Info = null then
                "generated-api"
            elif System.String.IsNullOrEmpty(doc.Info.Title) then
                "generated-api"
            else
                // Sanitize package name by removing special characters
                let sanitized = doc.Info.Title.Replace(" ", "").Replace("-", "").Replace(".", "").Replace("@", "").Replace("#", "").Replace("!", "").Replace("(", "").Replace(")", "").ToLower()
                if System.String.IsNullOrEmpty(sanitized) then "generated-api" else sanitized
        
        let dependencies = [
            "\"elm/core\": \"1.0.5\""
            "\"elm/json\": \"1.1.3\""
            "\"elm/http\": \"2.0.0\""
            "\"elm/url\": \"1.0.0\""
            "\"elm/time\": \"1.0.0\""
        ]
        
        let testDependencies = [
            "\"elm-explorations/test\": \"1.2.2\""
            "\"elm-community/json-extra\": \"4.3.0\""
        ]
        
        let elmJson = 
            sprintf "{\n    \"type\": \"package\",\n    \"name\": \"generated/%s\",\n    \"summary\": \"Generated Elm API client from OpenAPI specification\",\n    \"license\": \"MIT\",\n    \"version\": \"1.0.0\",\n    \"exposed-modules\": [\n        \"Api.Schemas\"\n    ],\n    \"elm-version\": \"0.19.0 <= v < 0.20.0\",\n    \"dependencies\": {\n        %s\n    },\n    \"test-dependencies\": {\n        %s\n    }\n}" packageName (String.concat ",\n        " dependencies) (String.concat ",\n        " testDependencies)
        
        elmJson
    
    // Generate GitHub Actions workflow
    let generateGitHubActions (ciConfig: CiCdIntegration) =
        let workflow = "name: CI\n\non:\n  push:\n    branches: [ main ]\n  pull_request:\n    branches: [ main ]\n\njobs:\n  test:\n    runs-on: ubuntu-latest\n    \n    steps:\n    - uses: actions/checkout@v3\n    \n    - name: Setup Elm\n      uses: jorelali/setup-elm@v5\n      with:\n        elm-version: 0.19.1\n    \n    - name: Install dependencies\n      run: elm make --optimize --output=/dev/null src/Api/Schemas.elm\n    \n    - name: Run elm-format\n      run: |\n        npm install -g elm-format\n        elm-format --validate src/\n    \n    - name: Run elm-review\n      run: |\n        npm install -g elm-review\n        elm-review\n    \n    - name: Run tests\n      run: |\n        elm-test"
        workflow
    
    // Generate elm-review configuration
    let generateElmReview () =
        let reviewConfig = "module ReviewConfig exposing (config)\n\nimport Review.Rule exposing (Rule)\nimport NoExposingEverything\nimport NoImportingEverything\nimport NoMissingTypeAnnotation\nimport NoUnused.CustomTypeConstructors\nimport NoUnused.Dependencies\nimport NoUnused.Exports\nimport NoUnused.Modules\nimport NoUnused.Parameters\nimport NoUnused.Patterns\nimport NoUnused.Variables\n\nconfig : List Rule\nconfig =\n    [ NoExposingEverything.rule\n    , NoImportingEverything.rule []\n    , NoMissingTypeAnnotation.rule\n    , NoUnused.CustomTypeConstructors.rule []\n    , NoUnused.Dependencies.rule\n    , NoUnused.Exports.rule\n    , NoUnused.Modules.rule\n    , NoUnused.Parameters.rule\n    , NoUnused.Patterns.rule\n    , NoUnused.Variables.rule\n    ]"
        reviewConfig
    
    // Generate editor support files
    let generateEditorSupport (doc: OpenApiDocument) (config: EditorSupport) =
        let vscodeSettings = "{\n    \"elm.formatOnSave\": true,\n    \"elm.makeSpecialFunctions\": [\"main\"],\n    \"elm.compiler\": \"./node_modules/.bin/elm\",\n    \"elm.makeCommand\": \"./node_modules/.bin/elm make\"\n}"
        
        vscodeSettings
    
    // Generate formatting configuration
    let generateFormattingConfig (option: FormattingOption) =
        let config = 
            match option with
            | Standard -> "{\"indent\": 4, \"maxLineLength\": 120}"
            | Compact -> "{\"indent\": 2, \"maxLineLength\": 80}"
            | Expanded -> "{\"indent\": 4, \"maxLineLength\": 160}"
        
        config
    
    // Main ecosystem integration function
    let generateEcosystemIntegration (doc: OpenApiDocument) (editorConfig: EditorSupport) (ciConfig: CiCdIntegration) (formatOption: FormattingOption) =
        let mutable files = []
        
        // Generate elm.json if requested
        if editorConfig.GenerateElmJson then
            let elmJson = generateElmJson doc editorConfig
            files <- ("elm.json", elmJson) :: files
        
        // Generate GitHub Actions workflow if requested
        if ciConfig.GenerateGitHubActions then
            let workflow = generateGitHubActions ciConfig
            files <- (".github/workflows/ci.yml", workflow) :: files
        
        // Generate elm-review configuration if requested
        if ciConfig.GenerateElmReview then
            let reviewConfig = generateElmReview ()
            files <- ("review/ReviewConfig.elm", reviewConfig) :: files
        
        // Generate editor support files if requested
        if editorConfig.EnableLanguageServer then
            let vscodeSettings = generateEditorSupport doc editorConfig
            files <- (".vscode/settings.json", vscodeSettings) :: files
        
        // Generate formatting configuration
        let formattingConfig = generateFormattingConfig formatOption
        files <- ("elm-format.json", formattingConfig) :: files
        
        // Generate package.json for CI/CD dependencies
        if ciConfig.GenerateElmTest || ciConfig.GenerateElmReview then
            let packageJson = "{\n  \"name\": \"generated-api-client\",\n  \"version\": \"1.0.0\",\n  \"description\": \"Generated Elm API client\",\n  \"scripts\": {\n    \"test\": \"elm-test\",\n    \"format\": \"elm-format --yes src/\",\n    \"review\": \"elm-review\",\n    \"build\": \"elm make src/Api/Schemas.elm --optimize --output=elm.js\"\n  },\n  \"devDependencies\": {\n    \"elm-test\": \"^0.19.1-revision7\",\n    \"elm-format\": \"^0.8.5\",\n    \"elm-review\": \"^2.10.2\"\n  }\n}"
            files <- ("package.json", packageJson) :: files
        
        List.rev files
    
    let private toElmEnum (name: string) (schema: OpenApiSchema) =
        let enumValues = schema.Enum

        if enumValues = null || enumValues.Count = 0 then
            None
        else
            let toCase (v: IOpenApiAny) =
                let value = (v :?> OpenApiString).Value
                value.Substring(0,1).ToUpper() + value.Substring(1).Replace("-", "_").Replace(" ", "_")

            let toStringLiteral (v: IOpenApiAny) =
                let value = (v :?> OpenApiString).Value
                value

            let cases = enumValues |> Seq.map toCase |> Seq.distinct |> Seq.toList
            let rawValues = enumValues |> Seq.map toStringLiteral |> Seq.toList

            // Elm union type
            let typeDef =
                match cases with
                | [] -> $"type {name} = -- No valid cases"
                | firstCase :: restCases ->
                    let firstLine = $"type {name} =\n    {firstCase}"
                    let otherLines = restCases |> List.map (fun c -> $"    | {c}") |> String.concat "\n"
                    if restCases.IsEmpty then firstLine else firstLine + "\n" + otherLines

            // Decoder
            let decoderCases =
                List.zip rawValues cases
                |> List.map (fun (raw, case) -> $"                    \"{raw}\" -> Decode.succeed {case}")
                |> String.concat "\n"

            let decoder =
                $"decoder{name} : Decoder {name}\n" +
                $"decoder{name} =\n" +
                $"    Decode.string\n" +
                $"        |> Decode.andThen\n" +
                $"            (\\str ->\n" +
                $"                case str of\n" +
                $"{decoderCases}\n" +
                $"                    _ -> Decode.fail (\"Invalid {name}: \" ++ str)\n" +
                $"            )"

            // Encoder
            let encoderCases =
                List.zip cases rawValues
                |> List.map (fun (case, raw) -> $"    {case} -> Encode.string \"{raw}\"")
                |> String.concat "\n"

            let encoder =
                $"encode{name} : {name} -> Value\n" +
                $"encode{name} value =\n" +
                $"    case value of\n" +
                $"{encoderCases}"

            Some (typeDef, decoder, encoder)

    // JSON Schema draft 2020-12 features
    let private hasConditionalSchema (schema: OpenApiSchema) =
        // Check for if/then/else constructs
        (schema.Extensions.ContainsKey("if") && schema.Extensions.ContainsKey("then")) ||
        schema.Extensions.ContainsKey("conditionalSchema")
        
    let private hasPatternProperties (schema: OpenApiSchema) =
        schema.Extensions.ContainsKey("patternProperties")
        
    let private isConstSchema (schema: OpenApiSchema) =
        schema.Extensions.ContainsKey("const")
        
    let private isNullType (schema: OpenApiSchema) =
        schema.Type = "null" || 
        (schema.Extensions.ContainsKey("type") && 
         schema.Extensions["type"].ToString().Contains("null"))
         
    // Advanced schema features
    let private isRecursiveType (schema: OpenApiSchema) (typeName: string) =
        // Check if schema references itself (direct recursion)
        let rec checkRecursion (s: OpenApiSchema) (visited: Set<string>) =
            if s.Reference <> null then
                let refId = s.Reference.Id.Split('/') |> Array.last
                if visited.Contains(refId) then true
                elif refId = typeName then true
                else false
            elif s.Properties <> null then
                s.Properties
                |> Seq.exists (fun kvp -> checkRecursion kvp.Value (visited.Add(typeName)))
            elif s.Items <> null then
                checkRecursion s.Items (visited.Add(typeName))
            else false
        checkRecursion schema Set.empty
        
    let private hasAdditionalProperties (schema: OpenApiSchema) =
        schema.AdditionalProperties <> null ||
        schema.Extensions.ContainsKey("additionalProperties")
    
// Enhanced type name generation with OpenAPI 3.1 / JSON Schema draft 2020-12 support
    let rec private toElmTypeName (schema: OpenApiSchema) =
        if not (isNull schema.Reference) then
            // Reference type name from last segment in reference path
            let refPath = schema.Reference.Id  // or schema.Reference.ReferenceV3
            let parts = refPath.Split('/')
            parts[parts.Length - 1]
        elif isConstSchema schema then
            // JSON Schema const values become singleton types
            "ConstValue" // Could be enhanced to generate specific const types
        elif isNullType schema then
            // Handle explicit null types from JSON Schema draft 2020-12
            "Maybe ()" // Represents nullable unit
        elif hasConditionalSchema schema then
            // Handle if/then/else schemas
            "ConditionalType" // Placeholder for conditional schema handling
        elif schema.Type = "object" && not (isNull schema.Properties) && schema.Properties.Count > 0 then
            // For inline objects, generate a new unique type name or inline type (for now assume inline alias)
            "InlineObject" // Placeholder — you can enhance this to generate nested types properly
        elif schema.Type = "array" then
            $"List {toElmTypeName schema.Items}"
        else
            // Enhanced primitive types mapping with format support and JSON Schema draft 2020-12
            let format = if isNull schema.Format then None else Some schema.Format
            match schema.Type, format with
            | "integer", Some "int32" -> "Int"
            | "integer", Some "int64" -> "Int" // Elm doesn't distinguish int sizes
            | "integer", _ -> "Int"
            | "number", Some "float" -> "Float"
            | "number", Some "double" -> "Float"
            | "number", _ -> "Float"
            | "boolean", _ -> "Bool"
            | "string", Some "date-time" -> "DateTime"
            | "string", Some "date" -> "Date"
            | "string", Some "time" -> "Time"
            | "string", Some "uuid" -> "Uuid"
            | "string", Some "uri" -> "Uri"
            | "string", Some "uri-reference" -> "UriReference"
            | "string", Some "email" -> "Email"
            | "string", Some "hostname" -> "Hostname"
            | "string", Some "ipv4" -> "IPv4"
            | "string", Some "ipv6" -> "IPv6"
            | "string", Some "byte" -> "Base64String"
            | "string", Some "binary" -> "BinaryString"
            | "string", Some "password" -> "Password"
            | "string", _ -> "String"
            | "null", _ -> "Maybe ()" // JSON Schema null type
            | _ -> "String"

    // Handle Elm reserved words by converting them to safe field names
    let private toSafeFieldName (name: string) =
        match name with
        | "type" -> "type_"
        | "if" -> "if_"
        | "then" -> "then_"
        | "else" -> "else_"
        | "case" -> "case_"
        | "of" -> "of_"
        | "let" -> "let_"
        | "in" -> "in_"
        | "module" -> "module_"
        | "import" -> "import_"
        | "exposing" -> "exposing_"
        | "as" -> "as_"
        | "where" -> "where_"
        | "port" -> "port_"
        | _ -> name

    let private toElmField (name: string) (schema: OpenApiSchema) =
        let rec resolveElmType (s: OpenApiSchema) =
            if s.Reference <> null then
                // It's a $ref — return referenced type name
                let refPath = s.Reference.Id
                let parts = refPath.Split('/')
                parts[parts.Length - 1]

            elif s.Type = "object" then
                if s.Properties <> null && s.Properties.Count > 0 then
                    // For inline objects, generate a unique type name based on field name
                    let typeName = name.Substring(0,1).ToUpper() + name.Substring(1) + "Object"
                    typeName
                else
                    // Empty object — treat as JSON dictionary?
                    "Dict String Value" // or "Encode.Value", depending on desired behavior

            elif s.Type = "array" then
                let itemType = resolveElmType s.Items
                $"List {itemType}"

            else
                let format = if isNull s.Format then None else Some s.Format
                match s.Type, format with
                | "integer", _ -> "Int"
                | "number", _ -> "Float"
                | "boolean", _ -> "Bool"
                | "string", Some "date-time" -> "DateTime"
                | "string", Some "uuid" -> "Uuid"
                | "string", Some "uri" -> "Uri"
                | "string", Some "email" -> "Email"
                | "string", _ -> "String"
                | _ -> "String" // Fallback

        let baseType = resolveElmType schema

        let elmType =
            if schema.Nullable then
                $"Maybe {baseType}"
            else
                baseType

        let safeName = toSafeFieldName name
        $"{safeName} : {elmType}"
        
    let private toTypeAlias (name: string) (schema: OpenApiSchema) =
        let isRecursive = isRecursiveType schema name
        
        // Generate simple documentation comment
        let documentation = 
            if not (System.String.IsNullOrEmpty(schema.Description)) then
                $"{{-| %s{schema.Description} -}}"
            else ""
        
        let fields =
            schema.Properties
            |> Seq.map (fun kvp -> 
                let field = toElmField kvp.Key kvp.Value
                // Handle recursive fields by making them lazy
                if isRecursive && field.EndsWith($" : {name}") then
                    field.Replace( $" : {name}", $" : (() -> {name})")
                else field)
            |> String.concat "\n    , "
            
        // Add additionalProperties support
        let additionalPropsField =
            if hasAdditionalProperties schema then
                if schema.Properties.Count > 0 then
                    "\n    , additionalProperties : Dict String Json.Decode.Value"
                else
                    "additionalProperties : Dict String Json.Decode.Value"
            else ""
            
        let typeDefinition = $"type alias %s{name} =\n    {{ %s{fields}%s{additionalPropsField}\n    }}"
        if documentation <> "" then
            $"%s{documentation}\n%s{typeDefinition}"
        else
            typeDefinition
    
    let private toDecoder (name: string) (schema: OpenApiSchema) =
        let isRecursive = isRecursiveType schema name
        let hasAdditionalProps = hasAdditionalProperties schema
        
        let props =
            schema.Properties
            |> Seq.toList

        let decodeField (kvp: KeyValuePair<string, OpenApiSchema>) =
            let propName = kvp.Key
            let propSchema = kvp.Value

            let baseDecoder =
                if propSchema.Reference <> null then
                    // It's a reference type - use the appropriate decoder
                    let refPath = propSchema.Reference.Id
                    let parts = refPath.Split('/')
                    let typeName = parts[parts.Length - 1]
                    // Handle recursive types with lazy evaluation
                    if isRecursive && typeName = name then
                        $"Decode.lazy (\\_ -> decoder%s{typeName})"
                    else
                        $"decoder%s{typeName}"
                elif propSchema.Type = "object" && propSchema.Properties <> null && propSchema.Properties.Count > 0 then
                    // Inline object - use generated type decoder
                    let typeName = propName.Substring(0,1).ToUpper() + propName.Substring(1) + "Object"
                    $"decoder%s{typeName}"
                else
                    let format = if isNull propSchema.Format then None else Some propSchema.Format
                    match propSchema.Type, format with
                    | "array", _ ->
                        match propSchema.Items with
                        | null -> "Decode.list Decode.string"
                        | item when item.Reference <> null ->
                            let refPath = item.Reference.Id
                            let parts = refPath.Split('/')
                            let typeName = parts[parts.Length - 1]
                            $"Decode.list decoder{typeName}"
                        | item when item.Type = "integer" -> "Decode.list Decode.int"
                        | item when item.Type = "number" -> "Decode.list Decode.float"
                        | item when item.Type = "boolean" -> "Decode.list Decode.bool"
                        | item when item.Type = "string" -> "Decode.list Decode.string"
                        | _ -> "Decode.list Decode.string"
                    | "integer", _ -> "Decode.int"
                    | "number", _ -> "Decode.float"
                    | "boolean", _ -> "Decode.bool"
                    | "string", Some "date-time" -> "decodeDateTimeFromString"
                    | "string", Some "uuid" -> "decodeUuidFromString"
                    | "string", Some "uri" -> "decodeUriFromString"
                    | "string", Some "email" -> "decodeEmailFromString"
                    | "string", _ -> "Decode.string"
                    | _ -> "Decode.string"

            let fullDecoder =
                if propSchema.Nullable then
                    $"Decode.nullable ({baseDecoder})"
                else
                    baseDecoder

            $"(Decode.field \"{propName}\" ({fullDecoder}))"

        let fields = props |> List.map decodeField
        
        // Add additionalProperties decoder if needed
        let allFields = 
            if hasAdditionalProps then
                fields @ ["(Decode.field \"additionalProperties\" (Decode.dict Decode.value) |> Decode.maybe |> Decode.map (Maybe.withDefault Dict.empty))"]
            else fields

        let decoderBody = 
            if allFields.Length <= 8 then
                // Use Decode.mapN for 8 or fewer fields
                let mapFn = $"Decode.map{allFields.Length} {name}"
                let joinedFields =
                    allFields
                    |> List.map (fun f -> "        " + f) // indent each decoder field line
                    |> String.concat "\n"
                $"    {mapFn}\n{joinedFields}"
            else
                // Use Decode.succeed with andMap for more than 8 fields
                let andMapFields =
                    allFields
                    |> List.map (fun f -> "        |> andMap " + f)
                    |> String.concat "\n"
                $"    Decode.succeed {name}\n{andMapFields}"

        $"decoder{name} : Decoder {name}\ndecoder{name} =\n{decoderBody}"

    let private toEncoder (name: string) (schema: OpenApiSchema) =
        let paramName = name.Substring(0,1).ToLower() + name.Substring(1)

        let props =
            schema.Properties
            |> Seq.toList

        let encodeField (kvp: KeyValuePair<string, OpenApiSchema>) =
            let fieldName = kvp.Key
            let fieldSchema = kvp.Value

            let baseEncoder =
                if fieldSchema.Reference <> null then
                    // It's a reference type - use the appropriate encoder
                    let refPath = fieldSchema.Reference.Id
                    let parts = refPath.Split('/')
                    let typeName = parts[parts.Length - 1]
                    $"encode{typeName}"
                elif fieldSchema.Type = "object" && fieldSchema.Properties <> null && fieldSchema.Properties.Count > 0 then
                    // Inline object - use generated type encoder
                    let typeName = fieldName.Substring(0,1).ToUpper() + fieldName.Substring(1) + "Object"
                    $"encode{typeName}"
                else
                    let format = if isNull fieldSchema.Format then None else Some fieldSchema.Format
                    match fieldSchema.Type, format with
                    | "array", _ ->
                        match fieldSchema.Items with
                        | null -> "Encode.list Encode.string"
                        | item when item.Reference <> null ->
                            let refPath = item.Reference.Id
                            let parts = refPath.Split('/')
                            let typeName = parts[parts.Length - 1]
                            $"Encode.list encode{typeName}"
                        | item when item.Type = "integer" -> "Encode.list Encode.int"
                        | item when item.Type = "number" -> "Encode.list Encode.float"
                        | item when item.Type = "boolean" -> "Encode.list Encode.bool"
                        | item when item.Type = "string" -> "Encode.list Encode.string"
                        | _ -> "Encode.list Encode.string"
                    | "integer", _ -> "Encode.int"
                    | "number", _ -> "Encode.float"
                    | "boolean", _ -> "Encode.bool"
                    | "string", Some "date-time" -> "encodeDateTimeToString"
                    | "string", Some "uuid" -> "encodeUuidToString"
                    | "string", Some "uri" -> "encodeUriToString"
                    | "string", Some "email" -> "encodeEmailToString"
                    | "string", _ -> "Encode.string"
                    | _ -> "Encode.string"

            let safeFieldName = toSafeFieldName fieldName
            if fieldSchema.Nullable then
                $"(\"{fieldName}\", Maybe.map ({baseEncoder}) {paramName}.{safeFieldName} |> Maybe.withDefault Encode.null)"
            else
                $"(\"{fieldName}\", {baseEncoder} {paramName}.{safeFieldName})"

        let fields = props |> List.map encodeField

        let joinedFields =
            fields
            |> List.mapi (fun i f -> if i = 0 then f else ", " + f)
            |> String.concat "\n        "

        $"encode{name} : {name} -> Value\nencode{name} {paramName} =\n    Encode.object\n        [ {joinedFields}\n        ]"

    // Helper function to collect inline objects from a schema
    let rec private collectInlineObjects (schema: OpenApiSchema) : (string * OpenApiSchema) list =
        let inlineObjects = ResizeArray<string * OpenApiSchema>()
        
        if schema.Properties <> null then
            for kvp in schema.Properties do
                let fieldName = kvp.Key
                let fieldSchema = kvp.Value
                
                // Only collect truly inline objects (not references)
                if fieldSchema.Reference = null && fieldSchema.Type = "object" && fieldSchema.Properties <> null && fieldSchema.Properties.Count > 0 then
                    let typeName = fieldName.Substring(0,1).ToUpper() + fieldName.Substring(1) + "Object"
                    inlineObjects.Add((typeName, fieldSchema))
                    // Recursively collect nested inline objects
                    let nestedObjects = collectInlineObjects fieldSchema
                    inlineObjects.AddRange(nestedObjects)
        
        inlineObjects |> List.ofSeq

    // Advanced Type System Features Implementation
    
    // Helper function for Elm type mapping (imported from RequestGenerator pattern)
    let private elmTypeForSchema (schema: OpenApiSchema) =
        match schema.Type with
        | "integer" -> "Int"
        | "number" -> "Float"
        | "boolean" -> "Bool"
        | "string" -> "String"
        | _ -> "String"
    
    // Generate phantom types for compile-time safety
    let generatePhantomType (name: string) (schema: OpenApiSchema) =
        let hasValidation = 
            schema.Pattern <> null || 
            schema.MinLength.HasValue || 
            schema.MaxLength.HasValue ||
            schema.Minimum.HasValue ||
            schema.Maximum.HasValue
            
        let requiresAuth = 
            // Check if this type is used in authenticated endpoints
            schema.Extensions.ContainsKey("x-requires-auth") ||
            schema.Extensions.ContainsKey("x-authenticated")
            
        let phantomTypes = ResizeArray<string>()
        
        if hasValidation then
            let validationType = $"type %s{name}Validated = Phantom%s{name}Validated"
            let unvalidatedType = $"type %s{name}Unvalidated = Phantom%s{name}Unvalidated"
            let validationFunction =
                $"-- Validation function for %s{name}\nvalidate%s{name} : %s{name}Unvalidated -> Result String %s{name}Validated\nvalidate%s{name} unvalidated =\n    -- Add validation logic here based on schema constraints\n    Ok unvalidated"

            phantomTypes.Add(validationType)
            phantomTypes.Add(unvalidatedType)
            phantomTypes.Add(validationFunction)
            
        if requiresAuth then
            let authType = $"type %s{name}Authenticated = Phantom%s{name}Authenticated"
            let unauthType = $"type %s{name}Unauthenticated = Phantom%s{name}Unauthenticated"
            phantomTypes.Add(authType)
            phantomTypes.Add(unauthType)
            
        phantomTypes |> List.ofSeq
        
    // Generate type-safe URL builders  
    let generateTypeSafeUrl (path: string) (operation: OpenApiOperation) =
        let segments = path.Split('/') |> Array.filter (fun s -> s <> "")
        let urlSegments = ResizeArray<UrlSegment>()
        
        for segment in segments do
            if segment.StartsWith("{") && segment.EndsWith("}") then
                let paramName = segment.Trim([|'{'; '}' |])
                let param = 
                    operation.Parameters 
                    |> Seq.tryFind (fun p -> p.Name = paramName && p.In.HasValue && p.In.Value = ParameterLocation.Path)
                let paramType = 
                    match param with
                    | Some p -> elmTypeForSchema p.Schema
                    | None -> "String"
                urlSegments.Add(ParameterSegment(paramName, paramType))
            else
                urlSegments.Add(StaticSegment(segment))
                
        // Add query parameters
        let queryParams = 
            operation.Parameters 
            |> Seq.filter (fun p -> p.In.HasValue && p.In.Value = ParameterLocation.Query)
            
        for queryParam in queryParams do
            let paramType = elmTypeForSchema queryParam.Schema
            let isRequired = queryParam.Required
            urlSegments.Add(QuerySegment(queryParam.Name, paramType, isRequired))
            
        urlSegments |> List.ofSeq
        
    // Generate middleware types and functions
    let generateMiddleware (doc: OpenApiDocument) (operation: OpenApiOperation) =
        let middlewares = ResizeArray<string>()
        
        // Authentication middleware
        if operation.Security <> null && operation.Security.Count > 0 then
            let authMiddleware = 
                "-- Authentication middleware\ntype AuthMiddleware config =\n    { config | authenticated : Bool }\n\napplyAuthMiddleware : Config -> AuthMiddleware Config\napplyAuthMiddleware config =\n    { config | authenticated = True }"
            middlewares.Add(authMiddleware)
            
        // Validation middleware for request bodies
        if operation.RequestBody <> null then
            let validationMiddleware = 
                "-- Request validation middleware\ntype ValidationMiddleware a =\n    ValidatedRequest a | InvalidRequest (List String)\n\nvalidateRequest : a -> ValidationMiddleware a\nvalidateRequest request =\n    -- Add request validation logic\n    ValidatedRequest request"
            middlewares.Add(validationMiddleware)
            
        // Rate limiting middleware
        let rateLimitMiddleware = 
            "-- Rate limiting middleware\ntype RateLimitMiddleware =\n    { requestsPerMinute : Int\n    , currentCount : Int\n    , resetTime : Time.Posix\n    }\n\ncheckRateLimit : RateLimitMiddleware -> Bool\ncheckRateLimit middleware =\n    middleware.currentCount < middleware.requestsPerMinute"
        middlewares.Add(rateLimitMiddleware)
        
        middlewares |> List.ofSeq
        
    // Generate type-safe endpoint builders
    let generateTypeSafeEndpoint (path: string) (method: string) (operation: OpenApiOperation) =
        let operationName = 
            if System.String.IsNullOrWhiteSpace(operation.OperationId) then
                let safePath = path.Replace("/", "_").Replace("{", "").Replace("}", "")
                $"%s{method}%s{safePath}"
            else
                operation.OperationId
                
        let urlSegments = generateTypeSafeUrl path operation
        let hasPathParams = urlSegments |> List.exists (function | ParameterSegment _ -> true | _ -> false)
        let hasQueryParams = urlSegments |> List.exists (function | QuerySegment _ -> true | _ -> false)
        
        // Generate type-safe URL builder
        let urlBuilderType = 
            if hasPathParams then
                let pathParamTypes = 
                    urlSegments 
                    |> List.choose (function 
                        | ParameterSegment(name, paramType) -> Some $"%s{name} : %s{paramType}"
                        | _ -> None)
                    |> String.concat "\n    , "

                $"type alias %s{operationName}PathParams =\n    {{ %s{pathParamTypes}\n    }}"
            else
                ""
                
        let queryBuilderType =
            if hasQueryParams then
                let queryParamTypes = 
                    urlSegments 
                    |> List.choose (function 
                        | QuerySegment(name, paramType, required) -> 
                            let typeDecl = if required then paramType else $"Maybe %s{paramType}"
                            Some $"%s{name} : %s{typeDecl}"
                        | _ -> None)
                    |> String.concat "\n    , "

                $"type alias %s{operationName}QueryParams =\n    {{ %s{queryParamTypes}\n    }}"
            else
                ""
                
        // Generate type-safe URL building function
        let urlBuilderFunction =
            let pathParamArg = if hasPathParams then $"pathParams : %s{operationName}PathParams -> " else ""
            let queryParamArg = if hasQueryParams then $"queryParams : %s{operationName}QueryParams -> " else ""
            
            let pathBuilder = 
                urlSegments
                |> List.choose (function
                    | StaticSegment(segment) -> Some $"\"%s{segment}\""
                    | ParameterSegment(name, "String") -> Some $"pathParams.%s{name}"
                    | ParameterSegment(name, "Int") -> Some $"(String.fromInt pathParams.%s{name})"
                    | ParameterSegment(name, "Float") -> Some $"(String.fromFloat pathParams.%s{name})"
                    | ParameterSegment(name, _) -> Some $"pathParams.%s{name}"
                    | _ -> None)
                |> String.concat " ++ \"/\" ++ "
                
            let queryBuilder =
                if hasQueryParams then
                    let queryItems = 
                        urlSegments
                        |> List.choose (function
                            | QuerySegment(name, paramType, required) ->
                                if required then
                                    match paramType with
                                    | "String" -> Some $"Url.string \"%s{name}\" queryParams.%s{name}"
                                    | "Int" -> Some $"Url.int \"%s{name}\" queryParams.%s{name}"
                                    | "Float" -> Some $"Url.string \"%s{name}\" (String.fromFloat queryParams.%s{name})"
                                    | _ -> Some $"Url.string \"%s{name}\" queryParams.%s{name}"
                                else
                                    match paramType with
                                    | "String" -> Some $"Maybe.map (Url.string \"%s{name}\") queryParams.%s{name} |> Maybe.withDefault []"
                                    | "Int" -> Some $"Maybe.map (Url.int \"%s{name}\") queryParams.%s{name} |> Maybe.withDefault []"
                                    | _ -> Some $"Maybe.map (Url.string \"%s{name}\") queryParams.%s{name} |> Maybe.withDefault []"
                            | _ -> None)
                        |> String.concat ", "

                    $" ++ Url.toQuery [%s{queryItems}]"
                else
                    ""
                    
            sprintf "build%sUrl : %s%sString\nbuild%sUrl %s%s=\n    \"/%s\"%s" 
                operationName pathParamArg queryParamArg operationName 
                (if hasPathParams then "pathParams " else "")
                (if hasQueryParams then "queryParams " else "")
                pathBuilder queryBuilder
                
        [urlBuilderType; queryBuilderType; urlBuilderFunction] |> List.filter (fun s -> s <> "")

    // Enhanced schema type detection for OpenAPI 3.1 / JSON Schema draft 2020-12
    let private isOneOf (schema: OpenApiSchema) =
        schema.OneOf <> null && schema.OneOf.Count > 0
    
    let private isAnyOf (schema: OpenApiSchema) =
        schema.AnyOf <> null && schema.AnyOf.Count > 0
    
    let private isAllOf (schema: OpenApiSchema) =
        schema.AllOf <> null && schema.AllOf.Count > 0
        
    // Generate conditional schema type (if/then/else)
    let private generateConditionalType (name: string) (schema: OpenApiSchema) =
        // For now, create a union type that represents the conditional possibilities
        // TODO: Implement proper if/then/else schema handling from JSON Schema draft 2020-12
        let conditionalType = 
            $"type {name} =\n" +
            $"    {name}Then ()\n" +
            $"    | {name}Else ()\n" +
            $"    | {name}Unknown Json.Decode.Value"
            
        let conditionalDecoder =
            $"decoder{name} : Decoder {name}\n" +
            $"decoder{name} =\n" +
            $"    -- Conditional schema decoder (if/then/else)\n" +
            $"    -- This is a simplified implementation\n" +
            $"    Decode.oneOf\n" +
            $"        [ Decode.map {name}Then (Decode.succeed ())\n" +
            $"        , Decode.map {name}Else (Decode.succeed ())\n" +
            $"        , Decode.map {name}Unknown Decode.value\n" +
            $"        ]"
            
        let conditionalEncoder =
            $"encode{name} : {name} -> Value\n" +
            $"encode{name} value =\n" +
            $"    case value of\n" +
            $"        {name}Then _ -> Encode.object [(\"type\", Encode.string \"then\")]\n" +
            $"        {name}Else _ -> Encode.object [(\"type\", Encode.string \"else\")]\n" +
            $"        {name}Unknown val -> val"
            
        (conditionalType, conditionalDecoder, conditionalEncoder)

    // Enhanced OneOf union type with OpenAPI 3.1 discriminator support
    let private generateOneOfType (name: string) (schema: OpenApiSchema) =
        // Check for OpenAPI 3.1 discriminator
        let discriminator = schema.Discriminator
        let hasDiscriminator = discriminator <> null && not (System.String.IsNullOrEmpty(discriminator.PropertyName))
        
        let variants = 
            schema.OneOf 
            |> Seq.mapi (fun i variant ->
                if variant.Reference <> null then
                    // It's a reference - use the referenced type name
                    let refPath = variant.Reference.Id
                    let parts = refPath.Split('/')
                    let referencedTypeName = parts[parts.Length - 1]
                    (referencedTypeName, variant, true) // true indicates it's a reference
                else
                    // Inline schema - create new type
                    let variantName = $"{name}Option{i + 1}"
                    (variantName, variant, false) // false indicates it's inline
            ) 
            |> List.ofSeq

        let unionType = 
            match variants with
            | [] -> $"type {name} = -- No valid variants"
            | firstVariant :: restVariants ->
                let firstName, _, _ = firstVariant
                let firstLine = $"type {name} =\n    {firstName}Constructor {firstName}"
                let otherLines = restVariants |> List.map (fun (variantName, _, _) -> $"    | {variantName}Constructor {variantName}") |> String.concat "\n"
                if restVariants.IsEmpty then firstLine else firstLine + "\n" + otherLines

        let variantSchemas = 
            variants 
            |> List.choose (fun (variantName, variant, isReference) ->
                if not isReference && variant.Properties <> null && variant.Properties.Count > 0 then
                    Some (variantName, variant)
                else
                    None
            )

        // Generate enhanced decoder with discriminator support
        let decoderCases = 
            if hasDiscriminator then
                // Use explicit discriminator property for OpenAPI 3.1
                variants
                |> List.choose (fun (variantName, variant, isReference) ->
                    if isReference then
                        // For references, use discriminator mapping if available
                        if discriminator.Mapping <> null && discriminator.Mapping.Count > 0 then
                            let mappingEntry = 
                                discriminator.Mapping 
                                |> Seq.tryFind _.Value.EndsWith(variantName)

                            match mappingEntry with
                            | Some kvp -> 
                                Some $"                    \"{kvp.Key}\" -> Decode.map {variantName}Constructor decoder{variantName}"
                            | None ->
                                // Fallback to type name
                                let discriminatorValue = variantName.ToLower()
                                Some $"                    \"{discriminatorValue}\" -> Decode.map {variantName}Constructor decoder{variantName}"
                        else
                            let discriminatorValue = variantName.ToLower()
                            Some $"                    \"{discriminatorValue}\" -> Decode.map {variantName}Constructor decoder{variantName}"
                    elif variant.Properties <> null && variant.Properties.Count > 0 then
                        // Look for discriminator field with explicit enum value
                        if variant.Properties.ContainsKey(discriminator.PropertyName) then
                            let discriminatorField = variant.Properties[discriminator.PropertyName]
                            if discriminatorField.Enum <> null && discriminatorField.Enum.Count > 0 then
                                let discriminatorValue = (discriminatorField.Enum[0] :?> OpenApiString).Value
                                Some $"                    \"{discriminatorValue}\" -> Decode.map {variantName}Constructor decoder{variantName}"
                            else
                                None
                        else
                            None
                    else
                        None
                )
            else
                // Fallback to original logic for backward compatibility
                variants
                |> List.choose (fun (variantName, variant, isReference) ->
                    if isReference then
                        // For references, try to find the discriminator from the original variant schema
                        if variant.Reference <> null then
                            // Try to get discriminator from reference - we'll use a simple heuristic
                            let refPath = variant.Reference.Id
                            let typeName = refPath.Split('/') |> Array.last
                            // Use the type name as discriminator (lowercase)
                            let discriminatorValue = typeName.ToLower()
                            Some $"                    \"{discriminatorValue}\" -> Decode.map {variantName}Constructor decoder{variantName}"
                        else
                            None
                    elif variant.Properties <> null && variant.Properties.Count > 0 then
                        // Look for discriminator field (usually "type")
                        let discriminatorField = 
                            variant.Properties
                            |> Seq.tryFind (fun kvp -> 
                                kvp.Key = "type" && kvp.Value.Enum <> null && kvp.Value.Enum.Count > 0)
                        
                        match discriminatorField with
                        | Some kvp ->
                            let discriminatorValue = (kvp.Value.Enum[0] :?> OpenApiString).Value
                            Some $"                    \"{discriminatorValue}\" -> Decode.map {variantName}Constructor decoder{variantName}"
                        | None ->
                            // Fallback: try to decode as this variant
                            Some $"                    _ -> Decode.map {variantName}Constructor decoder{variantName}"
                    else
                        None
                )
            |> String.concat "\n"

        let discriminatorProperty = 
            if hasDiscriminator then discriminator.PropertyName else "type"
            
        let decoder = 
            $"decoder{name} : Decoder {name}\n" +
            $"decoder{name} =\n" +
            $"    Decode.field \"{discriminatorProperty}\" Decode.string\n" +
            $"        |> Decode.andThen\n" +
            $"            (\\discriminator ->\n" +
            $"                case discriminator of\n" +
            $"{decoderCases}\n" +
            $"                    _ -> Decode.fail (\"Unknown {name} type: \" ++ discriminator)\n" +
            $"            )"

        // Generate encoder
        let encoderCases =
            variants
            |> List.map (fun (variantName, _, _) ->
                $"        {variantName}Constructor data -> encode{variantName} data"
            )
            |> String.concat "\n"

        let encoder =
            $"encode{name} : {name} -> Value\n" +
            $"encode{name} value =\n" +
            $"    case value of\n" +
            $"{encoderCases}"

        (unionType, variantSchemas, decoder, encoder)

    // Generate AnyOf union type (similar to OneOf but more permissive)
    let private generateAnyOfType (name: string) (schema: OpenApiSchema) =
        let variants = 
            schema.AnyOf 
            |> Seq.mapi (fun i variant ->
                if variant.Reference <> null then
                    // It's a reference - use the referenced type name
                    let refPath = variant.Reference.Id
                    let parts = refPath.Split('/')
                    let referencedTypeName = parts[parts.Length - 1]
                    (referencedTypeName, variant, true) // true indicates it's a reference
                else
                    // Inline schema - create new type
                    let variantName = $"{name}Variant{i + 1}"
                    (variantName, variant, false) // false indicates it's inline
            ) 
            |> List.ofSeq

        let unionType = 
            match variants with
            | [] -> $"type {name} = -- No valid variants"
            | firstVariant :: restVariants ->
                let firstName, _, _ = firstVariant
                let firstLine = $"type {name} =\n    {firstName}Constructor {firstName}"
                let otherLines = restVariants |> List.map (fun (variantName, _, _) -> $"    | {variantName}Constructor {variantName}") |> String.concat "\n"
                if restVariants.IsEmpty then firstLine else firstLine + "\n" + otherLines

        let variantSchemas = 
            variants 
            |> List.choose (fun (variantName, variant, isReference) ->
                if not isReference && variant.Properties <> null && variant.Properties.Count > 0 then
                    Some (variantName, variant)
                else
                    None
            )

        // AnyOf decoder tries each variant until one succeeds
        let decoderCases = 
            variants
            |> List.mapi (fun i (variantName, variant, isReference) ->
                if isReference || (variant.Properties <> null && variant.Properties.Count > 0) then
                    if i = 0 then
                        $"Decode.map {variantName}Constructor decoder{variantName}"
                    else
                        $", Decode.map {variantName}Constructor decoder{variantName}"
                else
                    if i = 0 then
                        "Decode.fail \"Primitive AnyOf variant not supported\""
                    else
                        ", Decode.fail \"Primitive AnyOf variant not supported\""
            )
            |> String.concat "\n        "

        let decoder = 
            $"decoder{name} : Decoder {name}\n" +
            $"decoder{name} =\n" +
            $"    Decode.oneOf\n" +
            $"        [ {decoderCases}\n" +
            $"        ]"

        // Generate encoder
        let encoderCases =
            variants
            |> List.map (fun (variantName, _, _) ->
                $"        {variantName}Constructor data -> encode{variantName} data"
            )
            |> String.concat "\n"

        let encoder =
            $"encode{name} : {name} -> Value\n" +
            $"encode{name} value =\n" +
            $"    case value of\n" +
            $"{encoderCases}"

        (unionType, variantSchemas, decoder, encoder)

    // Generate AllOf composition type (merge all schemas)
    let private generateAllOfType (name: string) (schema: OpenApiSchema) (components: OpenApiComponents) =
        // Collect all properties from all schemas in AllOf
        let allProperties = Dictionary<string, OpenApiSchema>()
        
        for subSchema in schema.AllOf do
            if subSchema.Reference <> null then
                // Resolve reference
                let refPath = subSchema.Reference.Id
                let refName = refPath.Split('/') |> Array.last
                if components.Schemas.ContainsKey(refName) then
                    let referencedSchema = components.Schemas[refName]
                    if referencedSchema.Properties <> null then
                        for kvp in referencedSchema.Properties do
                            allProperties[kvp.Key] <- kvp.Value
            elif subSchema.Properties <> null then
                // Direct properties
                for kvp in subSchema.Properties do
                    allProperties[kvp.Key] <- kvp.Value
        
        // Create a merged schema
        let mergedSchema = OpenApiSchema()
        mergedSchema.Type <- "object"
        mergedSchema.Properties <- allProperties
        
        let typeDef = toTypeAlias name mergedSchema
        let decoder = toDecoder name mergedSchema
        let encoder = toEncoder name mergedSchema
        
        (typeDef, decoder, encoder)

    // Performance optimization: Type deduplication and analysis
    let private analyzeTypeUsage (doc: OpenApiDocument) =
        let usedTypes = HashSet<string>()
        let typeDefinitions = Dictionary<string, OpenApiSchema>()
        let typeDependencies = Dictionary<string, Set<string>>()
        
        // Collect all type definitions
        if doc.Components <> null && doc.Components.Schemas <> null then
            for kvp in doc.Components.Schemas do
                typeDefinitions[kvp.Key] <- kvp.Value
        
        // Analyze dependencies and usage
        let rec analyzeDependencies (typeName: string) (schema: OpenApiSchema) =
            if not (usedTypes.Contains(typeName)) then
                usedTypes.Add(typeName) |> ignore
                let deps = HashSet<string>()
                
                // Check properties for references
                if schema.Properties <> null then
                    for prop in schema.Properties do
                        if prop.Value.Reference <> null then
                            let refName = prop.Value.Reference.Id.Split('/') |> Array.last
                            deps.Add(refName) |> ignore
                            if typeDefinitions.ContainsKey(refName) then
                                analyzeDependencies refName typeDefinitions[refName]
                        elif prop.Value.Items <> null && prop.Value.Items.Reference <> null then
                            let refName = prop.Value.Items.Reference.Id.Split('/') |> Array.last
                            deps.Add(refName) |> ignore
                            if typeDefinitions.ContainsKey(refName) then
                                analyzeDependencies refName typeDefinitions[refName]
                
                // Check OneOf/AnyOf/AllOf references
                if schema.OneOf <> null then
                    for variant in schema.OneOf do
                        if variant.Reference <> null then
                            let refName = variant.Reference.Id.Split('/') |> Array.last
                            deps.Add(refName) |> ignore
                            if typeDefinitions.ContainsKey(refName) then
                                analyzeDependencies refName typeDefinitions[refName]
                
                typeDependencies[typeName] <- Set.ofSeq deps
        
        // Start analysis from request/response types
        if doc.Paths <> null then
            for pathItem in doc.Paths do
                for operation in pathItem.Value.Operations do
                    // Analyze request body
                    if operation.Value.RequestBody <> null && operation.Value.RequestBody.Content <> null then
                        for content in operation.Value.RequestBody.Content do
                            if content.Value.Schema <> null && content.Value.Schema.Reference <> null then
                                let refName = content.Value.Schema.Reference.Id.Split('/') |> Array.last
                                if typeDefinitions.ContainsKey(refName) then
                                    analyzeDependencies refName typeDefinitions[refName]
                    
                    // Analyze responses
                    if operation.Value.Responses <> null then
                        for response in operation.Value.Responses do
                            if response.Value.Content <> null then
                                for content in response.Value.Content do
                                    if content.Value.Schema <> null && content.Value.Schema.Reference <> null then
                                        let refName = content.Value.Schema.Reference.Id.Split('/') |> Array.last
                                        if typeDefinitions.ContainsKey(refName) then
                                            analyzeDependencies refName typeDefinitions[refName]
        
        (usedTypes, typeDependencies)

    // Import optimization: Analyze what imports are actually needed
    let private analyzeImports (types: string list) (decoders: string list) (encoders: string list) (requests: string list) =
        let allContent = String.concat " " (types @ decoders @ encoders @ requests)
        
        let needsDict = allContent.Contains("Dict ") || allContent.Contains("additionalProperties")
        let needsHttp = allContent.Contains("Http.") || allContent.Contains("Task ")
        let needsJsonDecode = allContent.Contains("Decode.") || allContent.Contains("Decoder ")
        let needsAndMap = allContent.Contains("andMap") || allContent.Contains("|> andMap")
        let needsJsonEncode = allContent.Contains("Encode.") || allContent.Contains("Value")
        let needsTask = allContent.Contains("Task ") || requests.Length > 0
        let needsUrl = allContent.Contains("Url.") || allContent.Contains("toQuery")
        
        (needsDict, needsHttp, needsJsonDecode, needsAndMap, needsJsonEncode, needsTask, needsUrl)

    // Performance: Determine if we should split into multiple modules
    let private shouldSplitModules (schemas: seq<System.Collections.Generic.KeyValuePair<string, OpenApiSchema>>) =
        let schemaCount = Seq.length schemas
        let complexSchemaCount = 
            schemas
            |> Seq.filter (fun kvp -> 
                let schema = kvp.Value
                (schema.Properties <> null && schema.Properties.Count > 10) ||
                isOneOf schema || isAnyOf schema || isAllOf schema)
            |> Seq.length
        
        schemaCount > 50 || complexSchemaCount > 20

    // Generate optimized single module with tree-shaking
    let private generateSingleModule (doc: OpenApiDocument) (baseModuleName: string) (usedTypes: HashSet<string>) =
        let moduleName = baseModuleName + ".Schemas"
        
        // Tree-shaking: Only collect inline objects from used schemas
        let allInlineObjects = 
            if doc.Components <> null && doc.Components.Schemas <> null then
                doc.Components.Schemas
                |> Seq.filter (fun kvp -> usedTypes.Contains(kvp.Key))
                |> Seq.collect (fun kvp -> collectInlineObjects kvp.Value)
                |> List.ofSeq
            else []

        // Tree-shaking: Only generate types that are actually used
        let typeEntries =
            if doc.Components <> null && doc.Components.Schemas <> null then
                doc.Components.Schemas
                |> Seq.filter (fun kvp -> usedTypes.Contains(kvp.Key))
                |> Seq.map (fun kvp ->
                    let name = kvp.Key
                    let schema = kvp.Value

                    if isOneOf schema then
                        // Handle OneOf types - variants will be processed separately
                        let unionType, _, decoder, encoder = generateOneOfType name schema
                        (Some unionType, Some decoder, Some encoder, [])
                    elif isAnyOf schema then
                        // Handle AnyOf types - variants will be processed separately
                        let unionType, _, decoder, encoder = generateAnyOfType name schema
                        (Some unionType, Some decoder, Some encoder, [])
                    elif isAllOf schema then
                        // Handle AllOf types - merge all schemas
                        let typeDef, decoder, encoder = generateAllOfType name schema doc.Components
                        (Some typeDef, Some decoder, Some encoder, [])
                    elif hasConditionalSchema schema then
                        // Handle conditional schemas (if/then/else)
                        let typeDef, decoder, encoder = generateConditionalType name schema
                        (Some typeDef, Some decoder, Some encoder, [])
                    else
                        match toElmEnum name schema with
                        | Some (enumType, enumDecoder, enumEncoder) ->
                            (Some enumType, Some enumDecoder, Some enumEncoder, [])
                        | None ->
                            let typeDef = toTypeAlias name schema
                            let decoder = toDecoder name schema
                            let encoder = toEncoder name schema
                            (Some typeDef, Some decoder, Some encoder, [])
                )
                |> Seq.toList
            else
                []

        // Add inline object types
        let inlineTypeEntries = 
            allInlineObjects
            |> List.map (fun (name, schema) ->
                let typeDef = toTypeAlias name schema
                let decoder = toDecoder name schema
                let encoder = toEncoder name schema
                (Some typeDef, Some decoder, Some encoder, [])
            )

        // Collect OneOf and AnyOf variant schemas
        let variantSchemas =
            if doc.Components <> null && doc.Components.Schemas <> null then
                doc.Components.Schemas
                |> Seq.choose (fun kvp ->
                    if usedTypes.Contains(kvp.Key) then
                        if isOneOf kvp.Value then
                            let _, variantSchemas, _, _ = generateOneOfType kvp.Key kvp.Value
                            Some variantSchemas
                        elif isAnyOf kvp.Value then
                            let _, variantSchemas, _, _ = generateAnyOfType kvp.Key kvp.Value
                            Some variantSchemas
                        else
                            None
                    else
                        None
                )
                |> Seq.collect id
                |> List.ofSeq
            else
                []

        // Add variant types for OneOf and AnyOf
        let variantEntries =
            variantSchemas
            |> List.map (fun (variantName, variantSchema) ->
                let typeDef = toTypeAlias variantName variantSchema
                let decoder = toDecoder variantName variantSchema
                let encoder = toEncoder variantName variantSchema
                (Some typeDef, Some decoder, Some encoder, [])
            )

        let allTypeEntries = typeEntries @ inlineTypeEntries @ variantEntries

        // Enhanced formatted type aliases for OpenAPI 3.1 / JSON Schema draft 2020-12
        let formatTypeAliases = [
            "type alias DateTime = String"
            "type alias Date = String"
            "type alias Time = String"
            "type alias Uuid = String"
            "type alias Uri = String"
            "type alias UriReference = String"
            "type alias Email = String"
            "type alias Hostname = String"
            "type alias IPv4 = String"
            "type alias IPv6 = String"
            "type alias Base64String = String"
            "type alias BinaryString = String"
            "type alias Password = String"
        ]
        
        // Generate Config type for authentication, base URL, headers, and timeout
        let configType = 
            let hasSecuritySchemes = doc.Components <> null && doc.Components.SecuritySchemes <> null && doc.Components.SecuritySchemes.Count > 0
            let hasServers = doc.Servers <> null && doc.Servers.Count > 0
            
            // Always generate Config type for production readiness features
            let configFields = ResizeArray<string>()
            
            if hasServers then
                configFields.Add("baseUrl : String")
            else
                configFields.Add("baseUrl : String")  // Always include baseUrl for flexibility
            
            if hasSecuritySchemes then
                for kvp in doc.Components.SecuritySchemes do
                    let scheme = kvp.Value
                    match scheme.Type with
                    | SecuritySchemeType.ApiKey ->
                        configFields.Add("apiKey : String")
                    | SecuritySchemeType.Http ->
                        match scheme.Scheme.ToLower() with
                        | "bearer" ->
                            configFields.Add("bearerToken : String")
                        | "basic" ->
                            configFields.Add("basicAuth : String")
                        | _ -> ()
                    | _ -> ()
            
            // Add custom headers and timeout fields for production features
            configFields.Add("customHeaders : List (String, String)")
            configFields.Add("timeout : Maybe Float")
            
            if configFields.Count > 0 then
                let fields = String.concat "\n    , " configFields
                Some $"type alias Config =\n    {{ {fields}\n    }}"
            else
                None

        let formatDecoders = [
            "decodeDateTimeFromString : Decoder DateTime\ndecodeDateTimeFromString = Decode.string"
            "decodeDateFromString : Decoder Date\ndecodeDateFromString = Decode.string"
            "decodeTimeFromString : Decoder Time\ndecodeTimeFromString = Decode.string"
            "decodeUuidFromString : Decoder Uuid\ndecodeUuidFromString = Decode.string"
            "decodeUriFromString : Decoder Uri\ndecodeUriFromString = Decode.string"
            "decodeUriReferenceFromString : Decoder UriReference\ndecodeUriReferenceFromString = Decode.string"
            "decodeEmailFromString : Decoder Email\ndecodeEmailFromString = Decode.string"
            "decodeHostnameFromString : Decoder Hostname\ndecodeHostnameFromString = Decode.string"
            "decodeIPv4FromString : Decoder IPv4\ndecodeIPv4FromString = Decode.string"
            "decodeIPv6FromString : Decoder IPv6\ndecodeIPv6FromString = Decode.string"
            "decodeBase64StringFromString : Decoder Base64String\ndecodeBase64StringFromString = Decode.string"
            "decodeBinaryStringFromString : Decoder BinaryString\ndecodeBinaryStringFromString = Decode.string"
            "decodePasswordFromString : Decoder Password\ndecodePasswordFromString = Decode.string"
        ]

        let formatEncoders = [
            "encodeDateTimeToString : DateTime -> Value\nencodeDateTimeToString dateTime = Encode.string dateTime"
            "encodeDateToString : Date -> Value\nencodeDateToString date = Encode.string date"
            "encodeTimeToString : Time -> Value\nencodeTimeToString time = Encode.string time"
            "encodeUuidToString : Uuid -> Value\nencodeUuidToString uuid = Encode.string uuid"
            "encodeUriToString : Uri -> Value\nencodeUriToString uri = Encode.string uri"
            "encodeUriReferenceToString : UriReference -> Value\nencodeUriReferenceToString uriRef = Encode.string uriRef"
            "encodeEmailToString : Email -> Value\nencodeEmailToString email = Encode.string email"
            "encodeHostnameToString : Hostname -> Value\nencodeHostnameToString hostname = Encode.string hostname"
            "encodeIPv4ToString : IPv4 -> Value\nencodeIPv4ToString ipv4 = Encode.string ipv4"
            "encodeIPv6ToString : IPv6 -> Value\nencodeIPv6ToString ipv6 = Encode.string ipv6"
            "encodeBase64StringToString : Base64String -> Value\nencodeBase64StringToString base64 = Encode.string base64"
            "encodeBinaryStringToString : BinaryString -> Value\nencodeBinaryStringToString binary = Encode.string binary"
            "encodePasswordToString : Password -> Value\nencodePasswordToString password = Encode.string password"
        ]

        let requestPairs, errorTypes, advancedFeatures = RequestGenerator.generateRequestsWithAdvancedFeatures doc
        
        // Generate phantom types for schemas that need validation or authentication
        let phantomTypes = 
            if doc.Components <> null && doc.Components.Schemas <> null then
                doc.Components.Schemas
                |> Seq.filter (fun kvp -> usedTypes.Contains(kvp.Key))
                |> Seq.collect (fun kvp -> generatePhantomType kvp.Key kvp.Value)
                |> List.ofSeq
            else
                []
                
        // Generate basic testing infrastructure
        let mockGenerators = ["-- Mock generators would be generated here"]
        let validators = ["-- Validators would be generated here"]
        
        // Generate config helper functions
        let configHelpers = 
            match configType with
            | Some _ ->
                let hasServers = doc.Servers <> null && doc.Servers.Count > 0
                let defaultBaseUrl = 
                    if hasServers && doc.Servers.Count > 0 then
                        doc.Servers[0].Url
                    else
                        "https://api.example.com"
                
                let hasSecuritySchemes = doc.Components <> null && doc.Components.SecuritySchemes <> null && doc.Components.SecuritySchemes.Count > 0
                let configValues = ResizeArray<string>()
                
                // Always add baseUrl
                configValues.Add($"baseUrl = \"{defaultBaseUrl}\"")
                
                if hasSecuritySchemes then
                    for kvp in doc.Components.SecuritySchemes do
                        let scheme = kvp.Value
                        match scheme.Type with
                        | SecuritySchemeType.ApiKey ->
                            configValues.Add("apiKey = \"your-api-key\"")
                        | SecuritySchemeType.Http ->
                            match scheme.Scheme.ToLower() with
                            | "bearer" ->
                                configValues.Add("bearerToken = \"your-bearer-token\"")
                            | "basic" ->
                                configValues.Add("basicAuth = \"your-basic-auth\"")
                            | _ -> ()
                        | _ -> ()
                
                // Add custom headers and timeout defaults
                configValues.Add("customHeaders = []")
                configValues.Add("timeout = Nothing")
                
                if configValues.Count > 0 then
                    let values = String.concat "\n    , " configValues
                    Some $"defaultConfig : Config\ndefaultConfig =\n    {{ {values}\n    }}"
                else
                    None
            | None -> None

        // Separate into clean lists
        let configTypes = match configType with | Some t -> [t] | None -> []
        let types = 
            formatTypeAliases @ 
            configTypes @
            phantomTypes @
            (allTypeEntries |> List.collect (fun (t, _, _, variants) -> 
                match t with 
                | Some t -> t :: variants
                | None -> variants
            )) @
            errorTypes
        let decoders = 
            (allTypeEntries |> List.choose (fun (_, d, _, _) -> d)) @ formatDecoders
        let encoders = 
            (allTypeEntries |> List.choose (fun (_, _, e, _) -> e)) @ formatEncoders
        
        let configHelpersText = match configHelpers with | Some h -> [h] | None -> []
        let requests =
            configHelpersText @
            advancedFeatures @
            mockGenerators @
            validators @
            (requestPairs
            |> List.map (fun (signature, def) -> if System.String.IsNullOrEmpty signature then def else signature + "\n" + def))

        let apiDescription = 
            if doc.Info <> null then
                let title = if not (System.String.IsNullOrEmpty(doc.Info.Title)) then doc.Info.Title else "Generated API Client"
                let desc = if not (System.String.IsNullOrEmpty(doc.Info.Description)) then
                               $"\n\n%s{doc.Info.Description}" else ""
                let version = if not (System.String.IsNullOrEmpty(doc.Info.Version)) then
                                  $"\n\nVersion: %s{doc.Info.Version}" else ""

                $"%s{title}%s{desc}%s{version}"
            else "Generated API Client"

        // Import optimization: Analyze what imports are actually needed
        let needsDict, needsHttp, needsJsonDecode, needsAndMap, needsJsonEncode, needsTask, needsUrl = 
            analyzeImports types decoders encoders requests

        let context: ElmTemplateContext =
            {
                ModuleName = moduleName
                Types = types
                Decoders = decoders
                Encoders = encoders
                Requests = requests
                ApiDescription = apiDescription
                GenerationTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                TypeDocumentation = []
                FunctionDocumentation = []
                NeedsDict = needsDict
                NeedsHttp = needsHttp
                NeedsJsonDecode = needsJsonDecode
                NeedsAndMap = needsAndMap
                NeedsJsonEncode = needsJsonEncode
                NeedsTask = needsTask
                NeedsUrl = needsUrl
            }

        let templatePath = "Generator/Templates/module.scriban"

        [
            {
                QualifiedName = moduleName
                RelativePath = moduleName.Replace(".", "/") + ".elm"
                Source = renderModule templatePath context
            }
        ]

    // Generate multiple modules for large APIs (module splitting for performance)
    let private generateSplitModules (doc: OpenApiDocument) (baseModuleName: string) (usedTypes: HashSet<string>) (typeDependencies: Dictionary<string, Set<string>>) =
        let allModules = ResizeArray<ElmModule>()
        
        // Split types into logical groups
        let typeGroups = 
            if doc.Components <> null && doc.Components.Schemas <> null then
                doc.Components.Schemas
                |> Seq.filter (fun kvp -> usedTypes.Contains(kvp.Key))
                |> Seq.groupBy (fun kvp -> 
                    // Group by first few characters or logical grouping
                    let name = kvp.Key
                    if name.Length > 2 then
                        name.Substring(0, 2).ToUpper()
                    else
                        name.ToUpper()
                )
                |> Seq.map (fun (groupKey, schemas) -> (groupKey, schemas |> Seq.toList))
                |> List.ofSeq
            else
                []
        
        // Generate types module for each group
        for groupKey, groupSchemas in typeGroups do
            let moduleName = $"%s{baseModuleName}.Types.%s{groupKey}"
            let typeEntries = 
                groupSchemas
                |> List.map (fun kvp ->
                    let name = kvp.Key
                    let schema = kvp.Value
                    
                    if isOneOf schema then
                        let unionType, variantSchemas, decoder, encoder = generateOneOfType name schema
                        (Some unionType, Some decoder, Some encoder, [])
                    elif isAnyOf schema then
                        let unionType, variantSchemas, decoder, encoder = generateAnyOfType name schema
                        (Some unionType, Some decoder, Some encoder, [])
                    elif isAllOf schema then
                        let typeDef, decoder, encoder = generateAllOfType name schema doc.Components
                        (Some typeDef, Some decoder, Some encoder, [])
                    else
                        match toElmEnum name schema with
                        | Some (enumType, enumDecoder, enumEncoder) ->
                            (Some enumType, Some enumDecoder, Some enumEncoder, [])
                        | None ->
                            let typeDef = toTypeAlias name schema
                            let decoder = toDecoder name schema
                            let encoder = toEncoder name schema
                            (Some typeDef, Some decoder, Some encoder, [])
                )
            
            let types = typeEntries |> List.choose (fun (t, _, _, _) -> t)
            let decoders = typeEntries |> List.choose (fun (_, d, _, _) -> d)
            let encoders = typeEntries |> List.choose (fun (_, _, e, _) -> e)
            
            // Import optimization for this module
            let needsDict, needsHttp, needsJsonDecode, needsAndMap, needsJsonEncode, needsTask, needsUrl = 
                analyzeImports types decoders encoders []
            
            let context: ElmTemplateContext =
                {
                    ModuleName = moduleName
                    Types = types
                    Decoders = decoders
                    Encoders = encoders
                    Requests = []
                    ApiDescription = $"Type definitions for %s{groupKey} group"
                    GenerationTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    TypeDocumentation = []
                    FunctionDocumentation = []
                    NeedsDict = needsDict
                    NeedsHttp = needsHttp
                    NeedsJsonDecode = needsJsonDecode
                    NeedsAndMap = needsAndMap
                    NeedsJsonEncode = needsJsonEncode
                    NeedsTask = needsTask
                    NeedsUrl = needsUrl
                }
            
            let templatePath = "Generator/Templates/module.scriban"
            
            allModules.Add({
                QualifiedName = moduleName
                RelativePath = moduleName.Replace(".", "/") + ".elm"
                Source = renderModule templatePath context
            })
        
        // Generate separate API module with only requests
        let apiModuleName = baseModuleName + ".Api"
        let requestPairs, errorTypes, advancedFeatures = RequestGenerator.generateRequestsWithAdvancedFeatures doc
        
        // Generate Config type
        let configType = 
            let hasSecuritySchemes = doc.Components <> null && doc.Components.SecuritySchemes <> null && doc.Components.SecuritySchemes.Count > 0
            let hasServers = doc.Servers <> null && doc.Servers.Count > 0
            
            let configFields = ResizeArray<string>()
            
            if hasServers then
                configFields.Add("baseUrl : String")
            else
                configFields.Add("baseUrl : String")
            
            if hasSecuritySchemes then
                for kvp in doc.Components.SecuritySchemes do
                    let scheme = kvp.Value
                    match scheme.Type with
                    | SecuritySchemeType.ApiKey ->
                        configFields.Add("apiKey : String")
                    | SecuritySchemeType.Http ->
                        match scheme.Scheme.ToLower() with
                        | "bearer" ->
                            configFields.Add("bearerToken : String")
                        | "basic" ->
                            configFields.Add("basicAuth : String")
                        | _ -> ()
                    | _ -> ()
            
            configFields.Add("customHeaders : List (String, String)")
            configFields.Add("timeout : Maybe Float")
            
            if configFields.Count > 0 then
                let fields = String.concat "\n    , " configFields
                Some $"type alias Config =\n    {{ {fields}\n    }}"
            else
                None
        
        let configHelpers = 
            match configType with
            | Some _ ->
                let hasServers = doc.Servers <> null && doc.Servers.Count > 0
                let defaultBaseUrl = 
                    if hasServers && doc.Servers.Count > 0 then
                        doc.Servers[0].Url
                    else
                        "https://api.example.com"
                
                let hasSecuritySchemes = doc.Components <> null && doc.Components.SecuritySchemes <> null && doc.Components.SecuritySchemes.Count > 0
                let configValues = ResizeArray<string>()
                
                configValues.Add($"baseUrl = \"{defaultBaseUrl}\"")
                
                if hasSecuritySchemes then
                    for kvp in doc.Components.SecuritySchemes do
                        let scheme = kvp.Value
                        match scheme.Type with
                        | SecuritySchemeType.ApiKey ->
                            configValues.Add("apiKey = \"your-api-key\"")
                        | SecuritySchemeType.Http ->
                            match scheme.Scheme.ToLower() with
                            | "bearer" ->
                                configValues.Add("bearerToken = \"your-bearer-token\"")
                            | "basic" ->
                                configValues.Add("basicAuth = \"your-basic-auth\"")
                            | _ -> ()
                        | _ -> ()
                
                configValues.Add("customHeaders = []")
                configValues.Add("timeout = Nothing")
                
                if configValues.Count > 0 then
                    let values = String.concat "\n    , " configValues
                    Some $"defaultConfig : Config\ndefaultConfig =\n    {{ {values}\n    }}"
                else
                    None
            | None -> None
        
        let configTypes = match configType with | Some t -> [t] | None -> []
        let configHelpersText = match configHelpers with | Some h -> [h] | None -> []
        let requests = configHelpersText @ (requestPairs |> List.map (fun (signature, def) -> 
            if System.String.IsNullOrEmpty signature then def else signature + "\n" + def))
        
        // Import optimization for API module
        let needsDict, needsHttp, needsJsonDecode, needsAndMap, needsJsonEncode, needsTask, needsUrl = 
            analyzeImports configTypes [] [] requests
        
        let apiContext: ElmTemplateContext =
            {
                ModuleName = apiModuleName
                Types = configTypes @ errorTypes
                Decoders = []
                Encoders = []
                Requests = requests
                ApiDescription = 
                    if doc.Info <> null then
                        let title = if not (System.String.IsNullOrEmpty(doc.Info.Title)) then doc.Info.Title else "Generated API Client"
                        let desc = if not (System.String.IsNullOrEmpty(doc.Info.Description)) then
                                       $"\n\n%s{doc.Info.Description}" else ""
                        let version = if not (System.String.IsNullOrEmpty(doc.Info.Version)) then
                                          $"\n\nVersion: %s{doc.Info.Version}" else ""

                        $"%s{title}%s{desc}%s{version}"
                    else "Generated API Client"
                GenerationTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                TypeDocumentation = []
                FunctionDocumentation = []
                NeedsDict = needsDict
                NeedsHttp = needsHttp
                NeedsJsonDecode = needsJsonDecode
                NeedsAndMap = needsAndMap
                NeedsJsonEncode = needsJsonEncode
                NeedsTask = needsTask
                NeedsUrl = needsUrl
            }
        
        let templatePath = "Generator/Templates/module.scriban"
        
        allModules.Add({
            QualifiedName = apiModuleName
            RelativePath = apiModuleName.Replace(".", "/") + ".elm"
            Source = renderModule templatePath apiContext
        })
        
        allModules |> List.ofSeq
        
    let generateModules (doc: OpenApiDocument) (prefix: string option) (languageTarget: ILanguageTarget) (customTemplate: string option) : ElmModule list =
        let baseModuleName = defaultArg prefix languageTarget.DefaultModulePrefix
        
        // For now, use the new language target interface for simple generation
        // TODO: In Phase 2, we can add language-specific optimizations
        let context = {
            LanguageContext.Document = doc
            ModulePrefix = prefix
            OutputPath = ""
            Force = false
            ApiDescription = if doc.Info <> null then doc.Info.Title else "Generated API"
            GenerationTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            CustomTemplatePath = customTemplate
        }
        
        let moduleCode = languageTarget.GenerateModule context
        let outputPath = languageTarget.GetOutputPath "" prefix
        
        // Create ElmModule for backward compatibility
        let elmModule = {
            ElmModule.QualifiedName = $"{baseModuleName}.Schemas"
            RelativePath = outputPath
            Source = moduleCode
        }
        
        [elmModule]
