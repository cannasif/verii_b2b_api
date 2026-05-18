using Microsoft.AspNetCore.Http;

namespace Wms.WebApi.Telemetry;

public static class HttpContextTraceExtensions
{
    private const string TraceIdKey = "__WmsTraceId";
    private const string RequestOutcomeKey = "__WmsRequestOutcome";

    public static string? GetTraceId(this HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(TraceIdKey, out var value) ? value as string : null;
    }

    public static void SetTraceId(this HttpContext httpContext, string traceId)
    {
        httpContext.Items[TraceIdKey] = traceId;
    }

    public static string? GetRequestOutcome(this HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(RequestOutcomeKey, out var value) ? value as string : null;
    }

    public static void SetRequestOutcome(this HttpContext httpContext, string outcome)
    {
        httpContext.Items[RequestOutcomeKey] = outcome;
    }
}
