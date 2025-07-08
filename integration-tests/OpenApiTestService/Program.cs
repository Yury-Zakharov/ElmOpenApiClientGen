using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using System.Text;
using YamlDotNet.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add CORS support for integration tests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Load the comprehensive OpenAPI spec
var openApiYamlPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "comprehensive-openapi.yaml");
var openApiYaml = await File.ReadAllTextAsync(openApiYamlPath);

// Parse the OpenAPI document
var openApiReader = new OpenApiStringReader();
var openApiDoc = openApiReader.Read(openApiYaml, out var diagnostics);

if (diagnostics.Errors.Count > 0)
{
    Console.WriteLine("OpenAPI parsing errors:");
    foreach (var error in diagnostics.Errors)
    {
        Console.WriteLine($"  - {error.Message}");
    }
}

// Serve OpenAPI spec as YAML
app.MapGet("/openapi.yaml", () =>
{
    return Results.Content(openApiYaml, "application/yaml");
});

// Serve OpenAPI spec as JSON
app.MapGet("/openapi.json", () =>
{
    using var stringWriter = new StringWriter();
    var jsonWriter = new OpenApiJsonWriter(stringWriter);
    openApiDoc.SerializeAsV3(jsonWriter);
    var jsonContent = stringWriter.ToString();
    
    return Results.Content(jsonContent, "application/json");
});

// Health check endpoint
app.MapGet("/health", () => new
{
    Status = "healthy",
    Timestamp = DateTime.UtcNow,
    Services = new Dictionary<string, string>
    {
        ["openapi"] = "available"
    }
});

// Root endpoint with information about available endpoints
app.MapGet("/", () => new
{
    Message = "OpenAPI Test Service for ElmOpenApiClientGen Integration Tests",
    Endpoints = new[]
    {
        "/openapi.yaml - OpenAPI specification in YAML format",
        "/openapi.json - OpenAPI specification in JSON format",
        "/health - Health check endpoint"
    },
    OpenApiInfo = new
    {
        Title = openApiDoc.Info.Title,
        Version = openApiDoc.Info.Version,
        Description = openApiDoc.Info.Description
    }
});

Console.WriteLine("OpenAPI Test Service starting...");
Console.WriteLine($"OpenAPI spec loaded from: {openApiYamlPath}");
Console.WriteLine("Available endpoints:");
Console.WriteLine("  - GET /openapi.yaml");
Console.WriteLine("  - GET /openapi.json");
Console.WriteLine("  - GET /health");

app.Run();
