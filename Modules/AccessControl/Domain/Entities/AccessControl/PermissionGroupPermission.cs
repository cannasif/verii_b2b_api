using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.AccessControl;

public sealed class PermissionGroupPermission : BaseEntity
{
    public long PermissionGroupId { get; set; }
    public PermissionGroup PermissionGroup { get; set; } = null!;

    public long PermissionDefinitionId { get; set; }
    public PermissionDefinition PermissionDefinition { get; set; } = null!;
}
