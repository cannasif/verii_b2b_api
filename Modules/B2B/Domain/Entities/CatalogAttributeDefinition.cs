using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogAttributeDefinition : BaseEntity
{
    public long? CatalogCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsComparable { get; set; }
    public string? Unit { get; set; }
    public string? AllowedValuesJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public CatalogCategory? CatalogCategory { get; set; }
    public List<CatalogProductAttribute> ProductAttributes { get; set; } = new();
}
