using System.Runtime.InteropServices;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Net.Http;
using System.Net.Security;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Diagnostics.Metrics;

// Nécessaire si le collecteur n'expose pas un endpoint https
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Création du builder
var builder = WebApplication.CreateBuilder(args);

// Configuration des ressources pour identifier le service
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

// Configure OpenTelemetry pour les logs
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(loggerOptions =>
    {
        loggerOptions.IncludeFormattedMessage = true;
        loggerOptions.IncludeScopes = true;
        loggerOptions.ParseStateValues = true;
        loggerOptions
            .SetResourceBuilder(resourceBuilder)
            .AddProcessor(new CustomLogProcessor())
            .AddConsoleExporter()
            .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
    });

// Configure OpenTelemetry pour les traces
builder.Services.AddOpenTelemetry().WithTracing(tracerOptions =>
{
    tracerOptions
        .SetResourceBuilder(resourceBuilder)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddSource("MyActivitySource")
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Configure OpenTelemetry pour les metrics
builder.Services.AddOpenTelemetry().WithMetrics(MetricsOptions =>
{
    MetricsOptions
        .SetResourceBuilder(resourceBuilder)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddMeter("MyMeter")
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Création de l'app
var app = builder.Build();

// Création des providers
var activitySource = new ActivitySource("MyActivitySource");
var meter = new Meter("MyMeter");

// Création d'instruments de metrics
var requestCounter = meter.CreateCounter<int>("compute_requests");
var httpClient = new HttpClient();

// Endpoint racine qui va, à chaque appel : incrémenter le compteur de 1, créer une activité, logger des traces
app.MapGet("/", async (ILogger<Program> logger) =>
{
    requestCounter.Add(1);

    using (var activity = activitySource.StartActivity("Get data"))
    {
        // on peut ajouter des informations à l'activité, qu'on va pouvoir visionner sur Zipkin.
        activity?.AddTag("sample", "value");
        activity?.AddBaggage("SampleContext", "BaggageIci");

        // Les requêtes http sont suivies par AddHttpClientInstrumentation
        var str1 = await httpClient.GetStringAsync("https://example.com");
        var str2 = await httpClient.GetStringAsync("https://www.meziantou.net");

        logger.LogInformation("Response1 length: {Length}", str1.Length);
        logger.LogInformation("Response2 length: {Length}", str2.Length);
    }

    return Results.Ok();
});

// démarrage de l'app
app.Run();
