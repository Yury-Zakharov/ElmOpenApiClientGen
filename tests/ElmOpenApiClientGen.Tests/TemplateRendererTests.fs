module TemplateRendererTests

open System
open System.IO
open Xunit
open ElmOpenApiClientGen.Generator.TemplateRenderer

[<Fact>]
let ``ElmTemplateContext should have all required properties`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api.Schemas"
        Types = ["type User = { id : Int, name : String }"]
        Decoders = ["decoderUser : Decoder User"]
        Encoders = ["encodeUser : User -> Value"]
        Requests = ["getUser : Config -> Int -> Task Http.Error User"]
        ApiDescription = "Test API"
        GenerationTimestamp = "2025-07-07"
        TypeDocumentation = ["{-| User type -}"]
        FunctionDocumentation = ["{-| Get user by ID -}"]
        NeedsDict = true
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = false
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - All properties should be accessible
    Assert.Equal("Api.Schemas", context.ModuleName)
    Assert.Equal("Test API", context.ApiDescription)
    Assert.Equal("2025-07-07", context.GenerationTimestamp)
    Assert.NotEmpty(context.Types)
    Assert.NotEmpty(context.Decoders)
    Assert.NotEmpty(context.Encoders)
    Assert.NotEmpty(context.Requests)
    Assert.True(context.NeedsDict)
    Assert.True(context.NeedsHttp)
    Assert.True(context.NeedsJsonDecode)
    Assert.False(context.NeedsAndMap)

[<Fact>]
let ``ElmTemplateContext should support empty collections`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Empty.Module"
        Types = []
        Decoders = []
        Encoders = []
        Requests = []
        ApiDescription = ""
        GenerationTimestamp = ""
        TypeDocumentation = []
        FunctionDocumentation = []
        NeedsDict = false
        NeedsHttp = false
        NeedsJsonDecode = false
        NeedsAndMap = false
        NeedsJsonEncode = false
        NeedsTask = false
        NeedsUrl = false
    }
    
    // Assert - Empty collections should be handled
    Assert.Empty(context.Types)
    Assert.Empty(context.Decoders)
    Assert.Empty(context.Encoders)
    Assert.Empty(context.Requests)
    Assert.Empty(context.TypeDocumentation)
    Assert.Empty(context.FunctionDocumentation)
    Assert.False(context.NeedsDict)
    Assert.False(context.NeedsHttp)

[<Fact>]
let ``ElmTemplateContext should support complex type collections`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api.Complex"
        Types = [
            "type User = { id : Int, name : String, profile : UserProfile }"
            "type UserProfile = { firstName : String, lastName : String }"
            "type alias Config = { baseUrl : String, bearerToken : String }"
        ]
        Decoders = [
            "decoderUser : Decoder User"
            "decoderUserProfile : Decoder UserProfile"
        ]
        Encoders = [
            "encodeUser : User -> Value"
            "encodeUserProfile : UserProfile -> Value"
        ]
        Requests = [
            "getUser : Config -> Int -> Task GetUserError User"
            "updateUser : Config -> Int -> User -> Task UpdateUserError User"
        ]
        ApiDescription = "Complex API with multiple types"
        GenerationTimestamp = "2025-07-07 10:30:00"
        TypeDocumentation = [
            "{-| User represents a system user -}"
            "{-| UserProfile contains user profile information -}"
        ]
        FunctionDocumentation = [
            "{-| Get user by ID -}"
            "{-| Update user information -}"
        ]
        NeedsDict = true
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = true
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - Multiple items should be handled correctly
    Assert.Equal(3, context.Types.Length)
    Assert.Equal(2, context.Decoders.Length)
    Assert.Equal(2, context.Encoders.Length)
    Assert.Equal(2, context.Requests.Length)
    Assert.Equal(2, context.TypeDocumentation.Length)
    Assert.Equal(2, context.FunctionDocumentation.Length)
    Assert.Contains("User", context.Types[0])
    Assert.Contains("UserProfile", context.Types[1])
    Assert.Contains("Config", context.Types[2])

[<Fact>]
let ``ElmTemplateContext import flags should work independently`` () =
    // Test each import flag independently
    let baseContext = {
        ModuleName = "Test"
        Types = []
        Decoders = []
        Encoders = []
        Requests = []
        ApiDescription = ""
        GenerationTimestamp = ""
        TypeDocumentation = []
        FunctionDocumentation = []
        NeedsDict = false
        NeedsHttp = false
        NeedsJsonDecode = false
        NeedsAndMap = false
        NeedsJsonEncode = false
        NeedsTask = false
        NeedsUrl = false
    }
    
    // Test NeedsDict flag
    let dictContext = { baseContext with NeedsDict = true }
    Assert.True(dictContext.NeedsDict)
    Assert.False(dictContext.NeedsHttp)
    
    // Test NeedsHttp flag
    let httpContext = { baseContext with NeedsHttp = true }
    Assert.True(httpContext.NeedsHttp)
    Assert.False(httpContext.NeedsDict)
    
    // Test multiple flags
    let multiContext = { baseContext with NeedsDict = true; NeedsHttp = true; NeedsTask = true }
    Assert.True(multiContext.NeedsDict)
    Assert.True(multiContext.NeedsHttp)
    Assert.True(multiContext.NeedsTask)
    Assert.False(multiContext.NeedsUrl)

[<Fact>]
let ``ElmTemplateContext should handle special characters in strings`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api.Special"
        Types = ["type User = { name : String -- Name with \"quotes\" }"]
        Decoders = ["-- Decoder with special chars: <>&"]
        Encoders = ["-- Encoder with newlines:\n-- Second line"]
        Requests = ["-- Request with unicode: αβγ"]
        ApiDescription = "API with special chars: <>&\"'"
        GenerationTimestamp = "2025-07-07T10:30:00Z"
        TypeDocumentation = ["{-| Type with special chars: <>& -}"]
        FunctionDocumentation = ["{-| Function with quotes: \"test\" -}"]
        NeedsDict = false
        NeedsHttp = true
        NeedsJsonDecode = false
        NeedsAndMap = false
        NeedsJsonEncode = false
        NeedsTask = false
        NeedsUrl = false
    }
    
    // Assert - Special characters should be preserved
    Assert.Contains("\"quotes\"", context.Types[0])
    Assert.Contains("<>&", context.Decoders[0])
    Assert.Contains("\n", context.Encoders[0])
    Assert.Contains("αβγ", context.Requests[0])
    Assert.Contains("<>&\"'", context.ApiDescription)
    Assert.Contains("T", context.GenerationTimestamp)
    Assert.Contains("Z", context.GenerationTimestamp)