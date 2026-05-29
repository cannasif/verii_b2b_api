using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class HepsiburadaMarketplaceClient : IHepsiburadaMarketplaceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly HepsiburadaMarketplaceOptions _options;
    private readonly IHepsiburadaMarketplacePayloadMapper _payloadMapper;

    public HepsiburadaMarketplaceClient(
        HttpClient httpClient,
        IOptions<HepsiburadaMarketplaceOptions> options,
        IHepsiburadaMarketplacePayloadMapper payloadMapper)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _payloadMapper = payloadMapper;
    }

    public async Task<HepsiburadaClientResult> SendAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default)
    {
        if (syncEvent.Channel == null)
        {
            return Failed("Hepsiburada kanal bilgisi bulunamadı.", 400);
        }

        var credentials = ResolveCredentials(syncEvent.Channel);
        if (credentials == null)
        {
            return Failed("Hepsiburada merchantId, apiKey veya apiSecret eksik.", 400);
        }

        if (syncEvent.OperationType == "OrderImport")
        {
            return Failed("Hepsiburada sipariş çekme operasyonu ayrı order client ile yapılmalı; bu worker ürün/fiyat/stok içindir.", 400);
        }

        var path = syncEvent.OperationType == "ProductCreate"
            ? _options.ProductCreatePath
            : _options.PriceStockUpdatePath;
        var requestJson = _payloadMapper.BuildRequestJson(syncEvent);
        using var request = CreateRequest(HttpMethod.Post, BuildUrl(path), credentials, requestJson);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var transactionId = ExtractTransactionId(responseJson);
        if (!response.IsSuccessStatusCode)
        {
            return new HepsiburadaClientResult(false, transactionId, requestJson, responseJson, (int)response.StatusCode, responseJson);
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            transactionId = response.Headers.TryGetValues("X-Request-Id", out var requestIds) ? requestIds.FirstOrDefault() : null;
        }

        return new HepsiburadaClientResult(true, transactionId, requestJson, responseJson, (int)response.StatusCode, null);
    }

    public async Task<HepsiburadaTransactionStatusResult> GetTransactionStatusAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default)
    {
        if (syncEvent.Channel == null)
        {
            return new HepsiburadaTransactionStatusResult(true, false, string.Empty, 400, "Hepsiburada kanal bilgisi bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(syncEvent.ExternalBatchId))
        {
            return new HepsiburadaTransactionStatusResult(true, false, string.Empty, 400, "Hepsiburada transaction id bulunamadı.");
        }

        var credentials = ResolveCredentials(syncEvent.Channel);
        if (credentials == null)
        {
            return new HepsiburadaTransactionStatusResult(true, false, string.Empty, 400, "Hepsiburada merchantId, apiKey veya apiSecret eksik.");
        }

        using var request = CreateRequest(HttpMethod.Get, BuildUrl(_options.TransactionStatusPath, syncEvent.ExternalBatchId), credentials, null);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new HepsiburadaTransactionStatusResult(false, false, responseJson, (int)response.StatusCode, responseJson);
        }

        var status = ExtractStatus(responseJson);
        var isFinal = status is "COMPLETED" or "DONE" or "SUCCESS" or "FAILED" or "ERROR";
        var success = status is "COMPLETED" or "DONE" or "SUCCESS";
        return new HepsiburadaTransactionStatusResult(isFinal, success, responseJson, (int)response.StatusCode, success ? null : ExtractError(responseJson));
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, HepsiburadaCredentials credentials, string? body)
    {
        var request = new HttpRequestMessage(method, url);
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ApiKey}:{credentials.ApiSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);
        request.Headers.TryAddWithoutValidation("merchantId", credentials.MerchantId);
        if (!string.IsNullOrWhiteSpace(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private string BuildUrl(string path, string? transactionId = null)
    {
        var normalizedPath = path.Replace("{transactionId}", Uri.EscapeDataString(transactionId ?? string.Empty), StringComparison.OrdinalIgnoreCase);
        return $"{_options.BaseUrl.TrimEnd('/')}/{normalizedPath.TrimStart('/')}";
    }

    private static HepsiburadaCredentials? ResolveCredentials(MarketplaceChannel channel)
    {
        var merchantId = channel.SellerId;
        string? apiKey = null;
        string? apiSecret = null;
        if (!string.IsNullOrWhiteSpace(channel.CredentialsJson))
        {
            using var document = JsonDocument.Parse(channel.CredentialsJson);
            var root = document.RootElement;
            merchantId = GetString(root, "merchantId") ?? GetString(root, "sellerId") ?? merchantId;
            apiKey = GetString(root, "apiKey") ?? GetString(root, "username");
            apiSecret = GetString(root, "apiSecret") ?? GetString(root, "password");
        }

        return string.IsNullOrWhiteSpace(merchantId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret)
            ? null
            : new HepsiburadaCredentials(merchantId.Trim(), apiKey.Trim(), apiSecret.Trim());
    }

    private static string? ExtractTransactionId(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseJson);
        return GetString(document.RootElement, "trackingId")
            ?? GetString(document.RootElement, "transactionId")
            ?? GetString(document.RootElement, "id")
            ?? GetString(document.RootElement, "uploadId");
    }

    private static string? ExtractStatus(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseJson);
        return (GetString(document.RootElement, "status")
            ?? GetString(document.RootElement, "state")
            ?? GetString(document.RootElement, "result"))?.Trim().ToUpperInvariant();
    }

    private static string? ExtractError(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseJson);
        return GetString(document.RootElement, "message") ?? GetString(document.RootElement, "errorMessage") ?? responseJson;
    }

    private static string? GetString(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(name, out var element))
        {
            return null;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            _ => null
        };
    }

    private static HepsiburadaClientResult Failed(string message, int statusCode) => new(false, null, string.Empty, JsonSerializer.Serialize(new { error = message }, JsonOptions), statusCode, message);
}
