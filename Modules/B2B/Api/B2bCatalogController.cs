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

    [HttpPost("categories/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogCategoryDto>>>> GetCategories([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCatalogCategoriesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<CatalogCategoryDto>>> CreateCategory([FromBody] CreateCatalogCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateCatalogCategoryAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("categories/{id:long}")]
    public async Task<ActionResult<ApiResponse<CatalogCategoryDto>>> UpdateCategory(long id, [FromBody] UpdateCatalogCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateCatalogCategoryAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("favorites/paged")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogProductFavoriteDto>>>> GetProductFavorites(
        [FromBody] PagedRequest request,
        [FromQuery] long companyId,
        [FromQuery] long? buyerId,
        [FromQuery] long? userId,
        CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<PagedResponse<CatalogProductFavoriteDto>>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.GetCatalogProductFavoritesAsync(request, companyId, buyerId, userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("favorites/toggle")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CatalogFavoriteToggleResultDto>>> ToggleProductFavorite([FromBody] ToggleCatalogProductFavoriteDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<CatalogFavoriteToggleResultDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.ToggleCatalogProductFavoriteAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("category-favorites/paged")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogCategoryFavoriteDto>>>> GetCategoryFavorites(
        [FromBody] PagedRequest request,
        [FromQuery] long companyId,
        [FromQuery] long? buyerId,
        [FromQuery] long? userId,
        CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<PagedResponse<CatalogCategoryFavoriteDto>>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.GetCatalogCategoryFavoritesAsync(request, companyId, buyerId, userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("category-favorites/toggle")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CatalogFavoriteToggleResultDto>>> ToggleCategoryFavorite([FromBody] ToggleCatalogCategoryFavoriteDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _portalAccess.ValidateRequestAsync(Request, cancellationToken);
        if (!validation.Success)
        {
            return StatusCode(validation.StatusCode, ApiResponse<CatalogFavoriteToggleResultDto>.ErrorResult(validation.Message, validation.ExceptionMessage, validation.StatusCode));
        }

        var result = await _service.ToggleCatalogCategoryFavoriteAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{productId:long}/categories")]
    public async Task<ActionResult<ApiResponse<CatalogProductCategoryDto>>> AssignCategory(long productId, [FromBody] AssignCatalogProductCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.AssignCatalogProductCategoryAsync(productId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("attributes/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<CatalogAttributeDefinitionDto>>>> GetAttributeDefinitions([FromBody] PagedRequest request, [FromQuery] long? categoryId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCatalogAttributeDefinitionsAsync(request, categoryId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("attributes")]
    public async Task<ActionResult<ApiResponse<CatalogAttributeDefinitionDto>>> CreateAttributeDefinition([FromBody] CreateCatalogAttributeDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateCatalogAttributeDefinitionAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{productId:long}/attributes")]
    public async Task<ActionResult<ApiResponse<CatalogProductAttributeDto>>> UpsertProductAttribute(long productId, [FromBody] UpsertCatalogProductAttributeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertCatalogProductAttributeAsync(productId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{productId:long}/media")]
    public async Task<ActionResult<ApiResponse<CatalogProductMediaDto>>> UpsertProductMedia(long productId, [FromBody] UpsertCatalogProductMediaDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertCatalogProductMediaAsync(productId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{productId:long}/media/upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<List<CatalogProductMediaDto>>>> UploadProductMedia(long productId, [FromForm] UploadCatalogProductMediaDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UploadCatalogProductMediaAsync(productId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{productId:long}/documents")]
    public async Task<ActionResult<ApiResponse<CatalogProductDocumentDto>>> UpsertProductDocument(long productId, [FromBody] UpsertCatalogProductDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertCatalogProductDocumentAsync(productId, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
