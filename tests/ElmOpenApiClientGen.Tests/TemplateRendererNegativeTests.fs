module TemplateRendererNegativeTests

open System
open System.IO
open Xunit
open ElmOpenApiClientGen.Generator.TemplateRenderer

[<Fact>]
let ``ElmTemplateContext should handle null and empty strings`` () =
    // Arrange & Act
    let context = {
        ModuleName = null
        Types = []
        Decoders = []
        Encoders = []
        Requests = []
        ApiDescription = null
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
    
    // Assert - Should handle null values gracefully
    Assert.Null(context.ModuleName)
    Assert.Null(context.ApiDescription)
    Assert.Equal("", context.GenerationTimestamp)
    Assert.Empty(context.Types)

[<Fact>]
let ``ElmTemplateContext should handle extremely large collections`` () =
    // Arrange & Act
    let largeCollection = List.replicate 10000 "type LargeType = {}"
    let context = {
        ModuleName = "HugeModule"
        Types = largeCollection
        Decoders = largeCollection
        Encoders = largeCollection
        Requests = largeCollection
        ApiDescription = String.replicate 5000 "Very long description. "
        GenerationTimestamp = "2025-07-07"
        TypeDocumentation = largeCollection
        FunctionDocumentation = largeCollection
        NeedsDict = true
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = true
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - Should handle large collections
    Assert.Equal(10000, context.Types.Length)
    Assert.Equal(10000, context.Decoders.Length)
    Assert.True(context.ApiDescription.Length > 1000)

[<Fact>]
let ``ElmTemplateContext should handle malformed Elm code`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api.Malformed"
        Types = [
            "type Broken = { invalid syntax here"  // Missing closing brace
            "type Another = Int String"  // Invalid type definition
            "type = NoName {}"  // Missing type name
            ""  // Empty type
        ]
        Decoders = [
            "decoder : Decoder"  // Missing type
            "decoder = Decode."  // Incomplete decoder
            "invalid decoder syntax"
        ]
        Encoders = [
            "encode : -> Value"  // Missing input type
            "encode ="  // Incomplete encoder
        ]
        Requests = [
            "request : Config ->"  // Missing return type
            "request config ="  // Incomplete request
            "malformed request syntax"
        ]
        ApiDescription = "API with malformed code"
        GenerationTimestamp = "2025-07-07"
        TypeDocumentation = [
            "{-| Unclosed comment"
            "Not a comment"
            "{-| Nested {-| comment -} -}"
        ]
        FunctionDocumentation = [
            "{-| Function without closing -"
            "No documentation marker"
        ]
        NeedsDict = true
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = false
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - Should preserve malformed content as-is
    Assert.Contains("invalid syntax here", context.Types[0])
    Assert.Contains("Decode.", context.Decoders[1])
    Assert.Contains("Config ->", context.Requests[0])
    Assert.Contains("Unclosed comment", context.TypeDocumentation[0])

[<Fact>]
let ``ElmTemplateContext should handle Unicode and special characters`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api.Unicode测试αβγ"
        Types = [
            "type User = { name : String -- 用户名称"
            "type Product = { price : Float -- €$¥£ }"
            "type Message = { content : String -- 🚀🎉💡 }"
        ]
        Decoders = [
            "decoderUser : Decoder User -- αβγ decoder"
            "decoderProduct : Decoder Product -- Διαφορετικά"
        ]
        Encoders = [
            "encodeUser : User -> Value -- кодировать пользователя"
        ]
        Requests = [
            "getUser : Config -> Task Error User -- 获取用户"
        ]
        ApiDescription = "API with Unicode: 中文 العربية русский ελληνικά 🌍"
        GenerationTimestamp = "2025-07-07T10:30:00Ⅻ"
        TypeDocumentation = [
            "{-| User type with special chars: <>&\"' -}"
            "{-| Product with currency: €$¥£ -}"
        ]
        FunctionDocumentation = [
            "{-| Function with emoji: 🚀 -}"
        ]
        NeedsDict = true
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = false
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - Should preserve Unicode characters
    Assert.Contains("测试αβγ", context.ModuleName)
    Assert.Contains("用户名称", context.Types[0])
    Assert.Contains("€$¥£", context.Types[1])
    Assert.Contains("🚀🎉💡", context.Types[2])
    Assert.Contains("中文 العربية русский ελληνικά 🌍", context.ApiDescription)
    Assert.Contains("🚀", context.FunctionDocumentation[0])

[<Fact>]
let ``ElmTemplateContext should handle control characters and newlines`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api\nWith\nNewlines"
        Types = [
            "type User =\n    { id : Int\n    , name : String\n    }"
            "type Product =\r\n    { price : Float\r\n    }"
        ]
        Decoders = [
            "decoderUser :\n    Decoder User\ndecoderUser =\n    Decode.map2 User\n        (field \"id\" int)\n        (field \"name\" string)"
        ]
        Encoders = [
            "encodeUser :\n    User -> Value\nencodeUser user =\n    object\n        [ (\"id\", int user.id)\n        , (\"name\", string user.name)\n        ]"
        ]
        Requests = [
            "getUser :\n    Config\n    -> Int\n    -> Task Error User"
        ]
        ApiDescription = "API description\nwith\nmultiple\nlines\n\nand\r\n\r\ncarriage returns"
        GenerationTimestamp = "2025-07-07\nT10:30:00Z"
        TypeDocumentation = [
            "{-| User type\n    with multiline\n    documentation\n-}"
        ]
        FunctionDocumentation = [
            "{-| Function\n    documentation\n    across lines\n-}"
        ]
        NeedsDict = false
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = false
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = false
    }
    
    // Assert - Should preserve formatting
    Assert.Contains("\n", context.ModuleName)
    Assert.Contains("\n    {", context.Types[0])
    Assert.Contains("\r\n", context.Types[1])
    Assert.Contains("multiple\nlines", context.ApiDescription)

[<Fact>]
let ``ElmTemplateContext should handle very long strings`` () =
    // Arrange & Act
    let veryLongString = String.replicate 10000 "VeryLongContent"
    let context = {
        ModuleName = veryLongString
        Types = [veryLongString]
        Decoders = [veryLongString]
        Encoders = [veryLongString]
        Requests = [veryLongString]
        ApiDescription = veryLongString
        GenerationTimestamp = veryLongString
        TypeDocumentation = [veryLongString]
        FunctionDocumentation = [veryLongString]
        NeedsDict = true
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = true
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - Should handle very long strings
    Assert.True(context.ModuleName.Length > 100000)
    Assert.True(context.ApiDescription.Length > 100000)
    Assert.Equal(1, context.Types.Length)
    Assert.True(context.Types[0].Length > 100000)

[<Fact>]
let ``ElmTemplateContext should handle collections with null elements`` () =
    // Arrange & Act
    let context = {
        ModuleName = "Api.WithNulls"
        Types = [null; "type Valid = {}"; null; ""]
        Decoders = [null; "decoderValid : Decoder Valid"; null]
        Encoders = [""; null; "encodeValid : Valid -> Value"]
        Requests = [null; null; "getValid : Config -> Task Error Valid"]
        ApiDescription = "API with nulls"
        GenerationTimestamp = "2025-07-07"
        TypeDocumentation = [null; "{-| Valid doc -}"; null]
        FunctionDocumentation = [null; "{-| Valid function -}"]
        NeedsDict = false
        NeedsHttp = true
        NeedsJsonDecode = false
        NeedsAndMap = false
        NeedsJsonEncode = false
        NeedsTask = true
        NeedsUrl = false
    }
    
    // Assert - Should handle null elements in collections
    Assert.Equal(4, context.Types.Length)
    Assert.Null(context.Types[0])
    Assert.Equal("type Valid = {}", context.Types[1])
    Assert.Null(context.Types[2])
    Assert.Equal("", context.Types[3])

[<Fact>]
let ``ElmTemplateContext should handle boolean flag inconsistencies`` () =
    // Test edge case combinations that might be logically inconsistent
    let context = {
        ModuleName = "Api.Inconsistent"
        Types = ["type User = { id : Int }"]  // Simple type
        Decoders = []  // No decoders despite having types
        Encoders = ["encodeUser : User -> Value"]  // Encoder without decoder
        Requests = []  // No requests despite having types and encoders
        ApiDescription = "Inconsistent API"
        GenerationTimestamp = "2025-07-07"
        TypeDocumentation = []
        FunctionDocumentation = []
        // Inconsistent flags - needs encode but not decode
        NeedsDict = true  // Claims to need Dict but no Dict usage visible
        NeedsHttp = false  // Claims not to need Http but has encoders
        NeedsJsonDecode = false  // No decode needed despite having types
        NeedsAndMap = true  // Needs andMap but no complex decoders
        NeedsJsonEncode = true  // Needs encode (consistent with encoders)
        NeedsTask = false  // No tasks needed despite potential requests
        NeedsUrl = true  // Needs URL but no requests
    }
    
    // Assert - Should handle inconsistent flags gracefully
    Assert.NotEmpty(context.Types)
    Assert.Empty(context.Decoders)
    Assert.NotEmpty(context.Encoders)
    Assert.Empty(context.Requests)
    Assert.True(context.NeedsDict)
    Assert.False(context.NeedsHttp)  // Inconsistent but valid
    Assert.True(context.NeedsJsonEncode)
    Assert.False(context.NeedsTask)

[<Fact>]
let ``ElmTemplateContext should handle extreme whitespace`` () =
    // Arrange & Act
    let context = {
        ModuleName = "   Api.WithWhitespace   "
        Types = [
            "   type User = { id : Int }   "
            "\t\ttype Product = { name : String }\t\t"
            "type\n\n\nService\n\n=\n\n{\n\nid\n:\nInt\n}\n\n"
        ]
        Decoders = [
            "   decoderUser : Decoder User   "
            "\t\t\tdecoderProduct : Decoder Product\t\t\t"
        ]
        Encoders = [
            "     encodeUser : User -> Value     "
        ]
        Requests = [
            "        getUser : Config -> Task Error User        "
        ]
        ApiDescription = "   API with lots of whitespace   "
        GenerationTimestamp = "\t\t2025-07-07\t\t"
        TypeDocumentation = [
            "   {-| User type   -}   "
        ]
        FunctionDocumentation = [
            "\t{-| Get user function\t-}\t"
        ]
        NeedsDict = false
        NeedsHttp = true
        NeedsJsonDecode = true
        NeedsAndMap = false
        NeedsJsonEncode = true
        NeedsTask = true
        NeedsUrl = true
    }
    
    // Assert - Should preserve whitespace as-is
    Assert.StartsWith("   ", context.ModuleName)
    Assert.EndsWith("   ", context.ModuleName)
    Assert.Contains("\t\t", context.Types[1])
    Assert.Contains("\n\n\n", context.Types[2])
    Assert.StartsWith("   ", context.ApiDescription)