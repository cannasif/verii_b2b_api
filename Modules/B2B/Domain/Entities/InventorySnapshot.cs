using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class InventorySnapshot : BaseEntity
{
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string? ErpStockCode { get; set; }
    public int? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public string? Unit { get; set; }
    public DateTime SnapshotDate { get; set; }
    public DateTime? LastErpSyncDate { get; set; }
}
