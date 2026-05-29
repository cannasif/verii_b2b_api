using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

public abstract class B2bMarketplaceProviderControllerBase<TIntegration> : ControllerBase
    where TIntegration : IMarketplaceProviderIntegration
{
    private readonly TIntegration _integration;

    protected B2bMarketplaceProviderControllerBase(TIntegration integration)
    {
        _integration = integration;
    }

    [HttpPost("product-create")]
    public async Task<ActionResult<ApiResponse<MarketplaceSyncEventDto>>> QueueProductCreate([FromBody] QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _integration.QueueProductCreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("price-update")]
    public async Task<ActionResult<ApiResponse<MarketplaceSyncEventDto>>> QueuePriceUpdate([FromBody] QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _integration.QueuePriceUpdateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("stock-update")]
    public async Task<ActionResult<ApiResponse<MarketplaceSyncEventDto>>> QueueStockUpdate([FromBody] QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _integration.QueueStockUpdateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("order-import")]
    public async Task<ActionResult<ApiResponse<MarketplaceSyncEventDto>>> QueueOrderImport([FromBody] QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _integration.QueueOrderImportAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

[ApiController]
[Route("api/b2b/marketplaces/trendyol")]
[Authorize]
public sealed class B2bTrendyolMarketplaceController : B2bMarketplaceProviderControllerBase<ITrendyolMarketplaceIntegration>
{
    public B2bTrendyolMarketplaceController(ITrendyolMarketplaceIntegration integration)
        : base(integration)
    {
    }
}

[ApiController]
[Route("api/b2b/marketplaces/hepsiburada")]
[Authorize]
public sealed class B2bHepsiburadaMarketplaceController : B2bMarketplaceProviderControllerBase<IHepsiburadaMarketplaceIntegration>
{
    public B2bHepsiburadaMarketplaceController(IHepsiburadaMarketplaceIntegration integration)
        : base(integration)
    {
    }
}

[ApiController]
[Route("api/b2b/marketplaces/amazon")]
[Authorize]
public sealed class B2bAmazonMarketplaceController : B2bMarketplaceProviderControllerBase<IAmazonMarketplaceIntegration>
{
    public B2bAmazonMarketplaceController(IAmazonMarketplaceIntegration integration)
        : base(integration)
    {
    }
}

[ApiController]
[Route("api/b2b/marketplaces/etsy")]
[Authorize]
public sealed class B2bEtsyMarketplaceController : B2bMarketplaceProviderControllerBase<IEtsyMarketplaceIntegration>
{
    public B2bEtsyMarketplaceController(IEtsyMarketplaceIntegration integration)
        : base(integration)
    {
    }
}
