using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.AccessControl.Services;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
using Wms.Application.Identity.Services;

namespace Wms.WebApi.Controllers.Identity;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPermissionAccessService _permissionAccessService;
    private readonly ILocalizationService _localizationService;

    public AuthController(IAuthService authService, IPermissionAccessService permissionAccessService, ILocalizationService localizationService)
    {
        _authService = authService;
        _permissionAccessService = permissionAccessService;
        _localizationService = localizationService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RegisterUserAsync(registerDto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("me/permissions")]
    public async Task<ActionResult<ApiResponse<MyPermissionsDto>>> GetMyPermissions([FromQuery] string? platform = null, CancellationToken cancellationToken = default)
    {
        var result = await _permissionAccessService.GetMyPermissionsAsync(platform, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        var result = await _authService.GetAllUsersAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("users/active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetActiveUsers(CancellationToken cancellationToken = default)
    {
        var result = await _authService.GetActiveUsersAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("user/{id:long}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(long id, CancellationToken cancellationToken = default)
    {
        var result = await _authService.GetUserByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RequestPasswordResetAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("request-password-reset")]
    public async Task<ActionResult<ApiResponse<string>>> RequestPasswordReset([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RequestPasswordResetAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
        {
            var unauthorized = _localizationService.GetLocalizedString("UnauthorizedAccess");
            return StatusCode(401, ApiResponse<string>.ErrorResult(unauthorized, unauthorized, 401));
        }

        var result = await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
