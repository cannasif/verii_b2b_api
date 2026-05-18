namespace Wms.Application.Warehouse.Services;

public interface IWarehouseSyncJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
