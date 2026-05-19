using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/orders")]
[Authorize]
public sealed class B2bOrderController : ControllerBase
{
    private readonly IB2bCommerceService _service;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bOrderController(IB2bCommerceService service, IB2bPortalAccessService portalAccess)
    {
        _service = service;
        _portalAccess = portalAccess;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<OrderDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetOrdersAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetOrderAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("from-cart")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateFromCart([FromBody] CreateOrderFromCartDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateCartAccessAsync(Request, dto.CartId, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<OrderDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.CreateOrderFromCartAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("reorder")]
    public async Task<ActionResult<ApiResponse<QuickOrderResultDto>>> Reorder([FromBody] ReorderDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.ReorderAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("portal/{customerId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CustomerPortalSummaryDto>>> GetPortalSummary(long customerId, [FromQuery] long? userId = null, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateCustomerContextAsync(Request, customerId, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<CustomerPortalSummaryDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var context = validation.Data!;
        var scopedUserId = context.IsBackoffice || context.CanViewCompanyHistory ? userId : context.UserId;
        var scopedBuyerId = context.IsBackoffice || context.CanViewCompanyHistory ? null : context.BuyerId;
        var result = await _service.GetCustomerPortalSummaryAsync(customerId, scopedUserId, scopedBuyerId, context.IsBackoffice || context.CanViewCompanyHistory, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
