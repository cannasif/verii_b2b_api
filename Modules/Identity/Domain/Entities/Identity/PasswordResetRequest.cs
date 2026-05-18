using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Identity;

/// <summary>
/// Şifre sıfırlama taleplerini `_old` davranışına yakın şekilde saklar.
/// </summary>
public sealed class PasswordResetRequest : BaseEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? RequestIp { get; set; }
    public string? UserAgent { get; set; }
}
