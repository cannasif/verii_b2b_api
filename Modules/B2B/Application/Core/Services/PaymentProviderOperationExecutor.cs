using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class PaymentProviderOperationExecutor : IPaymentProviderOperationExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly PaytrOptions _paytrOptions;
    private readonly IyzicoOptions _iyzicoOptions;
    private readonly IRepository<PaymentProviderOperation> _operations;
    private readonly IRepository<PaymentOrder> _paymentOrders;
    private readonly IRepository<B2bIntegrationEvent> _integrationEvents;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentProviderOperationExecutor(
        HttpClient httpClient,
        IOptions<PaytrOptions> paytrOptions,
        IOptions<IyzicoOptions> iyzicoOptions,
        IRepository<PaymentProviderOperation> operations,
        IRepository<PaymentOrder> paymentOrders,
        IRepository<B2bIntegrationEvent> integrationEvents,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient;
        _paytrOptions = paytrOptions.Value;
        _iyzicoOptions = iyzicoOptions.Value;
        _operations = operations;
        _paymentOrders = paymentOrders;
        _integrationEvents = integrationEvents;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaymentProviderOperationDto>> ExecuteAsync(long operationId, string requestIp, CancellationToken cancellationToken = default)
    {
        var operation = await _operations.Query(tracking: true)
            .Include(x => x.PaymentTransaction)
            .FirstOrDefaultAsync(x => x.Id == operationId && !x.IsDeleted, cancellationToken);
        if (operation?.PaymentTransaction is null)
        {
            return ApiResponse<PaymentProviderOperationDto>.ErrorResult("Ödeme operasyonu bulunamadı.", statusCode: 404);
        }

        if (!string.Equals(operation.Status, B2bWorkflowStatuses.Pending, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(operation.Status, B2bWorkflowStatuses.Failed, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<PaymentProviderOperationDto>.ErrorResult("Sadece bekleyen veya hatalı ödeme operasyonu gönderilebilir.", statusCode: 400);
        }

        operation.Status = B2bWorkflowStatuses.Processing;
        operation.ErrorMessage = null;
        operation.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        ApiResponse<ProviderOperationResult> providerResult = operation.ProviderKey switch
        {
            "PAYTR" => await ExecutePaytrAsync(operation, cancellationToken),
            "IYZICO" or "IYZIPAY" => await ExecuteIyzicoAsync(operation, requestIp, cancellationToken),
            _ => ApiResponse<ProviderOperationResult>.ErrorResult($"Desteklenmeyen ödeme sağlayıcı: {operation.ProviderKey}", statusCode: 400)
        };

        if (!providerResult.Success || providerResult.Data is null)
        {
            operation.Status = B2bWorkflowStatuses.Failed;
            operation.ErrorMessage = providerResult.ExceptionMessage ?? providerResult.Message;
            operation.ResponseJson = providerResult.ExceptionMessage;
            operation.ProcessedDate = DateTimeProvider.Now;
            operation.SetUpdatedInfo();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<PaymentProviderOperationDto>.ErrorResult(providerResult.Message, providerResult.ExceptionMessage, providerResult.StatusCode);
        }

        operation.Status = B2bWorkflowStatuses.Completed;
        operation.ExternalOperationId = providerResult.Data.ExternalOperationId ?? operation.ExternalOperationId;
        operation.RequestJson = providerResult.Data.RequestJson;
        operation.ResponseJson = providerResult.Data.ResponseJson;
        operation.ProcessedDate = DateTimeProvider.Now;
        operation.SetUpdatedInfo();
        await ApplyOperationLedgerAsync(operation, cancellationToken);
        await UpsertIntegrationEventAsync(operation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<PaymentProviderOperationDto>.SuccessResult(Map(operation), "Ödeme operasyonu sağlayıcıya gönderildi.");
    }

    private async Task<ApiResponse<ProviderOperationResult>> ExecutePaytrAsync(PaymentProviderOperation operation, CancellationToken cancellationToken)
    {
        if (operation.OperationType is not ("REFUND" or "CANCEL"))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("PayTR için bu aşamada iade/iptal operasyonu destekleniyor.", statusCode: 400);
        }

        if (string.IsNullOrWhiteSpace(_paytrOptions.MerchantId) || string.IsNullOrWhiteSpace(_paytrOptions.MerchantKey) || string.IsNullOrWhiteSpace(_paytrOptions.MerchantSalt))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("PayTR iade ayarları eksik.", statusCode: 400);
        }

        var merchantOid = operation.PaymentTransaction!.ExternalTransactionId;
        if (string.IsNullOrWhiteSpace(merchantOid))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("PayTR merchant_oid bulunamadı.", statusCode: 400);
        }

        var amount = ToInvariant(operation.Amount);
        var token = PaytrHmacBase64(_paytrOptions.MerchantId + merchantOid + amount + _paytrOptions.MerchantSalt);
        var form = new Dictionary<string, string>
        {
            ["merchant_id"] = _paytrOptions.MerchantId,
            ["merchant_oid"] = merchantOid,
            ["return_amount"] = amount,
            ["paytr_token"] = token
        };

        var requestJson = JsonSerializer.Serialize(form, JsonOptions);
        using var response = await _httpClient.PostAsync(_paytrOptions.RefundUrl, new FormUrlEncodedContent(form), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("PayTR iade isteği başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        var status = GetString(json.RootElement, "status");
        if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("PayTR iade kabul edilmedi.", responseBody, statusCode: 400);
        }

        return ApiResponse<ProviderOperationResult>.SuccessResult(new ProviderOperationResult(requestJson, responseBody, merchantOid), "PayTR iade tamamlandı.");
    }

    private async Task<ApiResponse<ProviderOperationResult>> ExecuteIyzicoAsync(PaymentProviderOperation operation, string requestIp, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_iyzicoOptions.ApiKey) || string.IsNullOrWhiteSpace(_iyzicoOptions.SecretKey))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("iyzico ayarları eksik.", statusCode: 400);
        }

        var paymentId = operation.PaymentTransaction!.ExternalTransactionId;
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("iyzico paymentId bulunamadı.", statusCode: 400);
        }

        object payload;
        string path;
        if (operation.OperationType == "CANCEL")
        {
            path = _iyzicoOptions.CancelPath;
            payload = new
            {
                locale = _iyzicoOptions.Locale,
                conversationId = operation.IdempotencyKey,
                paymentId,
                ip = requestIp
            };
        }
        else if (operation.OperationType == "REFUND")
        {
            path = _iyzicoOptions.RefundPath;
            var usePaymentLevelRefund = path.Contains("/v2/", StringComparison.OrdinalIgnoreCase);
            var providerTransactionId = ResolveIyzicoPaymentTransactionId(operation.PaymentTransaction.CallbackPayloadJson);
            var refundPayload = new Dictionary<string, object?>
            {
                ["locale"] = _iyzicoOptions.Locale,
                ["conversationId"] = operation.IdempotencyKey,
                ["price"] = ToInvariant(operation.Amount),
                ["ip"] = requestIp,
                ["currency"] = NormalizeCurrency(operation.CurrencyCode)
            };
            refundPayload[usePaymentLevelRefund ? "paymentId" : "paymentTransactionId"] = usePaymentLevelRefund ? paymentId : providerTransactionId;
            payload = refundPayload;

            if (!usePaymentLevelRefund && string.IsNullOrWhiteSpace(providerTransactionId))
            {
                return ApiResponse<ProviderOperationResult>.ErrorResult("iyzico kalem bazlı iade için paymentTransactionId bulunamadı. RefundPath /v2/payment/refund olarak kullanılmalı veya callback snapshot içinde paymentTransactionId saklanmalı.", statusCode: 400);
            }
        }
        else
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("iyzico için bu aşamada iade/iptal operasyonu destekleniyor.", statusCode: 400);
        }

        var body = JsonSerializer.Serialize(payload, JsonOptions);
        using var request = CreateIyzicoRequest(path, body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("iyzico operasyon isteği başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        var status = GetString(json.RootElement, "status");
        if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<ProviderOperationResult>.ErrorResult("iyzico operasyonu kabul edilmedi.", responseBody, statusCode: 400);
        }

        return ApiResponse<ProviderOperationResult>.SuccessResult(new ProviderOperationResult(body, responseBody, GetString(json.RootElement, "paymentId") ?? paymentId), "iyzico operasyonu tamamlandı.");
    }

    private async Task ApplyOperationLedgerAsync(PaymentProviderOperation operation, CancellationToken cancellationToken)
    {
        if (operation.OperationType is not ("REFUND" or "CANCEL") || !operation.PaymentOrderId.HasValue)
        {
            return;
        }

        var paymentOrder = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == operation.PaymentOrderId.Value && !x.IsDeleted, cancellationToken);
        if (paymentOrder is null)
        {
            return;
        }

        var amount = operation.OperationType == "CANCEL" ? paymentOrder.PaidAmount : Math.Min(operation.Amount, paymentOrder.PaidAmount);
        paymentOrder.PaidAmount = Math.Max(0, paymentOrder.PaidAmount - amount);
        paymentOrder.RemainingAmount = Math.Max(0, paymentOrder.Amount - paymentOrder.PaidAmount);
        paymentOrder.Status = operation.OperationType == "CANCEL"
            ? B2bWorkflowStatuses.Cancelled
            : paymentOrder.PaidAmount <= 0 ? B2bWorkflowStatuses.Pending : B2bWorkflowStatuses.Processing;
        paymentOrder.SetUpdatedInfo();

        var remaining = amount;
        foreach (var installment in paymentOrder.Installments.Where(x => !x.IsDeleted).OrderByDescending(x => x.InstallmentNumber))
        {
            if (remaining <= 0) break;
            var refund = Math.Min(installment.PaidAmount, remaining);
            installment.PaidAmount -= refund;
            installment.Status = installment.PaidAmount <= 0 ? B2bWorkflowStatuses.Pending : B2bWorkflowStatuses.Processing;
            installment.SetUpdatedInfo();
            remaining -= refund;
        }
    }

    private async Task UpsertIntegrationEventAsync(PaymentProviderOperation operation, CancellationToken cancellationToken)
    {
        await _integrationEvents.AddAsync(new B2bIntegrationEvent
        {
            Direction = "Outbound",
            EventType = $"Payment{operation.OperationType.Replace("_", string.Empty, StringComparison.Ordinal)}Completed",
            EntityName = nameof(PaymentProviderOperation),
            EntityId = operation.Id,
            Status = B2bWorkflowStatuses.Pending,
            ExternalReference = operation.ExternalOperationId,
            PayloadJson = JsonSerializer.Serialize(new
            {
                operation.Id,
                operation.PaymentTransactionId,
                operation.PaymentOrderId,
                operation.ProviderKey,
                operation.OperationType,
                operation.Amount,
                operation.CurrencyCode,
                operation.ExternalOperationId
            }, JsonOptions),
            CreatedDate = DateTimeProvider.Now
        }, cancellationToken);
    }

    private HttpRequestMessage CreateIyzicoRequest(string path, string body)
    {
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var randomKey = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Guid.NewGuid():N}";
        var signature = CreateHmacSha256(_iyzicoOptions.SecretKey, randomKey + normalizedPath + body);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_iyzicoOptions.BaseUrl.TrimEnd('/')}{normalizedPath}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"IYZWSv2 apiKey:{_iyzicoOptions.ApiKey}&randomKey:{randomKey}&signature:{signature}");
        request.Headers.TryAddWithoutValidation("x-iyzi-rnd", randomKey);
        return request;
    }

    private static string? ResolveIyzicoPaymentTransactionId(string? callbackPayloadJson)
    {
        if (string.IsNullOrWhiteSpace(callbackPayloadJson))
        {
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(callbackPayloadJson);
            var root = json.RootElement;
            if (root.TryGetProperty("itemTransactions", out var itemTransactions) && itemTransactions.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemTransactions.EnumerateArray())
                {
                    var value = GetString(item, "paymentTransactionId");
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return GetString(root, "paymentTransactionId");
        }
        catch
        {
            return null;
        }
    }

    private string PaytrHmacBase64(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_paytrOptions.MerchantKey));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static PaymentProviderOperationDto Map(PaymentProviderOperation entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        PaymentTransactionId = entity.PaymentTransactionId,
        PaymentOrderId = entity.PaymentOrderId,
        PaymentInstallmentId = entity.PaymentInstallmentId,
        ProviderKey = entity.ProviderKey,
        OperationType = entity.OperationType,
        Status = entity.Status,
        Amount = entity.Amount,
        CurrencyCode = entity.CurrencyCode,
        ExternalOperationId = entity.ExternalOperationId,
        IdempotencyKey = entity.IdempotencyKey,
        Reason = entity.Reason,
        ErrorMessage = entity.ErrorMessage,
        RequestedDate = entity.RequestedDate,
        ProcessedDate = entity.ProcessedDate
    };

    private static string CreateHmacSha256(string secret, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static string ToInvariant(decimal value) => Math.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);
    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static string? GetString(JsonElement root, string propertyName) => root.TryGetProperty(propertyName, out var element) && element.ValueKind != JsonValueKind.Null ? element.ToString() : null;

    private sealed record ProviderOperationResult(string RequestJson, string ResponseJson, string? ExternalOperationId);
}
