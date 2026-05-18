using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.Customer.Dtos;

public sealed class CustomerDto : BaseEntityDto
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
    public bool IsErpIntegrated { get; set; }
    public string? ErpIntegrationNumber { get; set; }
    public DateTime? LastSyncDate { get; set; }
}

public sealed class CreateCustomerDto
{
    [Required, StringLength(50)]
    public string CustomerCode { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    [StringLength(100)] public string? TaxOffice { get; set; }
    [StringLength(50)] public string? TaxNumber { get; set; }
    [StringLength(11)] public string? TcknNumber { get; set; }
    [StringLength(50)] public string? SalesRepCode { get; set; }
    [StringLength(50)] public string? GroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public string BranchCode { get; set; } = "0";
    public short? BusinessUnitCode { get; set; }
    [StringLength(100)] public string? Email { get; set; }
    [StringLength(100)] public string? Website { get; set; }
    [StringLength(100)] public string? Phone1 { get; set; }
    [StringLength(100)] public string? Phone2 { get; set; }
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? District { get; set; }
    [StringLength(50)] public string? CountryCode { get; set; }
    public bool IsErpIntegrated { get; set; } = true;
    [StringLength(50)] public string? ErpIntegrationNumber { get; set; }
}

public sealed class UpdateCustomerDto
{
    [StringLength(50)] public string? CustomerCode { get; set; }
    [StringLength(200)] public string? CustomerName { get; set; }
    [StringLength(100)] public string? TaxOffice { get; set; }
    [StringLength(50)] public string? TaxNumber { get; set; }
    [StringLength(11)] public string? TcknNumber { get; set; }
    [StringLength(50)] public string? SalesRepCode { get; set; }
    [StringLength(50)] public string? GroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? BranchCode { get; set; }
    public short? BusinessUnitCode { get; set; }
    [StringLength(100)] public string? Email { get; set; }
    [StringLength(100)] public string? Website { get; set; }
    [StringLength(100)] public string? Phone1 { get; set; }
    [StringLength(100)] public string? Phone2 { get; set; }
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? District { get; set; }
    [StringLength(50)] public string? CountryCode { get; set; }
    public bool? IsErpIntegrated { get; set; }
    [StringLength(50)] public string? ErpIntegrationNumber { get; set; }
}

public sealed class SyncCustomerDto
{
    [Required, StringLength(50)]
    public string CustomerCode { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    [StringLength(100)] public string? TaxOffice { get; set; }
    [StringLength(50)] public string? TaxNumber { get; set; }
    [StringLength(11)] public string? TcknNumber { get; set; }
    [StringLength(50)] public string? SalesRepCode { get; set; }
    [StringLength(50)] public string? GroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public string BranchCode { get; set; } = "0";
    public short? BusinessUnitCode { get; set; }
    [StringLength(100)] public string? Email { get; set; }
    [StringLength(100)] public string? Website { get; set; }
    [StringLength(100)] public string? Phone1 { get; set; }
    [StringLength(100)] public string? Phone2 { get; set; }
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? District { get; set; }
    [StringLength(50)] public string? CountryCode { get; set; }
    public bool IsErpIntegrated { get; set; } = true;
    [StringLength(50)] public string? ErpIntegrationNumber { get; set; }
}
