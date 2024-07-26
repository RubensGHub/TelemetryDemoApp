using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Http.Features;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using AppDemoTelemetry.Models;

// Nécessaire si le collecteur n'expose pas un endpoint https
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Création du builder
var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddHttpClient();

// Configuration des ressources pour identifier le service
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: "AppDemo", serviceVersion: "1.0.0")
    .AddTelemetrySdk()
    .AddAttributes(new Dictionary<string, object>
    {
        ["Host.name"] = Environment.MachineName,
        ["OS.name"] = RuntimeInformation.OSDescription,
        ["Environment"] = builder.Environment.EnvironmentName.ToLowerInvariant(),
    });

// Configuration d'OpenTelemetry pour les logs
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(loggerOptions =>
    {
        loggerOptions.IncludeFormattedMessage = true;
        loggerOptions.IncludeScopes = true;
        loggerOptions.ParseStateValues = true;
        loggerOptions
            .SetResourceBuilder(resourceBuilder)
            .AddProcessor(new CustomLogProcessor())
            .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
    });

// Configuration d'OpenTelemetry pour les traces
builder.Services.AddOpenTelemetry().WithTracing(tracerOptions =>
{
    tracerOptions
        .SetResourceBuilder(resourceBuilder)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddSource("ActivitesApp")
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Configuration d'OpenTelemetry pour les metrics
builder.Services.AddOpenTelemetry().WithMetrics(MetricsOptions =>
{
    MetricsOptions
        .SetResourceBuilder(resourceBuilder)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddProcessInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("MyMeter")
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Création de l'app
var app = builder.Build();

// Création des providers
var activitySource = new ActivitySource("ActivitesApp");
var meter = new Meter("MyMeter");

// Création d'instruments de métriques
var requestCounter = meter.CreateCounter<int>("requetes");
var responseTimeHistogram = meter.CreateHistogram<float>("TempsDeReponse", unit: "ms", description: "Histogram of response times");
meter.CreateObservableGauge("ThreadCount", () => new[] { new Measurement<int>(ThreadPool.ThreadCount) });

using var handler = new SocketsHttpHandler()
{
    ActivityHeadersPropagator = DistributedContextPropagator.CreateDefaultPropagator(),
};
using var client = new HttpClient(handler);

// Endpoint racine qui va, à chaque appel : incrémenter le compteur de 1, créer une activité, logger des traces
app.MapGet("/", async (ILogger<Program> logger) =>
{   
    // On démarre le chronomètre pour l'histogramme
    var stopwatch = Stopwatch.StartNew();
    
    // On incrémente le compteur
    requestCounter.Add(1, KeyValuePair.Create<string, object?>("test", "test"));

    // On crée une activité
    using var activity = activitySource.StartActivity("Get data");

    // on peut ajouter des informations à l'activité, qu'on va pouvoir visionner sur Zipkin.
    activity?.AddTag("sample", "value");
    Baggage.SetBaggage("user.id", "12345");

    // Les requêtes http sont suivies par AddHttpClientInstrumentation
    var str1 = await client.GetStringAsync("https://example.com");
    var str2 = await client.GetStringAsync("https://localhost:7123/demo");



    logger.LogInformation("Response1 length: {Length}", str1.Length);

    Random random = new();
    int RandomInt = random.Next(101);

    stopwatch.Stop();

    responseTimeHistogram.Record(stopwatch.ElapsedMilliseconds, KeyValuePair.Create<string, object?>("endpoint", "root"));

    return ("Salut ! " + RandomInt);
});

// Autre endpoint de test
app.MapGet("/games", async (ILogger<Program> logger, IHttpClientFactory httpClientFactory) =>
{
    var stopwatch = Stopwatch.StartNew();

    using var activity = activitySource.StartActivity("Get Games");

    var client = httpClientFactory.CreateClient();

    // URL de l'API GameLibrary (Point de terminaison où sont stockés sous forme de JSON les jeux)
    var apiUrl = "https://localhost:7123/api/Games";

    // Envoyer une requête GET à l'API
    var response = await client.GetAsync(apiUrl);

    if (response.IsSuccessStatusCode)
    {
        // Lire le contenu de la réponse et le désérialiser en liste de GameDTO
        var games = await response.Content.ReadFromJsonAsync<IEnumerable<GameDTO>>();

        stopwatch.Stop();
        responseTimeHistogram.Record(stopwatch.ElapsedMilliseconds, KeyValuePair.Create<string, object?>("endpoint", "root"));

        return Results.Ok(games);
    }
    else
    {
        // Logger l'erreur et retourner un message d'erreur
        logger.LogError("Erreur lors de la récupération des jeux : {StatusCode}", response.StatusCode);

        stopwatch.Stop();
        responseTimeHistogram.Record(stopwatch.ElapsedMilliseconds, KeyValuePair.Create<string, object?>("endpoint", "root"));

        return Results.Problem("Erreur lors de la récupération des jeux");
    }
});

// démarrage de l'app
app.Run();
