using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using System.Runtime.InteropServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Net.Security;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Threading;

// This is required if the collector doesn't expose an https endpoint
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: "Cosoluce.AppDemoAPI", serviceVersion: "1.0.1")
    .AddTelemetrySdk()
    .AddAttributes(new Dictionary<string, object>
    {
        ["host.name"] = Environment.MachineName,
        ["OS.name"] = RuntimeInformation.OSDescription,
        ["environment"] =
            builder.Environment.EnvironmentName.ToLowerInvariant(),
    });

// Configure OpenTelemetry pour les logs
builder.Logging.ClearProviders()
    .AddOpenTelemetry(loggerOptions =>
    {
        loggerOptions
            .SetResourceBuilder(resourceBuilder)
            .AddProcessor(new CustomLogProcessor())
            .AddConsoleExporter();

        loggerOptions.IncludeFormattedMessage = false;
        loggerOptions.IncludeScopes = false;
        loggerOptions.ParseStateValues = false;
    });

// Configure OpenTelemetry pour les traces
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
        .AddMeter("MyMeter")
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

var app = builder.Build();

// Création des providers
var activitySource = new ActivitySource("ActivitesAPI");
var meter = new Meter("MyMeter");

var requestCounter = meter.CreateCounter<int>("compute_requests");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/demo", async () =>
{
    requestCounter.Add(1);
    using (var activity = activitySource.StartActivity("TestDemo"))
    {
        await Task.Delay(500);
        var client = new HttpClient();
        await client.GetStringAsync("https://localhost:7123/delay");
    }
    return Results.Ok();
});

app.MapGet("/delay", async() =>
{
    using (var activity = activitySource.StartActivity("TestDemo2"))
    {
        await Task.Delay(100);
    }

    return Results.Ok();
});

app.MapPost("/login", (ILogger<Program> logger, [FromBody] LoginData data) =>
{
    logger.LogInformation("User login attempted: Username {Username}, Password {Password}", data.Username, data.Password);
    logger.LogWarning("User login failed: Username {Username}", data.Username);
    return Results.Unauthorized();
});


app.Run();

internal record LoginData(string Username, string Password);