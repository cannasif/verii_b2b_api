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

public sealed class IyzicoPaymentService : IIyzicoPaymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly IyzicoOptions _options;
    private readonly IRepository<B2bOrder> _orders;
    private readonly IRepository<CustomerEntity> _customers;
    private readonly IRepository<PaymentOrder> _paymentOrders;
    private readonly IRepository<PaymentTransaction> _payments;
    private readonly IUnitOfWork _unitOfWork;

    public IyzicoPaymentService(
        HttpClient httpClient,
        IOptions<IyzicoOptions> options,
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

    public async Task<ApiResponse<Iyzico3dsInitializeDto>> Initialize3dsAsync(CreateIyzico3dsPaymentDto dto, string requestIp, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return ApiResponse<Iyzico3dsInitializeDto>.ErrorResult("iyzico ayarları eksik. ApiKey ve SecretKey tanımlanmalı.", statusCode: 400);
        }

        var order = await _orders.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.OrderId, cancellationToken);
        if (order is null)
        {
            return ApiResponse<Iyzico3dsInitializeDto>.ErrorResult("Sipariş bulunamadı.", statusCode: 404);
        }

        var paymentOrder = await GetOrCreatePaymentOrderAsync(order, cancellationToken);
        var amount = paymentOrder.ProviderTotalPrice ?? paymentOrder.RemainingAmount;
        var conversationId = $"B2B-IYZ-{order.Id}-{DateTimeProvider.Now:yyyyMMddHHmmssfff}";
        var firstOpenInstallment = paymentOrder.Installments
            .Where(x => !x.IsDeleted && x.Status != B2bWorkflowStatuses.Completed)
            .OrderBy(x => x.InstallmentNumber)
            .FirstOrDefault();
        var callbackUrl = Trim(dto.CallbackUrl) ?? Trim(_options.CallbackUrl);
        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            return ApiResponse<Iyzico3dsInitializeDto>.ErrorResult("iyzico 3DS callback URL ayarı eksik.", statusCode: 400);
        }

        var payload = new
        {
            locale = _options.Locale,
            conversationId,
            price = ToInvariant(paymentOrder.Amount),
            paidPrice = ToInvariant(amount),
            currency = NormalizeCurrency(order.CurrencyCode),
            installment = Math.Max(1, dto.InstallmentCount),
            basketId = order.OrderNumber,
            paymentChannel = "WEB",
            paymentGroup = "PRODUCT",
            callbackUrl,
            paymentCard = new
            {
                cardHolderName = dto.CardHolderName.Trim(),
                cardNumber = OnlyDigits(dto.CardNumber),
                expireMonth = dto.ExpireMonth.Trim(),
                expireYear = dto.ExpireYear.Trim(),
                cvc = dto.Cvc.Trim(),
                registerCard = "0"
            },
            buyer = new
            {
                id = order.CustomerId.ToString(CultureInfo.InvariantCulture),
                name = dto.BuyerName.Trim(),
                surname = dto.BuyerSurname.Trim(),
                gsmNumber = dto.BuyerPhone.Trim(),
                email = dto.Email.Trim(),
                identityNumber = "11111111111",
                registrationAddress = dto.BuyerAddress.Trim(),
                ip = Trim(dto.BuyerIp) ?? requestIp,
                city = dto.City.Trim(),
                country = dto.Country.Trim()
            },
            shippingAddress = new { contactName = $"{dto.BuyerName.Trim()} {dto.BuyerSurname.Trim()}".Trim(), city = dto.City.Trim(), country = dto.Country.Trim(), address = dto.BuyerAddress.Trim() },
            billingAddress = new { contactName = $"{dto.BuyerName.Trim()} {dto.BuyerSurname.Trim()}".Trim(), city = dto.City.Trim(), country = dto.Country.Trim(), address = dto.BuyerAddress.Trim() },
            basketItems = BuildBasketItems(order)
        };

        var body = JsonSerializer.Serialize(payload, JsonOptions);
        using var request = CreateIyzicoRequest(_options.ThreedsInitializePath, body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<Iyzico3dsInitializeDto>.ErrorResult("iyzico 3DS başlatma isteği başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;
        var status = GetString(root, "status") ?? string.Empty;
        if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<Iyzico3dsInitializeDto>.ErrorResult("iyzico 3DS başlatılamadı.", responseBody, statusCode: 400);
        }

        var payment = new PaymentTransaction
        {
            OrderId = order.Id,
            PaymentOrderId = paymentOrder.Id,
            PaymentInstallmentId = firstOpenInstallment?.Id,
            ProviderKey = "IYZICO",
            ExternalTransactionId = GetString(root, "paymentId") ?? conversationId,
            Status = B2bWorkflowStatuses.Pending,
            Amount = amount,
            ProviderPaymentAmount = amount,
            CurrencyCode = order.CurrencyCode,
            PaymentMethod = "iyzico 3DS",
            ProviderConversationId = conversationId,
            DueDate = firstOpenInstallment?.DueDate ?? paymentOrder.DueDate,
            PaymentTermDays = paymentOrder.PaymentTermDays,
            InstallmentCount = Math.Max(1, dto.InstallmentCount),
            InstallmentPlanJson = BuildInstallmentPlanJson(paymentOrder.Installments),
            CallbackPayloadJson = responseBody,
            RequestedDate = DateTimeProvider.Now
        };
        await _payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Iyzico3dsInitializeDto>.SuccessResult(new Iyzico3dsInitializeDto
        {
            PaymentTransactionId = payment.Id,
            OrderId = order.Id,
            ConversationId = conversationId,
            PaymentId = payment.ExternalTransactionId,
            Status = status,
            ThreeDSHtmlContent = GetString(root, "threeDSHtmlContent"),
            PaymentPageUrl = GetString(root, "paymentPageUrl"),
            Amount = amount,
            CurrencyCode = order.CurrencyCode
        }, "iyzico 3DS ödeme başlatıldı.");
    }

    public async Task<ApiResponse<string>> Handle3dsCallbackAsync(IFormCollection form, CancellationToken cancellationToken = default)
    {
        var conversationId = form["conversationId"].ToString();
        var paymentId = form["paymentId"].ToString();
        var conversationData = form["conversationData"].ToString();
        if (string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(paymentId))
        {
            return ApiResponse<string>.ErrorResult("iyzico 3DS callback alanları eksik.", statusCode: 400);
        }

        var payment = await _payments.Query(tracking: true)
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProviderKey == "IYZICO" && x.ProviderConversationId == conversationId, cancellationToken);
        if (payment is null)
        {
            return ApiResponse<string>.ErrorResult("iyzico ödeme kaydı bulunamadı.", statusCode: 404);
        }

        var body = JsonSerializer.Serialize(new
        {
            locale = _options.Locale,
            conversationId,
            paymentId,
            conversationData
        }, JsonOptions);
        using var request = CreateIyzicoRequest(_options.ThreedsAuthPath, body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        payment.CallbackPayloadJson = responseBody;
        payment.ExternalTransactionId = paymentId;
        payment.CompletedDate = DateTimeProvider.Now;

        if (!response.IsSuccessStatusCode)
        {
            payment.Status = B2bWorkflowStatuses.Failed;
            payment.SetUpdatedInfo();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<string>.ErrorResult("iyzico 3DS doğrulama isteği başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;
        var status = GetString(root, "status");
        payment.ProviderCollectedAmount = ParseDecimal(GetString(root, "paidPrice")) ?? payment.ProviderCollectedAmount ?? payment.Amount;
        payment.Status = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase) ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Failed;
        if (payment.Status == B2bWorkflowStatuses.Completed)
        {
            await ApplyPaymentOrderCompletionAsync(payment, cancellationToken);
            if (payment.Order is not null && string.Equals(payment.Order.Status, B2bWorkflowStatuses.WaitingPayment, StringComparison.OrdinalIgnoreCase))
            {
                payment.Order.Status = B2bWorkflowStatuses.Submitted;
                payment.Order.SetUpdatedInfo();
            }
        }

        payment.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return payment.Status == B2bWorkflowStatuses.Completed
            ? ApiResponse<string>.SuccessResult("OK", "iyzico 3DS callback işlendi.")
            : ApiResponse<string>.ErrorResult("iyzico 3DS ödeme başarısız.", responseBody, statusCode: 400);
    }

    private async Task<PaymentOrder> GetOrCreatePaymentOrderAsync(B2bOrder order, CancellationToken cancellationToken)
    {
        var existing = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.OrderId == order.Id && !x.IsDeleted && x.Status != B2bWorkflowStatuses.Cancelled, cancellationToken);
        if (existing is not null) return existing;

        var customer = await _customers.Query().FirstOrDefaultAsync(x => x.Id == order.CustomerId && !x.IsDeleted, cancellationToken);
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
            ProviderKey = "IYZICO",
            PaymentMethod = "iyzico 3DS",
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
        if (!payment.PaymentOrderId.HasValue) return;
        var paymentOrder = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == payment.PaymentOrderId.Value && !x.IsDeleted, cancellationToken);
        if (paymentOrder is null) return;

        var collectedAmount = payment.ProviderCollectedAmount ?? payment.Amount;
        paymentOrder.PaidAmount = Math.Min(paymentOrder.Amount, paymentOrder.PaidAmount + collectedAmount);
        paymentOrder.RemainingAmount = Math.Max(0, paymentOrder.Amount - paymentOrder.PaidAmount);
        paymentOrder.Status = paymentOrder.RemainingAmount <= 0 ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
        paymentOrder.SetUpdatedInfo();

        var installment = payment.PaymentInstallmentId.HasValue
            ? paymentOrder.Installments.FirstOrDefault(x => x.Id == payment.PaymentInstallmentId.Value)
            : paymentOrder.Installments.Where(x => !x.IsDeleted).OrderBy(x => x.InstallmentNumber).FirstOrDefault(x => x.Status != B2bWorkflowStatuses.Completed);
        if (installment is null) return;
        installment.PaidAmount = Math.Min(installment.Amount, installment.PaidAmount + collectedAmount);
        installment.Status = installment.PaidAmount >= installment.Amount ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
        installment.PaidDate = installment.Status == B2bWorkflowStatuses.Completed ? DateTimeProvider.Now : installment.PaidDate;
        installment.SetUpdatedInfo();
    }

    private HttpRequestMessage CreateIyzicoRequest(string path, string body)
    {
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var randomKey = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Guid.NewGuid():N}";
        var signature = CreateHmacSha256(_options.SecretKey, randomKey + normalizedPath + body);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}{normalizedPath}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"IYZWSv2 apiKey:{_options.ApiKey}&randomKey:{randomKey}&signature:{signature}");
        request.Headers.TryAddWithoutValidation("x-iyzi-rnd", randomKey);
        return request;
    }

    private bool HasCredentials() => !string.IsNullOrWhiteSpace(_options.ApiKey) && !string.IsNullOrWhiteSpace(_options.SecretKey);
    private static string CreateHmacSha256(string secret, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static List<object> BuildBasketItems(B2bOrder order)
    {
        var items = order.Lines.Where(x => !x.IsDeleted).Select(x => new
        {
            id = x.ErpStockId?.ToString(CultureInfo.InvariantCulture) ?? x.CatalogProductId?.ToString(CultureInfo.InvariantCulture) ?? x.Id.ToString(CultureInfo.InvariantCulture),
            name = x.ProductName ?? x.ProductSku ?? "B2B Ürün",
            category1 = "B2B",
            itemType = "PHYSICAL",
            price = ToInvariant(Math.Max(0.01m, x.LineGrandTotal))
        }).Cast<object>().ToList();

        if (items.Count == 0)
        {
            items.Add(new { id = order.Id.ToString(CultureInfo.InvariantCulture), name = order.OrderNumber, category1 = "B2B", itemType = "PHYSICAL", price = ToInvariant(order.GrandTotal) });
        }

        return items;
    }

    private static string BuildInstallmentPlanJson(IEnumerable<PaymentInstallment> installments) => JsonSerializer.Serialize(installments
        .Where(x => !x.IsDeleted)
        .OrderBy(x => x.InstallmentNumber)
        .Select(x => new { x.InstallmentNumber, x.DueDate, x.Amount, x.PaidAmount, x.Status }), JsonOptions);

    private static string ToInvariant(decimal value) => Math.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);
    private static string OnlyDigits(string value) => new(value.Where(char.IsDigit).ToArray());
    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static decimal? ParseDecimal(string? value) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? GetString(JsonElement root, string propertyName) => root.TryGetProperty(propertyName, out var element) && element.ValueKind != JsonValueKind.Null ? element.ToString() : null;
}
