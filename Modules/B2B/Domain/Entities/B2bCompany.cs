using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bCompany : BaseEntity
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    public long? ParentCompanyId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string Status { get; set; } = "Active";

    public List<B2bBuyer> Buyers { get; set; } = new();
    public List<B2bCompanyAddress> Addresses { get; set; } = new();
}
