using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Wms.WebApi.Telemetry;

public static class WmsTelemetry
{
    public const string ActivitySourceName = "WmsTelemetry";
    public const string MeterName = "WmsTelemetry";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> RequestCompletedCounter = Meter.CreateCounter<long>("wms.http.requests.completed");
    private static readonly Histogram<double> RequestDurationMs = Meter.CreateHistogram<double>("wms.http.requests.duration.ms");
    private static readonly Counter<long> AuditWriteCounter = Meter.CreateCounter<long>("wms.audit.write.completed");
    private static readonly Counter<long> AuditWriteFailureCounter = Meter.CreateCounter<long>("wms.audit.write.failed");
    private static readonly Counter<long> JobExecutionCounter = Meter.CreateCounter<long>("wms.jobs.completed");
    private static readonly Counter<long> IntegrationExecutionCounter = Meter.CreateCounter<long>("wms.integration.completed");

    public static Activity? StartActivity(string activityName, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(activityName, kind);
    }

    public static Activity? StartActivity(string activityName, ActivityKind kind, ActivityContext parentContext)
    {
        return ActivitySource.StartActivity(activityName, kind, parentContext);
    }

    public static void RecordRequestCompleted(string outcome, string method, string path, int statusCode, double durationMs)
    {
        var tags = new TagList
        {
            { "http.request.method", method },
            { "url.path", path },
            { "wms.request.outcome", outcome },
            { "http.response.status_code", statusCode }
        };

        RequestCompletedCounter.Add(1, tags);
        RequestDurationMs.Record(durationMs, tags);
    }

    public static void RecordAuditWrite(string result, string source, string entityType)
    {
        AuditWriteCounter.Add(1, new TagList
        {
            { "wms.audit.result", result },
            { "wms.audit.source", source },
            { "wms.audit.entity_type", entityType }
        });
    }

    public static void RecordAuditWriteFailure(string source, string entityType)
    {
        AuditWriteFailureCounter.Add(1, new TagList
        {
            { "wms.audit.source", source },
            { "wms.audit.entity_type", entityType }
        });
    }

    public static void RecordJobExecution(string status, string source)
    {
        JobExecutionCounter.Add(1, new TagList
        {
            { "wms.job.status", status },
            { "wms.job.source", source }
        });
    }

    public static void RecordIntegrationExecution(string targetSystem, string operation, string status)
    {
        IntegrationExecutionCounter.Add(1, new TagList
        {
            { "wms.integration.target_system", targetSystem },
            { "wms.integration.operation", operation },
            { "wms.integration.status", status }
        });
    }
}
