using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PaymentTransaction : BaseEntity
{
    public long OrderId { get; set; }
    public long? PaymentOrderId { get; set; }
    public long? PaymentInstallmentId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal Amount { get; set; }
    public decimal? ProviderPaymentAmount { get; set; }
    public decimal? ProviderCollectedAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? PaymentMethod { get; set; }
    public string? CallbackPayloadJson { get; set; }
    public DateTime? DueDate { get; set; }
    public short? PaymentTermDays { get; set; }
    public int InstallmentCount { get; set; } = 1;
    public string? InstallmentPlanJson { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public B2bOrder? Order { get; set; }
    public PaymentOrder? PaymentOrder { get; set; }
    public PaymentInstallment? PaymentInstallment { get; set; }
}
