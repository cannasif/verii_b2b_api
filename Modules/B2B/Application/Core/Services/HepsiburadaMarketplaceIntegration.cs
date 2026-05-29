using Wms.Application.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface IHepsiburadaMarketplaceIntegration : IMarketplaceProviderIntegration;

public sealed class HepsiburadaMarketplaceIntegration : MarketplaceProviderIntegrationBase, IHepsiburadaMarketplaceIntegration
{
    public HepsiburadaMarketplaceIntegration(IRepository<MarketplaceListing> listings, IRepository<MarketplaceSyncEvent> events, IUnitOfWork unitOfWork)
        : base(listings, events, unitOfWork)
    {
    }

    public override string ProviderKey => "Hepsiburada";
}
