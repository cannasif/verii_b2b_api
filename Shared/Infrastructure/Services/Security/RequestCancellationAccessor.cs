using Microsoft.AspNetCore.Http;
using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Security;

public sealed class RequestCancellationAccessor : IRequestCancellationAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestCancellationAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CancellationToken Get(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.CanBeCanceled)
        {
            return cancellationToken;
        }

        return _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
    }
}
