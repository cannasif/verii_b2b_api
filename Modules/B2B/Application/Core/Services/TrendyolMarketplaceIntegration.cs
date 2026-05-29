using Wms.Application.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface ITrendyolMarketplaceIntegration : IMarketplaceProviderIntegration;

public sealed class TrendyolMarketplaceIntegration : MarketplaceProviderIntegrationBase, ITrendyolMarketplaceIntegration
{
    public TrendyolMarketplaceIntegration(IRepository<MarketplaceListing> listings, IRepository<MarketplaceSyncEvent> events, IUnitOfWork unitOfWork)
        : base(listings, events, unitOfWork)
    {
    }

    public override string ProviderKey => "Trendyol";
}
