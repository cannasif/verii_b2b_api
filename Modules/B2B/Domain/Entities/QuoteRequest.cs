using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class QuoteRequest : BaseEntity
{
    public string QuoteNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = "Draft";
    public string CurrencyCode { get; set; } = "TRY";
    public decimal EstimatedTotal { get; set; }
    public string? CustomerNote { get; set; }
    public string? SalesNote { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public List<QuoteRequestLine> Lines { get; set; } = new();
}
