using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/inventory")]
[Authorize]
public sealed class B2bInventoryController : ControllerBase
{
    private readonly IB2bCommercialPolicyService _service;

    public B2bInventoryController(IB2bCommercialPolicyService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<InventorySnapshotDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetInventoryAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InventorySnapshotDto>>> Upsert([FromBody] UpsertInventorySnapshotDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertInventoryAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
