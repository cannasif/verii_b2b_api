using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/catalog-visibility")]
[Authorize]
public sealed class B2bCatalogVisibilityController : ControllerBase
{
    private readonly IB2bAccountService _service;

    public B2bCatalogVisibilityController(IB2bAccountService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogVisibilityRuleDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetVisibilityRulesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CatalogVisibilityRuleDto>>> Create([FromBody] CreateCatalogVisibilityRuleDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateVisibilityRuleAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
