using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class MarketplaceListing : BaseEntity
{
    public long ChannelId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? ErpStockId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? MarketplaceProductId { get; set; }
    public string? MarketplaceListingId { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal? LastPushedPrice { get; set; }
    public decimal? LastPushedQuantity { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? LastProductSyncDate { get; set; }
    public DateTime? LastPriceSyncDate { get; set; }
    public DateTime? LastStockSyncDate { get; set; }
    public string? ErrorMessage { get; set; }

    public MarketplaceChannel? Channel { get; set; }
    public CatalogProduct? CatalogProduct { get; set; }
    public List<MarketplaceSyncEvent> SyncEvents { get; set; } = new();
}
