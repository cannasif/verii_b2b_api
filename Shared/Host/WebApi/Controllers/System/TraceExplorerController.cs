using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.System.Dtos;
using Wms.Application.System.Services;

namespace Wms.WebApi.Controllers;

[ApiController]
[Route("api/trace-explorer")]
[Authorize]
public sealed class TraceExplorerController : ControllerBase
{
    private readonly ITraceExplorerService _service;

    public TraceExplorerController(ITraceExplorerService service)
    {
        _service = service;
    }

    [HttpGet("{traceId}")]
    public async Task<ActionResult<ApiResponse<TraceExplorerResponseDto>>> GetByTraceId([FromRoute] string traceId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByTraceIdAsync(traceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
