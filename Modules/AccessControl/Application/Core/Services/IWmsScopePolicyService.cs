using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public interface IWmsScopePolicyService
{
    Task<ApiResponse<PagedResponse<WmsScopePolicyDto>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<WmsScopePolicyDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<WmsScopePolicyDto>> CreateAsync(CreateWmsScopePolicyDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<WmsScopePolicyDto>> UpdateAsync(long id, UpdateWmsScopePolicyDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>> GetAssignmentsByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>> SetAssignmentsAsync(long userId, SetUserWmsScopePoliciesDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<WmsScopePolicyResolutionDto>> ResolveForUserAsync(long userId, string entityType, CancellationToken cancellationToken = default);
}
