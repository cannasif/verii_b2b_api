using Wms.Application.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface IEtsyMarketplaceIntegration : IMarketplaceProviderIntegration;

public sealed class EtsyMarketplaceIntegration : MarketplaceProviderIntegrationBase, IEtsyMarketplaceIntegration
{
    public EtsyMarketplaceIntegration(IRepository<MarketplaceListing> listings, IRepository<MarketplaceSyncEvent> events, IUnitOfWork unitOfWork)
        : base(listings, events, unitOfWork)
    {
    }

    public override string ProviderKey => "Etsy";
}
