using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bMarketplaceService : IB2bMarketplaceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> SupportedOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "ProductCreate",
        "PriceUpdate",
        "StockUpdate",
        "OrderImport"
    };

    private readonly IRepository<MarketplaceChannel> _channels;
    private readonly IRepository<MarketplaceListing> _listings;
    private readonly IRepository<MarketplaceSyncEvent> _events;
    private readonly IRepository<CatalogProduct> _catalogProducts;
    private readonly IUnitOfWork _unitOfWork;

    public B2bMarketplaceService(
        IRepository<MarketplaceChannel> channels,
        IRepository<MarketplaceListing> listings,
        IRepository<MarketplaceSyncEvent> events,
        IRepository<CatalogProduct> catalogProducts,
        IUnitOfWork unitOfWork)
    {
        _channels = channels;
        _listings = listings;
        _events = events;
        _catalogProducts = catalogProducts;
        _unitOfWork = unitOfWork;
    }

    public Task<ApiResponse<List<MarketplaceCapabilityDto>>> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var capabilities = GetProviderCapabilities();
        return Task.FromResult(ApiResponse<List<MarketplaceCapabilityDto>>.SuccessResult(capabilities, "Marketplace capability listesi getirildi."));
    }

    public async Task<ApiResponse<PagedResponse<MarketplaceChannelDto>>> GetChannelsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _channels.Query()
            .Where(x => !x.IsDeleted)
            .ApplySearch(request.Search, nameof(MarketplaceChannel.Code), nameof(MarketplaceChannel.Name), nameof(MarketplaceChannel.ProviderKey), nameof(MarketplaceChannel.SellerId))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(MarketplaceChannel.Id), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<MarketplaceChannelDto>>.SuccessResult(
            new PagedResponse<MarketplaceChannelDto>(items, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize),
            "Marketplace kanalları getirildi.");
    }

    public async Task<ApiResponse<MarketplaceChannelDto>> CreateChannelAsync(CreateMarketplaceChannelDto dto, CancellationToken cancellationToken = default)
    {
        var provider = FindProvider(dto.ProviderKey);
        if (provider == null)
        {
            return ApiResponse<MarketplaceChannelDto>.ErrorResult("Desteklenmeyen pazar yeri sağlayıcısı.", "ProviderKey Trendyol, Hepsiburada, Amazon veya Etsy olmalı.", 400);
        }

        var code = Normalize(dto.Code);
        var exists = await _channels.Query().AnyAsync(x => !x.IsDeleted && x.Code == code, cancellationToken);
        if (exists)
        {
            return ApiResponse<MarketplaceChannelDto>.ErrorResult("Bu kanal kodu zaten kullanılıyor.", "Marketplace channel code duplicated.", 400);
        }

        var entity = new MarketplaceChannel
        {
            Code = code,
            Name = dto.Name.Trim(),
            ProviderKey = provider.ProviderKey,
            SellerId = NullIfWhiteSpace(dto.SellerId),
            ApiBaseUrl = NullIfWhiteSpace(dto.ApiBaseUrl),
            AuthType = string.IsNullOrWhiteSpace(dto.AuthType) ? "ApiKey" : dto.AuthType.Trim(),
            CredentialsJson = NullIfWhiteSpace(dto.CredentialsJson),
            SupportsProductCreate = dto.SupportsProductCreate ?? provider.SupportsProductCreate,
            SupportsPriceUpdate = dto.SupportsPriceUpdate ?? provider.SupportsPriceUpdate,
            SupportsStockUpdate = dto.SupportsStockUpdate ?? provider.SupportsStockUpdate,
            SupportsOrderImport = dto.SupportsOrderImport ?? provider.SupportsOrderImport,
            IsActive = dto.IsActive,
            Notes = NullIfWhiteSpace(dto.Notes),
            CreatedDate = DateTimeProvider.Now
        };

        await _channels.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<MarketplaceChannelDto>.SuccessResult(ToDto(entity), "Marketplace kanalı oluşturuldu.");
    }

    public async Task<ApiResponse<PagedResponse<MarketplaceListingDto>>> GetListingsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _listings.Query()
            .Where(x => !x.IsDeleted)
            .Include(x => x.Channel)
            .Include(x => x.CatalogProduct)
            .ApplySearch(request.Search, nameof(MarketplaceListing.Sku), nameof(MarketplaceListing.Barcode), nameof(MarketplaceListing.MarketplaceProductId), nameof(MarketplaceListing.MarketplaceListingId), nameof(MarketplaceListing.Status))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(MarketplaceListing.Id), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<MarketplaceListingDto>>.SuccessResult(
            new PagedResponse<MarketplaceListingDto>(items, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize),
            "Marketplace ürün eşleşmeleri getirildi.");
    }

    public async Task<ApiResponse<MarketplaceListingDto>> UpsertListingAsync(UpsertMarketplaceListingDto dto, CancellationToken cancellationToken = default)
    {
        var channel = await _channels.Query().FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ChannelId, cancellationToken);
        if (channel == null)
        {
            return ApiResponse<MarketplaceListingDto>.ErrorResult("Pazar yeri kanalı bulunamadı.", "Marketplace channel not found.", 404);
        }

        CatalogProduct? product = null;
        if (dto.CatalogProductId.HasValue)
        {
            product = await _catalogProducts.Query().FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.CatalogProductId.Value, cancellationToken);
            if (product == null)
            {
                return ApiResponse<MarketplaceListingDto>.ErrorResult("Katalog ürünü bulunamadı.", "Catalog product not found.", 404);
            }
        }

        var sku = NormalizeSku(dto.Sku);
        var duplicate = await _listings.Query()
            .AnyAsync(x => !x.IsDeleted && x.ChannelId == dto.ChannelId && x.Sku == sku && (!dto.Id.HasValue || x.Id != dto.Id.Value), cancellationToken);
        if (duplicate)
        {
            return ApiResponse<MarketplaceListingDto>.ErrorResult("Bu pazar yeri kanalında aynı SKU zaten eşlenmiş.", "Marketplace listing SKU duplicated.", 400);
        }

        var entity = dto.Id.HasValue
            ? await _listings.Query(tracking: true).Include(x => x.Channel).Include(x => x.CatalogProduct).FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.Id.Value, cancellationToken)
            : null;

        if (dto.Id.HasValue && entity == null)
        {
            return ApiResponse<MarketplaceListingDto>.ErrorResult("Marketplace ürün eşleşmesi bulunamadı.", "Marketplace listing not found.", 404);
        }

        entity ??= new MarketplaceListing { CreatedDate = DateTimeProvider.Now };
        entity.ChannelId = dto.ChannelId;
        entity.Channel = channel;
        entity.CatalogProductId = dto.CatalogProductId;
        entity.CatalogProduct = product;
        entity.ErpStockId = dto.ErpStockId;
        entity.Sku = sku;
        entity.Barcode = NullIfWhiteSpace(dto.Barcode);
        entity.MarketplaceProductId = NullIfWhiteSpace(dto.MarketplaceProductId);
        entity.MarketplaceListingId = NullIfWhiteSpace(dto.MarketplaceListingId);
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status.Trim();
        entity.LastPushedPrice = dto.LastPushedPrice;
        entity.LastPushedQuantity = dto.LastPushedQuantity;
        entity.CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "TRY" : dto.CurrencyCode.Trim().ToUpperInvariant();
        entity.UpdatedDate = DateTimeProvider.Now;

        if (dto.Id.HasValue)
        {
            _listings.Update(entity);
        }
        else
        {
            await _listings.AddAsync(entity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<MarketplaceListingDto>.SuccessResult(ToDto(entity), dto.Id.HasValue ? "Marketplace ürün eşleşmesi güncellendi." : "Marketplace ürün eşleşmesi oluşturuldu.");
    }

    public async Task<ApiResponse<PagedResponse<MarketplaceSyncEventDto>>> GetSyncEventsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _events.Query()
            .Where(x => !x.IsDeleted)
            .Include(x => x.Channel)
            .Include(x => x.Listing)
            .ApplySearch(request.Search, nameof(MarketplaceSyncEvent.OperationType), nameof(MarketplaceSyncEvent.Status), nameof(MarketplaceSyncEvent.ExternalBatchId), nameof(MarketplaceSyncEvent.ErrorMessage))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(MarketplaceSyncEvent.RequestedDate), string.Equals(request.SortDirection ?? "desc", "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<MarketplaceSyncEventDto>>.SuccessResult(
            new PagedResponse<MarketplaceSyncEventDto>(items, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize),
            "Marketplace aktarım kuyruğu getirildi.");
    }

    public async Task<ApiResponse<MarketplaceSyncEventDto>> QueueSyncEventAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default)
    {
        if (!SupportedOperations.Contains(dto.OperationType))
        {
            return ApiResponse<MarketplaceSyncEventDto>.ErrorResult("Geçersiz marketplace operasyon tipi.", "OperationType ProductCreate, PriceUpdate, StockUpdate veya OrderImport olmalı.", 400);
        }

        var listing = await _listings.Query()
            .Include(x => x.Channel)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ListingId, cancellationToken);
        if (listing == null || listing.Channel == null)
        {
            return ApiResponse<MarketplaceSyncEventDto>.ErrorResult("Marketplace ürün eşleşmesi bulunamadı.", "Marketplace listing not found.", 404);
        }

        if (!SupportsOperation(listing.Channel, dto.OperationType))
        {
            return ApiResponse<MarketplaceSyncEventDto>.ErrorResult("Bu kanal seçilen operasyonu desteklemiyor.", "Marketplace channel does not support this operation.", 400);
        }

        var now = DateTimeProvider.Now;
        var entity = new MarketplaceSyncEvent
        {
            ChannelId = listing.ChannelId,
            ListingId = listing.Id,
            OperationType = dto.OperationType.Trim(),
            Status = "Pending",
            RequestJson = JsonSerializer.Serialize(new
            {
                dto.ListingId,
                listing.Sku,
                listing.MarketplaceListingId,
                dto.Price,
                dto.Quantity,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? listing.CurrencyCode : dto.CurrencyCode.Trim().ToUpperInvariant()
            }, JsonOptions),
            RequestedDate = now,
            CreatedDate = now,
            Channel = listing.Channel,
            Listing = listing
        };

        await _events.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<MarketplaceSyncEventDto>.SuccessResult(ToDto(entity), "Marketplace aktarım kuyruğuna eklendi.");
    }

    private static bool SupportsOperation(MarketplaceChannel channel, string operationType) =>
        operationType.Equals("ProductCreate", StringComparison.OrdinalIgnoreCase) && channel.SupportsProductCreate ||
        operationType.Equals("PriceUpdate", StringComparison.OrdinalIgnoreCase) && channel.SupportsPriceUpdate ||
        operationType.Equals("StockUpdate", StringComparison.OrdinalIgnoreCase) && channel.SupportsStockUpdate ||
        operationType.Equals("OrderImport", StringComparison.OrdinalIgnoreCase) && channel.SupportsOrderImport;

    private static MarketplaceCapabilityDto? FindProvider(string providerKey) =>
        GetProviderCapabilities().FirstOrDefault(x => x.ProviderKey.Equals(providerKey?.Trim(), StringComparison.OrdinalIgnoreCase));

    private static List<MarketplaceCapabilityDto> GetProviderCapabilities() => new()
    {
        Capability("Trendyol", "Trendyol", true, true, true, true, "https://developers.trendyol.com/", "Ürün aktarımı, fiyat/stok güncelleme ve sipariş entegrasyonu batch operasyonlarla ilerler."),
        Capability("Hepsiburada", "Hepsiburada", true, true, true, true, "https://developers.hepsiburada.com/", "Merchant ürün/listing, fiyat-stok ve sipariş senaryoları için ayrı servis uçları kullanılır."),
        Capability("Amazon", "Amazon SP-API", true, true, true, true, "https://developer-docs.amazon.com/sp-api/", "Listings Items, Feeds, Product Pricing ve Inventory akışları ayrı yetki ve rate limitlerle yönetilir."),
        Capability("Etsy", "Etsy", true, true, true, true, "https://developers.etsy.com/documentation/", "Listing ve inventory endpointleri ile ürün kartı, varyasyon, fiyat ve stok güncellenebilir.")
    };

    private static MarketplaceCapabilityDto Capability(string providerKey, string name, bool product, bool price, bool stock, bool order, string documentationUrl, string notes) => new()
    {
        ProviderKey = providerKey,
        Name = name,
        SupportsProductCreate = product,
        SupportsPriceUpdate = price,
        SupportsStockUpdate = stock,
        SupportsOrderImport = order,
        DocumentationUrl = documentationUrl,
        Notes = notes
    };

    private static MarketplaceChannelDto ToDto(MarketplaceChannel entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        Code = entity.Code,
        Name = entity.Name,
        ProviderKey = entity.ProviderKey,
        SellerId = entity.SellerId,
        ApiBaseUrl = entity.ApiBaseUrl,
        AuthType = entity.AuthType,
        SupportsProductCreate = entity.SupportsProductCreate,
        SupportsPriceUpdate = entity.SupportsPriceUpdate,
        SupportsStockUpdate = entity.SupportsStockUpdate,
        SupportsOrderImport = entity.SupportsOrderImport,
        IsActive = entity.IsActive,
        LastSyncDate = entity.LastSyncDate,
        Notes = entity.Notes
    };

    private static MarketplaceListingDto ToDto(MarketplaceListing entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        ChannelId = entity.ChannelId,
        ChannelName = entity.Channel?.Name,
        ProviderKey = entity.Channel?.ProviderKey,
        CatalogProductId = entity.CatalogProductId,
        CatalogProductName = entity.CatalogProduct?.Name,
        ErpStockId = entity.ErpStockId,
        Sku = entity.Sku,
        Barcode = entity.Barcode,
        MarketplaceProductId = entity.MarketplaceProductId,
        MarketplaceListingId = entity.MarketplaceListingId,
        Status = entity.Status,
        LastPushedPrice = entity.LastPushedPrice,
        LastPushedQuantity = entity.LastPushedQuantity,
        CurrencyCode = entity.CurrencyCode,
        LastProductSyncDate = entity.LastProductSyncDate,
        LastPriceSyncDate = entity.LastPriceSyncDate,
        LastStockSyncDate = entity.LastStockSyncDate,
        ErrorMessage = entity.ErrorMessage
    };

    private static MarketplaceSyncEventDto ToDto(MarketplaceSyncEvent entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        ChannelId = entity.ChannelId,
        ChannelName = entity.Channel?.Name,
        ProviderKey = entity.Channel?.ProviderKey,
        ListingId = entity.ListingId,
        Sku = entity.Listing?.Sku,
        OperationType = entity.OperationType,
        Status = entity.Status,
        ExternalBatchId = entity.ExternalBatchId,
        RequestJson = entity.RequestJson,
        ResponseJson = entity.ResponseJson,
        ErrorMessage = entity.ErrorMessage,
        RetryCount = entity.RetryCount,
        RequestedDate = entity.RequestedDate,
        ProcessedDate = entity.ProcessedDate
    };

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeSku(string value) => value.Trim().ToUpperInvariant();
    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
