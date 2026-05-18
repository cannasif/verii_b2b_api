namespace Wms.Domain.Entities.Common;

public sealed class WmsAuditLog : BaseEntity
{
    public string? TraceId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public long? PerformedByUserId { get; set; }
    public string? PerformedByUserEmail { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedFields { get; set; }
    public string? Reason { get; set; }
}
