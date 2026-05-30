using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/marketplaces")]
[Authorize]
public sealed class B2bMarketplaceController : ControllerBase
{
    private readonly IB2bMarketplaceService _service;

    public B2bMarketplaceController(IB2bMarketplaceService service)
    {
        _service = service;
    }

    [HttpGet("capabilities")]
    public async Task<ActionResult<ApiResponse<List<MarketplaceCapabilityDto>>>> GetCapabilities(CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCapabilitiesAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("channels/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<MarketplaceChannelDto>>>> GetChannels([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetChannelsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("channels")]
    public async Task<ActionResult<ApiResponse<MarketplaceChannelDto>>> CreateChannel([FromBody] CreateMarketplaceChannelDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateChannelAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("channels/{id:long}")]
    public async Task<ActionResult<ApiResponse<MarketplaceChannelDto>>> UpdateChannel(long id, [FromBody] UpdateMarketplaceChannelDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateChannelAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("listings/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<MarketplaceListingDto>>>> GetListings([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetListingsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("listings")]
    public async Task<ActionResult<ApiResponse<MarketplaceListingDto>>> UpsertListing([FromBody] UpsertMarketplaceListingDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpsertListingAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("sync-events/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<MarketplaceSyncEventDto>>>> GetSyncEvents([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetSyncEventsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("sync-events")]
    public async Task<ActionResult<ApiResponse<MarketplaceSyncEventDto>>> QueueSyncEvent([FromBody] QueueMarketplaceSyncDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.QueueSyncEventAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
