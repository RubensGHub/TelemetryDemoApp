using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using System.Runtime.InteropServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Define attributes for your application

var resourceBuilder = ResourceBuilder.CreateDefault()
    // add attributes for the name and version of the service
    .AddService(serviceName: "Cosoluce.RandomApp.LogsDemoApi", serviceVersion: "1.0.1")
    // add attributes for the OpenTelemetry SDK version
    .AddTelemetrySdk()
    // add custom attributes
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
            // .SetResourceBuilder(resourceBuilder)
            // .AddProcessor(new CustomLogProcessor())
            .AddConsoleExporter();

        loggerOptions.IncludeFormattedMessage = false;
        loggerOptions.IncludeScopes = false;
        loggerOptions.ParseStateValues = false;
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Hello user!");
    Console.WriteLine("Test erreur critique :");
    logger.LogCritical("Erreur critique");
    return("Hello world");
});

app.MapPost("/login", (ILogger<Program> logger, [FromBody] LoginData data) =>
{
    logger.LogInformation("User login attempted: Username {Username}, Password {Password}", data.Username, data.Password);
    logger.LogWarning("User login failed: Username {Username}", data.Username);
    return Results.Unauthorized();
});

app.Run();

internal record LoginData(string Username, string Password);