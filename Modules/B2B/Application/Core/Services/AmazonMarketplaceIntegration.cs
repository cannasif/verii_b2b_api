using Wms.Application.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface IAmazonMarketplaceIntegration : IMarketplaceProviderIntegration;

public sealed class AmazonMarketplaceIntegration : MarketplaceProviderIntegrationBase, IAmazonMarketplaceIntegration
{
    public AmazonMarketplaceIntegration(IRepository<MarketplaceListing> listings, IRepository<MarketplaceSyncEvent> events, IUnitOfWork unitOfWork)
        : base(listings, events, unitOfWork)
    {
    }

    public override string ProviderKey => "Amazon";
}
