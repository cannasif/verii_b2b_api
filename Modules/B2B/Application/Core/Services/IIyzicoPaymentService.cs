using Microsoft.AspNetCore.Http;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IIyzicoPaymentService
{
    Task<ApiResponse<Iyzico3dsInitializeDto>> Initialize3dsAsync(CreateIyzico3dsPaymentDto dto, string requestIp, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> Handle3dsCallbackAsync(IFormCollection form, CancellationToken cancellationToken = default);
}
