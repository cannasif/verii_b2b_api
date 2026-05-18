using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/pricing")]
[Authorize]
public sealed class B2bPricingController : ControllerBase
{
    private readonly IB2bCommercialPolicyService _service;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bPricingController(IB2bCommercialPolicyService service, IB2bPortalAccessService portalAccess)
    {
        _service = service;
        _portalAccess = portalAccess;
    }

    [HttpPost("price-lists/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<CustomerPriceListDto>>>> GetPriceLists([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPriceListsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("price-lists/{id:long}")]
    public async Task<ActionResult<ApiResponse<CustomerPriceListDto>>> GetPriceList(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPriceListAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("price-lists")]
    public async Task<ActionResult<ApiResponse<CustomerPriceListDto>>> CreatePriceList([FromBody] CreateCustomerPriceListDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreatePriceListAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("price-lists/{priceListId:long}/items")]
    public async Task<ActionResult<ApiResponse<CustomerPriceListItemDto>>> UpsertItem(long priceListId, [FromBody] UpsertCustomerPriceListItemDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertPriceListItemAsync(priceListId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("resolve")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<B2bPriceAvailabilityDto>>> Resolve([FromBody] ResolveB2bPriceAvailabilityDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateCustomerAccessAsync(Request, dto.CustomerId, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<B2bPriceAvailabilityDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.ResolvePriceAvailabilityAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
