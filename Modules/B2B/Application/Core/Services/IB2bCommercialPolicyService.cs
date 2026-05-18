using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bCommercialPolicyService
{
    Task<ApiResponse<PagedResponse<CustomerPriceListDto>>> GetPriceListsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerPriceListDto>> GetPriceListAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerPriceListDto>> CreatePriceListAsync(CreateCustomerPriceListDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerPriceListItemDto>> UpsertPriceListItemAsync(long priceListId, UpsertCustomerPriceListItemDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<B2bPriceAvailabilityDto>> ResolvePriceAvailabilityAsync(ResolveB2bPriceAvailabilityDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<InventorySnapshotDto>>> GetInventoryAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventorySnapshotDto>> UpsertInventoryAsync(UpsertInventorySnapshotDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<QuoteRequestDto>>> GetQuotesAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuoteRequestDto>> GetQuoteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuoteRequestDto>> CreateQuoteAsync(CreateQuoteRequestDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuoteRequestDto>> UpdateQuoteStatusAsync(long id, UpdateQuoteStatusDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> ConvertQuoteToCartAsync(ConvertQuoteToCartDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<B2bIntegrationEventDto>>> GetIntegrationEventsAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
