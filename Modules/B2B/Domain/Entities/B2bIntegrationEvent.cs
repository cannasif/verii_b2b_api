using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bIntegrationEvent : BaseEntity
{
    public string Direction { get; set; } = "Outbound";
    public string EventType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ExternalReference { get; set; }
    public string? PayloadJson { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedDate { get; set; }
}
