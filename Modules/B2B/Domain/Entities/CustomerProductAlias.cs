using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CustomerProductAlias : BaseEntity
{
    public long CustomerId { get; set; }
    public long? ErpStockId { get; set; }
    public long? CatalogProductId { get; set; }
    public string CustomerSku { get; set; } = string.Empty;
    public string? CustomerProductName { get; set; }
    public string MatchStatus { get; set; } = "Pending";
    public decimal? ConfidenceScore { get; set; }
    public string? Notes { get; set; }
    public DateTime? MatchedDate { get; set; }
}
