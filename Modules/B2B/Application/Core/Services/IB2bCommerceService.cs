using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bCommerceService
{
    Task<ApiResponse<PagedResponse<CatalogProductDto>>> GetCatalogProductsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CatalogProductDto>>> GetPublicCatalogProductsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDto>> GetCatalogProductAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDto>> CreateCatalogProductAsync(CreateCatalogProductDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDto>> UpdateCatalogProductAsync(long id, UpdateCatalogProductDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogVariantDto>> UpsertVariantAsync(long productId, UpsertCatalogVariantDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CatalogCategoryDto>>> GetCatalogCategoriesAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogCategoryDto>> CreateCatalogCategoryAsync(CreateCatalogCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogCategoryDto>> UpdateCatalogCategoryAsync(long id, UpdateCatalogCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CatalogProductFavoriteDto>>> GetCatalogProductFavoritesAsync(PagedRequest request, long companyId, long? buyerId = null, long? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogFavoriteToggleResultDto>> ToggleCatalogProductFavoriteAsync(ToggleCatalogProductFavoriteDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CatalogCategoryFavoriteDto>>> GetCatalogCategoryFavoritesAsync(PagedRequest request, long companyId, long? buyerId = null, long? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogFavoriteToggleResultDto>> ToggleCatalogCategoryFavoriteAsync(ToggleCatalogCategoryFavoriteDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductCategoryDto>> AssignCatalogProductCategoryAsync(long productId, AssignCatalogProductCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CatalogAttributeDefinitionDto>>> GetCatalogAttributeDefinitionsAsync(PagedRequest request, long? categoryId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogAttributeDefinitionDto>> CreateCatalogAttributeDefinitionAsync(CreateCatalogAttributeDefinitionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductAttributeDto>> UpsertCatalogProductAttributeAsync(long productId, UpsertCatalogProductAttributeDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductMediaDto>> UpsertCatalogProductMediaAsync(long productId, UpsertCatalogProductMediaDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CatalogProductMediaDto>>> UploadCatalogProductMediaAsync(long productId, UploadCatalogProductMediaDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDocumentDto>> UpsertCatalogProductDocumentAsync(long productId, UpsertCatalogProductDocumentDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CustomerProductAliasDto>>> GetAliasesAsync(PagedRequest request, long? customerId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerProductAliasDto>> CreateAliasAsync(CreateCustomerProductAliasDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerProductAliasDto>> UpdateAliasAsync(long id, UpdateCustomerProductAliasDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> GetDraftCartAsync(long customerId, long? userId = null, long? buyerId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> AddCartLineAsync(AddCartLineDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuickOrderResultDto>> AddQuickOrderLinesAsync(QuickOrderDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> UpdateCartLineAsync(long lineId, UpdateCartLineDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemoveCartLineAsync(long lineId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<OrderDto>>> GetOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> GetOrderAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuickOrderResultDto>> ReorderAsync(ReorderDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerPortalSummaryDto>> GetCustomerPortalSummaryAsync(long customerId, long? userId = null, long? buyerId = null, bool includeCompanyHistory = true, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<PaymentTransactionDto>>> GetPaymentsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentTransactionDto>> CreatePaymentTransactionAsync(CreatePaymentTransactionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentTransactionDto>> UpdatePaymentStatusAsync(long id, UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<PaymentOrderDto>>> GetPaymentOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentOrderDto>> CreatePaymentOrderAsync(CreatePaymentOrderDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentOrderDto>> UpdatePaymentOrderPlanAsync(long id, UpdatePaymentOrderPlanDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentOrderDto>> SelectPaymentProviderInstallmentAsync(long id, SelectPaymentProviderInstallmentDto dto, CancellationToken cancellationToken = default);
}
