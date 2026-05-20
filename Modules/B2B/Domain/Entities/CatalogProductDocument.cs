using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogProductDocument : BaseEntity
{
    public long CatalogProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "TechnicalSheet";
    public string? LanguageCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public CatalogProduct? CatalogProduct { get; set; }
}
