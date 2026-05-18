using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public interface IPermissionGroupService
{
    Task<ApiResponse<PagedResponse<PermissionGroupDto>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionGroupDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionGroupDto>> CreateAsync(CreatePermissionGroupDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionGroupDto>> UpdateAsync(long id, UpdatePermissionGroupDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionGroupDto>> SetPermissionsAsync(long id, SetPermissionGroupPermissionsDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
}
