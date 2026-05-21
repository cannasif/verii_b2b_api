using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProductFavorite : BaseEntity
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string FavoriteKey { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Note { get; set; }

    public B2bCompany? Company { get; set; }
    public B2bBuyer? Buyer { get; set; }
    public CatalogProduct? CatalogProduct { get; set; }
    public CatalogVariant? CatalogVariant { get; set; }
}
