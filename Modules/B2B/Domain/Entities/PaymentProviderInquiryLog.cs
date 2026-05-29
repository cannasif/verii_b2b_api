using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PaymentProviderInquiryLog : BaseEntity
{
    public string ProviderKey { get; set; } = string.Empty;
    public string InquiryType { get; set; } = string.Empty;
    public string? BinNumber { get; set; }
    public decimal? Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string Status { get; set; } = "Pending";
    public string? ConversationId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public DateTime RequestedDate { get; set; }
}
