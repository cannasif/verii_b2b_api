using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.AccessControl.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.AccessControl;

[ApiController]
[Route("api/user-permission-groups")]
[Authorize]
public sealed class UserPermissionGroupController : ControllerBase
{
    private readonly IUserPermissionGroupService _service;

    public UserPermissionGroupController(IUserPermissionGroupService service)
    {
        _service = service;
    }

    [HttpGet("{userId:long}")]
    public async Task<ActionResult<ApiResponse<UserPermissionGroupDto>>> GetByUserId(long userId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByUserIdAsync(userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{userId:long}")]
    public async Task<ActionResult<ApiResponse<UserPermissionGroupDto>>> SetUserGroups(long userId, [FromBody] SetUserPermissionGroupsDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.SetUserGroupsAsync(userId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
