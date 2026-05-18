using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Stock.Dtos;
using Wms.Application.Stock.Services;

namespace Wms.WebApi.Controllers.Stock;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StockImageController : ControllerBase
{
    private readonly IStockImageService _service;

    public StockImageController(IStockImageService service)
    {
        _service = service;
    }

    [HttpGet("by-stock/{stockId:long}")]
    public async Task<ActionResult<ApiResponse<List<StockImageDto>>>> GetByStockId(long stockId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByStockIdAsync(stockId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("upload/{stockId:long}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<List<StockImageDto>>>> Upload(
        long stockId,
        [FromForm] List<IFormFile> files,
        [FromForm] List<string>? altTexts,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UploadImagesAsync(stockId, files, altTexts, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("set-primary/{id:long}")]
    public async Task<ActionResult<ApiResponse<StockImageDto>>> SetPrimary(long id, CancellationToken cancellationToken = default)
    {
        var result = await _service.SetPrimaryImageAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
