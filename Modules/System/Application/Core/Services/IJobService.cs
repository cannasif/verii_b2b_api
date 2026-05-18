using Wms.Application.Common;
using Wms.Domain.Entities.Common;

namespace Wms.Application.System.Services;

public interface IJobService
{
    Task<ApiResponse<PagedResponse<object>>> GetFailedPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<object>>> GetDeadLetterPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<JobFailureLog>>> GetFailureLogsAsync(int pageNumber = 0, int pageSize = 20, CancellationToken cancellationToken = default);
}
