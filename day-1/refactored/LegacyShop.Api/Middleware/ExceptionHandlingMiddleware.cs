using System.Text.Json;

namespace LegacyShop.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request was cancelled: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = "https://httpstatuses.com/500",
                title = "An unexpected error occurred.",
                status = 500,
                detail = "The server encountered an unexpected error."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}