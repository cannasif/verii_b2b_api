using Wms.Domain.Entities.Warehouse.Functions;

namespace Wms.Application.Warehouse.Services;

public interface IWarehouseErpReadService
{
    Task<List<FnWarehouseRow>> GetAllAsync(CancellationToken cancellationToken = default);
}
