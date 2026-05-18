using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Dtos;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Modules.NetsisIntegrations.Api;

[ApiController]
[Route("api/netsis-read")]
[Route("api/Erp")]
public sealed class NetsisReadController : ControllerBase
{
    private readonly INetsisReadService _service;

    public NetsisReadController(INetsisReadService service)
    {
        _service = service;
    }

    [HttpGet("getBranches")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetBranches(
        [FromQuery] int? branchNo = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetBranchesAsync(branchNo, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("getExchangeRate")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<KurDto>>>> GetExchangeRate(
        [FromQuery] DateTime? tarih = null,
        [FromQuery] int fiyatTipi = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetExchangeRatesAsync(tarih ?? DateTime.Today, fiyatTipi, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
