using System.Globalization;
using System.Text.Json;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class TrendyolMarketplacePayloadMapper : ITrendyolMarketplacePayloadMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string BuildRequestJson(MarketplaceSyncEvent syncEvent)
    {
        if (TryGetProviderPayload(syncEvent.RequestJson, out var providerPayload))
        {
            return providerPayload;
        }

        var listing = syncEvent.Listing ?? throw new InvalidOperationException("Trendyol aktarımı için listing bilgisi zorunludur.");
        return syncEvent.OperationType switch
        {
            "ProductCreate" => JsonSerializer.Serialize(BuildProductCreatePayload(listing, syncEvent.RequestJson), JsonOptions),
            "PriceUpdate" => JsonSerializer.Serialize(BuildPriceStockPayload(listing, syncEvent.RequestJson, includePrice: true, includeStock: false), JsonOptions),
            "StockUpdate" => JsonSerializer.Serialize(BuildPriceStockPayload(listing, syncEvent.RequestJson, includePrice: false, includeStock: true), JsonOptions),
            _ => throw new InvalidOperationException($"Trendyol operasyon tipi desteklenmiyor: {syncEvent.OperationType}")
        };
    }

    private static object BuildProductCreatePayload(MarketplaceListing listing, string? requestJson)
    {
        var product = listing.CatalogProduct;
        var values = MergeJsonObjects(product?.AttributesJson, requestJson);
        var imageUrl = FirstNonEmpty(product?.PrimaryImageUrl, GetString(values, "imageUrl"), GetString(values, "mainImageUrl"));
        var images = string.IsNullOrWhiteSpace(imageUrl)
            ? Array.Empty<object>()
            : new object[] { new { url = imageUrl } };

        return new
        {
            items = new[]
            {
                new
                {
                    barcode = FirstNonEmpty(listing.Barcode, product?.Barcode, listing.Sku),
                    title = FirstNonEmpty(product?.Name, listing.Sku),
                    productMainId = FirstNonEmpty(GetString(values, "productMainId"), product?.Sku, listing.Sku),
                    brandId = GetLong(values, "brandId"),
                    categoryId = GetLong(values, "categoryId"),
                    quantity = GetDecimal(values, "quantity") ?? listing.LastPushedQuantity ?? 0,
                    stockCode = listing.Sku,
                    dimensionalWeight = GetDecimal(values, "dimensionalWeight") ?? 1,
                    description = FirstNonEmpty(product?.Description, product?.ShortDescription, product?.Name, listing.Sku),
                    currencyType = NormalizeCurrency(listing.CurrencyCode),
                    listPrice = GetDecimal(values, "listPrice") ?? listing.LastPushedPrice ?? 0,
                    salePrice = GetDecimal(values, "salePrice") ?? listing.LastPushedPrice ?? 0,
                    vatRate = GetDecimal(values, "vatRate") ?? 20,
                    cargoCompanyId = GetLong(values, "cargoCompanyId"),
                    images,
                    attributes = ExtractAttributes(values)
                }
            }
        };
    }

    private static object BuildPriceStockPayload(MarketplaceListing listing, string? requestJson, bool includePrice, bool includeStock)
    {
        var values = MergeJsonObjects(requestJson, null);
        var item = new Dictionary<string, object?>
        {
            ["barcode"] = FirstNonEmpty(listing.Barcode, listing.Sku)
        };

        if (includeStock)
        {
            item["quantity"] = GetDecimal(values, "quantity") ?? listing.LastPushedQuantity ?? 0;
        }

        if (includePrice)
        {
            var price = GetDecimal(values, "price") ?? GetDecimal(values, "salePrice") ?? listing.LastPushedPrice;
            item["salePrice"] = price ?? 0;
            item["listPrice"] = GetDecimal(values, "listPrice") ?? price ?? 0;
        }

        return new { items = new[] { item } };
    }

    private static bool TryGetProviderPayload(string? requestJson, out string providerPayload)
    {
        providerPayload = string.Empty;
        if (string.IsNullOrWhiteSpace(requestJson))
        {
            return false;
        }

        using var document = JsonDocument.Parse(requestJson);
        if (document.RootElement.TryGetProperty("providerPayload", out var payload))
        {
            providerPayload = payload.ValueKind == JsonValueKind.String ? payload.GetString() ?? string.Empty : payload.GetRawText();
            return !string.IsNullOrWhiteSpace(providerPayload);
        }

        return false;
    }

    private static Dictionary<string, JsonElement> MergeJsonObjects(string? firstJson, string? secondJson)
    {
        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        Merge(values, firstJson);
        Merge(values, secondJson);
        return values;
    }

    private static void Merge(Dictionary<string, JsonElement> values, string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                values[property.Name] = property.Value.Clone();
            }
        }
        catch (JsonException)
        {
        }
    }

    private static object[] ExtractAttributes(Dictionary<string, JsonElement> values)
    {
        if (!values.TryGetValue("trendyolAttributes", out var element) || element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<object>();
        }

        return element.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.Object)
            .Select(x => JsonSerializer.Deserialize<object>(x.GetRawText(), JsonOptions)!)
            .ToArray();
    }

    private static string? GetString(Dictionary<string, JsonElement> values, string key) =>
        values.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.String ? element.GetString() : null;

    private static long? GetLong(Dictionary<string, JsonElement> values, string key)
    {
        if (!values.TryGetValue(key, out var element))
        {
            return null;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var value))
        {
            return value;
        }

        return element.ValueKind == JsonValueKind.String && long.TryParse(element.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out value) ? value : null;
    }

    private static decimal? GetDecimal(Dictionary<string, JsonElement> values, string key)
    {
        if (!values.TryGetValue(key, out var element))
        {
            return null;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var value))
        {
            return value;
        }

        return element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out value) ? value : null;
    }

    private static string FirstNonEmpty(params string?[] values) => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;
    private static string NormalizeCurrency(string? currency) => string.IsNullOrWhiteSpace(currency) ? "TRY" : currency.Trim().ToUpperInvariant();
}
