using System.Collections.Concurrent;
using Hangfire;
using Wms.Application.Common;
using Wms.Application.Customer.Services;
using Wms.Application.Stock.Services;
using Wms.Application.System.Dtos;
using Wms.Application.Warehouse.Services;
using Wms.Application.YapKod.Services;

namespace Wms.Application.System.Services;

public sealed class HangfireManualSyncService : IHangfireManualSyncService
{
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<string, DateTime> LastTriggeredAtUtcByJobKey = new(StringComparer.OrdinalIgnoreCase);

    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly TimeProvider _timeProvider;
    private readonly ILocalizationService _localization;
    private readonly IReadOnlyDictionary<string, JobDefinition> _jobs;

    private sealed record JobDefinition(string Key, string DisplayNameResourceKey, Func<string> Enqueue);

    public HangfireManualSyncService(
        IBackgroundJobClient backgroundJobs,
        TimeProvider timeProvider,
        ILocalizationService localization)
    {
        _backgroundJobs = backgroundJobs;
        _timeProvider = timeProvider;
        _localization = localization;
        _jobs = new Dictionary<string, JobDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["customer"] = new("customer", "ManualSyncJobDisplayNameCustomer", () => _backgroundJobs.Enqueue<ICustomerSyncJob>(job => job.RunAsync(CancellationToken.None))),
            ["stock"] = new("stock", "ManualSyncJobDisplayNameStock", () => _backgroundJobs.Enqueue<IStockSyncJob>(job => job.RunAsync(CancellationToken.None))),
            ["warehouse"] = new("warehouse", "ManualSyncJobDisplayNameWarehouse", () => _backgroundJobs.Enqueue<IWarehouseSyncJob>(job => job.RunAsync(CancellationToken.None))),
            ["yapkod"] = new("yapkod", "ManualSyncJobDisplayNameYapKod", () => _backgroundJobs.Enqueue<IYapKodSyncJob>(job => job.RunAsync(CancellationToken.None))),
        };
    }

    public Task<ApiResponse<IReadOnlyList<ManualSyncJobStatusDto>>> GetJobStatusesAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var items = _jobs.Values
            .OrderBy(x => _localization.GetLocalizedString(x.DisplayNameResourceKey), StringComparer.OrdinalIgnoreCase)
            .Select(job => BuildStatus(job, now))
            .ToList()
            .AsReadOnly();

        return Task.FromResult(ApiResponse<IReadOnlyList<ManualSyncJobStatusDto>>.SuccessResult(
            items,
            _localization.GetLocalizedString("ManualSyncJobsRetrievedSuccessfully")));
    }

    public Task<ApiResponse<TriggerManualSyncJobResponseDto>> TriggerAsync(TriggerManualSyncJobRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.JobKey) || !_jobs.TryGetValue(request.JobKey.Trim(), out var job))
        {
            var msg = _localization.GetLocalizedString("ManualSyncUnknownJob");
            return Task.FromResult(ApiResponse<TriggerManualSyncJobResponseDto>.ErrorResult(msg, msg, 400));
        }

        var localizedName = _localization.GetLocalizedString(job.DisplayNameResourceKey);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var status = BuildStatus(job, now);
        if (status.IsCoolingDown)
        {
            return Task.FromResult(ApiResponse<TriggerManualSyncJobResponseDto>.ErrorResult(
                _localization.GetLocalizedString("ManualSyncJobCoolingDown", localizedName),
                _localization.GetLocalizedString("ManualSyncJobCoolingDownDetail", localizedName, status.NextAvailableAtUtc!.Value.ToString("O")),
                429));
        }

        var jobId = job.Enqueue();
        LastTriggeredAtUtcByJobKey[job.Key] = now;

        return Task.FromResult(ApiResponse<TriggerManualSyncJobResponseDto>.SuccessResult(
            new TriggerManualSyncJobResponseDto
            {
                JobKey = job.Key,
                JobName = localizedName,
                JobId = jobId,
                Queue = "default",
                EnqueuedAtUtc = now,
                NextAvailableAtUtc = now.Add(Cooldown),
                CooldownSecondsRemaining = (int)Math.Ceiling(Cooldown.TotalSeconds)
            },
            _localization.GetLocalizedString("ManualSyncJobQueuedSuccessfully", localizedName)));
    }

    private ManualSyncJobStatusDto BuildStatus(JobDefinition job, DateTime now)
    {
        LastTriggeredAtUtcByJobKey.TryGetValue(job.Key, out var lastTriggeredAtUtc);
        DateTime? last = lastTriggeredAtUtc == default ? null : lastTriggeredAtUtc;
        DateTime? next = last?.Add(Cooldown);
        var remaining = next.HasValue ? Math.Max(0, (int)Math.Ceiling((next.Value - now).TotalSeconds)) : 0;

        return new ManualSyncJobStatusDto
        {
            JobKey = job.Key,
            JobName = _localization.GetLocalizedString(job.DisplayNameResourceKey),
            LastTriggeredAtUtc = last,
            NextAvailableAtUtc = next,
            IsCoolingDown = remaining > 0,
            CooldownSecondsRemaining = remaining
        };
    }
}
