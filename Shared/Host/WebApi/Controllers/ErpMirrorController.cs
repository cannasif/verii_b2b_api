using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Customer.Dtos;
using Wms.Application.Customer.Services;
using Wms.Application.Stock.Dtos;
using Wms.Application.Stock.Services;
using Wms.Application.Warehouse.Dtos;
using Wms.Application.Warehouse.Services;
using Wms.Application.YapKod.Dtos;
using Wms.Application.YapKod.Services;

namespace Wms.WebApi.Controllers.System;

[ApiController]
[Route("api/erp-mirror")]
[Authorize]
public sealed class ErpMirrorController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IStockService _stockService;
    private readonly IWarehouseService _warehouseService;
    private readonly IYapKodService _yapKodService;

    public ErpMirrorController(
        ICustomerService customerService,
        IStockService stockService,
        IWarehouseService warehouseService,
        IYapKodService yapKodService)
    {
        _customerService = customerService;
        _stockService = stockService;
        _warehouseService = warehouseService;
        _yapKodService = yapKodService;
    }

    [HttpPost("customers/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<CustomerDto>>>> GetCustomers([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetPagedAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("customers/{id:long}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomer(long id, CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("stocks/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<StockDto>>>> GetStocks([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _stockService.GetPagedAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("stocks/{id:long}")]
    public async Task<ActionResult<ApiResponse<StockDto>>> GetStock(long id, CancellationToken cancellationToken = default)
    {
        var result = await _stockService.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("warehouses/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<WarehouseDto>>>> GetWarehouses([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.GetPagedAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("warehouses/{id:long}")]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> GetWarehouse(long id, CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("yapkod/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<YapKodDto>>>> GetYapKodlar([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _yapKodService.GetPagedAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("yapkod/{id:long}")]
    public async Task<ActionResult<ApiResponse<YapKodDto>>> GetYapKod(long id, CancellationToken cancellationToken = default)
    {
        var result = await _yapKodService.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
