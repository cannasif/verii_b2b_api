using Microsoft.AspNetCore.Http;
using Wms.Application.Common;
using Wms.WebApi.Telemetry;

namespace Wms.Infrastructure.Services.Security;

public sealed class HttpRequestTraceAccessor : IRequestTraceAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestTraceAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TraceId => _httpContextAccessor.HttpContext?.GetTraceId();
}
