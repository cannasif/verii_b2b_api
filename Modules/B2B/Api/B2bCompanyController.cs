using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/companies")]
[Authorize]
public sealed class B2bCompanyController : ControllerBase
{
    private readonly IB2bAccountService _service;

    public B2bCompanyController(IB2bAccountService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<B2bCompanyDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCompaniesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("public-paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<B2bCompanyDto>>>> GetPublicPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCompaniesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<B2bCompanyDto>>> Create([FromBody] CreateB2bCompanyDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateCompanyAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
