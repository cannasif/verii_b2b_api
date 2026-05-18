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

    public B2bOrderController(IB2bCommerceService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<OrderDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetOrdersAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("from-cart")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateFromCart([FromBody] CreateOrderFromCartDto dto, CancellationToken cancellationToken = default)
    {
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
    public async Task<ActionResult<ApiResponse<CustomerPortalSummaryDto>>> GetPortalSummary(long customerId, [FromQuery] long? userId = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCustomerPortalSummaryAsync(customerId, userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
