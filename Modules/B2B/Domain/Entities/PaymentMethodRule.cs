using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PaymentMethodRule : BaseEntity
{
    public long? CompanyId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string RuleType { get; set; } = "Include";
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
}
