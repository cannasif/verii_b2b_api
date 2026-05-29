using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface ITrendyolMarketplaceClient
{
    Task<TrendyolClientResult> SendAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default);
    Task<TrendyolBatchStatusResult> GetBatchStatusAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default);
}
