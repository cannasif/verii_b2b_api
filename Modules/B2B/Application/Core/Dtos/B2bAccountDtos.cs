using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.B2B.Dtos;

public sealed class B2bCompanyDto : BaseEntityDto
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    public long? ParentCompanyId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string Status { get; set; } = string.Empty;
}

public sealed class CreateB2bCompanyDto
{
    [StringLength(80)] public string? CompanyCode { get; set; }
    [StringLength(220)] public string? CompanyName { get; set; }
    public long? CustomerId { get; set; }
    public long? ParentCompanyId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    public decimal? CreditLimit { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
}

public sealed class CreateB2bPortalSessionDto
{
    [Required, StringLength(80)] public string CompanyCode { get; set; } = string.Empty;
    [StringLength(180)] public string? BuyerEmail { get; set; }
}

public sealed class B2bPortalSessionDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public B2bCompanyDto Company { get; set; } = new();
    public B2bBuyerDto? Buyer { get; set; }
    public string Scope { get; set; } = "Buyer";
    public bool CanViewCompanyHistory { get; set; }
}

public sealed class B2bPortalContextDto
{
    public bool IsBackoffice { get; set; }
    public long CompanyId { get; set; }
    public long CustomerId { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string? CustomerGroupCode { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerName { get; set; }
    public string RoleCode { get; set; } = "Buyer";
    public bool CanViewCompanyHistory { get; set; }
}

public sealed class B2bBuyerDto : BaseEntityDto
{
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public decimal? OrderLimit { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateB2bBuyerDto
{
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    [StringLength(180)] public string? Email { get; set; }
    [StringLength(180)] public string? FullName { get; set; }
    [StringLength(60)] public string RoleCode { get; set; } = "Buyer";
    public decimal? OrderLimit { get; set; }
    public bool RequiresApproval { get; set; }
}

public sealed class B2bCompanyAddressDto : BaseEntityDto
{
    public long CompanyId { get; set; }
    public string AddressType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class CatalogVisibilityRuleDto : BaseEntityDto
{
    public long? CompanyId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public long? CatalogProductId { get; set; }
    public string? CategoryPath { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class CreateCatalogVisibilityRuleDto
{
    public long? CompanyId { get; set; }
    public long? CustomerId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    public long? CatalogProductId { get; set; }
    [StringLength(500)] public string? CategoryPath { get; set; }
    [StringLength(40)] public string RuleType { get; set; } = "Include";
}

public sealed class ShoppingListDto : BaseEntityDto
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public string ListType { get; set; } = string.Empty;
}

public sealed class CreateShoppingListDto
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    [Required, StringLength(180)] public string Name { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    [StringLength(40)] public string ListType { get; set; } = "ShoppingList";
}

public sealed class PurchaseApprovalRuleDto : BaseEntityDto
{
    public long CompanyId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string ApproverRoleCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class CreatePurchaseApprovalRuleDto
{
    public long CompanyId { get; set; }
    [Required, StringLength(180)] public string RuleName { get; set; } = string.Empty;
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(60)] public string ApproverRoleCode { get; set; } = "Approver";
}
