using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/product-matches")]
[Authorize]
public sealed class B2bProductMatchController : ControllerBase
{
    private readonly IB2bCommerceService _service;

    public B2bProductMatchController(IB2bCommerceService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<CustomerProductAliasDto>>>> GetPaged([FromBody] PagedRequest request, [FromQuery] long? customerId = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAliasesAsync(request, customerId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerProductAliasDto>>> Create([FromBody] CreateCustomerProductAliasDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAliasAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<CustomerProductAliasDto>>> Update(long id, [FromBody] UpdateCustomerProductAliasDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAliasAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
