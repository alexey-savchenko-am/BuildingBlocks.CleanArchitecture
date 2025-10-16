using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
namespace BuildingBlocks.CleanArchitecture.Presentation.Middlewares;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[HeaderName] = correlationId;

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                _logger.LogDebug("Started processing request with CorrelationId {CorrelationId}", correlationId);

                var sw = Stopwatch.StartNew();
                await _next(context);
                sw.Stop();

                _logger.LogDebug("Finished processing request {CorrelationId} in {Elapsed:0.000}s",
                    correlationId, sw.Elapsed.TotalSeconds);
            }
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
            && Guid.TryParse(correlationId, out var parsedCorrelationId))
        {
            return parsedCorrelationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
