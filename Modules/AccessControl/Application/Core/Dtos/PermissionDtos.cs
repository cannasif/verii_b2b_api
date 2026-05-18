using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Dtos;

public sealed class PermissionDefinitionDto : BaseEntityDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool AvailableOnWeb { get; set; }
    public bool AvailableOnMobile { get; set; }
}

public sealed class CreatePermissionDefinitionDto
{
    [Required]
    [StringLength(120)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public bool AvailableOnWeb { get; set; } = true;
    public bool AvailableOnMobile { get; set; }
}

public sealed class UpdatePermissionDefinitionDto
{
    [StringLength(120)]
    public string? Code { get; set; }

    [StringLength(150)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
    public bool? AvailableOnWeb { get; set; }
    public bool? AvailableOnMobile { get; set; }
}

public sealed class SyncPermissionDefinitionsDto
{
    [Required]
    public List<SyncPermissionDefinitionItemDto> Items { get; set; } = new();
    public bool ReactivateSoftDeleted { get; set; } = true;
    public bool UpdateExistingNames { get; set; }
    public bool UpdateExistingDescriptions { get; set; }
    public bool UpdateExistingIsActive { get; set; }
}

public sealed class SyncPermissionDefinitionItemDto
{
    [Required]
    [StringLength(120)]
    public string Code { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public bool AvailableOnWeb { get; set; } = true;
    public bool AvailableOnMobile { get; set; }
}

public sealed class PermissionDefinitionSyncResultDto
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ReactivatedCount { get; set; }
    public int TotalProcessed { get; set; }
}

public sealed class PermissionGroupDto : BaseEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsActive { get; set; }
    public List<long> PermissionDefinitionIds { get; set; } = new();
    public List<string> PermissionCodes { get; set; } = new();
}

public sealed class CreatePermissionGroupDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsSystemAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public List<long> PermissionDefinitionIds { get; set; } = new();
}

public sealed class UpdatePermissionGroupDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsSystemAdmin { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SetPermissionGroupPermissionsDto
{
    [Required]
    public List<long> PermissionDefinitionIds { get; set; } = new();
}

public sealed class UserPermissionGroupDto
{
    public long UserId { get; set; }
    public List<long> PermissionGroupIds { get; set; } = new();
    public List<string> PermissionGroupNames { get; set; } = new();
}

public sealed class SetUserPermissionGroupsDto
{
    [Required]
    public List<long> PermissionGroupIds { get; set; } = new();
}

public sealed class MyPermissionsDto
{
    public long UserId { get; set; }
    public string RoleTitle { get; set; } = string.Empty;
    public bool IsSystemAdmin { get; set; }
    public string? Platform { get; set; }
    public List<string> PermissionGroups { get; set; } = new();
    public List<string> PermissionCodes { get; set; } = new();
}
