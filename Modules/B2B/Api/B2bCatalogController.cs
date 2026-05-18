using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/catalog")]
[Authorize]
public sealed class B2bCatalogController : ControllerBase
{
    private readonly IB2bCommerceService _service;
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bCatalogController(IB2bCommerceService service, IB2bPortalAccessService portalAccess)
    {
        _service = service;
        _portalAccess = portalAccess;
    }

    [HttpPost("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogProductDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<PagedResponse<CatalogProductDto>>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.GetCatalogProductsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("public-paged")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogProductDto>>>> GetPublicPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPublicCatalogProductsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CatalogProductDto>>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<CatalogProductDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.GetCatalogProductAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CatalogProductDto>>> Create([FromBody] CreateCatalogProductDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateCatalogProductAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<CatalogProductDto>>> Update(long id, [FromBody] UpdateCatalogProductDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateCatalogProductAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{productId:long}/variants")]
    public async Task<ActionResult<ApiResponse<CatalogVariantDto>>> UpsertVariant(long productId, [FromBody] UpsertCatalogVariantDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertVariantAsync(productId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
