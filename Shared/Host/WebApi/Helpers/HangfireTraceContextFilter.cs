using System.Diagnostics;
using Hangfire.Client;
using Hangfire.Server;
using Wms.Application.Common;
using Wms.WebApi.Telemetry;

namespace Wms.WebApi.Helpers;

public sealed class HangfireTraceContextFilter : IClientFilter, IServerFilter
{
    private const string TraceIdParameterName = "WmsTraceId";
    private const string ParentTraceIdParameterName = "WmsParentTraceId";
    private const string ParentActivityIdParameterName = "WmsParentActivityId";
    private const string CorrelationSourceParameterName = "WmsCorrelationSource";

    private readonly ILogger<HangfireTraceContextFilter> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJobTraceContextAccessor _jobTraceContextAccessor;

    public HangfireTraceContextFilter(
        ILogger<HangfireTraceContextFilter> logger,
        IHttpContextAccessor httpContextAccessor,
        IJobTraceContextAccessor jobTraceContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _jobTraceContextAccessor = jobTraceContextAccessor;
    }

    public void OnCreating(CreatingContext filterContext)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var traceId = httpContext?.GetTraceId()
            ?? _jobTraceContextAccessor.Current?.TraceId
            ?? Activity.Current?.GetTagItem("wms.trace_id")?.ToString()
            ?? Activity.Current?.TraceId.ToString();

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            filterContext.SetJobParameter(TraceIdParameterName, traceId);
        }

        var parentTraceId = _jobTraceContextAccessor.Current?.TraceId ?? httpContext?.GetTraceId();
        if (!string.IsNullOrWhiteSpace(parentTraceId))
        {
            filterContext.SetJobParameter(ParentTraceIdParameterName, parentTraceId);
        }

        if (!string.IsNullOrWhiteSpace(Activity.Current?.Id))
        {
            filterContext.SetJobParameter(ParentActivityIdParameterName, Activity.Current!.Id);
        }

        var source = httpContext != null
            ? "HttpRequest"
            : _jobTraceContextAccessor.Current != null
                ? "BackgroundJob"
                : "Manual";
        filterContext.SetJobParameter(CorrelationSourceParameterName, source);
    }

    public void OnCreated(CreatedContext filterContext)
    {
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        var recurringJobId = filterContext.GetJobParameter<string>("RecurringJobId");
        var traceId = filterContext.GetJobParameter<string>(TraceIdParameterName);
        var parentTraceId = filterContext.GetJobParameter<string>(ParentTraceIdParameterName);
        var parentActivityId = filterContext.GetJobParameter<string>(ParentActivityIdParameterName);
        var source = filterContext.GetJobParameter<string>(CorrelationSourceParameterName)
            ?? (!string.IsNullOrWhiteSpace(recurringJobId) ? "RecurringJob" : "Hangfire");

        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = Guid.NewGuid().ToString("N");
        }

        Activity? activity = null;
        if (!string.IsNullOrWhiteSpace(parentActivityId)
            && ActivityContext.TryParse(parentActivityId, null, out var parentContext))
        {
            activity = WmsTelemetry.StartActivity(BuildActivityName(filterContext), ActivityKind.Internal, parentContext);
        }
        else
        {
            activity = WmsTelemetry.StartActivity(BuildActivityName(filterContext), ActivityKind.Internal);
        }

        activity?.SetTag("wms.trace_id", traceId);
        activity?.SetTag("wms.parent_trace_id", parentTraceId);
        activity?.SetTag("wms.job.id", filterContext.BackgroundJob?.Id);
        activity?.SetTag("wms.job.recurring_id", recurringJobId);
        activity?.SetTag("wms.job.source", source);

        _jobTraceContextAccessor.Current = new JobTraceContext
        {
            TraceId = traceId,
            ParentTraceId = parentTraceId,
            Source = source,
            JobId = filterContext.BackgroundJob?.Id,
            RecurringJobId = recurringJobId,
            Activity = activity
        };

        _logger.LogInformation(
            "Hangfire job trace started JobId={JobId} RecurringJobId={RecurringJobId} TraceId={TraceId}",
            filterContext.BackgroundJob?.Id,
            recurringJobId,
            traceId);
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        var context = _jobTraceContextAccessor.Current;
        _logger.LogInformation(
            "Hangfire job trace completed JobId={JobId} RecurringJobId={RecurringJobId} TraceId={TraceId} Failed={Failed}",
            filterContext.BackgroundJob?.Id,
            filterContext.GetJobParameter<string>("RecurringJobId"),
            context?.TraceId,
            filterContext.Exception != null);

        context?.Activity?.Dispose();
        _jobTraceContextAccessor.Current = null;
    }

    private static string BuildActivityName(PerformingContext filterContext)
    {
        var job = filterContext.BackgroundJob?.Job;
        return job == null ? "hangfire.job" : $"hangfire.job.{job.Type.Name}.{job.Method.Name}";
    }
}
