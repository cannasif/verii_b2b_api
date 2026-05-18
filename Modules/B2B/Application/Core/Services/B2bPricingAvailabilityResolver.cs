using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;
using Wms.Modules.NetsisIntegrations.Application.Services;
using CustomerEntity = Wms.Domain.Entities.Customer.Customer;
using StockEntity = Wms.Domain.Entities.Stock.Stock;

namespace Wms.Application.B2B.Services;

public sealed class B2bPricingAvailabilityResolver : IB2bPricingAvailabilityResolver
{
    private readonly IRepository<B2bCompany> _companies;
    private readonly IRepository<CatalogProduct> _catalogProducts;
    private readonly IRepository<CatalogVariant> _catalogVariants;
    private readonly IRepository<CustomerProductAlias> _aliases;
    private readonly IRepository<CustomerPriceList> _priceLists;
    private readonly IRepository<CustomerPriceListItem> _priceListItems;
    private readonly IRepository<InventorySnapshot> _inventory;
    private readonly IRepository<StockEntity> _stocks;
    private readonly IRepository<CustomerEntity> _customers;
    private readonly INetsisReadService _netsisReadService;

    public B2bPricingAvailabilityResolver(
        IRepository<B2bCompany> companies,
        IRepository<CatalogProduct> catalogProducts,
        IRepository<CatalogVariant> catalogVariants,
        IRepository<CustomerProductAlias> aliases,
        IRepository<CustomerPriceList> priceLists,
        IRepository<CustomerPriceListItem> priceListItems,
        IRepository<InventorySnapshot> inventory,
        IRepository<StockEntity> stocks,
        IRepository<CustomerEntity> customers,
        INetsisReadService netsisReadService)
    {
        _companies = companies;
        _catalogProducts = catalogProducts;
        _catalogVariants = catalogVariants;
        _aliases = aliases;
        _priceLists = priceLists;
        _priceListItems = priceListItems;
        _inventory = inventory;
        _stocks = stocks;
        _customers = customers;
        _netsisReadService = netsisReadService;
    }

    public async Task<ApiResponse<B2bPriceAvailabilityDto>> ResolveAsync(ResolveB2bPriceAvailabilityDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CustomerId <= 0)
        {
            return ApiResponse<B2bPriceAvailabilityDto>.ErrorResult("Customer is required", statusCode: 400);
        }

        if (dto.Quantity <= 0)
        {
            return ApiResponse<B2bPriceAvailabilityDto>.ErrorResult("Quantity must be greater than zero", statusCode: 400);
        }

        var requestedDate = dto.RequestedDate ?? DateTimeProvider.Now;
        var currencyCode = NormalizeCurrency(dto.CurrencyCode);
        var request = await ResolveProductScopeAsync(dto, cancellationToken);
        if (!request.HasAnyProductScope)
        {
            return ApiResponse<B2bPriceAvailabilityDto>.ErrorResult("Catalog product, customer SKU, or ERP stock is required", statusCode: 400);
        }

        var customerGroupCode = await ResolveCustomerGroupCodeAsync(dto.CustomerId, dto.CustomerGroupCode, cancellationToken);
        var response = new B2bPriceAvailabilityDto
        {
            CustomerId = dto.CustomerId,
            CustomerGroupCode = customerGroupCode,
            CatalogProductId = request.CatalogProductId,
            CatalogVariantId = request.CatalogVariantId,
            ErpStockId = request.ErpStockId,
            ResolvedSku = request.ResolvedSku,
            ResolvedName = request.ResolvedName,
            RequestedQuantity = dto.Quantity,
            CurrencyCode = currencyCode
        };

        ApplyPrice(response, await ResolvePriceAsync(dto.CustomerId, customerGroupCode, request, dto.Quantity, currencyCode, requestedDate, cancellationToken));
        ApplyAvailability(response, await ResolveAvailabilityAsync(request, dto.WarehouseCode, cancellationToken), dto.Quantity);

        if (!response.IsPriceResolved)
        {
            response.Warnings.Add("Bu müşteri/ürün/miktar için geçerli fiyat bulunamadı.");
        }

        if (response.Warehouses.Count == 0)
        {
            response.Warnings.Add("Bu ürün için B2B stok snapshot kaydı bulunamadı.");
        }
        else if (!response.IsAvailable)
        {
            response.Warnings.Add("Talep edilen miktar için yeterli satılabilir stok yok.");
        }

