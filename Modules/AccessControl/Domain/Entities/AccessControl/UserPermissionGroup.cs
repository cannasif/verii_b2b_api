using Wms.Domain.Entities.Common;
using Wms.Domain.Entities.Identity;

namespace Wms.Domain.Entities.AccessControl;

public sealed class UserPermissionGroup : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long PermissionGroupId { get; set; }
    public PermissionGroup PermissionGroup { get; set; } = null!;
}
