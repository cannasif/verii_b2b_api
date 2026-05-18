using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wms.Application.Common;
using Wms.Application.System.Dtos;
using Wms.Application.System.Services;
using Wms.Domain.Entities.Common;
using Wms.Infrastructure.Options;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.WebApi.Controllers;

[ApiController]
[Route("api/hangfire")]
[Authorize]
public sealed class HangfireController : ControllerBase
{
    private readonly IJobService _jobs;
    private readonly IHangfireManualSyncService _manualSync;
    private readonly HangfireMonitoringOptions _options;
    private readonly WmsDbContext _db;
    private readonly ILocalizationService _localization;

    public HangfireController(
        IJobService jobs,
        IHangfireManualSyncService manualSync,
        IOptions<HangfireMonitoringOptions> options,
        WmsDbContext db,
        ILocalizationService localization)
    {
        _jobs = jobs;
        _manualSync = manualSync;
        _options = options.Value;
        _db = db;
        _localization = localization;
    }

    [HttpGet("recurring-jobs")]
    public IActionResult GetRecurringJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs()
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                JobName = x.Job?.Type?.Name ?? x.Id,
                Method = x.Job?.Method?.Name,
                x.Cron,
                x.Queue,
                NextExecution = x.NextExecution?.ToString("o"),
                LastExecution = x.LastExecution?.ToString("o"),
                x.LastJobId,
                x.Error,
            })
            .ToList();

        return Ok(new { Items = jobs, Total = jobs.Count, Timestamp = DateTime.UtcNow });
    }

    [HttpPost("recurring-jobs/{jobId}/trigger")]
    public IActionResult TriggerRecurringJob([FromRoute] string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return BadRequest(new { Message = _localization.GetLocalizedString("HangfireJobIdRequired") });
        }

        using var connection = JobStorage.Current.GetConnection();
        var exists = connection.GetRecurringJobs().Any(x => string.Equals(x.Id, jobId, StringComparison.OrdinalIgnoreCase));
        if (!exists)
        {
            return NotFound(new { Message = _localization.GetLocalizedString("HangfireRecurringJobNotFound") });
        }

        RecurringJob.TriggerJob(jobId);
        return Ok(new { JobId = jobId, TriggeredAt = DateTime.UtcNow, Message = _localization.GetLocalizedString("HangfireRecurringJobTriggeredSuccessfully") });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var manualSyncJobs = await _manualSync.GetJobStatusesAsync(cancellationToken);

        return Ok(new
        {
            Enqueued = monitoringApi.EnqueuedCount("default"),
            Processing = monitoringApi.ProcessingCount(),
            Scheduled = monitoringApi.ScheduledCount(),
            Succeeded = monitoringApi.SucceededListCount(),
            Failed = await _db.JobFailureLogs.AsNoTracking().CountAsync(cancellationToken),
            Deleted = monitoringApi.DeletedListCount(),
            Servers = monitoringApi.Servers().Count,
            Queues = monitoringApi.Queues().Count,
            Timestamp = DateTime.UtcNow,
            Monitoring = new
            {
                _options.EnableFailureSqlLog,
                _options.FinalRetryCountThreshold,
                _options.CriticalJobs,
                _options.AlertEmails,
            },
            ManualSyncJobs = manualSyncJobs.Data ?? Array.Empty<ManualSyncJobStatusDto>(),
        });
    }

    [HttpGet("failed")]
    public async Task<IActionResult> GetFailed([FromQuery] int from = 0, [FromQuery] int count = 20, CancellationToken cancellationToken = default)
    {
        return await GetFailuresFromDb(from, count, cancellationToken);
    }

    [HttpGet("failures-from-db")]
    public async Task<IActionResult> GetFailuresFromDb([FromQuery] int from = 0, [FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        if (from < 0) from = 0;
        if (count <= 0) count = 50;
        if (count > 200) count = 200;

        var total = await _db.JobFailureLogs.AsNoTracking().CountAsync(cancellationToken);
        var items = await _db.JobFailureLogs.AsNoTracking()
            .OrderByDescending(x => x.FailedAt)
            .Skip(from)
            .Take(count)
            .Select(x => new
            {
                x.JobId,
                x.JobName,
                FailedAt = x.FailedAt.ToString("o"),
                State = "Failed",
                Reason = x.ExceptionMessage ?? x.Reason,
                x.ExceptionType,
                x.RetryCount,
                x.Queue,
            })
            .ToListAsync(cancellationToken);

        return Ok(new { Items = items, Total = total, Timestamp = DateTime.UtcNow });
    }

    [HttpPost("failed/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<object>>>> GetFailedPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _jobs.GetFailedPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("successes-from-db")]
    public async Task<IActionResult> GetSuccessesFromDb([FromQuery] int from = 0, [FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        if (from < 0) from = 0;
        if (count <= 0) count = 50;
        if (count > 200) count = 200;

        var query = _db.JobExecutionLogs.AsNoTracking().Where(x => x.Status == "Succeeded");
        var items = await query
            .OrderByDescending(x => x.FinishedAt)
            .Skip(from)
            .Take(count)
            .Select(x => new
            {
                x.JobId,
                x.RecurringJobId,
                x.JobName,
                FinishedAt = x.FinishedAt.ToString("o"),
                x.DurationMs,
                x.Queue,
                x.RetryCount,
            })
            .ToListAsync(cancellationToken);

        return Ok(new { Items = items, Total = await query.CountAsync(cancellationToken), Timestamp = DateTime.UtcNow });
    }

    [HttpGet("dead-letter")]
    public async Task<IActionResult> GetDeadLetter([FromQuery] int from = 0, [FromQuery] int count = 20, CancellationToken cancellationToken = default)
    {
        if (from < 0) from = 0;
        if (count <= 0) count = 20;
        if (count > 200) count = 200;

        var query = _db.JobFailureLogs.AsNoTracking().Where(x => x.Queue == "dead-letter");
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.FailedAt)
            .Skip(from)
            .Take(count)
            .Select(x => new
            {
                x.JobId,
                x.JobName,
                EnqueuedAt = x.FailedAt.ToString("o"),
                State = "Enqueued",
                Reason = x.ExceptionMessage ?? x.Reason,
            })
            .ToListAsync(cancellationToken);

        return Ok(new { Queue = "dead-letter", Enqueued = total, Items = items, Timestamp = DateTime.UtcNow, Threshold = _options.FinalRetryCountThreshold });
    }

    [HttpPost("dead-letter/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<object>>>> GetDeadLetterPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _jobs.GetDeadLetterPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("failure-logs")]
    public async Task<ActionResult<ApiResponse<PagedResponse<JobFailureLog>>>> GetFailureLogs([FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _jobs.GetFailureLogsAsync(pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("manual-sync/jobs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ManualSyncJobStatusDto>>>> GetManualSyncJobs(CancellationToken cancellationToken = default)
    {
        var result = await _manualSync.GetJobStatusesAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("manual-sync/run")]
    public async Task<ActionResult<ApiResponse<TriggerManualSyncJobResponseDto>>> RunManualSync([FromBody] TriggerManualSyncJobRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await _manualSync.TriggerAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
