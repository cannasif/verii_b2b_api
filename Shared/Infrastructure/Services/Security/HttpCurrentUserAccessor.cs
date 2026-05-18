using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Security;

public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claimValue, out var userId) ? userId : null;
        }
    }

    public string? UserEmail
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Email)?.Value
                ?? user?.FindFirst("email")?.Value
                ?? user?.FindFirst(ClaimTypes.Upn)?.Value
                ?? user?.FindFirst("preferred_username")?.Value;
        }
    }

    public string? BranchCode
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            var itemValue = context?.Items["BranchCode"] as string;
            if (!string.IsNullOrWhiteSpace(itemValue))
            {
                return itemValue;
            }

            var headerValue = context?.Request.Headers["X-Branch-Code"].FirstOrDefault();
            return BranchCodeDefaults.Normalize(headerValue);
        }
    }

    public bool HasPermission(string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return false;
        }

        var isSystemAdmin = string.Equals(
            user.FindFirst("system_admin")?.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);

        return isSystemAdmin
               || user.FindAll("permission").Any(claim =>
                   string.Equals(claim.Value, permissionCode, StringComparison.OrdinalIgnoreCase));
    }
}
