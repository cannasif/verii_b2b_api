using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.System.Dtos;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.Application.System.Services;

public sealed class TraceExplorerService : ITraceExplorerService
{
    private readonly WmsDbContext _db;
    private readonly ILocalizationService _localization;

    public TraceExplorerService(WmsDbContext db, ILocalizationService localization)
    {
        _db = db;
        _localization = localization;
    }

    public async Task<ApiResponse<TraceExplorerResponseDto>> GetByTraceIdAsync(string traceId, CancellationToken cancellationToken = default)
    {
        var normalizedTraceId = traceId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTraceId))
        {
            return ApiResponse<TraceExplorerResponseDto>.ErrorResult(_localization.GetLocalizedString("SystemTraceExplorerTraceIdRequired"), statusCode: 400);
        }

        var auditLogsTask = _db.WmsAuditLogs.AsNoTracking()
            .Where(x => x.TraceId == normalizedTraceId)
            .OrderBy(x => x.CreatedDate)
            .Select(x => new TraceExplorerAuditItemDto
            {
                Id = x.Id,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                ActionType = x.ActionType,
                PerformedByUserId = x.PerformedByUserId,
                PerformedByUserEmail = x.PerformedByUserEmail,
                BranchCode = x.BranchCode,
                Result = x.Result,
                Source = x.Source,
                ChangedFields = x.ChangedFields,
                Reason = x.Reason,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                CreatedDate = x.CreatedDate,
            })
            .ToListAsync(cancellationToken);

        var jobExecutionsTask = _db.JobExecutionLogs.AsNoTracking()
            .Where(x => x.TraceId == normalizedTraceId)
            .OrderBy(x => x.StartedAt)
            .Select(x => new TraceExplorerJobExecutionItemDto
            {
                Id = x.Id,
                JobId = x.JobId,
                RecurringJobId = x.RecurringJobId,
                JobName = x.JobName,
                Status = x.Status,
                Queue = x.Queue,
                StartedAt = x.StartedAt,
                FinishedAt = x.FinishedAt,
                DurationMs = x.DurationMs,
                Reason = x.Reason,
                ExceptionType = x.ExceptionType,
                ExceptionMessage = x.ExceptionMessage,
                RetryCount = x.RetryCount,
                BranchCode = x.BranchCode,
            })
            .ToListAsync(cancellationToken);

        var jobFailuresTask = _db.JobFailureLogs.AsNoTracking()
            .Where(x => x.TraceId == normalizedTraceId)
            .OrderBy(x => x.FailedAt)
            .Select(x => new TraceExplorerJobFailureItemDto
            {
                Id = x.Id,
                JobId = x.JobId,
                JobName = x.JobName,
                FailedAt = x.FailedAt,
                Reason = x.Reason,
                ExceptionType = x.ExceptionType,
                ExceptionMessage = x.ExceptionMessage,
                Queue = x.Queue,
                RetryCount = x.RetryCount,
                BranchCode = x.BranchCode,
            })
            .ToListAsync(cancellationToken);

        var notificationsTask = _db.Notifications.AsNoTracking()
            .Where(x => x.TraceId == normalizedTraceId)
            .OrderBy(x => x.CreatedDate)
            .Select(x => new TraceExplorerNotificationItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                Channel = x.Channel.ToString(),
                Severity = x.Severity != null ? x.Severity.ToString() : null,
                IsRead = x.IsRead,
                ScheduledAt = x.ScheduledAt,
                DeliveredAt = x.DeliveredAt,
                RecipientUserId = x.RecipientUserId,
                RelatedEntityType = x.RelatedEntityType,
                RelatedEntityId = x.RelatedEntityId,
                ActionUrl = x.ActionUrl,
                BranchCode = x.BranchCode,
                CreatedDate = x.CreatedDate,
            })
            .ToListAsync(cancellationToken);

        var integrationsTask = _db.WmsIntegrationLogs.AsNoTracking()
            .Where(x => x.TraceId == normalizedTraceId)
            .OrderBy(x => x.StartedAt)
            .Select(x => new TraceExplorerIntegrationItemDto
            {
                Id = x.Id,
                IntegrationType = x.IntegrationType,
                TargetSystem = x.TargetSystem,
                Operation = x.Operation,
                Status = x.Status,
                Source = x.Source,
                RequestMetadata = x.RequestMetadata,
                ResponseMetadata = x.ResponseMetadata,
                ErrorMessage = x.ErrorMessage,
                ErrorType = x.ErrorType,
                StartedAt = x.StartedAt,
                FinishedAt = x.FinishedAt,
                DurationMs = x.DurationMs,
                BranchCode = x.BranchCode,
                CreatedDate = x.CreatedDate,
            })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(auditLogsTask, jobExecutionsTask, jobFailuresTask, notificationsTask, integrationsTask);

        var response = new TraceExplorerResponseDto
        {
            TraceId = normalizedTraceId,
            AuditLogs = auditLogsTask.Result,
            JobExecutions = jobExecutionsTask.Result,
            JobFailures = jobFailuresTask.Result,
            Notifications = notificationsTask.Result,
            Integrations = integrationsTask.Result,
        };

        var timestamps = new List<DateTime>();
        timestamps.AddRange(response.AuditLogs.Where(x => x.CreatedDate.HasValue).Select(x => x.CreatedDate!.Value));
        timestamps.AddRange(response.JobExecutions.Select(x => x.StartedAt));
        timestamps.AddRange(response.JobFailures.Select(x => x.FailedAt));
        timestamps.AddRange(response.Notifications.Where(x => x.CreatedDate.HasValue).Select(x => x.CreatedDate!.Value));
        timestamps.AddRange(response.Integrations.Where(x => x.CreatedDate.HasValue).Select(x => x.CreatedDate!.Value));

        response.Summary = new TraceExplorerSummaryDto
        {
            AuditCount = response.AuditLogs.Count,
            JobExecutionCount = response.JobExecutions.Count,
            JobFailureCount = response.JobFailures.Count,
            NotificationCount = response.Notifications.Count,
            IntegrationCount = response.Integrations.Count,
            FirstSeenAt = timestamps.Count > 0 ? timestamps.Min() : null,
            LastSeenAt = timestamps.Count > 0 ? timestamps.Max() : null,
        };

        if (response.Summary.AuditCount == 0
            && response.Summary.JobExecutionCount == 0
            && response.Summary.JobFailureCount == 0
            && response.Summary.NotificationCount == 0
            && response.Summary.IntegrationCount == 0)
        {
            return ApiResponse<TraceExplorerResponseDto>.ErrorResult(_localization.GetLocalizedString("SystemTraceExplorerNotFound"), statusCode: 404);
        }

        return ApiResponse<TraceExplorerResponseDto>.SuccessResult(response, _localization.GetLocalizedString("SystemTraceExplorerRetrieved"));
    }
}
