using Microsoft.AspNetCore.Http;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IPaytrPaymentService
{
    Task<ApiResponse<PaytrIframeTokenDto>> CreateIframeTokenAsync(CreatePaytrIframeTokenDto dto, string requestIp, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> HandleCallbackAsync(IFormCollection form, CancellationToken cancellationToken = default);
}
