using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/payments")]
[Authorize]
public sealed class B2bPaymentController : ControllerBase
{
    private readonly IB2bCommerceService _service;

    public B2bPaymentController(IB2bCommerceService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentTransactionDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPaymentsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("orders/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentOrderDto>>>> GetPaymentOrders([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPaymentOrdersAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<ApiResponse<PaymentOrderDto>>> CreatePaymentOrder([FromBody] CreatePaymentOrderDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreatePaymentOrderAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("orders/{id:long}/plan")]
    public async Task<ActionResult<ApiResponse<PaymentOrderDto>>> UpdatePaymentOrderPlan(long id, [FromBody] UpdatePaymentOrderPlanDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdatePaymentOrderPlanAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("orders/{id:long}/provider-installment")]
    public async Task<ActionResult<ApiResponse<PaymentOrderDto>>> SelectProviderInstallment(long id, [FromBody] SelectPaymentProviderInstallmentDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.SelectPaymentProviderInstallmentAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> Create([FromBody] CreatePaymentTransactionDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreatePaymentTransactionAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> UpdateStatus(long id, [FromBody] UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdatePaymentStatusAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
