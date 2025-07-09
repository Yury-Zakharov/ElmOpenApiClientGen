namespace ElmOpenApiClientGen.Languages

open ElmOpenApiClientGen.Languages.Elm

/// Factory for creating language-specific generators
module GeneratorFactory =
    
    /// Create a language target implementation based on the target type
    let createLanguageTarget (target: LanguageTarget) : ILanguageTarget =
        match target with
        | Elm -> new ElmLanguageTarget() :> ILanguageTarget
    
    /// Get all available language targets
    let getAvailableTargets () : LanguageTarget list =
        [ Elm ]
    
    /// Parse a language target from string
    let parseLanguageTarget (targetStr: string) : Result<LanguageTarget, string> =
        match targetStr.ToLower() with
        | "elm" -> Ok Elm
        | _ -> Error $"Unsupported target language: {targetStr}. Available targets: elm"
    
    /// Get the default language target
    let getDefaultTarget () : LanguageTarget = Elm