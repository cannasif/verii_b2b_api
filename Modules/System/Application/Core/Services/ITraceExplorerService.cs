using Wms.Application.Common;
using Wms.Application.System.Dtos;

namespace Wms.Application.System.Services;

public interface ITraceExplorerService
{
    Task<ApiResponse<TraceExplorerResponseDto>> GetByTraceIdAsync(string traceId, CancellationToken cancellationToken = default);
}