        return ApiResponse<B2bPriceAvailabilityDto>.SuccessResult(response, "B2B price and availability resolved successfully");
    }

    private async Task<ResolvedProductScope> ResolveProductScopeAsync(ResolveB2bPriceAvailabilityDto dto, CancellationToken cancellationToken)
    {
        var scope = new ResolvedProductScope
        {
            CatalogProductId = dto.CatalogProductId,
            CatalogVariantId = dto.CatalogVariantId,
            ErpStockId = dto.ErpStockId
        };

        if (!string.IsNullOrWhiteSpace(dto.CustomerSku))
        {
            var customerSku = Normalize(dto.CustomerSku);
            var alias = await _aliases.Query()
                .Where(x => !x.IsDeleted && x.CustomerId == dto.CustomerId && x.CustomerSku == customerSku)
                .OrderByDescending(x => x.MatchStatus == "Matched")
                .ThenByDescending(x => x.ConfidenceScore)
                .FirstOrDefaultAsync(cancellationToken);

            if (alias != null)
            {
                scope.CatalogProductId ??= alias.CatalogProductId;
                scope.ErpStockId ??= alias.ErpStockId;
                scope.ResolvedSku = alias.CustomerSku;
                scope.ResolvedName = alias.CustomerProductName;
            }
        }

        if (scope.CatalogVariantId.HasValue)
        {
            var variant = await _catalogVariants.Query()
                .Include(x => x.CatalogProduct)
                .FirstOrDefaultAsync(x => x.Id == scope.CatalogVariantId.Value && !x.IsDeleted && x.IsActive, cancellationToken);
            if (variant != null)
            {
                scope.CatalogProductId ??= variant.CatalogProductId;
                scope.ErpStockId ??= variant.ErpStockId;
                scope.ResolvedSku ??= variant.VariantSku;
                scope.ResolvedName ??= variant.VariantName;
                scope.CatalogProductId = variant.CatalogProductId;
            }
        }

        if (scope.CatalogProductId.HasValue)
        {
            var product = await _catalogProducts.Query()
                .FirstOrDefaultAsync(x => x.Id == scope.CatalogProductId.Value && !x.IsDeleted, cancellationToken);
            if (product != null)
            {
                scope.ErpStockId ??= product.DefaultStockId;
                scope.ResolvedSku ??= product.Sku;
                scope.ResolvedName ??= product.Name;
            }
        }

        return scope;
    }

    private async Task<string?> ResolveCustomerGroupCodeAsync(long customerId, string? requestedGroupCode, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedGroupCode))
        {
            return requestedGroupCode.Trim();
        }

        var company = await _companies.Query()
            .Where(x => !x.IsDeleted && x.CustomerId == customerId && x.Status != "Passive")
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(company?.CustomerGroupCode) ? null : company.CustomerGroupCode.Trim();
    }

    private async Task<ResolvedPrice?> ResolvePriceAsync(
        long customerId,
        string? customerGroupCode,
        ResolvedProductScope scope,
        decimal quantity,
        string currencyCode,
        DateTime requestedDate,
        CancellationToken cancellationToken)
    {
        var query = _priceListItems.Query()
            .Include(x => x.PriceList)
            .Where(x =>
                !x.IsDeleted &&
                x.PriceList != null &&
                !x.PriceList.IsDeleted &&
                x.PriceList.IsActive &&
                x.CurrencyCode == currencyCode &&
                x.PriceList.CurrencyCode == currencyCode &&
                (!x.ValidFrom.HasValue || x.ValidFrom <= requestedDate) &&
                (!x.ValidTo.HasValue || x.ValidTo >= requestedDate) &&
                (!x.PriceList.ValidFrom.HasValue || x.PriceList.ValidFrom <= requestedDate) &&
                (!x.PriceList.ValidTo.HasValue || x.PriceList.ValidTo >= requestedDate) &&
                (!x.MinQuantity.HasValue || x.MinQuantity <= quantity) &&
                (!x.CustomerId.HasValue || x.CustomerId == customerId) &&
                (!x.PriceList.CustomerId.HasValue || x.PriceList.CustomerId == customerId));

        if (!string.IsNullOrWhiteSpace(customerGroupCode))
        {
            query = query.Where(x => x.PriceList!.CustomerGroupCode == null || x.PriceList.CustomerGroupCode == customerGroupCode);
        }
        else
        {
            query = query.Where(x => x.PriceList!.CustomerGroupCode == null);
        }

        query = query.Where(x =>
            (scope.CatalogVariantId.HasValue && x.CatalogVariantId == scope.CatalogVariantId.Value) ||
            (scope.CatalogProductId.HasValue && x.CatalogProductId == scope.CatalogProductId.Value) ||
            (scope.ErpStockId.HasValue && x.ErpStockId == scope.ErpStockId.Value));

        var candidates = await query.ToListAsync(cancellationToken);
        var manualPrice = candidates
            .Select(x => new ResolvedPrice(x, CalculatePriceSpecificity(x, customerId, customerGroupCode, scope)))
            .OrderByDescending(x => x.Specificity)
            .ThenByDescending(x => x.MinQuantity ?? 0)
            .ThenBy(x => x.EffectiveUnitPrice)
            .FirstOrDefault();

        if (manualPrice != null)
        {
            return manualPrice;
        }

        return await ResolveErpPriceAsync(customerId, scope, quantity, currencyCode, requestedDate, cancellationToken);
    }

    private async Task<ResolvedPrice?> ResolveErpPriceAsync(
        long customerId,
        ResolvedProductScope scope,
        decimal quantity,
        string currencyCode,
        DateTime requestedDate,
        CancellationToken cancellationToken)
    {
        if (!scope.ErpStockId.HasValue)
        {
            return null;
        }

        var stock = await _stocks.Query()
            .Where(x => !x.IsDeleted && x.Id == scope.ErpStockId.Value)
            .FirstOrDefaultAsync(cancellationToken);
        if (stock == null)
        {
            return null;
        }

        var customer = await _customers.Query()
            .Where(x => !x.IsDeleted && x.Id == customerId)
            .FirstOrDefaultAsync(cancellationToken);

        var priceListNumber = NormalizePriceListNumber(customer?.PriceListNumber);
        var basePrice = ResolveStockPrice(stock, priceListNumber);
        if (!basePrice.HasValue || basePrice.Value <= 0)
        {
            return null;
        }

        var exchangeRate = await ResolveExchangeRateAsync(currencyCode, requestedDate, cancellationToken);
        if (exchangeRate <= 0)
        {
            return null;
        }

        var unitPrice = NormalizeCurrency(currencyCode) is "TRY" or "TL"
            ? basePrice.Value
            : Math.Round(basePrice.Value / exchangeRate, 4);

        return new ResolvedPrice(
            unitPrice,
            "ErpPriceList" + priceListNumber,
            vatRate: stock.VatRate ?? 0,
            exchangeRate: exchangeRate);
    }

    private async Task<List<B2bWarehouseAvailabilityDto>> ResolveAvailabilityAsync(ResolvedProductScope scope, int? warehouseCode, CancellationToken cancellationToken)
    {
        var query = _inventory.Query().Where(x => !x.IsDeleted);
        if (warehouseCode.HasValue)
        {
            query = query.Where(x => x.WarehouseCode == warehouseCode.Value);
        }

        query = query.Where(x =>
            (scope.CatalogVariantId.HasValue && x.CatalogVariantId == scope.CatalogVariantId.Value) ||
            (scope.CatalogProductId.HasValue && x.CatalogProductId == scope.CatalogProductId.Value) ||
            (scope.ErpStockId.HasValue && x.ErpStockId == scope.ErpStockId.Value));

        var snapshots = await query
            .OrderByDescending(x => x.SnapshotDate)
            .ToListAsync(cancellationToken);

        return snapshots
            .GroupBy(x => x.WarehouseCode)
            .Select(group => group.OrderByDescending(x => x.SnapshotDate).First())
            .OrderByDescending(x => Math.Max(0, x.AvailableQuantity - x.ReservedQuantity))
            .Select(x => new B2bWarehouseAvailabilityDto
            {
                WarehouseCode = x.WarehouseCode,
                WarehouseName = x.WarehouseName,
                AvailableQuantity = x.AvailableQuantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableToSell = Math.Max(0, x.AvailableQuantity - x.ReservedQuantity),
                Unit = x.Unit,
                SnapshotDate = x.SnapshotDate,
                LastErpSyncDate = x.LastErpSyncDate
            })
            .ToList();
    }

    private static void ApplyPrice(B2bPriceAvailabilityDto response, ResolvedPrice? price)
    {
        if (price == null)
        {
            return;
        }

        response.IsPriceResolved = true;
        response.UnitPrice = price.EffectiveUnitPrice;
        response.DiscountRate = price.DiscountRate;
        response.LineTotal = price.EffectiveUnitPrice * response.RequestedQuantity;
        response.VatRate = price.VatRate;
        response.VatAmount = Math.Round((response.LineTotal ?? 0) * price.VatRate / 100m, 4);
        response.ExchangeRate = price.ExchangeRate;
        response.PriceListId = price.PriceListId;
        response.PriceListCode = price.PriceListCode;
        response.PriceSource = price.Source;
        response.PriceResolvedAt = DateTimeProvider.Now;
    }

    private static void ApplyAvailability(B2bPriceAvailabilityDto response, List<B2bWarehouseAvailabilityDto> warehouses, decimal quantity)
    {
        response.Warehouses = warehouses;
        response.AvailableToSell = warehouses.Sum(x => x.AvailableToSell);
        response.ReservedQuantity = warehouses.Sum(x => x.ReservedQuantity);
        response.IsAvailable = response.AvailableToSell >= quantity;
        var preferredWarehouse = warehouses.FirstOrDefault(x => x.AvailableToSell > 0) ?? warehouses.FirstOrDefault();
        response.PreferredWarehouseCode = preferredWarehouse?.WarehouseCode;
        response.InventorySnapshotDate = warehouses.OrderByDescending(x => x.SnapshotDate).FirstOrDefault()?.SnapshotDate;
    }

    private static int CalculatePriceSpecificity(CustomerPriceListItem item, long customerId, string? customerGroupCode, ResolvedProductScope scope)
    {
        var score = 0;
        if (item.PriceList?.CustomerId == customerId) score += 100;
        if (item.CustomerId == customerId) score += 80;
        if (!string.IsNullOrWhiteSpace(customerGroupCode) && item.PriceList?.CustomerGroupCode == customerGroupCode) score += 50;
        if (scope.CatalogVariantId.HasValue && item.CatalogVariantId == scope.CatalogVariantId.Value) score += 40;
        if (scope.CatalogProductId.HasValue && item.CatalogProductId == scope.CatalogProductId.Value) score += 30;
        if (scope.ErpStockId.HasValue && item.ErpStockId == scope.ErpStockId.Value) score += 20;
        if (item.MinQuantity.HasValue) score += Math.Min(15, (int)item.MinQuantity.Value);
        return score;
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeCurrency(string? value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static short NormalizePriceListNumber(short? value) => value is >= 1 and <= 4 ? value.Value : (short)1;

    private static decimal? ResolveStockPrice(StockEntity stock, short priceListNumber)
    {
        return priceListNumber switch
        {
            2 => stock.SalesPrice2,
            3 => stock.SalesPrice3,
            4 => stock.SalesPrice4,
            _ => stock.SalesPrice1
        };
    }

    private async Task<decimal> ResolveExchangeRateAsync(string currencyCode, DateTime requestedDate, CancellationToken cancellationToken)
    {
        var normalized = NormalizeCurrency(currencyCode);
        if (normalized is "TRY" or "TL")
        {
            return 1m;
        }

        var response = await _netsisReadService.GetExchangeRatesAsync(requestedDate.Date, pricingType: 2, cancellationToken);
        var rate = response.Data?
            .FirstOrDefault(x =>
                string.Equals(x.DovizIsmi, normalized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.DovizTipi.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
            ?.KurDegeri;
        return rate is > 0 ? Convert.ToDecimal(rate.Value) : 0m;
    }

    private sealed class ResolvedProductScope
    {
        public long? CatalogProductId { get; set; }
        public long? CatalogVariantId { get; set; }
        public long? ErpStockId { get; set; }
        public string? ResolvedSku { get; set; }
        public string? ResolvedName { get; set; }
        public bool HasAnyProductScope => CatalogProductId.HasValue || CatalogVariantId.HasValue || ErpStockId.HasValue;
    }

    private sealed class ResolvedPrice
    {
        public ResolvedPrice(CustomerPriceListItem item, int specificity)
        {
            Specificity = specificity;
            EffectiveUnitPrice = item.DiscountRate.HasValue
                ? item.UnitPrice * (1 - item.DiscountRate.Value / 100)
                : item.UnitPrice;
            Source = item.CatalogVariantId.HasValue ? "ManualVariantPrice" : item.CatalogProductId.HasValue ? "ManualCatalogPrice" : "ManualErpStockPrice";
            DiscountRate = item.DiscountRate;
            PriceListId = item.PriceListId;
            PriceListCode = item.PriceList?.Code;
            MinQuantity = item.MinQuantity;
        }

        public ResolvedPrice(decimal effectiveUnitPrice, string source, decimal vatRate, decimal exchangeRate)
        {
            EffectiveUnitPrice = effectiveUnitPrice;
            Source = source;
            VatRate = vatRate;
            ExchangeRate = exchangeRate;
        }

        public int Specificity { get; }
        public decimal EffectiveUnitPrice { get; }
        public string Source { get; }
        public decimal? DiscountRate { get; }
        public long? PriceListId { get; }
        public string? PriceListCode { get; }
        public decimal? MinQuantity { get; }
        public decimal VatRate { get; }
        public decimal ExchangeRate { get; } = 1;
    }
}
