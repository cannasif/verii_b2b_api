using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogCategoryFavorite : BaseEntity
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public long CatalogCategoryId { get; set; }
    public string? Note { get; set; }

    public B2bCompany? Company { get; set; }
    public B2bBuyer? Buyer { get; set; }
    public CatalogCategory? CatalogCategory { get; set; }
}
