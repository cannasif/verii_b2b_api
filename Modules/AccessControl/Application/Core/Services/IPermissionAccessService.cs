using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public interface IPermissionAccessService
{
    Task<ApiResponse<MyPermissionsDto>> GetMyPermissionsAsync(string? platform = null, CancellationToken cancellationToken = default);
}
