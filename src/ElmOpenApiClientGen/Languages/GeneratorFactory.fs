namespace ElmOpenApiClientGen.Languages

open ElmOpenApiClientGen.Languages.Elm
open ElmOpenApiClientGen.Languages.Haskell

/// Factory for creating language-specific generators
module GeneratorFactory =
    
    /// Create a language target implementation based on the target type
    let createLanguageTarget (target: LanguageTarget) : ILanguageTarget =
        match target with
        | Elm -> new ElmLanguageTarget() :> ILanguageTarget
        | Haskell -> new HaskellLanguageTarget() :> ILanguageTarget
    
    /// Get all available language targets
    let getAvailableTargets () : LanguageTarget list =
        [ Elm; Haskell ]
    
    /// Parse a language target from string
    let parseLanguageTarget (targetStr: string) : Result<LanguageTarget, string> =
        match targetStr.ToLower() with
        | "elm" -> Ok Elm
        | "haskell" | "hs" -> Ok Haskell
        | _ -> Error $"Unsupported target language: {targetStr}. Available targets: elm, haskell"
    
    /// Get the default language target
    let getDefaultTarget () : LanguageTarget = Elm