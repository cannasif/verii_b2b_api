using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/payments/paytr")]
public sealed class B2bPaytrPaymentController : ControllerBase
{
    private readonly IPaytrPaymentService _paytrPaymentService;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bPaytrPaymentController(IPaytrPaymentService paytrPaymentService, IB2bPortalAccessService portalAccess)
    {
        _paytrPaymentService = paytrPaymentService;
        _portalAccess = portalAccess;
    }

    [HttpPost("iframe-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaytrIframeTokenDto>>> CreateIframeToken([FromBody] CreatePaytrIframeTokenDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidateOrderAccessAsync(Request, dto.OrderId, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<PaytrIframeTokenDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _paytrPaymentService.CreateIframeTokenAsync(dto, ResolveRequestIp(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("callback")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Callback(CancellationToken cancellationToken = default)
    {
        var result = await _paytrPaymentService.HandleCallbackAsync(Request.Form, cancellationToken);
        return result.Success ? Content("OK", "text/plain") : StatusCode(result.StatusCode, result.Message);
    }

    private string ResolveRequestIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}
