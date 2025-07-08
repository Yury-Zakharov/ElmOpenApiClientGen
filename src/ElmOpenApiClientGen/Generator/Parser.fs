namespace ElmOpenApiClientGen.Generator

open System
open System.IO
open System.Net.Http
open System.Threading.Tasks
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Readers
open System.Text.Json

module Parser =

    let private isUrl (input: string) =
        input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        input.StartsWith("https://", StringComparison.OrdinalIgnoreCase)

    let private readFile (path: string) : string =
        if not (File.Exists path) then
            failwith $"Spec file not found: {path}"
        File.ReadAllText path

    let private downloadFromUrl (url: string) : string =
        try
            // Validate URL format
            if not (Uri.IsWellFormedUriString(url, UriKind.Absolute)) then
                failwith $"Invalid URL format: {url}"
            
            use client = new HttpClient()
            client.Timeout <- TimeSpan.FromSeconds(30.0)
            
            // Add common headers for better compatibility
            client.DefaultRequestHeaders.Add("User-Agent", "ElmOpenApiClientGen/1.0")
            
            let response = client.GetAsync(url).Result
            if response.IsSuccessStatusCode then
                let content = response.Content.ReadAsStringAsync().Result
                if String.IsNullOrWhiteSpace(content) then
                    failwith $"Downloaded content from URL is empty: {url}"
                content
            else
                failwith $"HTTP error {response.StatusCode} ({response.ReasonPhrase}) when downloading from URL: {url}"
        with
        | :? AggregateException as aggEx when aggEx.InnerException <> null ->
            match aggEx.InnerException with
            | :? TaskCanceledException -> failwith $"Request timeout (30 seconds) when downloading from URL: {url}"
            | :? HttpRequestException as httpEx -> failwith $"Network error when downloading from URL {url}: {httpEx.Message}"
            | ex -> failwith $"Failed to download from URL {url}: {ex.Message}"
        | ex ->
            failwith $"Failed to download from URL {url}: {ex.Message}"

    let private isYaml (pathOrUrl: string) =
        pathOrUrl.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
        pathOrUrl.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)

    let private detectYamlFromContent (content: string) =
        // Simple heuristic: if content starts with "openapi:" it's likely YAML
        let trimmedContent = content.Trim()
        trimmedContent.StartsWith("openapi:", StringComparison.OrdinalIgnoreCase) ||
        trimmedContent.StartsWith("swagger:", StringComparison.OrdinalIgnoreCase)

    let loadSpec (input: string) : OpenApiDocument =
        let content = 
            if isUrl input then
                downloadFromUrl input
            else
                readFile input
        
        let reader = OpenApiStringReader()
        let mutable diagnostic = Unchecked.defaultof<OpenApiDiagnostic>

        let isYamlContent = 
            if isUrl input then
                // For URLs, detect format from URL extension or content
                isYaml input || detectYamlFromContent content
            else
                isYaml input

        if isYamlContent then
            let yaml = YamlDotNet.Serialization.Deserializer()
            let obj = yaml.Deserialize<obj>(content)
            let json = JsonSerializer.Serialize(obj)
            reader.Read(json, &diagnostic)
        else
            reader.Read(content, &diagnostic)
