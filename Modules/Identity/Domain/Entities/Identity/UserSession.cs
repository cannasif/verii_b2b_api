using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Identity;

/// <summary>
/// Aktif oturum takibi için `_old` UserSession davranışını taşır.
/// </summary>
public sealed class UserSession : BaseEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public Guid SessionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
}
