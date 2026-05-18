using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CatalogVisibilityRule : BaseEntity
{
    public long? CompanyId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public long? CatalogProductId { get; set; }
    public string? CategoryPath { get; set; }
    public string RuleType { get; set; } = "Include";
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
