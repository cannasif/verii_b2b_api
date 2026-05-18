using Microsoft.AspNetCore.Localization;
using Wms.Application.Common;

namespace Wms.WebApi.Localization;

/// <summary>
/// Reads the preferred UI culture from x-language / X-Language and normalizes short forms like tr or en-US.
/// </summary>
public sealed class CustomHeaderRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue("x-language", out var headerValue))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var raw = headerValue.ToString().Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        if (!LocalizationDefaults.TryNormalizeSpecificCulture(raw, out var culture))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
    }
}
