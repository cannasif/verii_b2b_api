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

    public B2bPaymentProviderController(IPaymentProviderLookupService service)
    {
        _service = service;
    }

    [HttpPost("bin-lookup")]
    public async Task<ActionResult<ApiResponse<PaymentBinLookupDto>>> LookupBin([FromBody] PaymentBinLookupRequestDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.LookupBinAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("installments")]
    public async Task<ActionResult<ApiResponse<PaymentInstallmentOptionsDto>>> GetInstallments([FromBody] PaymentInstallmentOptionsRequestDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetInstallmentOptionsAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
