using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class MarketplaceSyncEvent : BaseEntity
{
    public long ChannelId { get; set; }
    public long? ListingId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ExternalBatchId { get; set; }
    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public MarketplaceChannel? Channel { get; set; }
    public MarketplaceListing? Listing { get; set; }
}
