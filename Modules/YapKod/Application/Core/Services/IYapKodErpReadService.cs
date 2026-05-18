using Wms.Domain.Entities.YapKod.Functions;

namespace Wms.Application.YapKod.Services;

public interface IYapKodErpReadService
{
    Task<List<FnYapKodRow>> GetAllAsync(CancellationToken cancellationToken = default);
}
