using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QuotesApi.Data;
using QuotesApi.Extensions;
using QuotesApi.Middleware;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry tracing
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
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Correlate Serilog logs with the OpenTelemetry Activity TraceId
app.Use(async (ctx, next) =>
{
    var traceId = Activity.Current?.TraceId.ToString()
        ?? ctx.TraceIdentifier;

    using (LogContext.PushProperty("TraceId", traceId))
    {
        await next();
    }
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply pending EF Core migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapQuoteEndpoints();
app.MapCollectionEndpoints();
app.MapAuthEndpoints();

app.Run();

public partial class Program
{
}