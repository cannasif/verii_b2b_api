using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PaymentProviderOperation : BaseEntity
{
    public long PaymentTransactionId { get; set; }
    public long? PaymentOrderId { get; set; }
    public long? PaymentInstallmentId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? ExternalOperationId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Reason { get; set; }
    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public PaymentTransaction? PaymentTransaction { get; set; }
    public PaymentOrder? PaymentOrder { get; set; }
    public PaymentInstallment? PaymentInstallment { get; set; }
}
