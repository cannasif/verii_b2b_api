using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bCartLine : BaseEntity
{
    public long CartId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public int? WarehouseCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? PriceSource { get; set; }
    public long? PriceListId { get; set; }
    public decimal? DiscountRate { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public DateTime? PriceResolvedAt { get; set; }

    public B2bCart? Cart { get; set; }
}
