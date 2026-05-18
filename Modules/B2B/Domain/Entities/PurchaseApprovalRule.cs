using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class PurchaseApprovalRule : BaseEntity
{
    public long CompanyId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string ApproverRoleCode { get; set; } = "Approver";
    public bool IsActive { get; set; } = true;
}
