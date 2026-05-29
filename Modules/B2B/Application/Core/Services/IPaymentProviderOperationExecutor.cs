using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IPaymentProviderOperationExecutor
{
    Task<ApiResponse<PaymentProviderOperationDto>> ExecuteAsync(long operationId, string requestIp, CancellationToken cancellationToken = default);
}
