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
            
        member _.GenerateModule(context: LanguageContext) =
            // For Phase 1, use the existing Codegen.generateModules function
            // We'll refactor this properly in Phase 2
            let doc = context.Document
            let prefix = context.ModulePrefix
            
            // Create a minimal template context to generate basic module
            let moduleName = 
                match prefix with
                | Some p -> $"{p}.Schemas"
                | None -> "Api.Schemas"
            
            // Generate a basic module structure
            $"""module {moduleName} exposing (..)

{{-| {context.ApiDescription}

Generated on: {context.GenerationTimestamp}

This module was generated from an OpenAPI specification by [ElmOpenApiClientGen](https://github.com/Yury-Zakharov/ElmOpenApiClientGen).
💖 Support the project: https://github.com/sponsors/Yury-Zakharov

-}}

import Dict exposing (Dict)
import Http
import Json.Decode as Decode exposing (Decoder)
import Json.Decode.Extra exposing (andMap)
import Json.Encode as Encode exposing (Value)
import Task exposing (Task)
import Url.Builder as Url

-- Basic configuration type
type alias Config =
    {{ baseUrl : String
    , apiKey : String
    , bearerToken : String
    , basicAuth : String
    , customHeaders : List (String, String)
    , timeout : Maybe Float
    }}

-- TODO: Add actual types and requests from the OpenAPI spec
"""
            
        member _.ValidateOutput(output: string) =
            // Basic validation - check if the output contains basic Elm module structure
            if output.Contains("module ") && output.Contains("exposing") then
                Ok ()
            else
                Error "Generated Elm code does not contain valid module structure"
                
        member _.GetOutputPath(basePath: string) (modulePrefix: string option) =
            let prefix = modulePrefix |> Option.defaultValue "Api"
            Path.Combine(basePath, prefix, "Schemas.elm")