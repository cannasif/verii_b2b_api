using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class MarketplaceChannel : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? SellerId { get; set; }
    public string? ApiBaseUrl { get; set; }
    public string AuthType { get; set; } = "ApiKey";
    public string? CredentialsJson { get; set; }
    public bool SupportsProductCreate { get; set; }
    public bool SupportsPriceUpdate { get; set; }
    public bool SupportsStockUpdate { get; set; }
    public bool SupportsOrderImport { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncDate { get; set; }
    public string? Notes { get; set; }

    public List<MarketplaceListing> Listings { get; set; } = new();
}
