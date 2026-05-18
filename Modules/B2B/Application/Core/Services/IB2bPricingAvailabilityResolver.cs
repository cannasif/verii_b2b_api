using Wms.Application.B2B.Dtos;
using Wms.Application.Common;

namespace Wms.Application.B2B.Services;

public interface IB2bPricingAvailabilityResolver
{
    Task<ApiResponse<B2bPriceAvailabilityDto>> ResolveAsync(ResolveB2bPriceAvailabilityDto dto, CancellationToken cancellationToken = default);
}
