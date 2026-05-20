using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProductAttribute : BaseEntity
{
    public long CatalogProductId { get; set; }
    public long AttributeDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? NormalizedValue { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }

    public CatalogProduct? CatalogProduct { get; set; }
    public CatalogAttributeDefinition? AttributeDefinition { get; set; }
}
