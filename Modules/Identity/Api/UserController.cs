using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
using Wms.Application.Identity.Services;

namespace Wms.WebApi.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _service; public UserController(IUserService service) => _service = service;
    [HttpGet] public async Task<IActionResult> Get([FromQuery] PagedRequest request, CancellationToken cancellationToken = default) { var r = await _service.GetAllUsersAsync(request, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpPost("paged")] public async Task<IActionResult> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default) { var r = await _service.GetAllUsersAsync(request, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpGet("{id:long}")] public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken = default) { var r = await _service.GetUserByIdAsync(id, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpPost] public async Task<IActionResult> Post([FromBody] CreateUserDto dto, CancellationToken cancellationToken = default) { var r = await _service.CreateUserAsync(dto, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpPut("{id:long}")] public async Task<IActionResult> Put(long id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken = default) { var r = await _service.UpdateUserAsync(id, dto, cancellationToken); return StatusCode(r.StatusCode, r); }
    [HttpDelete("{id:long}")] public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken = default) { var r = await _service.DeleteUserAsync(id, cancellationToken); return StatusCode(r.StatusCode, r); }
}
