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

public sealed class PaymentProviderLookupService : IPaymentProviderLookupService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly PaytrOptions _paytrOptions;
    private readonly IyzicoOptions _iyzicoOptions;
    private readonly IRepository<PaymentProviderInquiryLog> _logs;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentProviderLookupService(
        HttpClient httpClient,
        IOptions<PaytrOptions> paytrOptions,
        IOptions<IyzicoOptions> iyzicoOptions,
        IRepository<PaymentProviderInquiryLog> logs,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient;
        _paytrOptions = paytrOptions.Value;
        _iyzicoOptions = iyzicoOptions.Value;
        _logs = logs;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaymentBinLookupDto>> LookupBinAsync(PaymentBinLookupRequestDto dto, CancellationToken cancellationToken = default)
    {
        var provider = NormalizeProvider(dto.ProviderKey);
        var binNumber = NormalizeBin(dto.BinNumber);
        if (binNumber.Length is < 6 or > 8)
        {
            return ApiResponse<PaymentBinLookupDto>.ErrorResult("BIN 6 veya 8 hane olmalı.", statusCode: 400);
        }

        return provider switch
        {
            "PAYTR" => await LookupPaytrBinAsync(dto, binNumber, cancellationToken),
            "IYZICO" or "IYZIPAY" => await LookupIyzicoBinAsync(dto, binNumber, cancellationToken),
            _ => ApiResponse<PaymentBinLookupDto>.ErrorResult($"Desteklenmeyen ödeme sağlayıcı: {dto.ProviderKey}", statusCode: 400)
        };
    }

    public async Task<ApiResponse<PaymentInstallmentOptionsDto>> GetInstallmentOptionsAsync(PaymentInstallmentOptionsRequestDto dto, CancellationToken cancellationToken = default)
    {
        var provider = NormalizeProvider(dto.ProviderKey);
        var binNumber = string.IsNullOrWhiteSpace(dto.BinNumber) ? null : NormalizeBin(dto.BinNumber);
        if (binNumber is { Length: < 6 or > 8 })
        {
            return ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult("BIN 6 veya 8 hane olmalı.", statusCode: 400);
        }

        return provider switch
        {
            "PAYTR" => await GetPaytrInstallmentsAsync(dto, binNumber, cancellationToken),
            "IYZICO" or "IYZIPAY" => await GetIyzicoInstallmentsAsync(dto, binNumber, cancellationToken),
            _ => ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult($"Desteklenmeyen ödeme sağlayıcı: {dto.ProviderKey}", statusCode: 400)
        };
    }

    private async Task<ApiResponse<PaymentBinLookupDto>> LookupPaytrBinAsync(PaymentBinLookupRequestDto dto, string binNumber, CancellationToken cancellationToken)
    {
        if (!HasPaytrCredentials())
        {
            return ApiResponse<PaymentBinLookupDto>.ErrorResult("PayTR ayarları eksik.", statusCode: 400);
        }

        var request = new Dictionary<string, string>
        {
            ["merchant_id"] = _paytrOptions.MerchantId,
            ["bin_number"] = binNumber,
            ["paytr_token"] = CreatePaytrLookupToken(binNumber)
        };

        return await PostFormBinAsync("PAYTR", "BinLookup", _paytrOptions.BinLookupUrl, request, binNumber, dto.Amount, dto.CurrencyCode, dto.ConversationId, MapPaytrBinResponse, cancellationToken);
    }

    private async Task<ApiResponse<PaymentInstallmentOptionsDto>> GetPaytrInstallmentsAsync(PaymentInstallmentOptionsRequestDto dto, string? binNumber, CancellationToken cancellationToken)
    {
        if (!HasPaytrCredentials())
        {
            return ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult("PayTR ayarları eksik.", statusCode: 400);
        }

        var request = new Dictionary<string, string>
        {
            ["merchant_id"] = _paytrOptions.MerchantId,
            ["amount"] = ToInvariant(dto.Amount),
            ["paytr_token"] = CreatePaytrInstallmentToken(binNumber, dto.Amount)
        };
        if (!string.IsNullOrWhiteSpace(binNumber)) request["bin_number"] = binNumber;

        return await PostFormInstallmentAsync("PAYTR", _paytrOptions.InstallmentRatesUrl, request, binNumber, dto.Amount, dto.CurrencyCode, dto.ConversationId, MapPaytrInstallmentResponse, cancellationToken);
    }

    private async Task<ApiResponse<PaymentBinLookupDto>> LookupIyzicoBinAsync(PaymentBinLookupRequestDto dto, string binNumber, CancellationToken cancellationToken)
    {
        if (!HasIyzicoCredentials())
        {
            return ApiResponse<PaymentBinLookupDto>.ErrorResult("iyzico ayarları eksik.", statusCode: 400);
        }

        var payload = new
        {
            locale = _iyzicoOptions.Locale,
            conversationId = dto.ConversationId ?? Guid.NewGuid().ToString("N"),
            binNumber
        };
        return await PostIyzicoBinAsync(_iyzicoOptions.BinLookupPath, payload, binNumber, dto.Amount, dto.CurrencyCode, payload.conversationId, cancellationToken);
    }

    private async Task<ApiResponse<PaymentInstallmentOptionsDto>> GetIyzicoInstallmentsAsync(PaymentInstallmentOptionsRequestDto dto, string? binNumber, CancellationToken cancellationToken)
    {
        if (!HasIyzicoCredentials())
        {
            return ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult("iyzico ayarları eksik.", statusCode: 400);
        }

        var conversationId = dto.ConversationId ?? Guid.NewGuid().ToString("N");
        var payload = new
        {
            locale = _iyzicoOptions.Locale,
            conversationId,
            binNumber,
            price = ToInvariant(dto.Amount)
        };
        return await PostIyzicoInstallmentAsync(_iyzicoOptions.InstallmentPath, payload, binNumber, dto.Amount, dto.CurrencyCode, conversationId, cancellationToken);
    }

    private async Task<ApiResponse<PaymentBinLookupDto>> PostFormBinAsync(
        string provider,
        string inquiryType,
        string url,
        Dictionary<string, string> request,
        string binNumber,
        decimal? amount,
        string currencyCode,
        string? conversationId,
        Func<JsonElement, string, string?, PaymentBinLookupDto> mapper,
        CancellationToken cancellationToken)
    {
        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        using var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(request), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await SaveLogAsync(provider, inquiryType, binNumber, amount, currencyCode, conversationId, response.IsSuccessStatusCode ? "Completed" : "Failed", requestJson, responseBody, response.IsSuccessStatusCode ? null : responseBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<PaymentBinLookupDto>.ErrorResult($"{provider} BIN sorgusu başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        return ApiResponse<PaymentBinLookupDto>.SuccessResult(mapper(json.RootElement, binNumber, conversationId), $"{provider} BIN sorgusu tamamlandı.");
    }

    private async Task<ApiResponse<PaymentInstallmentOptionsDto>> PostFormInstallmentAsync(
        string provider,
        string url,
        Dictionary<string, string> request,
        string? binNumber,
        decimal amount,
        string currencyCode,
        string? conversationId,
        Func<JsonElement, string?, decimal, string, string?, PaymentInstallmentOptionsDto> mapper,
        CancellationToken cancellationToken)
    {
        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        using var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(request), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await SaveLogAsync(provider, "InstallmentOptions", binNumber, amount, currencyCode, conversationId, response.IsSuccessStatusCode ? "Completed" : "Failed", requestJson, responseBody, response.IsSuccessStatusCode ? null : responseBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult($"{provider} taksit sorgusu başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        return ApiResponse<PaymentInstallmentOptionsDto>.SuccessResult(mapper(json.RootElement, binNumber, amount, currencyCode, conversationId), $"{provider} taksit seçenekleri alındı.");
    }

    private async Task<ApiResponse<PaymentBinLookupDto>> PostIyzicoBinAsync(string path, object payload, string binNumber, decimal? amount, string currencyCode, string conversationId, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.Serialize(payload, JsonOptions);
        using var request = CreateIyzicoRequest(path, body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await SaveLogAsync("IYZICO", "BinLookup", binNumber, amount, currencyCode, conversationId, response.IsSuccessStatusCode ? "Completed" : "Failed", body, responseBody, response.IsSuccessStatusCode ? null : responseBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<PaymentBinLookupDto>.ErrorResult("iyzico BIN sorgusu başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        return ApiResponse<PaymentBinLookupDto>.SuccessResult(MapIyzicoBinResponse(json.RootElement, binNumber, conversationId), "iyzico BIN sorgusu tamamlandı.");
    }

    private async Task<ApiResponse<PaymentInstallmentOptionsDto>> PostIyzicoInstallmentAsync(string path, object payload, string? binNumber, decimal amount, string currencyCode, string conversationId, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.Serialize(payload, JsonOptions);
        using var request = CreateIyzicoRequest(path, body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await SaveLogAsync("IYZICO", "InstallmentOptions", binNumber, amount, currencyCode, conversationId, response.IsSuccessStatusCode ? "Completed" : "Failed", body, responseBody, response.IsSuccessStatusCode ? null : responseBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<PaymentInstallmentOptionsDto>.ErrorResult("iyzico taksit sorgusu başarısız.", responseBody, (int)response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        return ApiResponse<PaymentInstallmentOptionsDto>.SuccessResult(MapIyzicoInstallmentResponse(json.RootElement, binNumber, amount, currencyCode, conversationId), "iyzico taksit seçenekleri alındı.");
    }

    private HttpRequestMessage CreateIyzicoRequest(string path, string body)
    {
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var randomKey = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Guid.NewGuid():N}";
        var signature = CreateHmacSha256(_iyzicoOptions.SecretKey, randomKey + normalizedPath + body);
        var authorization = $"IYZWSv2 apiKey:{_iyzicoOptions.ApiKey}&randomKey:{randomKey}&signature:{signature}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_iyzicoOptions.BaseUrl.TrimEnd('/')}{normalizedPath}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Authorization", authorization);
        request.Headers.TryAddWithoutValidation("x-iyzi-rnd", randomKey);
        return request;
    }

    private async Task SaveLogAsync(string provider, string inquiryType, string? binNumber, decimal? amount, string currencyCode, string? conversationId, string status, string requestJson, string responseJson, string? errorMessage, CancellationToken cancellationToken)
    {
        await _logs.AddAsync(new PaymentProviderInquiryLog
        {
            ProviderKey = provider,
            InquiryType = inquiryType,
            BinNumber = binNumber,
            Amount = amount,
            CurrencyCode = NormalizeCurrency(currencyCode),
            Status = status,
            ConversationId = conversationId,
            ErrorMessage = errorMessage,
            RequestJson = requestJson,
            ResponseJson = responseJson,
            RequestedDate = DateTimeProvider.Now,
            CreatedDate = DateTimeProvider.Now
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static PaymentBinLookupDto MapPaytrBinResponse(JsonElement root, string binNumber, string? conversationId) => new()
    {
        ProviderKey = "PAYTR",
        BinNumber = binNumber,
        ProviderStatus = GetString(root, "status"),
        CardType = GetString(root, "cardType") ?? GetString(root, "card_type"),
        CardAssociation = GetString(root, "cardAssociation") ?? GetString(root, "brand") ?? GetString(root, "schema"),
        CardFamily = GetString(root, "cardFamily") ?? GetString(root, "card_family"),
        BankName = GetString(root, "bankName") ?? GetString(root, "bank"),
        BankCode = GetString(root, "bankCode") ?? GetString(root, "bank_code"),
        IsCommercial = GetBool(root, "isCommercial") ?? GetBool(root, "commercial") ?? GetBool(root, "businessCard"),
        ConversationId = conversationId,
        RawResponseJson = root.GetRawText()
    };

    private static PaymentBinLookupDto MapIyzicoBinResponse(JsonElement root, string binNumber, string? conversationId) => new()
    {
        ProviderKey = "IYZICO",
        BinNumber = GetString(root, "binNumber") ?? binNumber,
        ProviderStatus = GetString(root, "status"),
        CardType = GetString(root, "cardType"),
        CardAssociation = GetString(root, "cardAssociation"),
        CardFamily = GetString(root, "cardFamily"),
        BankName = GetString(root, "bankName"),
        BankCode = GetString(root, "bankCode"),
        IsCommercial = GetBool(root, "commercial"),
        ConversationId = GetString(root, "conversationId") ?? conversationId,
        RawResponseJson = root.GetRawText()
    };

    private static PaymentInstallmentOptionsDto MapPaytrInstallmentResponse(JsonElement root, string? binNumber, decimal amount, string currencyCode, string? conversationId) => new()
    {
        ProviderKey = "PAYTR",
        BinNumber = binNumber,
        Amount = amount,
        CurrencyCode = NormalizeCurrency(currencyCode),
        ProviderStatus = GetString(root, "status"),
        ConversationId = conversationId,
        Options = ExtractInstallments(root, amount),
        RawResponseJson = root.GetRawText()
    };

    private static PaymentInstallmentOptionsDto MapIyzicoInstallmentResponse(JsonElement root, string? binNumber, decimal amount, string currencyCode, string? conversationId)
    {
        var dto = new PaymentInstallmentOptionsDto
        {
            ProviderKey = "IYZICO",
            BinNumber = binNumber,
            Amount = amount,
            CurrencyCode = NormalizeCurrency(currencyCode),
            ProviderStatus = GetString(root, "status"),
            ConversationId = GetString(root, "conversationId") ?? conversationId,
            RawResponseJson = root.GetRawText()
        };

        if (root.TryGetProperty("installmentDetails", out var details) && details.ValueKind == JsonValueKind.Array)
        {
            var firstDetail = details.EnumerateArray().FirstOrDefault();
            if (firstDetail.ValueKind == JsonValueKind.Object)
            {
                dto.Card = new PaymentBinLookupDto
                {
                    ProviderKey = "IYZICO",
                    BinNumber = GetString(firstDetail, "binNumber") ?? binNumber ?? string.Empty,
                    CardType = GetString(firstDetail, "cardType"),
                    CardAssociation = GetString(firstDetail, "cardAssociation"),
                    CardFamily = GetString(firstDetail, "cardFamilyName") ?? GetString(firstDetail, "cardFamily"),
                    BankName = GetString(firstDetail, "bankName"),
                    BankCode = GetString(firstDetail, "bankCode"),
                    ConversationId = dto.ConversationId
                };

                if (firstDetail.TryGetProperty("installmentPrices", out var prices) && prices.ValueKind == JsonValueKind.Array)
                {
                    dto.Options = prices.EnumerateArray().Select(item => new PaymentInstallmentOptionDto
                    {
                        InstallmentNumber = GetInt(item, "installmentNumber") ?? 1,
                        InstallmentPrice = GetDecimal(item, "installmentPrice") ?? amount,
                        TotalPrice = GetDecimal(item, "totalPrice") ?? amount,
                        ProviderRate = GetDecimal(item, "providerRate") ?? GetDecimal(item, "merchantCommissionRate"),
                        CommissionAmount = GetDecimal(item, "merchantCommissionRate").HasValue
                            ? Math.Round(amount * GetDecimal(item, "merchantCommissionRate")!.Value / 100m, 4)
                            : null
                    }).ToList();
                }
            }
        }

        if (dto.Options.Count == 0)
        {
            dto.Options.Add(new PaymentInstallmentOptionDto { InstallmentNumber = 1, InstallmentPrice = amount, TotalPrice = amount });
        }

        return dto;
    }

    private static List<PaymentInstallmentOptionDto> ExtractInstallments(JsonElement root, decimal amount)
    {
        var options = new List<PaymentInstallmentOptionDto>();
        foreach (var arrayName in new[] { "installments", "rates", "data" })
        {
            if (!root.TryGetProperty(arrayName, out var array) || array.ValueKind != JsonValueKind.Array) continue;
            options.AddRange(array.EnumerateArray().Select(item => new PaymentInstallmentOptionDto
            {
                InstallmentNumber = GetInt(item, "installment") ?? GetInt(item, "installmentNumber") ?? GetInt(item, "taksit") ?? 1,
                InstallmentPrice = GetDecimal(item, "installmentPrice") ?? GetDecimal(item, "price") ?? amount,
                TotalPrice = GetDecimal(item, "totalPrice") ?? GetDecimal(item, "total") ?? amount,
                ProviderRate = GetDecimal(item, "rate") ?? GetDecimal(item, "providerRate"),
                CommissionAmount = GetDecimal(item, "commissionAmount")
            }));
        }

        if (options.Count == 0)
        {
            options.Add(new PaymentInstallmentOptionDto { InstallmentNumber = 1, InstallmentPrice = amount, TotalPrice = amount });
        }

        return options.OrderBy(x => x.InstallmentNumber).ToList();
    }

    private string CreatePaytrLookupToken(string binNumber) => CreateHmacSha256Base64(_paytrOptions.MerchantKey, _paytrOptions.MerchantId + binNumber + _paytrOptions.MerchantSalt);
    private string CreatePaytrInstallmentToken(string? binNumber, decimal amount) => CreateHmacSha256Base64(_paytrOptions.MerchantKey, _paytrOptions.MerchantId + (binNumber ?? string.Empty) + ToInvariant(amount) + _paytrOptions.MerchantSalt);
    private bool HasPaytrCredentials() => !string.IsNullOrWhiteSpace(_paytrOptions.MerchantId) && !string.IsNullOrWhiteSpace(_paytrOptions.MerchantKey) && !string.IsNullOrWhiteSpace(_paytrOptions.MerchantSalt);
    private bool HasIyzicoCredentials() => !string.IsNullOrWhiteSpace(_iyzicoOptions.ApiKey) && !string.IsNullOrWhiteSpace(_iyzicoOptions.SecretKey);
    private static string NormalizeProvider(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeCurrency(string? value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static string NormalizeBin(string value) => new(value.Where(char.IsDigit).Take(8).ToArray());
    private static string ToInvariant(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static string CreateHmacSha256Base64(string key, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static string CreateHmacSha256(string key, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static string? GetString(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value.ToString() : null;
    private static int? GetInt(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.TryGetInt32(out var result) ? result : null;
    private static decimal? GetDecimal(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var numeric)) return numeric;
        return decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }
    private static bool? GetBool(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
            _ => null
        };
    }
}
