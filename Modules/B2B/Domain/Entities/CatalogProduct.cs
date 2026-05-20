using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProduct : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ProductType { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? Barcode { get; set; }
    public string? Unit { get; set; }
    public string? CategoryPath { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? BulletPointsJson { get; set; }
    public string? AttributesJson { get; set; }
    public string? MediaGalleryJson { get; set; }
    public string? DocumentsJson { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? SearchKeywords { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? PackageQuantity { get; set; }
    public int SortOrder { get; set; }
    public int CompletenessScore { get; set; }
    public bool IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
    public string? SearchText { get; set; }
    public DateTime? PublishedDate { get; set; }

    public List<CatalogVariant> Variants { get; set; } = new();
    public List<CatalogProductCategory> ProductCategories { get; set; } = new();
    public List<CatalogProductAttribute> ProductAttributes { get; set; } = new();
    public List<CatalogProductMedia> MediaItems { get; set; } = new();
    public List<CatalogProductDocument> Documents { get; set; } = new();
}
