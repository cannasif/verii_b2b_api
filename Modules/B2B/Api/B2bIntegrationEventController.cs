using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/integration-events")]
[Authorize]
public sealed class B2bIntegrationEventController : ControllerBase
{
    private readonly IB2bCommercialPolicyService _service;

    public B2bIntegrationEventController(IB2bCommercialPolicyService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<B2bIntegrationEventDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetIntegrationEventsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
