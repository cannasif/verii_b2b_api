using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class QuoteRequest : BaseEntity
{
    public string QuoteNumber { get; set; } = string.Empty;
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
    public string? ErpProjectCode { get; set; }
    public decimal? GeneralDiscountRate { get; set; }
    public decimal? GeneralDiscountAmount { get; set; }
    public decimal Total { get; set; }
    public decimal EstimatedTotal { get; set; }
    public string? CustomerNote { get; set; }
    public string? SalesNote { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public List<QuoteRequestLine> Lines { get; set; } = new();
}
