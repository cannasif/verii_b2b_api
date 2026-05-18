namespace Wms.Modules.NetsisIntegrations.Application.Services;

public interface INetsisRestClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
