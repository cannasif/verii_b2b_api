using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
namespace Wms.Application.Identity.Services;
public interface IUserAuthorityService
{
    Task<ApiResponse<IEnumerable<UserAuthorityDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<UserAuthorityDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserAuthorityDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserAuthorityDto>> CreateAsync(CreateUserAuthorityDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserAuthorityDto>> UpdateAsync(long id, UpdateUserAuthorityDto updateDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
}
