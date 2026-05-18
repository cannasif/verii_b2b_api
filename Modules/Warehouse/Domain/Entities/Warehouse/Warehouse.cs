using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Warehouse;

public sealed class Warehouse : BaseEntity
{
    public int WarehouseCode { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}
