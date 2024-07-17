using System.Runtime.InteropServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Net.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyAspNetCoreService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        });
});

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: "Cosoluce.RandomApp.LogsDemoApi", serviceVersion: "1.0.1")
    .AddTelemetrySdk()
    .AddAttributes(new Dictionary<string, object>
    {
        ["Nom/ID du host"] = Environment.MachineName,
        ["Système d'exploitation"] = RuntimeInformation.OSDescription,
        ["environnement"] =
            builder.Environment.EnvironmentName.ToLowerInvariant(),
        ["texte aléatoire"] = "Je rajoute ce que je veux"
        
    });

builder.Logging.ClearProviders()
    .AddOpenTelemetry(loggerOptions =>
    {
        loggerOptions
            .SetResourceBuilder(resourceBuilder)
            .AddProcessor(new CustomLogProcessor())
            .AddConsoleExporter();

        loggerOptions.IncludeFormattedMessage = true;
        loggerOptions.IncludeScopes = true;
        loggerOptions.ParseStateValues = true;
    });

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogCritical("Alerte");
    return("Salut");
}); 

app.Run();
