using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;
using CustomerEntity = Wms.Domain.Entities.Customer.Customer;

namespace Wms.Application.B2B.Services;

public sealed class PaytrPaymentService : IPaytrPaymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly PaytrOptions _options;
    private readonly IRepository<B2bOrder> _orders;
    private readonly IRepository<CustomerEntity> _customers;
    private readonly IRepository<PaymentOrder> _paymentOrders;
    private readonly IRepository<PaymentTransaction> _payments;
    private readonly IUnitOfWork _unitOfWork;

    public PaytrPaymentService(
        HttpClient httpClient,
        IOptions<PaytrOptions> options,
        IRepository<B2bOrder> orders,
        IRepository<CustomerEntity> customers,
        IRepository<PaymentOrder> paymentOrders,
        IRepository<PaymentTransaction> payments,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _orders = orders;
        _customers = customers;
        _paymentOrders = paymentOrders;
        _payments = payments;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaytrIframeTokenDto>> CreateIframeTokenAsync(CreatePaytrIframeTokenDto dto, string requestIp, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return ApiResponse<PaytrIframeTokenDto>.ErrorResult("PayTR ayarları eksik. MerchantId, MerchantKey ve MerchantSalt tanımlanmalı.", statusCode: 400);
        }

        var order = await _orders.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.OrderId, cancellationToken);
        if (order is null)
        {
            return ApiResponse<PaytrIframeTokenDto>.ErrorResult("Sipariş bulunamadı.", statusCode: 404);
        }

        if (order.GrandTotal <= 0)
        {
            return ApiResponse<PaytrIframeTokenDto>.ErrorResult("Ödeme alınacak sipariş tutarı geçersiz.", statusCode: 400);
        }

        var merchantOid = CreateMerchantOid(order.Id);
        var paymentOrder = await GetOrCreatePaymentOrderAsync(order, "PAYTR", "PayTR iFrame", cancellationToken);
        var paymentAmount = paymentOrder.ProviderTotalPrice ?? paymentOrder.RemainingAmount;
        var amountInKurus = ToMinorUnit(paymentAmount);
        var currency = ToPaytrCurrency(order.CurrencyCode);
        var userIp = Trim(dto.UserIp) ?? requestIp;
        var userBasket = CreateBasket(order);
        var firstOpenInstallment = paymentOrder.Installments
            .Where(x => !x.IsDeleted && x.Status != B2bWorkflowStatuses.Completed)
            .OrderBy(x => x.InstallmentNumber)
            .FirstOrDefault();
        var okUrl = Trim(dto.OkUrl) ?? _options.OkUrl;
        var failUrl = Trim(dto.FailUrl) ?? _options.FailUrl;
        if (string.IsNullOrWhiteSpace(okUrl) || string.IsNullOrWhiteSpace(failUrl))
        {
            return ApiResponse<PaytrIframeTokenDto>.ErrorResult("PayTR dönüş URL ayarları eksik.", statusCode: 400);
        }

        var token = CreateRequestToken(userIp, merchantOid, dto.Email, amountInKurus, userBasket, currency);
        var form = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["user_ip"] = userIp,
            ["merchant_oid"] = merchantOid,
            ["email"] = dto.Email.Trim(),
            ["payment_amount"] = amountInKurus,
            ["paytr_token"] = token,
            ["user_basket"] = userBasket,
            ["debug_on"] = _options.DebugOn ? "1" : "0",
            ["no_installment"] = _options.NoInstallment ? "1" : "0",
            ["max_installment"] = _options.MaxInstallment.ToString(CultureInfo.InvariantCulture),
            ["user_name"] = dto.UserName.Trim(),
            ["user_address"] = dto.UserAddress.Trim(),
            ["user_phone"] = dto.UserPhone.Trim(),
            ["merchant_ok_url"] = okUrl,
            ["merchant_fail_url"] = failUrl,
            ["timeout_limit"] = _options.TimeoutLimit.ToString(CultureInfo.InvariantCulture),
            ["currency"] = currency,
            ["test_mode"] = _options.TestMode ? "1" : "0",
            ["lang"] = string.IsNullOrWhiteSpace(_options.Lang) ? "tr" : _options.Lang.Trim()
        };
        if (_options.IframeV2) form["iframe_v2"] = "1";
        if (_options.IframeV2Dark) form["iframe_v2_dark"] = "1";

        using var response = await _httpClient.PostAsync(_options.TokenUrl, new FormUrlEncodedContent(form), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<PaytrIframeTokenDto>.ErrorResult("PayTR token isteği başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;
        var status = root.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
        if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            var reason = root.TryGetProperty("reason", out var reasonElement) ? reasonElement.GetString() : responseBody;
            return ApiResponse<PaytrIframeTokenDto>.ErrorResult("PayTR token üretilemedi.", reason, statusCode: 400);
        }

        var iframeToken = root.GetProperty("token").GetString() ?? string.Empty;
        var payment = new PaymentTransaction
        {
            OrderId = order.Id,
            PaymentOrderId = paymentOrder.Id,
            PaymentInstallmentId = firstOpenInstallment?.Id,
            ProviderKey = "PAYTR",
            ExternalTransactionId = merchantOid,
            Status = B2bWorkflowStatuses.Pending,
            Amount = paymentAmount,
            ProviderPaymentAmount = paymentAmount,
            CurrencyCode = order.CurrencyCode,
            PaymentMethod = "PayTR iFrame",
            ProviderConversationId = paymentOrder.ProviderConversationId,
            BinNumber = paymentOrder.BinNumber,
            CardType = paymentOrder.CardType,
            CardAssociation = paymentOrder.CardAssociation,
            CardFamily = paymentOrder.CardFamily,
            BankName = paymentOrder.BankName,
            BankCode = paymentOrder.BankCode,
            IsCommercialCard = paymentOrder.IsCommercialCard,
            ProviderRate = paymentOrder.ProviderRate,
            ProviderCommissionAmount = paymentOrder.ProviderCommissionAmount,
            DueDate = firstOpenInstallment?.DueDate ?? paymentOrder.DueDate,
            PaymentTermDays = paymentOrder.PaymentTermDays,
            InstallmentCount = paymentOrder.InstallmentCount,
            InstallmentPlanJson = BuildInstallmentPlanJson(paymentOrder.Installments),
            RequestedDate = DateTimeProvider.Now
        };
        await _payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<PaytrIframeTokenDto>.SuccessResult(new PaytrIframeTokenDto
        {
            PaymentTransactionId = payment.Id,
            OrderId = order.Id,
            MerchantOid = merchantOid,
            IframeToken = iframeToken,
            IframeUrl = $"{_options.IframeBaseUrl.TrimEnd('/')}/{iframeToken}",
            Amount = paymentAmount,
            CurrencyCode = order.CurrencyCode,
            TestMode = _options.TestMode
        }, "PayTR iFrame token oluşturuldu.");
    }

    public async Task<ApiResponse<string>> HandleCallbackAsync(IFormCollection form, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return ApiResponse<string>.ErrorResult("PayTR ayarları eksik.", statusCode: 400);
        }

        var merchantOid = form["merchant_oid"].ToString();
        var status = form["status"].ToString();
        var totalAmount = form["total_amount"].ToString();
        var incomingHash = form["hash"].ToString();
        if (string.IsNullOrWhiteSpace(merchantOid) || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(totalAmount) || string.IsNullOrWhiteSpace(incomingHash))
        {
            return ApiResponse<string>.ErrorResult("PayTR callback alanları eksik.", statusCode: 400);
        }

        var expectedHash = CreateCallbackHash(merchantOid, status, totalAmount);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedHash), Encoding.UTF8.GetBytes(incomingHash)))
        {
            return ApiResponse<string>.ErrorResult("PAYTR notification failed: bad hash", statusCode: 400);
        }

        var payment = await _payments.Query(tracking: true)
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProviderKey == "PAYTR" && x.ExternalTransactionId == merchantOid, cancellationToken);
        if (payment is null)
        {
            return ApiResponse<string>.ErrorResult("PayTR ödeme kaydı bulunamadı.", statusCode: 404);
        }

        if (string.Equals(payment.Status, B2bWorkflowStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<string>.SuccessResult("OK", "PayTR callback daha önce işlendi.");
        }

        payment.CallbackPayloadJson = JsonSerializer.Serialize(form.ToDictionary(x => x.Key, x => x.Value.ToString()), JsonOptions);
        payment.ProviderPaymentAmount = ParseMinorUnitToAmount(form["payment_amount"].ToString()) ?? payment.ProviderPaymentAmount;
        payment.ProviderCollectedAmount = ParseMinorUnitToAmount(totalAmount) ?? payment.ProviderCollectedAmount;
        payment.CurrencyCode = NormalizePaytrCurrency(form["currency"].ToString(), payment.CurrencyCode);
        payment.PaymentMethod = string.IsNullOrWhiteSpace(form["payment_type"].ToString()) ? payment.PaymentMethod : form["payment_type"].ToString();
        if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = B2bWorkflowStatuses.Completed;
            payment.CompletedDate = DateTimeProvider.Now;
            await ApplyPaymentOrderCompletionAsync(payment, cancellationToken);
            if (payment.Order is not null && string.Equals(payment.Order.Status, B2bWorkflowStatuses.WaitingPayment, StringComparison.OrdinalIgnoreCase))
            {
                payment.Order.Status = B2bWorkflowStatuses.Submitted;
                payment.Order.SetUpdatedInfo();
            }
        }
        else
        {
            payment.Status = B2bWorkflowStatuses.Failed;
            payment.CompletedDate = DateTimeProvider.Now;
        }

        payment.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<string>.SuccessResult("OK", "PayTR callback işlendi.");
    }

    private async Task<PaymentOrder> GetOrCreatePaymentOrderAsync(B2bOrder order, string providerKey, string paymentMethod, CancellationToken cancellationToken)
    {
        var existing = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.OrderId == order.Id && !x.IsDeleted && x.Status != B2bWorkflowStatuses.Cancelled, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var customer = await _customers.Query()
            .FirstOrDefaultAsync(x => x.Id == order.CustomerId && !x.IsDeleted, cancellationToken);
        var termDays = customer?.PaymentTermDays ?? 0;
        var dueDate = DateTimeProvider.Now.Date.AddDays(Math.Max(0, (int)termDays));
        var paymentOrder = new PaymentOrder
        {
            PaymentOrderNumber = $"PO-{order.Id}-{DateTimeProvider.Now:yyyyMMddHHmmssfff}",
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            BuyerId = order.BuyerId,
            UserId = order.UserId,
            Status = B2bWorkflowStatuses.Pending,
            Amount = order.GrandTotal,
            PaidAmount = 0,
            RemainingAmount = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            PaymentTermDays = termDays,
            DueDate = dueDate,
            InstallmentCount = 1,
            ProviderKey = providerKey,
            PaymentMethod = paymentMethod,
            CreatedDate = DateTimeProvider.Now,
            UpdatedDate = DateTimeProvider.Now
        };
        paymentOrder.Installments.Add(new PaymentInstallment
        {
            InstallmentNumber = 1,
            Status = B2bWorkflowStatuses.Pending,
            DueDate = dueDate,
            Amount = order.GrandTotal,
            PaidAmount = 0,
            CreatedDate = DateTimeProvider.Now,
            UpdatedDate = DateTimeProvider.Now
        });

        await _paymentOrders.AddAsync(paymentOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return paymentOrder;
    }

    private async Task ApplyPaymentOrderCompletionAsync(PaymentTransaction payment, CancellationToken cancellationToken)
    {
        if (!payment.PaymentOrderId.HasValue)
        {
            return;
        }

        var paymentOrder = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == payment.PaymentOrderId.Value && !x.IsDeleted, cancellationToken);
        if (paymentOrder == null)
        {
            return;
        }

        var collectedAmount = payment.ProviderCollectedAmount ?? payment.Amount;
        paymentOrder.PaidAmount = Math.Min(paymentOrder.Amount, paymentOrder.PaidAmount + collectedAmount);
        paymentOrder.RemainingAmount = Math.Max(0, paymentOrder.Amount - paymentOrder.PaidAmount);
        paymentOrder.Status = paymentOrder.RemainingAmount <= 0 ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
        paymentOrder.SetUpdatedInfo();

        var targetInstallment = payment.PaymentInstallmentId.HasValue
            ? paymentOrder.Installments.FirstOrDefault(x => x.Id == payment.PaymentInstallmentId.Value)
            : paymentOrder.Installments.Where(x => !x.IsDeleted).OrderBy(x => x.InstallmentNumber).FirstOrDefault(x => x.Status != B2bWorkflowStatuses.Completed);
        if (targetInstallment == null)
        {
            return;
        }

        targetInstallment.PaidAmount = Math.Min(targetInstallment.Amount, targetInstallment.PaidAmount + collectedAmount);
        targetInstallment.Status = targetInstallment.PaidAmount >= targetInstallment.Amount ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
        targetInstallment.PaidDate = targetInstallment.Status == B2bWorkflowStatuses.Completed ? DateTimeProvider.Now : targetInstallment.PaidDate;
        targetInstallment.SetUpdatedInfo();
    }

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_options.MerchantId)
            && !string.IsNullOrWhiteSpace(_options.MerchantKey)
            && !string.IsNullOrWhiteSpace(_options.MerchantSalt);
    }

    private string CreateRequestToken(string userIp, string merchantOid, string email, string amountInKurus, string userBasket, string currency)
    {
        var hashString = _options.MerchantId
            + userIp
            + merchantOid
            + email.Trim()
            + amountInKurus
            + userBasket
            + (_options.NoInstallment ? "1" : "0")
            + _options.MaxInstallment.ToString(CultureInfo.InvariantCulture)
            + currency
            + (_options.TestMode ? "1" : "0");
        return HmacBase64(hashString + _options.MerchantSalt);
    }

    private string CreateCallbackHash(string merchantOid, string status, string totalAmount)
    {
        return HmacBase64(merchantOid + _options.MerchantSalt + status + totalAmount);
    }

    private string HmacBase64(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.MerchantKey));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static string CreateBasket(B2bOrder order)
    {
        var lines = order.Lines.Where(x => !x.IsDeleted).Select(x => new object[]
        {
            x.ProductName ?? x.ProductSku ?? x.ErpStockId?.ToString(CultureInfo.InvariantCulture) ?? "B2B Ürün",
            Math.Round(x.UnitPrice, 2),
            Math.Max(1, (int)Math.Ceiling(x.Quantity))
        }).ToList();
        if (lines.Count == 0)
        {
            lines.Add(new object[] { order.OrderNumber, Math.Round(order.GrandTotal, 2), 1 });
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(lines, JsonOptions)));
    }

    private static string CreateMerchantOid(long orderId)
    {
        return $"B2B{orderId}{DateTimeProvider.Now:yyyyMMddHHmmssfff}";
    }

    private static string ToMinorUnit(decimal amount)
    {
        return ((int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
    }

    private static decimal? ParseMinorUnitToAmount(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var minorUnit)
            ? Math.Round(minorUnit / 100m, 4)
            : null;
    }

    private static string ToPaytrCurrency(string currencyCode)
    {
        var value = string.IsNullOrWhiteSpace(currencyCode) ? "TL" : currencyCode.Trim().ToUpperInvariant();
        return value is "TRY" ? "TL" : value;
    }

    private static string NormalizePaytrCurrency(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim().ToUpperInvariant();
        return normalized == "TL" ? "TRY" : normalized;
    }

    private static string BuildInstallmentPlanJson(IEnumerable<PaymentInstallment> installments)
    {
        return JsonSerializer.Serialize(installments
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.InstallmentNumber)
            .Select(x => new
            {
                x.InstallmentNumber,
                x.DueDate,
                x.Amount,
                x.PaidAmount,
                x.Status
            }), JsonOptions);
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
