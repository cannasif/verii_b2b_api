namespace Wms.Application.Common;

public sealed class AuditLogWriteRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public long? PerformedByUserId { get; set; }
    public string? PerformedByUserEmail { get; set; }
    public string? BranchCode { get; set; }
    public string Result { get; set; } = "Succeeded";
    public string Source { get; set; } = string.Empty;
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public IReadOnlyCollection<string>? ChangedFields { get; set; }
    public string? Reason { get; set; }
    public string? TraceId { get; set; }
}
