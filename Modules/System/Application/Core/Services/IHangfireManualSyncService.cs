using Wms.Application.Common;
using Wms.Application.System.Dtos;

namespace Wms.Application.System.Services;

public interface IHangfireManualSyncService
{
    Task<ApiResponse<IReadOnlyList<ManualSyncJobStatusDto>>> GetJobStatusesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<TriggerManualSyncJobResponseDto>> TriggerAsync(TriggerManualSyncJobRequestDto request, CancellationToken cancellationToken = default);
}
