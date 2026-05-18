using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Dtos;

namespace Wms.Modules.NetsisIntegrations.Application.Services;

public interface INetsisReadService
{
    Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<KurDto>>> GetExchangeRatesAsync(DateTime date, int pricingType, CancellationToken cancellationToken = default);
}
