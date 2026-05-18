using System.Globalization;

namespace Wms.Application.Common;

public static class LocalizationDefaults
{
    public const string DefaultCulture = "tr-TR";
    public const string EnglishCulture = "en-US";

    private static readonly IReadOnlyDictionary<string, string> SupportedCultureMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tr"] = "tr-TR",
            ["tr-tr"] = "tr-TR",
            ["en"] = "en-US",
            ["en-us"] = "en-US",
            ["de"] = "de-DE",
            ["de-de"] = "de-DE",
            ["fr"] = "fr-FR",
            ["fr-fr"] = "fr-FR",
            ["es"] = "es-ES",
            ["es-es"] = "es-ES",
            ["it"] = "it-IT",
            ["it-it"] = "it-IT",
            ["ar"] = "ar-SA",
            ["ar-sa"] = "ar-SA"
        };

    public static IReadOnlyList<CultureInfo> SupportedCultures { get; } =
        SupportedCultureMap.Values
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static culture => new CultureInfo(culture))
            .ToArray();

    public static string NormalizeSpecificCulture(string? value)
    {
        return TryNormalizeSpecificCulture(value, out var culture)
            ? culture
            : DefaultCulture;
    }

    public static bool TryNormalizeSpecificCulture(string? value, out string culture)
    {
        culture = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = ExtractLanguageToken(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        if (SupportedCultureMap.TryGetValue(normalized, out var supportedCulture))
        {
            culture = supportedCulture;
            return true;
        }

        var primary = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        if (primary == null || !SupportedCultureMap.TryGetValue(primary, out supportedCulture))
        {
            return false;
        }

        culture = supportedCulture;
        return true;
    }

    public static string NormalizeNeutralCulture(string? value)
    {
        var specificCulture = NormalizeSpecificCulture(value);
        return specificCulture.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()
            ?.ToLowerInvariant() ?? "tr";
    }

    private static string ExtractLanguageToken(string value)
    {
        return value.Split(new[] { ',', ';', '_' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()
            ?.Replace('_', '-')
            .ToLowerInvariant() ?? string.Empty;
    }
}
