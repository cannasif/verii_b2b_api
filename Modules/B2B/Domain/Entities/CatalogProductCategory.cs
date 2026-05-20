using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProductCategory : BaseEntity
{
    public long CatalogProductId { get; set; }
    public long CatalogCategoryId { get; set; }
    public bool IsPrimary { get; set; } = true;
    public int SortOrder { get; set; }
    public string? AssignmentSource { get; set; }

    public CatalogProduct? CatalogProduct { get; set; }
    public CatalogCategory? CatalogCategory { get; set; }
}
