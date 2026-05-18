using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Identity;

/// <summary>
/// `_old` kullanıcı modelinin auth ve temel user management için gereken alanlarını taşır.
/// </summary>
public sealed class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public long RoleId { get; set; } = 1;
    public UserAuthority? RoleNavigation { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public UserDetail? UserDetail { get; set; }
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
