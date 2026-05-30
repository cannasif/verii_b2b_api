using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.B2B.Dtos;

public sealed class MarketplaceCapabilityDto
{
    public string ProviderKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool SupportsProductCreate { get; set; }
    public bool SupportsPriceUpdate { get; set; }
    public bool SupportsStockUpdate { get; set; }
    public bool SupportsOrderImport { get; set; }
    public string DocumentationUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public sealed class MarketplaceChannelDto : BaseEntityDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? SellerId { get; set; }
    public string? ApiBaseUrl { get; set; }
    public string AuthType { get; set; } = string.Empty;
    public string? CredentialsMasked { get; set; }
    public bool SupportsProductCreate { get; set; }
    public bool SupportsPriceUpdate { get; set; }
    public bool SupportsStockUpdate { get; set; }
    public bool SupportsOrderImport { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreateMarketplaceChannelDto
{
    [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(160)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(40)] public string ProviderKey { get; set; } = string.Empty;
    [StringLength(120)] public string? SellerId { get; set; }
    [StringLength(500)] public string? ApiBaseUrl { get; set; }
    [StringLength(40)] public string AuthType { get; set; } = "ApiKey";
    public string? CredentialsJson { get; set; }
    public bool? SupportsProductCreate { get; set; }
    public bool? SupportsPriceUpdate { get; set; }
    public bool? SupportsStockUpdate { get; set; }
    public bool? SupportsOrderImport { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class UpdateMarketplaceChannelDto
{
    [StringLength(160)] public string? Name { get; set; }
    [StringLength(120)] public string? SellerId { get; set; }
    [StringLength(500)] public string? ApiBaseUrl { get; set; }
    [StringLength(40)] public string? AuthType { get; set; }
    public string? CredentialsJson { get; set; }
    public bool? SupportsProductCreate { get; set; }
    public bool? SupportsPriceUpdate { get; set; }
    public bool? SupportsStockUpdate { get; set; }
    public bool? SupportsOrderImport { get; set; }
    public bool? IsActive { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class MarketplaceListingDto : BaseEntityDto
{
    public long ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public string? ProviderKey { get; set; }
    public long? CatalogProductId { get; set; }
    public string? CatalogProductName { get; set; }
    public long? ErpStockId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? MarketplaceProductId { get; set; }
    public string? MarketplaceListingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? LastPushedPrice { get; set; }
    public decimal? LastPushedQuantity { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? LastProductSyncDate { get; set; }
    public DateTime? LastPriceSyncDate { get; set; }
    public DateTime? LastStockSyncDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class UpsertMarketplaceListingDto
{
    public long? Id { get; set; }
    public long ChannelId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? ErpStockId { get; set; }
    [Required, StringLength(80)] public string Sku { get; set; } = string.Empty;
    [StringLength(80)] public string? Barcode { get; set; }
    [StringLength(160)] public string? MarketplaceProductId { get; set; }
    [StringLength(160)] public string? MarketplaceListingId { get; set; }
    [StringLength(40)] public string Status { get; set; } = "Draft";
    public decimal? LastPushedPrice { get; set; }
    public decimal? LastPushedQuantity { get; set; }
    [StringLength(10)] public string CurrencyCode { get; set; } = "TRY";
}

public sealed class MarketplaceSyncEventDto : BaseEntityDto
{
    public long ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public string? ProviderKey { get; set; }
    public long? ListingId { get; set; }
    public string? Sku { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ExternalBatchId { get; set; }
    public string? RequestJson { get; set; }
    public string? ResponseJson { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
}

public sealed class QueueMarketplaceSyncDto
{
    public long ListingId { get; set; }
    [Required, StringLength(40)] public string OperationType { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal? Quantity { get; set; }
    [StringLength(10)] public string? CurrencyCode { get; set; }
}
