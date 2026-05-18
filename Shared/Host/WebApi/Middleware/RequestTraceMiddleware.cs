using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Wms.Application.Common;
using Wms.WebApi.Telemetry;

namespace Wms.WebApi.Middleware;

public sealed class RequestTraceMiddleware
{
    private const string TraceHeaderName = "X-Trace-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTraceMiddleware> _logger;

    public RequestTraceMiddleware(RequestDelegate next, ILogger<RequestTraceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserAccessor currentUserAccessor)
    {
        var traceId = ResolveTraceId(context);
        context.SetTraceId(traceId);
        context.Response.Headers[TraceHeaderName] = traceId;

        Activity.Current?.SetTag("wms.trace_id", traceId);
        Activity.Current?.SetTag("wms.branch_code", currentUserAccessor.BranchCode);

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = traceId,
            ["Path"] = context.Request.Path.Value,
            ["Method"] = context.Request.Method
        });

        _logger.LogInformation(
            "HTTP request started {Method} {Path} TraceId={TraceId} BranchCode={BranchCode}",
            context.Request.Method,
            context.Request.Path.Value,
            traceId,
            currentUserAccessor.BranchCode);

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var outcome = context.GetRequestOutcome() ?? ClassifyOutcome(context.Response.StatusCode);
        WmsTelemetry.RecordRequestCompleted(
            outcome,
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty,
            context.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds);

        _logger.LogInformation(
            "HTTP request completed {Method} {Path} TraceId={TraceId} StatusCode={StatusCode} Outcome={Outcome} DurationMs={DurationMs} UserId={UserId} UserEmail={UserEmail} BranchCode={BranchCode}",
            context.Request.Method,
            context.Request.Path.Value,
            traceId,
            context.Response.StatusCode,
            outcome,
            stopwatch.Elapsed.TotalMilliseconds,
            currentUserAccessor.UserId,
            currentUserAccessor.UserEmail,
            currentUserAccessor.BranchCode);
    }

    private static string ResolveTraceId(HttpContext context)
    {
        var headerTraceId = context.Request.Headers[TraceHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerTraceId))
        {
            return headerTraceId.Trim();
        }

        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }

    private static string ClassifyOutcome(int statusCode)
    {
        if (statusCode is >= 200 and < 300)
        {
            return RequestOutcome.Succeeded;
        }

        return statusCode switch
        {
            StatusCodes.Status400BadRequest => RequestOutcome.ValidationFailed,
            StatusCodes.Status401Unauthorized => RequestOutcome.Unauthorized,
            StatusCodes.Status403Forbidden => RequestOutcome.Forbidden,
            StatusCodes.Status404NotFound => RequestOutcome.NotFound,
            StatusCodes.Status409Conflict => RequestOutcome.Conflict,
            _ when statusCode >= 500 => RequestOutcome.Failed,
            _ => RequestOutcome.Failed
        };
    }
}
