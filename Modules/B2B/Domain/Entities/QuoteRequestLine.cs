using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class QuoteRequestLine : BaseEntity
{
    public long QuoteRequestId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string? RequestedSku { get; set; }
    public string? RequestedName { get; set; }
    public decimal Quantity { get; set; }
    public decimal? TargetUnitPrice { get; set; }
    public decimal? ApprovedUnitPrice { get; set; }

    public QuoteRequest? QuoteRequest { get; set; }
}
