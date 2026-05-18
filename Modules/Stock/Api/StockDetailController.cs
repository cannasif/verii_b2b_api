using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Stock.Dtos;
using Wms.Application.Stock.Services;

namespace Wms.WebApi.Controllers.Stock;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StockDetailController : ControllerBase
{
    private readonly IStockDetailService _service;

    public StockDetailController(IStockDetailService service)
    {
        _service = service;
    }

    [HttpGet("stock/{stockId:long}")]
    public async Task<ActionResult<ApiResponse<StockDetailDto?>>> GetByStockId(long stockId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByStockIdAsync(stockId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StockDetailDto>>> Create([FromBody] CreateStockDetailDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<StockDetailDto>>> Update(long id, [FromBody] UpdateStockDetailDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
