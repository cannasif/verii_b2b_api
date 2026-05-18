using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Dtos;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Modules.NetsisIntegrations.Api;

[ApiController]
[Route("api/netsis-auth")]
[Authorize]
public sealed class NetsisAuthController : ControllerBase
{
    private readonly INetsisAuthTokenService _tokenService;
    private readonly ILogger<NetsisAuthController> _logger;

    public NetsisAuthController(
        INetsisAuthTokenService tokenService,
        ILogger<NetsisAuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpGet("login")]
    public async Task<ActionResult<ApiResponse<NetsisTokenResultDto>>> Login(
        [FromQuery] bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenService.GetTokenAsync(forceRefresh, cancellationToken);
            token.AccessToken = string.Empty;
            token.RefreshToken = null;

            return Ok(ApiResponse<NetsisTokenResultDto>.SuccessResult(token, "Netsis token resolved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Netsis login check failed.");

            var response = ApiResponse<NetsisTokenResultDto>.ErrorResult(
                "Netsis login failed.",
                ex.Message,
                StatusCodes.Status502BadGateway,
                errorCode: "netsis_login_failed");

            return StatusCode(response.StatusCode, response);
        }
    }
}
