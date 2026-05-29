using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public interface IHepsiburadaMarketplaceClient
{
    Task<HepsiburadaClientResult> SendAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default);
    Task<HepsiburadaTransactionStatusResult> GetTransactionStatusAsync(MarketplaceSyncEvent syncEvent, CancellationToken cancellationToken = default);
}
