using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class ShoppingList : BaseEntity
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public string ListType { get; set; } = "ShoppingList";

    public List<ShoppingListLine> Lines { get; set; } = new();
}
