using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bInsightService : IB2bInsightService
{
    private readonly IRepository<CatalogProduct> _catalogProducts;
    private readonly IRepository<CustomerProductAlias> _aliases;
    private readonly IRepository<CustomerPriceList> _priceLists;
    private readonly IRepository<CustomerPriceListItem> _priceListItems;
    private readonly IRepository<InventorySnapshot> _inventory;
    private readonly IRepository<QuoteRequest> _quotes;
    private readonly IRepository<B2bOrder> _orders;
    private readonly IRepository<B2bCart> _carts;
    private readonly IRepository<PaymentTransaction> _payments;
    private readonly IRepository<B2bIntegrationEvent> _integrationEvents;

    public B2bInsightService(
        IRepository<CatalogProduct> catalogProducts,
        IRepository<CustomerProductAlias> aliases,
        IRepository<CustomerPriceList> priceLists,
        IRepository<CustomerPriceListItem> priceListItems,
        IRepository<InventorySnapshot> inventory,
        IRepository<QuoteRequest> quotes,
        IRepository<B2bOrder> orders,
        IRepository<B2bCart> carts,
        IRepository<PaymentTransaction> payments,
        IRepository<B2bIntegrationEvent> integrationEvents)
    {
        _catalogProducts = catalogProducts;
        _aliases = aliases;
        _priceLists = priceLists;
        _priceListItems = priceListItems;
        _inventory = inventory;
        _quotes = quotes;
        _orders = orders;
        _carts = carts;
        _payments = payments;
        _integrationEvents = integrationEvents;
    }

    public async Task<ApiResponse<B2bInsightSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeProvider.Now;
        var staleInventoryThreshold = now.AddHours(-24);

        var totalCatalog = await _catalogProducts.Query().CountAsync(x => !x.IsDeleted, cancellationToken);
        var publishedCatalog = await _catalogProducts.Query().CountAsync(x => !x.IsDeleted && x.IsPublished, cancellationToken);
        var catalogWithoutErp = await _catalogProducts.Query().CountAsync(x => !x.IsDeleted && !x.DefaultStockId.HasValue, cancellationToken);
        var catalogWithoutVariants = await _catalogProducts.Query()
            .CountAsync(x => !x.IsDeleted && !x.Variants.Any(v => !v.IsDeleted && v.IsActive), cancellationToken);

        var totalAliases = await _aliases.Query().CountAsync(x => !x.IsDeleted, cancellationToken);
        var matchedAliases = await _aliases.Query().CountAsync(x => !x.IsDeleted && x.MatchStatus == "Matched", cancellationToken);
        var pendingAliases = await _aliases.Query().CountAsync(x => !x.IsDeleted && x.MatchStatus != "Matched", cancellationToken);
        var lowConfidenceAliases = await _aliases.Query()
            .CountAsync(x => !x.IsDeleted && x.MatchStatus == "Matched" && x.ConfidenceScore.HasValue && x.ConfidenceScore < 80, cancellationToken);

        var activePriceLists = await _priceLists.Query().CountAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);
        var activePriceItems = await _priceListItems.Query().CountAsync(x => !x.IsDeleted, cancellationToken);
        var priceItemsQuery = _priceListItems.Query();
        var pricedCatalogProducts = await _catalogProducts.Query().CountAsync(x => !x.IsDeleted && priceItemsQuery.Any(p =>
            !p.IsDeleted &&
            (p.CatalogProductId == x.Id || (x.DefaultStockId.HasValue && p.ErpStockId == x.DefaultStockId.Value))), cancellationToken);

        var inventorySnapshots = await _inventory.Query().CountAsync(x => !x.IsDeleted, cancellationToken);
        var staleInventory = await _inventory.Query().CountAsync(x => !x.IsDeleted && x.SnapshotDate < staleInventoryThreshold, cancellationToken);
        var zeroAvailableInventory = await _inventory.Query().CountAsync(x => !x.IsDeleted && x.AvailableQuantity <= 0, cancellationToken);

        var draftCarts = await _carts.Query().CountAsync(x => !x.IsDeleted && x.Status == "Draft", cancellationToken);
        var orders = await _orders.Query().CountAsync(x => !x.IsDeleted, cancellationToken);
        var waitingPaymentOrders = await _orders.Query().CountAsync(x => !x.IsDeleted && x.Status == "WaitingPayment", cancellationToken);
        var quoteRequests = await _quotes.Query().CountAsync(x => !x.IsDeleted, cancellationToken);
        var openQuotes = await _quotes.Query().CountAsync(x => !x.IsDeleted && x.Status != "Approved" && x.Status != "Rejected", cancellationToken);
        var pendingPayments = await _payments.Query().CountAsync(x => !x.IsDeleted && x.Status == "Pending", cancellationToken);

        var pendingIntegrationEvents = await _integrationEvents.Query().CountAsync(x => !x.IsDeleted && x.Status == "Pending", cancellationToken);
        var failedIntegrationEvents = await _integrationEvents.Query().CountAsync(x => !x.IsDeleted && x.Status == "Failed", cancellationToken);
        var processedIntegrationEvents = await _integrationEvents.Query()
            .CountAsync(x => !x.IsDeleted && x.ProcessedDate.HasValue && x.ProcessedDate >= now.AddHours(-24), cancellationToken);

        var metrics = new List<B2bInsightMetricDto>
        {
            Metric("catalog.total", "Katalog ürünleri", "Katalog", totalCatalog, publishedCatalog, "adet", StatusByPositive(totalCatalog), "Yayınlanan ürün sayısı ikinci değer olarak gösterilir."),
            Metric("catalog.erpLinked", "ERP bağlantı oranı", "Katalog", Percent(totalCatalog - catalogWithoutErp, totalCatalog), null, "%", StatusByRate(Percent(totalCatalog - catalogWithoutErp, totalCatalog), 80, 95), $"{catalogWithoutErp} ürün ERP stok bağlantısı bekliyor."),
            Metric("catalog.variantCoverage", "Varyant kapsamı", "Katalog", Percent(totalCatalog - catalogWithoutVariants, totalCatalog), null, "%", StatusByRate(Percent(totalCatalog - catalogWithoutVariants, totalCatalog), 70, 90), $"{catalogWithoutVariants} ürün aktif varyant olmadan duruyor."),
            Metric("matching.rate", "Ürün eşleşme oranı", "Eşleştirme", Percent(matchedAliases, totalAliases), matchedAliases, "%", StatusByRate(Percent(matchedAliases, totalAliases), 75, 92), $"{pendingAliases} müşteri SKU eşleşme bekliyor."),
            Metric("matching.lowConfidence", "Düşük güvenli eşleşme", "Eşleştirme", lowConfidenceAliases, null, "adet", lowConfidenceAliases > 0 ? "Warning" : "Good", "Güven skoru 80 altındaki onaylı eşleşmeler."),
            Metric("pricing.activeLists", "Aktif fiyat listesi", "Fiyat", activePriceLists, activePriceItems, "adet", StatusByPositive(activePriceLists), "Fiyat kalemleri ikinci değer olarak gösterilir."),
            Metric("pricing.catalogCoverage", "Fiyat kapsamı", "Fiyat", Percent(pricedCatalogProducts, totalCatalog), null, "%", StatusByRate(Percent(pricedCatalogProducts, totalCatalog), 70, 90), "Katalog ürünlerinin fiyat listesi kalemiyle kapsanma oranı."),
            Metric("inventory.snapshots", "Stok snapshot", "Stok", inventorySnapshots, zeroAvailableInventory, "adet", StatusByPositive(inventorySnapshots), "Sıfır/negatif kullanılabilir stok ikinci değer olarak gösterilir."),
            Metric("inventory.freshness", "Güncel stok oranı", "Stok", Percent(inventorySnapshots - staleInventory, inventorySnapshots), null, "%", StatusByRate(Percent(inventorySnapshots - staleInventory, inventorySnapshots), 80, 95), $"{staleInventory} snapshot 24 saatten eski."),
            Metric("commerce.orders", "Siparişler", "Ticaret", orders, waitingPaymentOrders, "adet", StatusByPositive(orders), "Ödeme bekleyen siparişler ikinci değer olarak gösterilir."),
            Metric("commerce.quotes", "Teklif talepleri", "Ticaret", quoteRequests, openQuotes, "adet", StatusByPositive(quoteRequests), "Açık teklif talepleri ikinci değer olarak gösterilir."),
            Metric("commerce.draftCarts", "Taslak sepetler", "Ticaret", draftCarts, null, "adet", draftCarts > 20 ? "Warning" : "Neutral", "Sepetten siparişe dönüşmeyen aktif taslaklar."),
            Metric("payments.pending", "Bekleyen ödemeler", "Ödeme", pendingPayments, null, "adet", pendingPayments > 0 ? "Warning" : "Good", "Dış ödeme API entegrasyonu için takip edilecek bekleyen kayıtlar."),
            Metric("integration.pending", "ERP bekleyen event", "ERP", pendingIntegrationEvents, processedIntegrationEvents, "adet", pendingIntegrationEvents > 0 ? "Warning" : "Good", "Son 24 saatte işlenen event sayısı ikinci değer olarak gösterilir."),
            Metric("integration.failed", "ERP hatalı event", "ERP", failedIntegrationEvents, null, "adet", failedIntegrationEvents > 0 ? "Critical" : "Good", "ERP aktarımında hata alan kayıtlar.")
        };

        var actions = BuildActions(
            totalCatalog,
            catalogWithoutErp,
            catalogWithoutVariants,
            pendingAliases,
            lowConfidenceAliases,
            activePriceLists,
            activePriceItems,
            staleInventory,
            pendingPayments,
            pendingIntegrationEvents,
            failedIntegrationEvents);

        var readinessScore = CalculateReadiness(metrics);
        var summary = new B2bInsightSummaryDto
        {
            GeneratedAt = now,
            Readiness = new B2bInsightScoreDto
            {
                Score = readinessScore,
                Status = readinessScore >= 85 ? "Good" : readinessScore >= 65 ? "Warning" : "Critical",
                Message = readinessScore >= 85
                    ? "B2B v1 operasyonel olarak sağlıklı görünüyor."
                    : readinessScore >= 65
                        ? "B2B v1 çalışır durumda; canlıya almadan önce açıkları azaltmak iyi olur."
                        : "B2B v1 için katalog, fiyat, stok veya ERP kuyruğunda kritik hazırlık eksikleri var."
            },
            Metrics = metrics,
            Actions = actions
        };

        return ApiResponse<B2bInsightSummaryDto>.SuccessResult(summary, "B2B insight summary retrieved successfully");
    }

    private static List<B2bInsightActionDto> BuildActions(
        int totalCatalog,
        int catalogWithoutErp,
        int catalogWithoutVariants,
        int pendingAliases,
        int lowConfidenceAliases,
        int activePriceLists,
        int activePriceItems,
        int staleInventory,
        int pendingPayments,
        int pendingIntegrationEvents,
        int failedIntegrationEvents)
    {
        var actions = new List<B2bInsightActionDto>();
        if (totalCatalog == 0) actions.Add(Action("Critical", "Ön katalog boş", "ERP stoklarından B2B katalog ürün havuzu oluşturulmalı.", "/b2b/catalog"));
        if (catalogWithoutErp > 0) actions.Add(Action("Warning", "ERP stok bağlantısı eksik", $"{catalogWithoutErp} katalog ürünü ERP stok referansı olmadan duruyor.", "/b2b/catalog"));
        if (catalogWithoutVariants > 0) actions.Add(Action("Info", "Varyant kapsamını artır", $"{catalogWithoutVariants} ürün aktif varyant olmadan görünüyor.", "/b2b/catalog"));
        if (pendingAliases > 0) actions.Add(Action("Warning", "Müşteri SKU eşleşmeleri bekliyor", $"{pendingAliases} müşteri ürün kodu manuel/otomatik eşleştirme kuyruğunda.", "/b2b/product-matches"));
        if (lowConfidenceAliases > 0) actions.Add(Action("Info", "Düşük güven skorlarını kontrol et", $"{lowConfidenceAliases} eşleşme 80 altı güven skoru ile onaylanmış.", "/b2b/product-matches"));
        if (activePriceLists == 0 || activePriceItems == 0) actions.Add(Action("Critical", "Fiyatlandırma eksik", "Canlı siparişten önce en az bir aktif fiyat listesi ve fiyat kalemi gerekir.", "/b2b/pricing"));
        if (staleInventory > 0) actions.Add(Action("Warning", "Stok snapshot güncelliğini izle", $"{staleInventory} stok snapshot kaydı 24 saatten eski.", "/b2b/inventory"));
        if (pendingPayments > 0) actions.Add(Action("Info", "Ödeme callback kuyruğunu bağla", $"{pendingPayments} ödeme işlemi dış sağlayıcı durum güncellemesi bekliyor.", "/b2b/payments"));
        if (pendingIntegrationEvents > 0) actions.Add(Action("Warning", "ERP event kuyruğunu işle", $"{pendingIntegrationEvents} event ERP aktarımı bekliyor.", "/b2b/integrations"));
        if (failedIntegrationEvents > 0) actions.Add(Action("Critical", "ERP aktarım hatalarını çöz", $"{failedIntegrationEvents} event hata durumunda.", "/b2b/integrations"));
        if (actions.Count == 0) actions.Add(Action("Info", "Yeni faza hazır", "Temel göstergeler temiz. Sıradaki iyi adım hızlı sipariş/reorder veya ödeme sağlayıcı entegrasyonu.", "/b2b/orders"));
        return actions;
    }

    private static int CalculateReadiness(IEnumerable<B2bInsightMetricDto> metrics)
    {
        var score = 100;
        foreach (var metric in metrics)
        {
            score -= metric.Status switch
            {
                "Critical" => 12,
                "Warning" => 6,
                _ => 0
            };
        }
        return Math.Clamp(score, 0, 100);
    }

    private static B2bInsightMetricDto Metric(string key, string label, string group, decimal value, decimal? secondaryValue, string? unit, string status, string? description) => new()
    {
        Key = key,
        Label = label,
        Group = group,
        Value = decimal.Round(value, 2),
        SecondaryValue = secondaryValue.HasValue ? decimal.Round(secondaryValue.Value, 2) : null,
        Unit = unit,
        Status = status,
        Description = description
    };

    private static B2bInsightActionDto Action(string severity, string title, string description, string? route) => new()
    {
        Severity = severity,
        Title = title,
        Description = description,
        TargetRoute = route
    };

    private static decimal Percent(decimal numerator, decimal denominator) => denominator <= 0 ? 0 : numerator / denominator * 100;
    private static string StatusByPositive(int value) => value > 0 ? "Good" : "Warning";
    private static string StatusByRate(decimal value, decimal warningThreshold, decimal goodThreshold) => value >= goodThreshold ? "Good" : value >= warningThreshold ? "Warning" : "Critical";
}
