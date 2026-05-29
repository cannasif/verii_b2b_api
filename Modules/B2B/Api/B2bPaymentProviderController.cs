using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/payments/providers")]
[Authorize]
public sealed class B2bPaymentProviderController : ControllerBase
{
    private readonly IPaymentProviderLookupService _service;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bPaymentProviderController(IPaymentProviderLookupService service, IB2bPortalAccessService portalAccess)
    {
        _service = service;
        _portalAccess = portalAccess;
    }

    [HttpPost("bin-lookup")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentBinLookupDto>>> LookupBin([FromBody] PaymentBinLookupRequestDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<PaymentBinLookupDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _service.LookupBinAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("installments")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentInstallmentOptionsDto>>> GetInstallments([FromBody] PaymentInstallmentOptionsRequestDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _service.GetInstallmentOptionsAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
