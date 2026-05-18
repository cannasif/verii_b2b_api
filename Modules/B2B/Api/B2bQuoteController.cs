using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/quotes")]
[Authorize]
public sealed class B2bQuoteController : ControllerBase
{
    private readonly IB2bCommercialPolicyService _service;

    public B2bQuoteController(IB2bCommercialPolicyService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<QuoteRequestDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetQuotesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<QuoteRequestDto>>> Create([FromBody] CreateQuoteRequestDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateQuoteAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<ApiResponse<QuoteRequestDto>>> UpdateStatus(long id, [FromBody] UpdateQuoteStatusDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateQuoteStatusAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id:long}/convert-to-cart")]
    public async Task<ActionResult<ApiResponse<CartDto>>> ConvertToCart(long id, [FromBody] ConvertQuoteToCartDto dto, CancellationToken cancellationToken = default)
    {
        dto.QuoteId = id;
        var result = await _service.ConvertQuoteToCartAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
