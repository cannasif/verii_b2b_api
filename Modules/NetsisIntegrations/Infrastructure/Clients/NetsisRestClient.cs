using System.Net.Http.Headers;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Modules.NetsisIntegrations.Infrastructure.Clients;

public sealed class NetsisRestClient : INetsisRestClient
{
    private readonly HttpClient _httpClient;
    private readonly INetsisAuthTokenService _tokenService;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly ILogger<NetsisRestClient> _logger;

    public NetsisRestClient(
        HttpClient httpClient,
        INetsisAuthTokenService tokenService,
        IRequestTraceAccessor requestTraceAccessor,
        ILogger<NetsisRestClient> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _requestTraceAccessor = requestTraceAccessor;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        if (request.Headers.Authorization == null)
        {
            var accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        _logger.LogInformation(
            "Netsis REST request sending TraceId={TraceId} Method={Method} Path={Path}",
            _requestTraceAccessor.TraceId,
            request.Method.Method,
            request.RequestUri?.IsAbsoluteUri == true ? request.RequestUri.AbsolutePath : request.RequestUri?.ToString());

        return await _httpClient.SendAsync(request, cancellationToken);
    }
}
