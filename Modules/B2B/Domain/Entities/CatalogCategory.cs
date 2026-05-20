using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogCategory : BaseEntity
{
    public long? ParentCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; } = 1;
    public string? FullPath { get; set; }
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }
    public string? IconName { get; set; }
    public string? ColorHex { get; set; }
    public bool IsLeaf { get; set; }
    public bool IsActive { get; set; } = true;

    public CatalogCategory? ParentCategory { get; set; }
    public List<CatalogCategory> Children { get; set; } = new();
    public List<CatalogProductCategory> ProductCategories { get; set; } = new();
    public List<CatalogAttributeDefinition> AttributeDefinitions { get; set; } = new();
}
