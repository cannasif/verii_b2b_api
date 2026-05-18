using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bCommerceService
{
    Task<ApiResponse<PagedResponse<CatalogProductDto>>> GetCatalogProductsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDto>> GetCatalogProductAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDto>> CreateCatalogProductAsync(CreateCatalogProductDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogProductDto>> UpdateCatalogProductAsync(long id, UpdateCatalogProductDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CatalogVariantDto>> UpsertVariantAsync(long productId, UpsertCatalogVariantDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CustomerProductAliasDto>>> GetAliasesAsync(PagedRequest request, long? customerId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerProductAliasDto>> CreateAliasAsync(CreateCustomerProductAliasDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerProductAliasDto>> UpdateAliasAsync(long id, UpdateCustomerProductAliasDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> GetDraftCartAsync(long customerId, long? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> AddCartLineAsync(AddCartLineDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuickOrderResultDto>> AddQuickOrderLinesAsync(QuickOrderDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> UpdateCartLineAsync(long lineId, UpdateCartLineDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemoveCartLineAsync(long lineId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<OrderDto>>> GetOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuickOrderResultDto>> ReorderAsync(ReorderDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerPortalSummaryDto>> GetCustomerPortalSummaryAsync(long customerId, long? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<PaymentTransactionDto>>> GetPaymentsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentTransactionDto>> CreatePaymentTransactionAsync(CreatePaymentTransactionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentTransactionDto>> UpdatePaymentStatusAsync(long id, UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default);
}
