using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProduct : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? CategoryPath { get; set; }
    public string? Description { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
    public string? SearchText { get; set; }
    public DateTime? PublishedDate { get; set; }

    public List<CatalogVariant> Variants { get; set; } = new();
}
