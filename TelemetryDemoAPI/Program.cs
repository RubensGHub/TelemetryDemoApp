using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using GameLibraryAPI.Models;

// Nécessaire si le collecteur n'expose pas un endpoint https
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Création du builder
var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddSingleton(new Meter("MyMeter"));  

// Ajout des services aux conteneurs
builder.Services.AddControllers();
builder.Services.AddDbContext<GameContext>(opt =>
    opt.UseInMemoryDatabase("GameLibrary"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: "AppDemoAPI", serviceVersion: "1.0.0")
    .AddTelemetrySdk()
    .AddAttributes(new Dictionary<string, object>
    {
        ["host.name"] = Environment.MachineName,
        ["OS.name"] = RuntimeInformation.OSDescription,
        ["environment"] = builder.Environment.EnvironmentName.ToLowerInvariant(),
    });

// Configuration d'OpenTelemetry pour les logs
builder.Logging.ClearProviders();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
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
        .AddSource("ActivitesAPI")
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Configure OpenTelemetry pour les metrics
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
var activitySource = new ActivitySource("ActivitesAPI");
var meter = new Meter("MyMeter");

// Création d'instruments de métriques
var requestCounter = meter.CreateCounter<int>("Requests");
var gameCountUpDownCounter = meter.CreateUpDownCounter<int>("game_count");

// Configuration du pipeline de requête HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoint de démonstration d'une requête passant par plusieurs services
app.MapGet("/demo", async () =>
{
    requestCounter.Add(1);
    await Task.Delay(200);
    using (var activity = activitySource.StartActivity("TestDemo"))
    {
        await Task.Delay(300);
        var client = new HttpClient();
        await client.GetStringAsync("https://localhost:7123/delay");
    }
    return Results.Ok();
});

// Second endpoint pour simuler une action 
app.MapGet("/delay", async() =>
{   
    requestCounter.Add(1);
    await Task.Delay(100);
    using (var activity = activitySource.StartActivity("TestDemo2"))
    {
        await Task.Delay(100);
    }

    return Results.Ok();
});

// Endpoint pour montrer que l'on peut cacher des informations sensibles comme les mots de passes etc.
app.MapPost("/login", (ILogger<Program> logger, [FromBody] LoginData data) =>
{
    logger.LogInformation("User login attempted: Username {Username}, Password {Password}", data.Username, data.Password);
    logger.LogWarning("User login failed: Username {Username}", data.Username);
    return Results.Unauthorized();
});

// démarrage de l'app
app.Run();

internal record LoginData(string Username, string Password);