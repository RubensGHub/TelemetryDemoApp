using System.Runtime.InteropServices;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;
using System.Diagnostics.Metrics;



// Nécessaire si le collecteur n'expose pas un endpoint https
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Création du builder
var builder = WebApplication.CreateBuilder(args);

// Configuration des ressources pour identifier le service
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: "Cosoluce.AppDemo", serviceVersion: "1.0.1")
    .AddTelemetrySdk()
    .AddAttributes(new Dictionary<string, object>
    {
        ["Host.name"] = Environment.MachineName,
        ["OS.name"] = RuntimeInformation.OSDescription,
        ["Environment"] =
            builder.Environment.EnvironmentName.ToLowerInvariant(),
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
        .AddSource("ActivitesApp")
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
var activitySource = new ActivitySource("ActiviteApp");
var meter = new Meter("MyMeter");

// Création d'instruments de metrics
var requestCounter = meter.CreateCounter<int>("compute_requests");
using var handler = new SocketsHttpHandler()
{
    ActivityHeadersPropagator = DistributedContextPropagator.CreateDefaultPropagator(),
};
using var client = new HttpClient(handler);

// Endpoint racine qui va, à chaque appel : incrémenter le compteur de 1, créer une activité, logger des traces
app.MapGet("/", async (ILogger<Program> logger) =>
{
    requestCounter.Add(1);

    using (var activity = activitySource.StartActivity("Get data"))
    {
        // on peut ajouter des informations à l'activité, qu'on va pouvoir visionner sur Zipkin.
        activity?.AddTag("sample", "value");
        Baggage.SetBaggage("user.id", "12345");

        // Les requêtes http sont suivies par AddHttpClientInstrumentation
        await client.GetStringAsync("https://example.com");
        var str1 = await client.GetStringAsync("https://localhost:7123/demo");

        logger.LogInformation("Response1 length: {Length}", str1.Length);
    }

    return Results.Ok();
});

// Autre endpoint de test
app.MapGet("/test", (HttpContext context) =>
{
    var activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
    activity?.SetTag("foo", "bar");

    return Results.Ok();
});

// démarrage de l'app
app.Run();
