namespace Wms.Application.B2B.Services;

public sealed class TrendyolMarketplaceOptions
{
    public const string SectionName = "MarketplaceProviders:Trendyol";

    public string BaseUrl { get; set; } = "https://api.trendyol.com/sapigw";
    public string ProductCreatePath { get; set; } = "/suppliers/{supplierId}/v2/products";
    public string PriceStockUpdatePath { get; set; } = "/suppliers/{supplierId}/products/price-and-inventory";
    public string BatchStatusPath { get; set; } = "/suppliers/{supplierId}/products/batch-requests/{batchRequestId}";
    public string UserAgent { get; set; } = "V3RII-B2B-Marketplace";
    public int BatchSize { get; set; } = 20;
}
