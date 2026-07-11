using System.Net;
using System.Text.Json;

namespace LeaveManagement.Api.Services;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                timestamp = DateTime.UtcNow,
                status = 500,
                errorCode = "INTERNAL_SERVER_ERROR",
                message = "An unexpected error occurred.",
                path = context.Request.Path.Value,
                traceId = context.TraceIdentifier
            }));
        }
    }
}
