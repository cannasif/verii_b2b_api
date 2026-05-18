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
    public decimal DiscountRate1 { get; set; }
    public decimal DiscountAmount1 { get; set; }
    public decimal DiscountRate2 { get; set; }
    public decimal DiscountAmount2 { get; set; }
    public decimal DiscountRate3 { get; set; }
    public decimal DiscountAmount3 { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal LineGrandTotal { get; set; }
    public string? Description { get; set; }
    public string? Description1 { get; set; }
    public string? Description2 { get; set; }
    public string? Description3 { get; set; }
    public long? PricingRuleHeaderId { get; set; }
    public string? RelatedProductKey { get; set; }
    public bool IsMainRelatedProduct { get; set; }
    public string? ErpProjectCode { get; set; }

    public QuoteRequest? QuoteRequest { get; set; }
}
