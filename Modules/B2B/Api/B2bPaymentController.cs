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
    private readonly IB2bPortalAccessService _portalAccess;

    public B2bPaymentController(IB2bCommerceService service, IB2bPortalAccessService portalAccess)
    {
        _service = service;
        _portalAccess = portalAccess;
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
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentOrderDto>>> CreatePaymentOrder([FromBody] CreatePaymentOrderDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidateOrderAccessAsync(Request, dto.OrderId, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<PaymentOrderDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _service.CreatePaymentOrderAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("orders/{id:long}/plan")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentOrderDto>>> UpdatePaymentOrderPlan(long id, [FromBody] UpdatePaymentOrderPlanDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidatePaymentOrderAccessAsync(Request, id, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<PaymentOrderDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _service.UpdatePaymentOrderPlanAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("orders/{id:long}/provider-installment")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaymentOrderDto>>> SelectProviderInstallment(long id, [FromBody] SelectPaymentProviderInstallmentDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidatePaymentOrderAccessAsync(Request, id, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<PaymentOrderDto>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _service.SelectPaymentProviderInstallmentAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("method-rules/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentMethodRuleDto>>>> GetPaymentMethodRules([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPaymentMethodRulesAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("method-rules")]
    public async Task<ActionResult<ApiResponse<PaymentMethodRuleDto>>> CreatePaymentMethodRule([FromBody] CreatePaymentMethodRuleDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreatePaymentMethodRuleAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("methods/resolve")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<PaymentMethodOptionDto>>>> ResolvePaymentMethods([FromBody] ResolvePaymentMethodsDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _portalAccess.ValidateCustomerContextAsync(Request, dto.CustomerId, cancellationToken);
        if (!access.Success)
        {
            return StatusCode(access.StatusCode, ApiResponse<List<PaymentMethodOptionDto>>.ErrorResult(access.Message, access.ExceptionMessage, access.StatusCode));
        }

        var result = await _service.ResolvePaymentMethodsAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("operations/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentProviderOperationDto>>>> GetPaymentProviderOperations([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPaymentProviderOperationsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("operations")]
    public async Task<ActionResult<ApiResponse<PaymentProviderOperationDto>>> CreatePaymentProviderOperation([FromBody] CreatePaymentProviderOperationDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreatePaymentProviderOperationAsync(dto, cancellationToken);
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
