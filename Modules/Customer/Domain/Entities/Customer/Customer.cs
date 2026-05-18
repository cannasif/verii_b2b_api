using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Customer;

public sealed class Customer : BaseEntity
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? TcknNumber { get; set; }
    public string? SalesRepCode { get; set; }
    public string? GroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public short? BusinessUnitCode { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? CountryCode { get; set; }
    public bool IsErpIntegrated { get; set; } = true;
    public string? ErpIntegrationNumber { get; set; }
    public DateTime? LastSyncDate { get; set; }
}
