using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/buyers")]
[Authorize]
public sealed class B2bBuyerController : ControllerBase
{
    private readonly IB2bAccountService _service;

    public B2bBuyerController(IB2bAccountService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<B2bBuyerDto>>>> GetPaged([FromBody] PagedRequest request, [FromQuery] long? companyId = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetBuyersAsync(request, companyId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<B2bBuyerDto>>> Create([FromBody] CreateB2bBuyerDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateBuyerAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
