using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
namespace Wms.Application.Identity.Services;
public interface IUserService
{
    Task<ApiResponse<PagedResponse<UserDto>>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateUserAsync(long id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> DeleteUserAsync(long id, CancellationToken cancellationToken = default);
}
