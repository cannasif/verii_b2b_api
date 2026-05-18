using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.Identity.Dtos;

public sealed class UserAuthorityDto : BaseEntityDto
{
    [Required]
    [StringLength(30)]
    public string Title { get; set; } = string.Empty;
}

public sealed class CreateUserAuthorityDto
{
    [Required]
    [StringLength(30)]
    public string Title { get; set; } = string.Empty;
}

public sealed class UpdateUserAuthorityDto
{
    [Required]
    [StringLength(30)]
    public string Title { get; set; } = string.Empty;
}

public sealed class UserDetailDto : BaseEntityDto
{
    public long UserId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? Description { get; set; }
    public Gender? Gender { get; set; }
}

public sealed class CreateUserDetailDto
{
    [Required]
    public long UserId { get; set; }
    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }
    [Range(0,300)]
    public decimal? Height { get; set; }
    [Range(0,500)]
    public decimal? Weight { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
    public Gender? Gender { get; set; }
}

public sealed class UpdateUserDetailDto
{
    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }
    [Range(0,300)]
    public decimal? Height { get; set; }
    [Range(0,500)]
    public decimal? Weight { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
    public Gender? Gender { get; set; }
}

public sealed class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    [Required]
    public long RoleId { get; set; }
    public bool? IsActive { get; set; }
    public List<long>? PermissionGroupIds { get; set; }
}

public sealed class UpdateUserDto
{
    [EmailAddress]
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public long? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public List<long>? PermissionGroupIds { get; set; }
}
