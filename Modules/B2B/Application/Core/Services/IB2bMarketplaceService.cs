using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bMarketplaceService
{
    Task<ApiResponse<List<MarketplaceCapabilityDto>>> GetCapabilitiesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<MarketplaceChannelDto>>> GetChannelsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceChannelDto>> CreateChannelAsync(CreateMarketplaceChannelDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceChannelDto>> UpdateChannelAsync(long id, UpdateMarketplaceChannelDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<MarketplaceListingDto>>> GetListingsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceListingDto>> UpsertListingAsync(UpsertMarketplaceListingDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<MarketplaceSyncEventDto>>> GetSyncEventsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceSyncEventDto>> QueueSyncEventAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default);
}
