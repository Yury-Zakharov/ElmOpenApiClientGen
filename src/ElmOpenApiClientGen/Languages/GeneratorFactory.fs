namespace ElmOpenApiClientGen.Languages

open ElmOpenApiClientGen.Languages.Elm
open ElmOpenApiClientGen.Languages.Haskell
open ElmOpenApiClientGen.Languages.CSharp

/// Factory for creating language-specific generators
module GeneratorFactory =
    
    /// Create a language target implementation based on the target type
    let createLanguageTarget (target: LanguageTarget) : ILanguageTarget =
        match target with
        | Elm -> new ElmLanguageTarget() :> ILanguageTarget
        | Haskell -> new HaskellLanguageTarget() :> ILanguageTarget
        | CSharp -> new CSharpLanguageTarget() :> ILanguageTarget
    
    /// Get all available language targets
    let getAvailableTargets () : LanguageTarget list =
        [ Elm; Haskell; CSharp ]
    
    /// Parse a language target from string
    let parseLanguageTarget (targetStr: string) : Result<LanguageTarget, string> =
        match targetStr.ToLower() with
        | "elm" -> Ok Elm
        | "haskell" | "hs" -> Ok Haskell
        | "csharp" | "cs" | "c#" -> Ok CSharp
        | _ -> Error $"Unsupported target language: {targetStr}. Available targets: elm, haskell, csharp"
    
    /// Get the default language target
    let getDefaultTarget () : LanguageTarget = Elm