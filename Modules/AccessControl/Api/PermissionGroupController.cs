using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.AccessControl.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.AccessControl;

[ApiController]
[Route("api/permission-groups")]
[Authorize]
public sealed class PermissionGroupController : ControllerBase
{
    private readonly IPermissionGroupService _service;

    public PermissionGroupController(IPermissionGroupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PermissionGroupDto>>>> GetAll([FromQuery] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PermissionGroupDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> Create([FromBody] CreatePermissionGroupDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> Update(long id, [FromBody] UpdatePermissionGroupDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}/permissions")]
    public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> SetPermissions(long id, [FromBody] SetPermissionGroupPermissionsDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.SetPermissionsAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.SoftDeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
