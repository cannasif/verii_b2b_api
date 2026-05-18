using Hangfire.Dashboard;
using System.Security.Claims;

namespace Wms.WebApi.Helpers;

public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var user = httpContext.User;
        return user.IsInRole("Admin")
            || user.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "Admin");
    }
}
