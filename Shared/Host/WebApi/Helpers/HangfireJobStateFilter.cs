using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Options;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.Common;
using Wms.Infrastructure.Options;
using Wms.Infrastructure.Persistence.Context;
using Wms.WebApi.Telemetry;

namespace Wms.WebApi.Helpers;

public sealed class HangfireJobStateFilter : IApplyStateFilter
{
    private readonly ILogger<HangfireJobStateFilter> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IJobTraceContextAccessor _jobTraceContextAccessor;
    private readonly HangfireMonitoringOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public HangfireJobStateFilter(
        ILogger<HangfireJobStateFilter> logger,
        IBackgroundJobClient backgroundJobClient,
        IJobTraceContextAccessor jobTraceContextAccessor,
        IOptions<HangfireMonitoringOptions> options,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
        _jobTraceContextAccessor = jobTraceContextAccessor;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var jobId = context.BackgroundJob?.Id ?? "unknown";
        var job = context.BackgroundJob?.Job;
        var jobName = job == null ? "unknown" : $"{job.Type.FullName}.{job.Method.Name}";
        var queue = context.GetJobParameter<string>("Queue");
        var recurringJobId = context.GetJobParameter<string>("RecurringJobId");
        var traceId = context.GetJobParameter<string>("WmsTraceId") ?? _jobTraceContextAccessor.Current?.TraceId;
        var createdAt = context.BackgroundJob?.CreatedAt ?? DateTime.UtcNow;

        if (context.NewState is FailedState failedState)
        {
            var retryCount = context.GetJobParameter<int>("RetryCount");
            _logger.LogError(failedState.Exception, "Hangfire job failed. JobId={JobId} Job={JobName}", jobId, jobName);

            if (_options.EnableFailureSqlLog)
            {
                TryPersistFailure(jobId, jobName, queue, retryCount, recurringJobId, traceId, createdAt, failedState);
            }

            if (IsCriticalJob(jobName) && retryCount >= _options.FinalRetryCountThreshold)
            {
                var payload = new HangfireDeadLetterPayload
                {
                    TraceId = traceId,
                    JobId = jobId,
                    JobName = jobName,
                    Queue = "dead-letter",
                    RetryCount = retryCount,
                    Reason = failedState.Reason,
                    ExceptionType = failedState.Exception == null ? null : failedState.Exception.GetType().FullName,
                    ExceptionMessage = failedState.Exception == null ? null : failedState.Exception.Message,
                    OccurredAtUtc = DateTime.UtcNow
                };

                _backgroundJobClient.Create<IHangfireDeadLetterJob>(
                    x => x.ProcessAsync(payload),
                    new EnqueuedState("dead-letter"));
            }

            WmsTelemetry.RecordJobExecution("Failed", string.IsNullOrWhiteSpace(recurringJobId) ? "Hangfire" : "RecurringJob");
        }
        else if (context.NewState is SucceededState succeededState)
        {
            TryPersistSuccess(jobId, jobName, queue, recurringJobId, traceId, createdAt, succeededState, context.GetJobParameter<int>("RetryCount"));
            WmsTelemetry.RecordJobExecution("Succeeded", string.IsNullOrWhiteSpace(recurringJobId) ? "Hangfire" : "RecurringJob");
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    private void TryPersistFailure(string jobId, string jobName, string? queue, int retryCount, string? recurringJobId, string? traceId, DateTime createdAt, FailedState failedState)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WmsDbContext>();
            db.JobFailureLogs.Add(new JobFailureLog
            {
                TraceId = traceId,
                JobId = jobId,
                JobName = jobName,
                FailedAt = DateTime.UtcNow,
                Reason = failedState.Reason,
                ExceptionType = failedState.Exception?.GetType().FullName,
                ExceptionMessage = failedState.Exception?.Message,
                StackTrace = Truncate(failedState.Exception?.StackTrace, 4000),
                Queue = queue,
                RetryCount = retryCount,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            });
            db.JobExecutionLogs.Add(new JobExecutionLog
            {
                TraceId = traceId,
                JobId = jobId,
                RecurringJobId = recurringJobId,
                JobName = jobName,
                Status = "Failed",
                Queue = queue,
                StartedAt = createdAt,
                FinishedAt = DateTime.UtcNow,
                DurationMs = Math.Max(0, (int)(DateTime.UtcNow - createdAt).TotalMilliseconds),
                Reason = failedState.Reason,
                ExceptionType = failedState.Exception?.GetType().FullName,
                ExceptionMessage = failedState.Exception?.Message,
                RetryCount = retryCount,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            });
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JobFailureLog write failed. JobId={JobId}", jobId);
        }
    }

    private void TryPersistSuccess(string jobId, string jobName, string? queue, string? recurringJobId, string? traceId, DateTime createdAt, SucceededState succeededState, int retryCount)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WmsDbContext>();
            db.JobExecutionLogs.Add(new JobExecutionLog
            {
                TraceId = traceId,
                JobId = jobId,
                RecurringJobId = recurringJobId,
                JobName = jobName,
                Status = "Succeeded",
                Queue = queue,
                StartedAt = createdAt,
                FinishedAt = DateTime.UtcNow,
                DurationMs = (int)Math.Max(0L, succeededState.Latency + succeededState.PerformanceDuration),
                RetryCount = retryCount,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            });
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JobExecutionLog write failed. JobId={JobId}", jobId);
        }
    }

    private bool IsCriticalJob(string jobName)
        => _options.CriticalJobs.Any(pattern => !string.IsNullOrWhiteSpace(pattern) && jobName.Contains(pattern, StringComparison.OrdinalIgnoreCase));

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
}
