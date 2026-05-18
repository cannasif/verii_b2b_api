using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bAccountService
{
    Task<ApiResponse<PagedResponse<B2bCompanyDto>>> GetCompaniesAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<B2bCompanyDto>> CreateCompanyAsync(CreateB2bCompanyDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<B2bBuyerDto>>> GetBuyersAsync(PagedRequest request, long? companyId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<B2bBuyerDto>> CreateBuyerAsync(CreateB2bBuyerDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CatalogVisibilityRuleDto>>> GetVisibilityRulesAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogVisibilityRuleDto>> CreateVisibilityRuleAsync(CreateCatalogVisibilityRuleDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<ShoppingListDto>>> GetShoppingListsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShoppingListDto>> CreateShoppingListAsync(CreateShoppingListDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<PurchaseApprovalRuleDto>>> GetApprovalRulesAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PurchaseApprovalRuleDto>> CreateApprovalRuleAsync(CreatePurchaseApprovalRuleDto dto, CancellationToken cancellationToken = default);
}
