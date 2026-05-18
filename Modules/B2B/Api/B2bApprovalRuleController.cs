using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/approval-rules")]
[Authorize]
public sealed class B2bApprovalRuleController : ControllerBase
{
    private readonly IB2bAccountService _service;

    public B2bApprovalRuleController(IB2bAccountService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseApprovalRuleDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetApprovalRulesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseApprovalRuleDto>>> Create([FromBody] CreatePurchaseApprovalRuleDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateApprovalRuleAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
