using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CustomerPriceListItem : BaseEntity
{
    public long PriceListId { get; set; }
    public long? CustomerId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? DiscountRate { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public CustomerPriceList? PriceList { get; set; }
}
