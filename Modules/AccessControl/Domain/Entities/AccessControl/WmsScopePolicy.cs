using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.AccessControl;

public sealed class WmsScopePolicy : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScopeType { get; set; } = WmsScopePolicyScopeTypes.AssignedOnly;
    public bool IncludeSelf { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<UserWmsScopePolicy> UserAssignments { get; set; } = new List<UserWmsScopePolicy>();
}
