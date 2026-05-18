namespace Wms.Application.YapKod.Services;

public interface IYapKodSyncJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
