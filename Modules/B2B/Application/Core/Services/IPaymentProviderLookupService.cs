using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IPaymentProviderLookupService
{
    Task<ApiResponse<PaymentBinLookupDto>> LookupBinAsync(PaymentBinLookupRequestDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentInstallmentOptionsDto>> GetInstallmentOptionsAsync(PaymentInstallmentOptionsRequestDto dto, CancellationToken cancellationToken = default);
}
