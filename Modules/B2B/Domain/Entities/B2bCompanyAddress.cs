using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bCompanyAddress : BaseEntity
{
    public long CompanyId { get; set; }
    public string AddressType { get; set; } = "Shipping";
    public string Title { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }

    public B2bCompany? Company { get; set; }
}
