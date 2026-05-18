using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.AccessControl.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.AccessControl;

[ApiController]
[Route("api/wms-scope-policies")]
[Authorize]
public sealed class WmsScopePolicyController : ControllerBase
{
    private readonly IWmsScopePolicyService _service;

    public WmsScopePolicyController(IWmsScopePolicyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<WmsScopePolicyDto>>>> GetAll([FromQuery] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<WmsScopePolicyDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<WmsScopePolicyDto>>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WmsScopePolicyDto>>> Create([FromBody] CreateWmsScopePolicyDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<WmsScopePolicyDto>>> Update(long id, [FromBody] UpdateWmsScopePolicyDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.SoftDeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("users/{userId:long}/assignments")]
    public async Task<ActionResult<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>>> GetUserAssignments(long userId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAssignmentsByUserIdAsync(userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("users/{userId:long}/assignments")]
    public async Task<ActionResult<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>>> SetUserAssignments(long userId, [FromBody] SetUserWmsScopePoliciesDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.SetAssignmentsAsync(userId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("users/{userId:long}/resolve")]
    public async Task<ActionResult<ApiResponse<WmsScopePolicyResolutionDto>>> Resolve(long userId, [FromQuery] string entityType, CancellationToken cancellationToken = default)
    {
        var result = await _service.ResolveForUserAsync(userId, entityType, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
