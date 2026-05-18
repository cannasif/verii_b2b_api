using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/insights")]
[Authorize]
public sealed class B2bInsightController : ControllerBase
{
    private readonly IB2bInsightService _service;

    public B2bInsightController(IB2bInsightService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<B2bInsightSummaryDto>>> GetSummary(CancellationToken cancellationToken = default)
    {
        var result = await _service.GetSummaryAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
