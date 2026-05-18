using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public interface IUserPermissionGroupService
{
    Task<ApiResponse<UserPermissionGroupDto>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserPermissionGroupDto>> SetUserGroupsAsync(long userId, SetUserPermissionGroupsDto dto, CancellationToken cancellationToken = default);
}
