namespace Wms.Application.System.Dtos;

public sealed class TraceExplorerResponseDto
{
    public string TraceId { get; set; } = string.Empty;
    public TraceExplorerSummaryDto Summary { get; set; } = new();
    public List<TraceExplorerAuditItemDto> AuditLogs { get; set; } = new();
    public List<TraceExplorerJobExecutionItemDto> JobExecutions { get; set; } = new();
    public List<TraceExplorerJobFailureItemDto> JobFailures { get; set; } = new();
    public List<TraceExplorerNotificationItemDto> Notifications { get; set; } = new();
    public List<TraceExplorerIntegrationItemDto> Integrations { get; set; } = new();
}

public sealed class TraceExplorerSummaryDto
{
    public int AuditCount { get; set; }
    public int JobExecutionCount { get; set; }
    public int JobFailureCount { get; set; }
    public int NotificationCount { get; set; }
    public int IntegrationCount { get; set; }
    public DateTime? FirstSeenAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public sealed class TraceExplorerAuditItemDto
{
    public long Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public long? PerformedByUserId { get; set; }
    public string? PerformedByUserEmail { get; set; }
    public string BranchCode { get; set; } = "0";
    public string Result { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? ChangedFields { get; set; }
    public string? Reason { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public sealed class TraceExplorerJobExecutionItemDto
{
    public long Id { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string? RecurringJobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Queue { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public int DurationMs { get; set; }
    public string? Reason { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public int RetryCount { get; set; }
    public string BranchCode { get; set; } = "0";
}

public sealed class TraceExplorerJobFailureItemDto
{
    public long Id { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
    public string? Reason { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? Queue { get; set; }
    public int RetryCount { get; set; }
    public string BranchCode { get; set; } = "0";
}

public sealed class TraceExplorerNotificationItemDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public long? RecipientUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string BranchCode { get; set; } = "0";
    public DateTime? CreatedDate { get; set; }
}

public sealed class TraceExplorerIntegrationItemDto
{
    public long Id { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RequestMetadata { get; set; }
    public string? ResponseMetadata { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public int DurationMs { get; set; }
    public string BranchCode { get; set; } = "0";
    public DateTime? CreatedDate { get; set; }
}
