using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface IHepsiburadaMarketplacePayloadMapper
{
    string BuildRequestJson(MarketplaceSyncEvent syncEvent);
}
