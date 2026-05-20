using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProductMedia : BaseEntity
{
    public long CatalogProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Image";
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public CatalogProduct? CatalogProduct { get; set; }
}
