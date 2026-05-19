using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/portal")]
[AllowAnonymous]
public sealed class B2bPortalController : ControllerBase
{
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bPortalController(IB2bPortalAccessService portalAccess)
    {
        _portalAccess = portalAccess;
    }

    [HttpPost("session")]
    public async Task<ActionResult<ApiResponse<B2bPortalSessionDto>>> CreateSession([FromBody] CreateB2bPortalSessionDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _portalAccess.CreateSessionAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("me/session")]
    public async Task<ActionResult<ApiResponse<B2bPortalSessionDto>>> CreateMySession(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
        {
            return StatusCode(401, ApiResponse<B2bPortalSessionDto>.ErrorResult("Kullanıcı oturumu doğrulanamadı", statusCode: 401));
        }

        var result = await _portalAccess.CreateSessionForUserAsync(userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
