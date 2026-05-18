using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Localization;

public sealed class LocalizationRegistry
{
    public LocalizationRegistry(IEnumerable<ILocalizationResource> resources)
    {
        var merged = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var resource in resources)
        {
            foreach (var cultureEntry in resource.MessagesByCulture)
            {
                if (!merged.TryGetValue(cultureEntry.Key, out var cultureMessages))
                {
                    cultureMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    merged[cultureEntry.Key] = cultureMessages;
                }

                foreach (var entry in cultureEntry.Value)
                {
                    cultureMessages[entry.Key] = entry.Value;
                }
            }
        }

        Messages = merged;
    }

    public IReadOnlyDictionary<string, Dictionary<string, string>> Messages { get; }
}
