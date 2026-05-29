namespace Wms.Application.B2B.Services;

public interface ITrendyolMarketplaceSyncJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
