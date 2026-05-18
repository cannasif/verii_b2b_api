using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
using Wms.Application.Identity.Services;

namespace Wms.WebApi.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UserAuthorityController : ControllerBase
{
    private readonly IUserAuthorityService _service; public UserAuthorityController(IUserAuthorityService service) => _service = service;
    [HttpGet] public async Task<IActionResult> Get([FromQuery] PagedRequest request, CancellationToken cancellationToken = default) { var r = await _service.GetPagedAsync(request, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpPost("paged")] public async Task<ActionResult<ApiResponse<PagedResponse<UserAuthorityDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default) { var r = await _service.GetPagedAsync(request, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpGet("{id:long}")] public async Task<ActionResult<ApiResponse<UserAuthorityDto>>> GetById(long id, CancellationToken cancellationToken = default) { var r = await _service.GetByIdAsync(id, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpPost] public async Task<ActionResult<ApiResponse<UserAuthorityDto>>> Create([FromBody] CreateUserAuthorityDto dto, CancellationToken cancellationToken = default) { var r = await _service.CreateAsync(dto, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpPut("{id:long}")] public async Task<ActionResult<ApiResponse<UserAuthorityDto>>> Update(long id, [FromBody] UpdateUserAuthorityDto dto, CancellationToken cancellationToken = default) { var r = await _service.UpdateAsync(id, dto, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpDelete("{id:long}")] public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id, CancellationToken cancellationToken = default) { var r = await _service.SoftDeleteAsync(id, cancellationToken); return StatusCode(r.StatusCode, r); }
}
