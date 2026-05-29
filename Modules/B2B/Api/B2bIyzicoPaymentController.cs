using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/payments/iyzico")]
public sealed class B2bIyzicoPaymentController : ControllerBase
{
    private readonly IIyzicoPaymentService _iyzicoPaymentService;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bIyzicoPaymentController(IIyzicoPaymentService iyzicoPaymentService, IB2bPortalAccessService portalAccess)
    {
        _iyzicoPaymentService = iyzicoPaymentService;
        _portalAccess = portalAccess;
    }

    [HttpPost("3ds/initialize")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Iyzico3dsInitializeDto>>> Initialize3ds([FromBody] CreateIyzico3dsPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidateOrderAccessAsync(Request, dto.OrderId, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<Iyzico3dsInitializeDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _iyzicoPaymentService.Initialize3dsAsync(dto, ResolveRequestIp(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("3ds/callback")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Callback(CancellationToken cancellationToken = default)
    {
        var result = await _iyzicoPaymentService.Handle3dsCallbackAsync(Request.Form, cancellationToken);
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
