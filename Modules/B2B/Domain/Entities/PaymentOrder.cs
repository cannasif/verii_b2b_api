using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PaymentOrder : BaseEntity
{
    public string PaymentOrderNumber { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public short? PaymentTermDays { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsDueDateOverridden { get; set; }
    public int InstallmentCount { get; set; } = 1;
    public string? PaymentMethod { get; set; }
    public string? ProviderKey { get; set; }
    public string? ProviderConversationId { get; set; }
    public string? BinNumber { get; set; }
    public string? CardType { get; set; }
    public string? CardAssociation { get; set; }
    public string? CardFamily { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public bool? IsCommercialCard { get; set; }
    public int? ProviderInstallmentNumber { get; set; }
    public decimal? ProviderInstallmentPrice { get; set; }
    public decimal? ProviderTotalPrice { get; set; }
    public decimal? ProviderRate { get; set; }
    public decimal? ProviderCommissionAmount { get; set; }
    public string? ProviderInstallmentSnapshotJson { get; set; }
    public string? Notes { get; set; }

    public B2bOrder? Order { get; set; }
    public List<PaymentInstallment> Installments { get; set; } = new();
    public List<PaymentTransaction> Transactions { get; set; } = new();
}
