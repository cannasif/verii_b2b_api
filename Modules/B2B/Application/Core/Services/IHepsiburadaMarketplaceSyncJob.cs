namespace Wms.Application.B2B.Services;

public interface IHepsiburadaMarketplaceSyncJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
