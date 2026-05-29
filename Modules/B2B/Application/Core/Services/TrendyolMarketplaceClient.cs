using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class TrendyolMarketplaceClient : ITrendyolMarketplaceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly TrendyolMarketplaceOptions _options;
    private readonly ITrendyolMarketplacePayloadMapper _payloadMapper;

    public TrendyolMarketplaceClient(
        HttpClient httpClient,
        IOptions<TrendyolMarketplaceOptions> options,
        ITrendyolMarketplacePayloadMapper payloadMapper)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _payloadMapper = payloadMapper;
    }

    public async Task<TrendyolClientResult> SendAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default)
    {
        if (syncEvent.Channel == null)
        {
            return Failed("Trendyol kanal bilgisi bulunamadı.", 400);
        }

        var credentials = ResolveCredentials(syncEvent.Channel);
        if (credentials == null)
        {
            return Failed("Trendyol supplierId, apiKey veya apiSecret eksik.", 400);
        }

        if (syncEvent.OperationType == "OrderImport")
        {
            return Failed("Trendyol sipariş çekme operasyonu ayrı order client ile yapılmalı; bu worker ürün/fiyat/stok içindir.", 400);
        }

        var path = syncEvent.OperationType == "ProductCreate"
            ? _options.ProductCreatePath
            : _options.PriceStockUpdatePath;
        var url = BuildUrl(path, credentials.SupplierId);
        var requestJson = _payloadMapper.BuildRequestJson(syncEvent);
        using var request = CreateRequest(HttpMethod.Post, url, credentials, requestJson);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var batchId = ExtractBatchRequestId(responseJson);
        if (!response.IsSuccessStatusCode)
        {
            return new TrendyolClientResult(false, batchId, requestJson, responseJson, (int)response.StatusCode, responseJson);
        }

        if (string.IsNullOrWhiteSpace(batchId))
        {
            return new TrendyolClientResult(false, null, requestJson, responseJson, (int)response.StatusCode, "Trendyol batchRequestId dönmedi.");
        }

        return new TrendyolClientResult(true, batchId, requestJson, responseJson, (int)response.StatusCode, null);
    }

    public async Task<TrendyolBatchStatusResult> GetBatchStatusAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default)
    {
        if (syncEvent.Channel == null)
        {
            return new TrendyolBatchStatusResult(true, false, string.Empty, 400, "Trendyol kanal bilgisi bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(syncEvent.ExternalBatchId))
        {
            return new TrendyolBatchStatusResult(true, false, string.Empty, 400, "Trendyol batchRequestId bulunamadı.");
        }

        var credentials = ResolveCredentials(syncEvent.Channel);
        if (credentials == null)
        {
            return new TrendyolBatchStatusResult(true, false, string.Empty, 400, "Trendyol supplierId, apiKey veya apiSecret eksik.");
        }

        var url = BuildUrl(_options.BatchStatusPath, credentials.SupplierId, syncEvent.ExternalBatchId);
        using var request = CreateRequest(HttpMethod.Get, url, credentials, null);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new TrendyolBatchStatusResult(false, false, responseJson, (int)response.StatusCode, responseJson);
        }

        var status = ExtractStatus(responseJson);
        var failed = HasFailedItems(responseJson);
        var isFinal = status is "COMPLETED" or "FAILED" or "SUCCESS" || failed;
        var success = isFinal && !failed && status is not "FAILED";
        return new TrendyolBatchStatusResult(isFinal, success, responseJson, (int)response.StatusCode, failed ? "Trendyol batch içinde hatalı item var." : null);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, TrendyolCredentials credentials, string? body)
    {
        var request = new HttpRequestMessage(method, url);
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ApiKey}:{credentials.ApiSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);
        if (!string.IsNullOrWhiteSpace(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private string BuildUrl(string path, string supplierId, string? batchRequestId = null)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var normalizedPath = path
            .Replace("{supplierId}", Uri.EscapeDataString(supplierId), StringComparison.OrdinalIgnoreCase)
            .Replace("{batchRequestId}", Uri.EscapeDataString(batchRequestId ?? string.Empty), StringComparison.OrdinalIgnoreCase);
        return $"{baseUrl}/{normalizedPath.TrimStart('/')}";
    }

    private static TrendyolCredentials? ResolveCredentials(MarketplaceChannel channel)
    {
        var supplierId = channel.SellerId;
        string? apiKey = null;
        string? apiSecret = null;
        if (!string.IsNullOrWhiteSpace(channel.CredentialsJson))
        {
            using var document = JsonDocument.Parse(channel.CredentialsJson);
            var root = document.RootElement;
            supplierId = GetString(root, "supplierId") ?? GetString(root, "sellerId") ?? supplierId;
            apiKey = GetString(root, "apiKey") ?? GetString(root, "username");
            apiSecret = GetString(root, "apiSecret") ?? GetString(root, "password");
        }

        return string.IsNullOrWhiteSpace(supplierId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret)
            ? null
            : new TrendyolCredentials(supplierId.Trim(), apiKey.Trim(), apiSecret.Trim());
    }

    private static string? ExtractBatchRequestId(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseJson);
        return GetString(document.RootElement, "batchRequestId") ?? GetString(document.RootElement, "batchId");
    }

    private static string? ExtractStatus(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseJson);
        return GetString(document.RootElement, "status")?.Trim().ToUpperInvariant();
    }

    private static bool HasFailedItems(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return false;
        }

        using var document = JsonDocument.Parse(responseJson);
        if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return items.EnumerateArray().Any(item =>
            string.Equals(GetString(item, "status"), "FAILURE", StringComparison.OrdinalIgnoreCase) ||
            item.TryGetProperty("failureReasons", out var reasons) && reasons.ValueKind == JsonValueKind.Array && reasons.GetArrayLength() > 0);
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

    private static TrendyolClientResult Failed(string message, int statusCode) => new(false, null, string.Empty, JsonSerializer.Serialize(new { error = message }, JsonOptions), statusCode, message);
}
