using Wms.Modules.NetsisIntegrations.Application.Dtos;

namespace Wms.Modules.NetsisIntegrations.Application.Services;

public interface INetsisAuthTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<NetsisTokenResultDto> GetTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
}
