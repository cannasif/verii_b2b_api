using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Identity;

public sealed class UserDetail : BaseEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? Description { get; set; }
    public Gender? Gender { get; set; }
}
