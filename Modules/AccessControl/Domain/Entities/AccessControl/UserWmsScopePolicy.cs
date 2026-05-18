using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.AccessControl;

public sealed class UserWmsScopePolicy : BaseEntity
{
    public long UserId { get; set; }
    public long WmsScopePolicyId { get; set; }
    public long? WarehouseId { get; set; }

    public WmsScopePolicy WmsScopePolicy { get; set; } = null!;
}
