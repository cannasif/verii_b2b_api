using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.AccessControl.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.AccessControl;

[ApiController]
[Route("api/permission-definitions")]
[Authorize]
public sealed class PermissionDefinitionController : ControllerBase
{
    private readonly IPermissionDefinitionService _service;

    public PermissionDefinitionController(IPermissionDefinitionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PermissionDefinitionDto>>>> GetAll([FromQuery] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PermissionDefinitionDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PermissionDefinitionDto>>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PermissionDefinitionDto>>> Create([FromBody] CreatePermissionDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<PermissionDefinitionDto>>> Update(long id, [FromBody] UpdatePermissionDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("sync")]
    public async Task<ActionResult<ApiResponse<PermissionDefinitionSyncResultDto>>> Sync([FromBody] SyncPermissionDefinitionsDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.SyncAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.SoftDeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
