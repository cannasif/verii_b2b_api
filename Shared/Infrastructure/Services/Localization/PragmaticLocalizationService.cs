using System.Globalization;
using Microsoft.AspNetCore.Http;
using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Localization;

public sealed class PragmaticLocalizationService : ILocalizationService
{
    private readonly IReadOnlyDictionary<string, Dictionary<string, string>> _messages;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PragmaticLocalizationService(LocalizationRegistry registry, IHttpContextAccessor httpContextAccessor)
    {
        _messages = registry.Messages;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetLocalizedString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var culture = ResolveCulture();

        if (TryGetRegistryValue(culture, key, out var value))
        {
            return value;
        }

        if (TryGetRegistryValue("en", key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public string GetLocalizedString(string key, params object[] args)
    {
        var template = GetLocalizedString(key);
        try
        {
            return args == null || args.Length == 0 ? template : string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }

    private bool TryGetRegistryValue(string culture, string key, out string value)
    {
        value = string.Empty;

        if (!_messages.TryGetValue(culture, out var cultureMessages))
        {
            return false;
        }

        if (!cultureMessages.TryGetValue(key, out var localizedValue) || string.IsNullOrWhiteSpace(localizedValue))
        {
            return false;
        }

        value = localizedValue;
        return true;
    }

    private string ResolveCulture()
    {
        var context = _httpContextAccessor.HttpContext;
        var requestedCulture =
            context?.Request.Headers["X-Language"].FirstOrDefault()
            ?? context?.Request.Headers.AcceptLanguage.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(requestedCulture))
        {
            return NormalizeCulture(requestedCulture);
        }

        return NormalizeCulture(CultureInfo.CurrentUICulture.Name);
    }

    private static string NormalizeCulture(string value)
    {
        return LocalizationDefaults.NormalizeNeutralCulture(value);
    }
}
