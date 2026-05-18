using Wms.Application.Common;
using Wms.Application.Identity.Dtos;

namespace Wms.Application.Identity.Services;

/// <summary>
/// `_old` auth use-case yüzeyinin pragmatik karşılığıdır.
/// </summary>
public interface IAuthService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<UserDto>>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> LoginAsync(LoginRequest loginDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
