using Wms.Application.Common;
using Wms.Application.YapKod.Dtos;

namespace Wms.Application.YapKod.Services;

public interface IYapKodService
{
    Task<ApiResponse<IEnumerable<YapKodDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<YapKodDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<YapKodDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<YapKodDto>> CreateAsync(CreateYapKodDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<YapKodDto>> UpdateAsync(long id, UpdateYapKodDto updateDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> YapKodSyncAsync(IEnumerable<SyncYapKodDto> items, CancellationToken cancellationToken = default);
}
