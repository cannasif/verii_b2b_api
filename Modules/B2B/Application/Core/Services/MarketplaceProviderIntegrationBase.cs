using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface IMarketplaceProviderIntegration
{
    string ProviderKey { get; }
    Task<ApiResponse<MarketplaceSyncEventDto>> QueueProductCreateAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceSyncEventDto>> QueuePriceUpdateAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceSyncEventDto>> QueueStockUpdateAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketplaceSyncEventDto>> QueueOrderImportAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default);
}

public abstract class MarketplaceProviderIntegrationBase : IMarketplaceProviderIntegration
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IRepository<MarketplaceListing> _listings;
    private readonly IRepository<MarketplaceSyncEvent> _events;
    private readonly IUnitOfWork _unitOfWork;

    protected MarketplaceProviderIntegrationBase(
        IRepository<MarketplaceListing> listings,
        IRepository<MarketplaceSyncEvent> events,
        IUnitOfWork unitOfWork)
    {
        _listings = listings;
        _events = events;
        _unitOfWork = unitOfWork;
    }

    public abstract string ProviderKey { get; }

    public Task<ApiResponse<MarketplaceSyncEventDto>> QueueProductCreateAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default) =>
        QueueAsync(dto, "ProductCreate", cancellationToken);

    public Task<ApiResponse<MarketplaceSyncEventDto>> QueuePriceUpdateAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default) =>
        QueueAsync(dto, "PriceUpdate", cancellationToken);

    public Task<ApiResponse<MarketplaceSyncEventDto>> QueueStockUpdateAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default) =>
        QueueAsync(dto, "StockUpdate", cancellationToken);

    public Task<ApiResponse<MarketplaceSyncEventDto>> QueueOrderImportAsync(QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default) =>
        QueueAsync(dto, "OrderImport", cancellationToken);

    private async Task<ApiResponse<MarketplaceSyncEventDto>> QueueAsync(QueueMarketplaceSyncDto dto, string operationType, CancellationToken cancellationToken)
    {
        var listing = await _listings.Query()
            .Include(x => x.Channel)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ListingId, cancellationToken);
        if (listing == null || listing.Channel == null)
        {
            return ApiResponse<MarketplaceSyncEventDto>.ErrorResult("Pazar yeri ürün yayını bulunamadı.", "Marketplace listing not found.", 404);
        }

        if (!listing.Channel.ProviderKey.Equals(ProviderKey, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<MarketplaceSyncEventDto>.ErrorResult(
                $"{ProviderKey} modülünde farklı pazar yeri kanalı çalıştırılamaz.",
                $"Listing provider is {listing.Channel.ProviderKey}.",
                400);
        }

        if (!SupportsOperation(listing.Channel, operationType))
        {
            return ApiResponse<MarketplaceSyncEventDto>.ErrorResult("Bu kanal seçilen operasyonu desteklemiyor.", "Marketplace channel does not support this operation.", 400);
        }

        var now = DateTimeProvider.Now;
        var entity = new MarketplaceSyncEvent
        {
            ChannelId = listing.ChannelId,
            ListingId = listing.Id,
            OperationType = operationType,
            Status = "Pending",
            RequestJson = JsonSerializer.Serialize(BuildProviderPayload(listing, dto, operationType), JsonOptions),
            RequestedDate = now,
            CreatedDate = now,
            Channel = listing.Channel,
            Listing = listing
        };

        await _events.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<MarketplaceSyncEventDto>.SuccessResult(ToDto(entity), $"{ProviderKey} {operationType} kuyruğa alındı.");
    }

    protected virtual object BuildProviderPayload(MarketplaceListing listing, QueueMarketplaceSyncDto dto, string operationType) => new
    {
        ProviderKey,
        OperationType = operationType,
        dto.ListingId,
        listing.ChannelId,
        listing.Sku,
        listing.Barcode,
        listing.MarketplaceProductId,
        listing.MarketplaceListingId,
        dto.Price,
        dto.Quantity,
        CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? listing.CurrencyCode : dto.CurrencyCode.Trim().ToUpperInvariant()
    };

    private static bool SupportsOperation(MarketplaceChannel channel, string operationType) =>
        operationType.Equals("ProductCreate", StringComparison.OrdinalIgnoreCase) && channel.SupportsProductCreate ||
        operationType.Equals("PriceUpdate", StringComparison.OrdinalIgnoreCase) && channel.SupportsPriceUpdate ||
        operationType.Equals("StockUpdate", StringComparison.OrdinalIgnoreCase) && channel.SupportsStockUpdate ||
        operationType.Equals("OrderImport", StringComparison.OrdinalIgnoreCase) && channel.SupportsOrderImport;

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
}
