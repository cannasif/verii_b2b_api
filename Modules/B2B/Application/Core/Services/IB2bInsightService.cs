using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bInsightService
{
    Task<ApiResponse<B2bInsightSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
