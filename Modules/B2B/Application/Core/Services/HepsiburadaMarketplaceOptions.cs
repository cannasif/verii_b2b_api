namespace Wms.Application.B2B.Services;

public sealed class HepsiburadaMarketplaceOptions
{
    public const string SectionName = "MarketplaceProviders:Hepsiburada";

    public string BaseUrl { get; set; } = "https://mpop.hepsiburada.com";
    public string ProductCreatePath { get; set; } = "/product/api/products/import";
    public string PriceStockUpdatePath { get; set; } = "/product/api/products/price-and-stock";
    public string TransactionStatusPath { get; set; } = "/product/api/products/status/{transactionId}";
    public string UserAgent { get; set; } = "V3RII-B2B-Marketplace";
    public int BatchSize { get; set; } = 20;
}
