using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/cart")]
[Authorize]
public sealed class B2bCartController : ControllerBase
{
    private readonly IB2bCommerceService _service;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bCartController(IB2bCommerceService service, IB2bPortalAccessService portalAccess)
    {
        _service = service;
        _portalAccess = portalAccess;
    }

    [HttpGet("draft/{customerId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetDraft(long customerId, [FromQuery] long? userId = null, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateCustomerContextAsync(Request, customerId, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<CartDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var context = validation.Data!;
        var scopedUserId = context.IsBackoffice || context.CanViewCompanyHistory ? userId : context.UserId;
        var scopedBuyerId = context.IsBackoffice || context.CanViewCompanyHistory ? null : context.BuyerId;
        var result = await _service.GetDraftCartAsync(customerId, scopedUserId, scopedBuyerId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("lines")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddLine([FromBody] AddCartLineDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateCustomerContextAsync(Request, dto.CustomerId, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<CartDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        ApplyPortalScope(dto, validation.Data!);
        var result = await _service.AddCartLineAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("quick-order")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<QuickOrderResultDto>>> QuickOrder([FromBody] QuickOrderDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateCustomerContextAsync(Request, dto.CustomerId, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<QuickOrderResultDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        ApplyPortalScope(dto, validation.Data!);
        var result = await _service.AddQuickOrderLinesAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("lines/{lineId:long}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateLine(long lineId, [FromBody] UpdateCartLineDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateCartLineAsync(lineId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("lines/{lineId:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveLine(long lineId, CancellationToken cancellationToken = default)
    {
        var result = await _service.RemoveCartLineAsync(lineId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private static void ApplyPortalScope(AddCartLineDto dto, B2bPortalContextDto context)
    {
        if (context.IsBackoffice || context.CanViewCompanyHistory)
        {
            return;
        }

        dto.BuyerId = context.BuyerId;
        dto.UserId = context.UserId;
        dto.CustomerGroupCode ??= context.CustomerGroupCode;
    }

    private static void ApplyPortalScope(QuickOrderDto dto, B2bPortalContextDto context)
    {
        if (context.IsBackoffice || context.CanViewCompanyHistory)
        {
            return;
        }

        dto.BuyerId = context.BuyerId;
        dto.UserId = context.UserId;
        dto.CustomerGroupCode ??= context.CustomerGroupCode;
    }
}
