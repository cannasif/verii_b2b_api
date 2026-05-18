using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bBuyer : BaseEntity
{
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = "Buyer";
    public decimal? OrderLimit { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; } = true;

    public B2bCompany? Company { get; set; }
}
