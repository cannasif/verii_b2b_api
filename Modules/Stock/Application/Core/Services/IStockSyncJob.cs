using Wms.Application.Stock.Dtos;

namespace Wms.Application.Stock.Services;

public interface IStockSyncJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
