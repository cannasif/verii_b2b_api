using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class ShoppingListLine : BaseEntity
{
    public long ShoppingListId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string? Sku { get; set; }
    public decimal Quantity { get; set; }

    public ShoppingList? ShoppingList { get; set; }
}
