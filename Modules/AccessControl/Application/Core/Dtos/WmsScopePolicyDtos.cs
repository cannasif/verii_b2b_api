using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Dtos;

public sealed class WmsScopePolicyDto : BaseEntityDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public bool IncludeSelf { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateWmsScopePolicyDto
{
    [Required]
    [StringLength(120)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string EntityType { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string ScopeType { get; set; } = string.Empty;

    public bool IncludeSelf { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateWmsScopePolicyDto
{
    [StringLength(120)]
    public string? Code { get; set; }

    [StringLength(150)]
    public string? Name { get; set; }

    [StringLength(120)]
    public string? EntityType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? ScopeType { get; set; }

    public bool? IncludeSelf { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class UserWmsScopePolicyAssignmentDto : BaseEntityDto
{
    public long UserId { get; set; }
    public long WmsScopePolicyId { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty;
    public long? WarehouseId { get; set; }
}

public sealed class SetUserWmsScopePoliciesDto
{
    [Required]
    public List<UserWmsScopePolicyAssignmentInputDto> Items { get; set; } = new();
}

public sealed class UserWmsScopePolicyAssignmentInputDto
{
    [Required]
    public long WmsScopePolicyId { get; set; }

    [StringLength(20)]
    public string? BranchCode { get; set; }

    public long? WarehouseId { get; set; }
}

public sealed class WmsScopePolicyResolutionDto
{
    public long UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public bool HasExplicitPolicy { get; set; }
    public bool IsUnrestricted { get; set; }
    public bool RequiresAssignedRecords { get; set; }
    public bool IncludeSelf { get; set; }
    public List<string> BranchCodes { get; set; } = new();
    public List<long> WarehouseIds { get; set; } = new();
    public List<string> ScopeTypes { get; set; } = new();
}
