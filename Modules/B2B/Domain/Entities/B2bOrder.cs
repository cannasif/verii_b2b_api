using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = "Draft";
    public string CurrencyCode { get; set; } = "TRY";
    public string? OfferType { get; set; }
    public DateTime? OfferDate { get; set; }
    public string? OfferNo { get; set; }
    public string? RevisionNo { get; set; }
    public long? RevisionId { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryMethod { get; set; }
    public long? PaymentTypeId { get; set; }
    public long? QuoteRequestId { get; set; }
    public string? ErpProjectCode { get; set; }
    public decimal? GeneralDiscountRate { get; set; }
    public decimal? GeneralDiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Description { get; set; }
    public string? ExternalErpOrderNumber { get; set; }
    public DateTime? SubmittedDate { get; set; }

    public List<B2bOrderLine> Lines { get; set; } = new();
}
