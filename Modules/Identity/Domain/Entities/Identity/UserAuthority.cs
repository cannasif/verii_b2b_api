using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Identity;

public sealed class UserAuthority : BaseEntity
{
    public string Title { get; set; } = string.Empty;
}
