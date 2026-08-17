using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using QuotesApi.Data;
using QuotesApi.Extensions;
using QuotesApi.Middleware;
using QuotesApi.Options;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// Configuration - strongly typed JWT options
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

// OpenTelemetry tracing + Azure Monitor
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService("QuotesApi"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            });
    })
    .UseAzureMonitor(options =>
    {
        options.ConnectionString =
            builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    });

// Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtOptions>()!);

var app = builder.Build();

// Correlation ID for Serilog
app.Use(async (ctx, next) =>
{
    using (LogContext.PushProperty("TraceId", ctx.TraceIdentifier))
    {
        await next();
    }
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapQuoteEndpoints();

app.Run();