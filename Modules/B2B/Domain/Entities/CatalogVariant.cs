using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogVariant : BaseEntity
{
    public long CatalogProductId { get; set; }
    public long? ErpStockId { get; set; }
    public string VariantSku { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? AttributesJson { get; set; }
    public bool IsActive { get; set; } = true;

    public CatalogProduct? CatalogProduct { get; set; }
}
