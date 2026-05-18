using Wms.Domain.Entities.Common;
using Wms.Domain.Entities.Communications.Enums;
using Wms.Domain.Entities.Identity;

namespace Wms.Domain.Entities.Communications;

public sealed class Notification : BaseEntity
{
    public string? TraceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TitleKey { get; set; }
    public string? MessageKey { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationSeverity? Severity { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long? RecipientUserId { get; set; }
    public User? RecipientUser { get; set; }
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? TerminalActionCode { get; set; }
}
