using Wms.Domain.Entities.Stock.Functions;

namespace Wms.Application.Stock.Services;

public interface IStockErpReadService
{
    Task<List<FnStockRow>> GetAllAsync(CancellationToken cancellationToken = default);
}
