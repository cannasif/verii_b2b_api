using Microsoft.AspNetCore.Http;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bPortalAccessService
{
    Task<ApiResponse<B2bPortalSessionDto>> CreateSessionAsync(CreateB2bPortalSessionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<B2bPortalSessionDto>> CreateSessionForUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<long>> ValidateRequestAsync(HttpRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<B2bPortalContextDto>> ValidateContextAsync(HttpRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<B2bPortalContextDto>> ValidateCustomerContextAsync(HttpRequest request, long customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<long>> ValidateCustomerAccessAsync(HttpRequest request, long customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<long>> ValidateCartAccessAsync(HttpRequest request, long cartId, CancellationToken cancellationToken = default);
}
