using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Domain.Entities.Common;

namespace Wms.Application.System.Services;

public sealed class JobService : IJobService
{
    private readonly IRepository<JobFailureLog> _jobFailureLogs;
    private readonly ILocalizationService _localization;

    public JobService(IRepository<JobFailureLog> jobFailureLogs, ILocalizationService localization)
    {
        _jobFailureLogs = jobFailureLogs;
        _localization = localization;
    }

    public async Task<ApiResponse<PagedResponse<object>>> GetFailedPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var pageNumber = request.PageNumber < 0 ? 0 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _jobFailureLogs.Query()
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(JobFailureLog.FailedAt), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select(x => new
        {
            jobId = x.JobId,
            jobName = x.JobName,
            failedAt = x.FailedAt,
            state = "Failed",
            reason = BuildUserFriendlyReason(x.ExceptionMessage ?? x.Reason, x.JobName),
            technicalReason = x.ExceptionMessage ?? x.Reason,
            exceptionType = x.ExceptionType,
            retryCount = x.RetryCount,
            queue = x.Queue
        }).Cast<object>().ToList();

        return ApiResponse<PagedResponse<object>>.SuccessResult(
            new PagedResponse<object>(items, total, pageNumber, pageSize),
            _localization.GetLocalizedString("SystemHangfireFailedJobsRetrieved"));
    }

    public async Task<ApiResponse<PagedResponse<object>>> GetDeadLetterPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var pageNumber = request.PageNumber < 0 ? 0 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _jobFailureLogs.Query()
            .Where(x => x.Queue == "dead-letter")
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(JobFailureLog.FailedAt), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select(x => new
        {
            jobId = x.JobId,
            jobName = x.JobName,
            enqueuedAt = x.FailedAt,
            state = "Enqueued",
            reason = BuildUserFriendlyReason(x.ExceptionMessage ?? x.Reason, x.JobName),
            technicalReason = x.ExceptionMessage ?? x.Reason,
            retryCount = x.RetryCount,
            queue = x.Queue
        }).Cast<object>().ToList();

        return ApiResponse<PagedResponse<object>>.SuccessResult(
            new PagedResponse<object>(items, total, pageNumber, pageSize),
            _localization.GetLocalizedString("SystemHangfireDeadLetterJobsRetrieved"));
    }

    public async Task<ApiResponse<PagedResponse<JobFailureLog>>> GetFailureLogsAsync(int pageNumber = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 0) pageNumber = 0;
        if (pageSize <= 0) pageSize = 20;

        var query = _jobFailureLogs.Query().OrderByDescending(x => x.FailedAt);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return ApiResponse<PagedResponse<JobFailureLog>>.SuccessResult(
            new PagedResponse<JobFailureLog>(items, total, pageNumber, pageSize),
            _localization.GetLocalizedString("SystemHangfireFailureLogsRetrieved"));
    }

    private string BuildUserFriendlyReason(string? technicalReason, string? jobName)
    {
        if (string.IsNullOrWhiteSpace(technicalReason))
        {
            return _localization.GetLocalizedString("JobFailureUserReasonNoTechnicalDetail");
        }

        var message = technicalReason.Trim();
        if (message.Contains("ErpConnection is not configured", StringComparison.OrdinalIgnoreCase))
        {
            return _localization.GetLocalizedString("JobFailureUserReasonErpConnectionNotConfigured");
        }

        if (message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase)
            || message.Contains("could not find stored procedure", StringComparison.OrdinalIgnoreCase))
        {
            return _localization.GetLocalizedString("JobFailureUserReasonErpMissingDbObject");
        }

        if (message.Contains("login failed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cannot open database", StringComparison.OrdinalIgnoreCase))
        {
            return _localization.GetLocalizedString("JobFailureUserReasonErpAuthentication");
        }

        if (message.Contains("network-related", StringComparison.OrdinalIgnoreCase)
            || message.Contains("server was not found", StringComparison.OrdinalIgnoreCase)
            || message.Contains("connection", StringComparison.OrdinalIgnoreCase) && message.Contains("open", StringComparison.OrdinalIgnoreCase))
        {
            return _localization.GetLocalizedString("JobFailureUserReasonErpUnreachable");
        }

        if (jobName?.Contains("SyncJob", StringComparison.OrdinalIgnoreCase) == true)
        {
            return _localization.GetLocalizedString("JobFailureUserReasonSyncIncomplete", message);
        }

        return message;
    }
}
