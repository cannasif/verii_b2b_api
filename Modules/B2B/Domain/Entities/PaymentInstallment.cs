using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PaymentInstallment : BaseEntity
{
    public long PaymentOrderId { get; set; }
    public int InstallmentNumber { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }

    public PaymentOrder? PaymentOrder { get; set; }
}
