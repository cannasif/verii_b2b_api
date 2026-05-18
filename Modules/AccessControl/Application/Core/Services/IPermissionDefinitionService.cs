using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public interface IPermissionDefinitionService
{
    Task<ApiResponse<PagedResponse<PermissionDefinitionDto>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDefinitionDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDefinitionDto>> CreateAsync(CreatePermissionDefinitionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDefinitionDto>> UpdateAsync(long id, UpdatePermissionDefinitionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDefinitionSyncResultDto>> SyncAsync(SyncPermissionDefinitionsDto dto, CancellationToken cancellationToken = default);
}
